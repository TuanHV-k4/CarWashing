using BusinessLayer.Dtos.AI;

namespace BusinessLayer.IService.AI
{
    public interface ICustomerAIContextProvider
    {
        Task<CustomerAiContextDto> GetContextAsync(Guid customerId);
    }

    public interface IAdminAIContextProvider
    {
        Task<AdminAiContextDto> GetContextAsync();
    }

    public interface IGenerativeAIClient
    {
        Task<(string Text, string Source)> GenerateAsync(
            string systemPrompt,
            IEnumerable<(string Role, string Content)> messages,
            CancellationToken cancellationToken = default);
    }

    public interface IAIService
    {
        Task<AiChatResponseDto> CustomerChatAsync(Guid customerId, AiChatRequestDto request);
        Task<AiSuggestServicesResponseDto> SuggestServicesAsync(Guid customerId, AiSuggestServicesRequestDto request);
        Task<AiChatResponseDto> AdminChatAsync(AiAdminChatRequestDto request);
    }

    public interface IAiInsightsService
    {
        Task<AiCustomerAssistantResponseDto> GetCustomerAssistantAsync(Guid customerId, AiCustomerAssistantRequestDto request, CancellationToken cancellationToken = default);
        Task<AiFeedbackInsightsResponseDto> GetFeedbackInsightsAsync(Guid? requestedBranchId, Guid userId, bool isAdmin, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task<AiOperationsCopilotResponseDto> AskOperationsAsync(Guid userId, bool isAdmin, AiOperationsCopilotRequestDto request, CancellationToken cancellationToken = default);
    }
}
