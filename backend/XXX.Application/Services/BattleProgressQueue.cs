using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using XXX.Application.Events;
using XXX.Application.Interfaces;

namespace XXX.Application.Services
{
    /// <summary>
    /// 基于有界 Channel 的战斗进度事件队列。
    /// </summary>
    public sealed class BattleProgressQueue : IBattleProgressQueue
    {
        private const int Capacity = 2048;
        private readonly Channel<BattleProgressEvent> _channel;
        private readonly ILogger<BattleProgressQueue> _logger;

        /// <summary>初始化战斗进度队列。</summary>
        public BattleProgressQueue(ILogger<BattleProgressQueue> logger)
        {
            _logger = logger;
            _channel = Channel.CreateBounded<BattleProgressEvent>(new BoundedChannelOptions(Capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
        }

        /// <summary>写入战斗进度事件；队列满时等待，不静默丢失。</summary>
        public async ValueTask EnqueueAsync(BattleProgressEvent progressEvent, CancellationToken cancellationToken = default)
        {
            if (progressEvent == null || string.IsNullOrWhiteSpace(progressEvent.PlayerId))
            {
                return;
            }

            try
            {
                await _channel.Writer.WriteAsync(progressEvent, cancellationToken);
            }
            catch (ChannelClosedException ex)
            {
                _logger.LogError(ex, "Battle progress queue is closed. EventId={EventId}, PlayerId={PlayerId}", progressEvent.EventId, progressEvent.PlayerId);
                throw;
            }
        }

        /// <summary>
        /// 异步读取战斗进度事件。
        /// </summary>
        public IAsyncEnumerable<BattleProgressEvent> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        /// <summary>
        /// 尝试立即取出一个战斗进度事件，不阻塞当前线程。
        /// </summary>
        public bool TryDequeue(out BattleProgressEvent? progressEvent)
        {
            return _channel.Reader.TryRead(out progressEvent);
        }
    }
}
