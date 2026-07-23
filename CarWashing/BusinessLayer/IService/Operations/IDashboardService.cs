using BusinessLayer.Dtos.Operations;
namespace BusinessLayer.IService.Operations;
public interface IDashboardService { Task<ManagerDashboardResponse?> GetManagerAsync(Guid branchId, DateTime date); Task<AdminDashboardResponse> GetAdminAsync(DateTime from, DateTime to, Guid? branchId); }
