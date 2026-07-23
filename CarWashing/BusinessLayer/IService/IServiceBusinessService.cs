using BusinessLayer.Dtos;

namespace BusinessLayer.IService
{
    public interface IServiceBusinessService
    {
        Task<IEnumerable<ServiceResponseDto>> GetAllAsync();
        Task<ServiceResponseDto?> GetByIdAsync(Guid id);
        Task<ServiceResponseDto> CreateAsync(CreateServiceDto dto);
        Task<ServiceResponseDto?> UpdateAsync(Guid id, UpdateServiceDto dto);
        Task<ServiceResponseDto?> PatchAsync(Guid id, PatchServiceDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
