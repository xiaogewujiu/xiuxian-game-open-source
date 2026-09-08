using XXX.Application.Interfaces;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 世界 Boss 后台推进任务。
    /// </summary>
    public sealed class WorldBossWorker : BackgroundService
    {
        /// <summary>
        /// 服务作用域工厂。
        /// 每轮 Tick 都会创建独立作用域来解析 Scoped 服务。
        /// </summary>
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// 世界 Boss 后台任务日志记录器。
        /// </summary>
        private readonly ILogger<WorldBossWorker> _logger;

        /// <summary>
        /// 初始化世界 Boss 后台推进任务。
        /// </summary>
        /// <param name="serviceScopeFactory">服务作用域工厂。</param>
        /// <param name="logger">日志记录器。</param>
        public WorldBossWorker(IServiceScopeFactory serviceScopeFactory, ILogger<WorldBossWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// 按固定节奏推进世界 Boss 运行时。
        /// 当前策略为服务启动后延迟 2 秒，然后每秒执行一次 Tick。
        /// </summary>
        /// <param name="stoppingToken">后台任务停止令牌。</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var worldBossService = scope.ServiceProvider.GetRequiredService<IWorldBossService>();
                    await worldBossService.ProcessTickAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "世界Boss后台任务 Tick 执行失败。");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
    }
}
