using XXX.Application.Interfaces;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 邮件过期自动清理后台服务。
    /// 每6小时清理一次已过期的邮件。
    /// </summary>
    public sealed class MailCleanupWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MailCleanupWorker> _logger;

        public MailCleanupWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<MailCleanupWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

                    var count = await mailService.CleanupExpiredMailsAsync();
                    if (count > 0)
                    {
                        _logger.LogInformation("邮件过期清理完成，清理了 {Count} 封邮件。", count);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "邮件过期清理 Tick 执行失败。");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
    }
}
