using BusinessLayer.Dtos.AI;

namespace BusinessLayer.IService.AI
{
    public interface IServiceSuggestionContextProvider
    {
        Task<CustomerAiContextDto> GetContextAsync(Guid customerId);
    }
}
