using XXX.Application.Interfaces;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 好感度每日赠送次数重置服务。
    /// 每日零点清空所有玩家的好感度赠送记录。
    /// </summary>
    public class FavorabilityResetService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FavorabilityResetService> _logger;

        public FavorabilityResetService(IServiceProvider serviceProvider, ILogger<FavorabilityResetService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("好感度每日重置服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextReset = now.Date.AddDays(1);
                    var delay = nextReset - now;

                    _logger.LogInformation("下次好感度重置时间: {NextReset}", nextReset);
                    await Task.Delay(delay, stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IFavorabilityService>();
                    await service.ResetDailyGiftLimitsAsync();

                    _logger.LogInformation("好感度每日赠送次数已重置");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "好感度每日重置失败");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
