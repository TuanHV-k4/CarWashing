using System.Text;
using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class BehavioralLogService : IBehavioralLogService, IBehavioralLogWriter
    {
        private readonly ApplicationDbContext _context;

        public BehavioralLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<BehavioralLogItemDto>> GetLogsAsync(BehavioralLogFilterDto filter)
        {
            var query = BuildQuery(filter);

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);
            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new BehavioralLogItemDto
                {
                    LogID = l.LogID,
                    CustomerID = l.CustomerID,
                    CustomerName = l.Customer != null ? l.Customer.User.FullName : null,
                    ActionType = l.ActionType,
                    ActionTime = l.ActionTime,
                    PointsChanged = l.PointsChanged,
                    SpendingAmount = l.SpendingAmount,
                    Notes = l.Notes
                })
                .ToListAsync();

            return new PagedResult<BehavioralLogItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<byte[]> ExportLogsAsync(BehavioralLogFilterDto filter)
        {
            var query = BuildQuery(filter);
            var logs = await query
                .OrderByDescending(l => l.ActionTime)
                .Take(5000)
                .Select(l => new
                {
                    l.LogID,
                    l.CustomerID,
                    CustomerName = l.Customer != null ? l.Customer.User.FullName : null,
                    l.ActionType,
                    l.ActionTime,
                    l.PointsChanged,
                    l.SpendingAmount,
                    l.Notes
                })
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("LogID,CustomerID,CustomerName,ActionType,ActionTime,PointsChanged,SpendingAmount,Notes");
            foreach (var log in logs)
            {
                sb.AppendLine($"{log.LogID},{log.CustomerID},{EscapeCsv(log.CustomerName)},{log.ActionType},{log.ActionTime:O},{log.PointsChanged},{log.SpendingAmount},{EscapeCsv(log.Notes)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task WriteAsync(BehavioralLogWriteRequest request, CancellationToken cancellationToken = default)
        {
            _context.BehavioralLogs.Add(new DataAccessLayer.Entity.BehavioralLog
            {
                CustomerID = request.CustomerId,
                BookingID = request.BookingId,
                ServiceID = request.ServiceId,
                PromotionID = request.PromotionId,
                ActionType = request.ActionType,
                ActionTime = request.ActionTime ?? DateTime.UtcNow,
                PointsChanged = request.PointsChanged,
                SpendingAmount = request.SpendingAmount,
                RewardUsed = request.RewardUsed,
                PromotionUsed = request.PromotionUsed,
                MetadataJson = request.MetadataJson,
                Notes = request.Notes
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<DataAccessLayer.Entity.BehavioralLog> BuildQuery(BehavioralLogFilterDto filter)
        {
            var query = _context.BehavioralLogs
                .Include(l => l.Customer!)
                    .ThenInclude(c => c.User)
                .AsQueryable();

            if (filter.CustomerID.HasValue)
                query = query.Where(l => l.CustomerID == filter.CustomerID);

            if (filter.ActionType.HasValue)
                query = query.Where(l => l.ActionType == filter.ActionType);

            if (filter.From.HasValue)
                query = query.Where(l => l.ActionTime >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(l => l.ActionTime <= filter.To.Value);

            return query;
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
