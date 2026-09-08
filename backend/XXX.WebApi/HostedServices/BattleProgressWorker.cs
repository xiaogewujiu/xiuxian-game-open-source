using XXX.Application.Events;
using XXX.Application.Interfaces;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 战斗任务与成就进度后台处理任务。
    /// </summary>
    public sealed class BattleProgressWorker : BackgroundService
    {
        private const int BatchSize = 32;
        private const int MaxRetryCount = 3;
        private readonly IBattleProgressQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BattleProgressWorker> _logger;

        /// <summary>
        /// 初始化战斗进度后台任务。
        /// </summary>
        public BattleProgressWorker(
            IBattleProgressQueue queue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BattleProgressWorker> logger)
        {
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// 持续批量消费战斗进度事件。
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var firstEvent in _queue.ReadAllAsync(stoppingToken))
                {
                    var events = new List<BattleProgressEvent>(BatchSize)
                    {
                        firstEvent
                    };
                    var deadline = DateTime.UtcNow.AddMilliseconds(100);

                    while (events.Count < BatchSize && DateTime.UtcNow < deadline)
                    {
                        while (events.Count < BatchSize && _queue.TryDequeue(out var nextEvent))
                        {
                            if (nextEvent != null)
                            {
                                events.Add(nextEvent);
                            }
                        }

                        if (events.Count >= BatchSize || DateTime.UtcNow >= deadline)
                        {
                            break;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(10), stoppingToken);
                    }

                    await ProcessBatchAsync(events, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // 中文注释：正常停机，不把取消记录为失败。
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Battle progress worker stopped unexpectedly.");
            }
        }

        /// <summary>
        /// 按事件顺序处理一批战斗进度事件。
        /// </summary>
        private async Task ProcessBatchAsync(
            IReadOnlyCollection<BattleProgressEvent> events,
            CancellationToken stoppingToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var battleService = scope.ServiceProvider.GetRequiredService<IBattleService>();

            foreach (var progressEvent in events)
            {
                var succeeded = false;
                for (var retry = 0; retry < MaxRetryCount && !succeeded; retry++)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    try
                    {
                        await battleService.ProcessBattleProgressEventAsync(progressEvent);
                        succeeded = true;
                        _logger.LogDebug(
                            "Battle progress event processed. EventId={EventId}, PlayerId={PlayerId}",
                            progressEvent.EventId,
                            progressEvent.PlayerId);
                    }
                    catch (Exception ex) when (retry < MaxRetryCount - 1)
                    {
                        _logger.LogWarning(
                            ex,
                            "Battle progress event failed, retrying. EventId={EventId}, PlayerId={PlayerId}, Retry={Retry}",
                            progressEvent.EventId,
                            progressEvent.PlayerId,
                            retry + 1);
                        await Task.Delay(TimeSpan.FromMilliseconds(200 * (retry + 1)), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Battle progress event permanently failed after retries. EventId={EventId}, PlayerId={PlayerId}",
                            progressEvent.EventId,
                            progressEvent.PlayerId);
                    }
                }
            }
        }
    }
}
