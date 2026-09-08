namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 核心游戏态同步服务。
    /// </summary>
    public interface IGameSyncService
    {
        /// <summary>
        /// 兼容旧调用：同步玩家当前等级与财富排行榜。
        /// </summary>
        Task SyncPlayerAsync(string playerId);


    }
}
