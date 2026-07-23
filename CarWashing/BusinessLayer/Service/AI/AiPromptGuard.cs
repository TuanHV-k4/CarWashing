using System.Text.RegularExpressions;

namespace BusinessLayer.Service.AI
{
    public static class AiPromptGuard
    {
        private static readonly Regex InjectionPattern = new(
            "(ignore|bypass|override|forget).*(instruction|system|prompt|policy)|" +
            "(system prompt|developer message|api key|secret|jailbreak)|" +
            "(tiet lo|bo qua|ghi de|quen|tiết lộ|bỏ qua|ghi đè|quên).*(huong dan|system|prompt|chinh sach|hướng dẫn|chính sách)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool IsPromptInjectionAttempt(params string?[] inputs)
        {
            return inputs
                .Where(input => !string.IsNullOrWhiteSpace(input))
                .Any(input => InjectionPattern.IsMatch(input!));
        }
    }
}
