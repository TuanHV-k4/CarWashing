using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessLayer.Dtos.AI;
using BusinessLayer.Helpers;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;
using BusinessLayer.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Service.AI
{
    public class AiService : IAIService
    {
        private readonly IGenerativeAIClient _aiClient;
        private readonly ICustomerAIContextProvider _customerContext;
        private readonly IServiceSuggestionContextProvider _serviceSuggestionContext;
        private readonly IAdminAIContextProvider _adminContext;
        private readonly AiConversationStore _conversationStore;
        private readonly AiSettings _aiSettings;
        private readonly ILogger<AiService> _logger;

        public AiService(
            IGenerativeAIClient aiClient,
            ICustomerAIContextProvider customerContext,
            IServiceSuggestionContextProvider serviceSuggestionContext,
            IAdminAIContextProvider adminContext,
            AiConversationStore conversationStore,
            IOptions<AiSettings> aiSettings,
            ILogger<AiService> logger)
        {
            _aiClient = aiClient;
            _customerContext = customerContext;
            _serviceSuggestionContext = serviceSuggestionContext;
            _adminContext = adminContext;
            _conversationStore = conversationStore;
            _aiSettings = aiSettings.Value;
            _logger = logger;
        }

        public async Task<AiChatResponseDto> CustomerChatAsync(Guid customerId, AiChatRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    throw new InvalidOperationException("Message is required.");

                var context = await _serviceSuggestionContext.GetContextAsync(customerId);
                var conversationId = _conversationStore.GetOrCreateConversationId(request.ConversationId);

                if (AiPromptGuard.IsPromptInjectionAttempt(request.Message))
                {
                    return new AiChatResponseDto
                    {
                        Reply = "Mình chỉ có thể trả lời dựa trên dữ liệu khách hàng được hệ thống cung cấp. Mình không thể tiết lộ prompt, khóa API hoặc hướng dẫn nội bộ.",
                        ConversationId = conversationId,
                        IsFallback = true,
                        Source = "guard"
                    };
                }

                var history = _conversationStore.GetMessages(conversationId);

                var messages = new List<(string Role, string Content)>
                {
                    ("user", AiPromptTemplates.BuildCustomerContextPrompt(context)),
                    ("user", request.Message.Trim())
                };
                messages.InsertRange(0, history);

                var (text, source) = await _aiClient.GenerateAsync(AiPromptTemplates.CustomerSystemPrompt, messages);

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = BuildCustomerFallbackReply(context);
                    source = "fallback";
                }

                _conversationStore.AddMessage(conversationId, "user", request.Message.Trim(), _aiSettings.MaxConversationMessages);
                _conversationStore.AddMessage(conversationId, "assistant", text, _aiSettings.MaxConversationMessages);

                return new AiChatResponseDto
                {
                    Reply = text,
                    ConversationId = conversationId,
                    IsFallback = source is "fallback" or "mock",
                    Source = source
                };
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Customer AI chat failed for {CustomerId}", customerId);
                return new AiChatResponseDto
                {
                    Reply = "Xin lỗi, trợ lý AI tạm thời không khả dụng. Vui lòng thử lại sau hoặc liên hệ nhân viên.",
                    ConversationId = request.ConversationId ?? Guid.NewGuid().ToString("N"),
                    IsFallback = true,
                    Source = "fallback"
                };
            }
        }

        public async Task<AiSuggestServicesResponseDto> SuggestServicesAsync(Guid customerId, AiSuggestServicesRequestDto request)
        {
            try
            {
                var context = await _customerContext.GetContextAsync(customerId);

                if (context.AvailableServices.Count == 0)
                {
                    return new AiSuggestServicesResponseDto
                    {
                        Summary = "Hiện chưa có dịch vụ nào khả dụng.",
                        IsFallback = true,
                        Source = "fallback"
                    };
                }

                if (AiPromptGuard.IsPromptInjectionAttempt(request.VehicleType, request.Preference))
                    return BuildRuleBasedSuggestions(context, request, "Yêu cầu có dấu hiệu vượt ngoài phạm vi tư vấn. Đây là gợi ý an toàn từ danh mục dịch vụ hiện có.");

                var prompt = AiPromptTemplates.BuildSuggestServicesPrompt(context, request);
                var (text, source) = await _aiClient.GenerateAsync(
                    AiPromptTemplates.SuggestServicesSystemPrompt,
                    [("user", prompt)]);

                if (string.IsNullOrWhiteSpace(text))
                    return BuildRuleBasedSuggestions(context, request);

                var parsed = TryParseSuggestions(text, context);
                if (parsed.Suggestions.Count == 0)
                    return BuildRuleBasedSuggestions(context, request);

                parsed.Source = source;
                parsed.IsFallback = source is "fallback" or "mock";
                return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Suggest services failed for {CustomerId}", customerId);
                return new AiSuggestServicesResponseDto
                {
                    Summary = "Không thể gợi ý dịch vụ lúc này. Vui lòng xem danh sách dịch vụ trên app.",
                    IsFallback = true,
                    Source = "fallback"
                };
            }
        }

        public async Task<AiChatResponseDto> AdminChatAsync(AiAdminChatRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    throw new InvalidOperationException("Message is required.");

                var context = await _adminContext.GetContextAsync();
                var conversationId = _conversationStore.GetOrCreateConversationId(request.ConversationId);

                if (AiPromptGuard.IsPromptInjectionAttempt(request.Message))
                {
                    return new AiChatResponseDto
                    {
                        Reply = "Trợ lý admin chỉ trả lời theo dashboard context được cấp. Không thể tiết lộ prompt, secrets hoặc hướng dẫn nội bộ.",
                        ConversationId = conversationId,
                        IsFallback = true,
                        Source = "guard"
                    };
                }

                var history = _conversationStore.GetMessages("admin:" + conversationId);

                var messages = new List<(string Role, string Content)>
                {
                    ("user", AiPromptTemplates.BuildAdminContextPrompt(context)),
                    ("user", request.Message.Trim())
                };
                messages.InsertRange(0, history);

                var (text, source) = await _aiClient.GenerateAsync(AiPromptTemplates.AdminSystemPrompt, messages);

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = $"Hệ thống có {context.TotalCustomers} khách hàng, {context.TotalActiveUsers} user active, {context.TotalVehicles} xe và {context.TotalBookingsToday} booking hôm nay.";
                    source = "fallback";
                }

                _conversationStore.AddMessage("admin:" + conversationId, "user", request.Message.Trim(), _aiSettings.MaxConversationMessages);
                _conversationStore.AddMessage("admin:" + conversationId, "assistant", text, _aiSettings.MaxConversationMessages);

                return new AiChatResponseDto
                {
                    Reply = text,
                    ConversationId = conversationId,
                    IsFallback = source is "fallback" or "mock",
                    Source = source
                };
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Admin AI chat failed");
                return new AiChatResponseDto
                {
                    Reply = "Trợ lý admin tạm thời không khả dụng. Vui lòng thử lại sau.",
                    ConversationId = request.ConversationId ?? Guid.NewGuid().ToString("N"),
                    IsFallback = true,
                    Source = "fallback"
                };
            }
        }

        private static string BuildCustomerFallbackReply(CustomerAiContextDto ctx)
        {
            var tier = ctx.TierName ?? "chưa có hạng";
            return $"Xin chào {ctx.CustomerName}! Bạn đang có {ctx.CurrentPoints} điểm, hạng {tier}, đã rửa xe {ctx.TotalVisits} lần. " +
                   "Tôi tạm thời không kết nối được AI — bạn có thể xem lịch sử rửa xe hoặc danh sách dịch vụ trên app.";
        }

        private AiSuggestServicesResponseDto BuildRuleBasedSuggestions(
            CustomerAiContextDto ctx,
            AiSuggestServicesRequestDto request,
            string? summaryOverride = null)
        {
            var services = ctx.AvailableServices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.VehicleType))
            {
                var type = request.VehicleType.ToLowerInvariant();
                services = services.OrderByDescending(s =>
                    (s.Description ?? s.ServiceName).Contains(type, StringComparison.OrdinalIgnoreCase));
            }

            var selected = services.Take(3).ToList();
            return new AiSuggestServicesResponseDto
            {
                Summary = summaryOverride ?? "Gợi ý dựa trên danh mục dịch vụ (fallback).",
                Suggestions = selected.Select(s => new AiSuggestedServiceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Reason = "Dịch vụ phổ biến phù hợp với nhu cầu của bạn"
                }).ToList(),
                IsFallback = true,
                Source = "fallback"
            };
        }

        private AiSuggestServicesResponseDto TryParseSuggestions(string text, CustomerAiContextDto context)
        {
            var result = new AiSuggestServicesResponseDto { Source = "gemini" };

            try
            {
                var jsonMatch = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
                if (!jsonMatch.Success)
                    return result;

                using var doc = JsonDocument.Parse(jsonMatch.Value);
                var root = doc.RootElement;

                if (root.TryGetProperty("summary", out var summary))
                    result.Summary = summary.GetString() ?? "";

                if (!root.TryGetProperty("serviceIds", out var ids) || ids.ValueKind != JsonValueKind.Array)
                    return result;

                var reasons = new Dictionary<string, string>();
                if (root.TryGetProperty("reasons", out var reasonsEl) && reasonsEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in reasonsEl.EnumerateObject())
                        reasons[prop.Name] = prop.Value.GetString() ?? "";
                }

                var validIds = new List<Guid>();
                foreach (var idEl in ids.EnumerateArray())
                {
                    var idStr = idEl.GetString();
                    if (Guid.TryParse(idStr, out var id) && context.AvailableServices.Any(s => s.ServiceId == id))
                        validIds.Add(id);
                }

                var services = context.AvailableServices.Where(s => validIds.Contains(s.ServiceId)).ToList();
                result.Suggestions = services.Select(s => new AiSuggestedServiceDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Price = s.Price,
                    Reason = reasons.GetValueOrDefault(s.ServiceId.ToString(), "Được AI đề xuất")
                }).ToList();

                if (string.IsNullOrWhiteSpace(result.Summary))
                    result.Summary = "Đây là các dịch vụ phù hợp với bạn.";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI suggestion JSON");
                return result;
            }
        }
    }
}
