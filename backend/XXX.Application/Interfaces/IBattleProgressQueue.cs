using XXX.Application.Events;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 战斗进度汇总事件队列。
    /// </summary>
    public interface IBattleProgressQueue
    {
        /// <summary>
        /// 将战斗进度事件写入有界队列。
        /// </summary>
        ValueTask EnqueueAsync(BattleProgressEvent progressEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步读取战斗进度事件。
        /// </summary>
        IAsyncEnumerable<BattleProgressEvent> ReadAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 尝试立即取出一个战斗进度事件，不阻塞当前线程。
        /// </summary>
        bool TryDequeue(out BattleProgressEvent? progressEvent);
    }
}
