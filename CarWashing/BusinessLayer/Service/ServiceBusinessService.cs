using BusinessLayer.Dtos;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = DataAccessLayer.Entity.Service;

namespace BusinessLayer.Service
{
    public class ServiceBusinessService : IServiceBusinessService
    {
        private readonly ApplicationDbContext _context;

        public ServiceBusinessService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceResponseDto>> GetAllAsync()
        {
            return await _context.Services
                .AsNoTracking()
                .Select(MapToDtoExpression)
                .ToListAsync();
        }

        public async Task<ServiceResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Services
                .AsNoTracking()
                .Where(s => s.ServiceID == id)
                .Select(MapToDtoExpression)
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseDto> CreateAsync(CreateServiceDto dto)
        {
            var entity = new ServiceEntity
            {
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                Price = dto.Price,
                EstimatedDuration = dto.EstimatedDuration,
                Status = dto.Status
            };

            _context.Services.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDto(entity);
        }

        public async Task<ServiceResponseDto?> UpdateAsync(Guid id, UpdateServiceDto dto)
        {
            var entity = await _context.Services.FindAsync(id);
            if (entity is null)
                return null;

            entity.ServiceName = dto.ServiceName;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.EstimatedDuration = dto.EstimatedDuration;
            entity.Status = dto.Status;

            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<ServiceResponseDto?> PatchAsync(Guid id, PatchServiceDto dto)
        {
            var entity = await _context.Services.FindAsync(id);
            if (entity is null)
                return null;

            if (dto.ServiceName is not null)
                entity.ServiceName = dto.ServiceName;
            if (dto.Description is not null)
                entity.Description = dto.Description;
            if (dto.Price.HasValue)
                entity.Price = dto.Price.Value;
            if (dto.EstimatedDuration.HasValue)
                entity.EstimatedDuration = dto.EstimatedDuration;
            if (dto.Status.HasValue)
                entity.Status = dto.Status.Value;

            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Services.FindAsync(id);
            if (entity is null)
                return false;

            _context.Services.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ServiceResponseDto MapToDto(ServiceEntity entity) => new()
        {
            ServiceID = entity.ServiceID,
            ServiceName = entity.ServiceName,
            Description = entity.Description,
            Price = entity.Price,
            EstimatedDuration = entity.EstimatedDuration,
            Status = entity.Status
        };

        private static readonly System.Linq.Expressions.Expression<Func<ServiceEntity, ServiceResponseDto>> MapToDtoExpression =
            s => new ServiceResponseDto
            {
                ServiceID = s.ServiceID,
                ServiceName = s.ServiceName,
                Description = s.Description,
                Price = s.Price,
                EstimatedDuration = s.EstimatedDuration,
                Status = s.Status
            };
    }
}
