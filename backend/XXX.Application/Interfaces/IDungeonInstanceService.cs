using XXX.Application.DTOs.DungeonInstance;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 秘境实例玩家服务接口。
    /// </summary>
    public interface IDungeonInstanceService
    {
        /// <summary>
        /// 进入秘境。
        /// </summary>
        Task<DungeonInstanceEnterResponseDto> EnterAsync(string playerId, string dungeonId);

        /// <summary>
        /// 查询当前秘境状态。
        /// </summary>
        Task<DungeonInstanceStatusDto> GetStatusAsync(string playerId);

        /// <summary>
        /// 主动退出秘境。
        /// </summary>
        Task<DungeonInstanceSettlementDto> QuitAsync(string playerId);

        /// <summary>
        /// 处理一次 tick（由后台 Worker 调用）。
        /// </summary>
        Task ProcessTickAsync(string instanceId);

        /// <summary>
        /// 结算秘境。
        /// </summary>
        Task SettleAsync(string instanceId, int settleReason);

        /// <summary>
        /// 获取可用秘境列表。
        /// </summary>
        Task<List<DungeonInstanceAvailableDto>> GetAvailableDungeonsAsync(string playerId);

        /// <summary>
        /// 获取玩家的秘境探索历史记录。
        /// </summary>
        Task<List<DungeonInstanceHistoryDto>> GetHistoryAsync(string playerId, int limit = 20);

        /// <summary>
        /// 强制触发指定事件（测试用，复用真实 tick 流程）。
        /// </summary>
        Task<string> ForceEventAsync(string playerId, string eventId);

        /// <summary>
        /// 获取调试状态快照（测试用，包含buff详情）。
        /// </summary>
        Task<string> GetDebugSnapshotAsync(string playerId);

        /// <summary>
        /// 获取战斗属性调试信息（测试用，显示buff加成后的实际战斗属性）。
        /// </summary>
        Task<string> GetDebugFighterStatsAsync(string playerId);

        /// <summary>
        /// 强制组队（测试用）。
        /// </summary>
        Task<string> ForcePartyAsync(string leaderPlayerId, string targetPlayerId);
    }
}
