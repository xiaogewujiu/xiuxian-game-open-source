using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 世界 Boss 玩家服务。
    /// </summary>
    public interface IWorldBossService
    {
        /// <summary>
        /// 推进世界 Boss 的全局运行时时钟。
        /// 由后台任务按固定频率调用，用来处理刷新、出手、复活和结算。
        /// </summary>
        /// <param name="cancellationToken">后台任务取消令牌。</param>
        Task ProcessTickAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取当前世界 Boss 面板所需的完整状态。
        /// </summary>
        /// <param name="playerId">当前查看面板的玩家编号。</param>
        /// <returns>世界 Boss 当前实例、Boss 状态和玩家自身状态。</returns>
        Task<WorldBossCurrentDto> GetCurrentAsync(string playerId);

        /// <summary>
        /// 让玩家进入当前世界 Boss 战场。
        /// 首次进入时会创建参战记录并快照玩家实时属性。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>加入后的当前世界 Boss 状态。</returns>
        Task<WorldBossCurrentDto> JoinAsync(string playerId);

        /// <summary>
        /// 获取世界 Boss 伤害排行榜。
        /// </summary>
        /// <param name="playerId">当前查看排行的玩家编号，用于标记“自己”。</param>
        /// <param name="top">返回的排行数量上限。</param>
        /// <returns>按伤害倒序排列的排行列表。</returns>
        Task<List<WorldBossRankingEntryDto>> GetRankingAsync(string playerId, int top = 10);

        /// <summary>
        /// 获取当前世界 Boss 的战斗日志。
        /// </summary>
        /// <param name="playerId">当前查看日志的玩家编号。</param>
        /// <param name="count">返回的日志条数上限。</param>
        /// <returns>按最新记录在前排序的日志列表。</returns>
        Task<List<WorldBossLogDto>> GetLogsAsync(string playerId, int count = 100);

        /// <summary>
        /// 执行玩家在世界 Boss 战中的一次主动操作。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">本次出手请求，可能是技能或普通攻击。</param>
        /// <returns>本次出手的执行结果与最新战斗片段。</returns>
        Task<WorldBossActionResultDto> ExecuteActionAsync(string playerId, WorldBossActionRequestDto request);

        /// <summary>
        /// 切换玩家在世界 Boss 战中的自动战斗状态。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="enabled">是否开启自动战斗。</param>
        /// <returns>切换后的玩家战斗状态。</returns>
        Task<WorldBossParticipantStatusDto> ToggleAutoAsync(string playerId, bool enabled);

        /// <summary>
        /// 立即生成一个世界 Boss 实例。
        /// 后台手动测试时会走这个入口。
        /// </summary>
        /// <param name="bossId">指定的 Boss 模板编号；为空时按排期策略随机或轮换选择。</param>
        /// <returns>生成后的当前世界 Boss 状态。</returns>
        Task<WorldBossCurrentDto> SpawnNowAsync(string? bossId = null);

        /// <summary>
        /// 关闭当前正在运行的世界 Boss 实例并触发结算。
        /// </summary>
        /// <param name="reason">关闭原因，会写入结算日志并影响最终实例状态。</param>
        /// <returns>存在可关闭实例时返回真。</returns>
        Task<bool> CloseCurrentAsync(string? reason = null);

        /// <summary>
        /// 领取玩家尚未领取的世界 Boss 奖励。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>领取结果；没有待领奖励时返回空。</returns>
        Task<WorldBossRewardClaimDto?> ClaimRewardAsync(string playerId);
    }
}
