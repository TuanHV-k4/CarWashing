namespace BusinessLayer.IService
{
    public interface ICurrentCustomerService
    {
        Task<Guid> GetCurrentCustomerIdAsync();
    }
}
