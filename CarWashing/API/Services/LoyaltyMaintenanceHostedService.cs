using BusinessLayer.IService.Loyalty;

namespace API.Services
{
    public class LoyaltyMaintenanceHostedService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LoyaltyMaintenanceHostedService> _logger;

        public LoyaltyMaintenanceHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<LoyaltyMaintenanceHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await RunOnceAsync(stoppingToken);

                using var timer = new PeriodicTimer(Interval);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunOnceAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Loyalty maintenance service is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loyalty maintenance background loop stopped unexpectedly.");
                throw;
            }
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var maintenanceService = scope.ServiceProvider.GetRequiredService<ILoyaltyMaintenanceService>();
                var result = await maintenanceService.ExpireOverdueItemsAsync(DateTime.UtcNow, cancellationToken);

                if (result.ExpiredPointTransactions > 0 || result.ExpiredRedemptions > 0)
                {
                    _logger.LogInformation(
                        "Loyalty maintenance expired {ExpiredPoints} points from {ExpiredPointTransactions} transactions and {ExpiredRedemptions} redemptions.",
                        result.ExpiredPoints,
                        result.ExpiredPointTransactions,
                        result.ExpiredRedemptions);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loyalty maintenance failed.");
            }
        }
    }
}
