using XXX.Application.Interfaces;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 单人普通地图离线挂机后台推进任务。
    /// </summary>
    public sealed class OfflineBattleWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OfflineBattleWorker> _logger;

        /// <summary>
        /// 初始化离线挂机后台任务。
        /// </summary>
        public OfflineBattleWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OfflineBattleWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// 周期推进离线挂机账号。
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var battleService = scope.ServiceProvider.GetRequiredService<IBattleService>();
                    await battleService.ProcessOfflineBattleTicksAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Offline battle worker tick failed.");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
    }
}
