using BusinessLayer.IService.Operations;

namespace BusinessLayer.Service.Operations
{
    public class NoOpWashCompletionService : IWashCompletionService
    {
        public Task CompleteWashAsync(WashCompletionPayload payload)
        {
            return Task.CompletedTask;
        }
    }
}
