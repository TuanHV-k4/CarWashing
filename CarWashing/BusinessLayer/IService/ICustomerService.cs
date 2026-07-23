using BusinessLayer.Dtos.Customer;

namespace BusinessLayer.IService
{
    public interface ICustomerService
    {
        Task<CustomerProfileDto> GetMyProfileAsync();
        Task<CustomerProfileDto> GetProfileByCustomerIdAsync(Guid customerId);
        Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileRequest request);
    }
}
