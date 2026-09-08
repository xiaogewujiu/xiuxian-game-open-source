using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 战斗服务接口
    /// 负责管理玩家战斗相关的业务逻辑
    /// </summary>
    /// <remarks>
    /// 战斗系统是游戏的核心玩法系统，与其他系统的关联：
    /// 1. 与玩家系统关联：使用玩家属性进行战斗计算
    /// 2. 与装备系统关联：装备属性影响战斗结果
    /// 3. 与灵宠系统关联：出战灵宠参与战斗
    /// 4. 与背包系统关联：战斗奖励存入背包
    /// 
    /// 战斗类型：
    /// - PVE：玩家vs怪物
    /// - PVP：玩家vs玩家
    /// - 副本：多人协作挑战
    /// </remarks>
    public interface IBattleService
    {
        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">战斗请求，包含目标ID和战斗类型</param>
        /// <returns>战斗结果</returns>
        /// <remarks>
        /// 战斗流程：
        /// 1. 获取玩家属性（基础属性 + 装备加成 + 灵宠加成）
        /// 2. 获取目标属性
        /// 3. 进行回合制战斗计算
        /// 4. 判断胜负
        /// 5. 发放奖励（如果胜利）
        /// 
        /// 战斗奖励：
        /// - 经验值
        /// - 金币
        /// - 物品掉落
        /// </remarks>
        Task<BattleResultDto> StartBattleAsync(string playerId, BattleRequestDto request);

        /// <summary>
        /// 获取普通地图列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>普通地图列表</returns>
        /// <remarks>
        /// 普通地图与副本不同：
        /// 1. 不消耗每日次数
        /// 2. 主要按玩家等级决定是否开放
        /// 3. 用于主界面的常规刷怪战斗
        /// </remarks>
        Task<List<BattleMapDto>> GetAvailableMapsAsync(string playerId);

        /// <summary>
        /// 获取可挑战的副本列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>副本列表</returns>
        /// <remarks>
        /// 根据玩家等级筛选可挑战的副本
        /// 包含副本的推荐等级、挑战限制等信息
        /// </remarks>
        Task<List<DungeonDto>> GetAvailableDungeonsAsync(string playerId);

        /// <summary>
        /// 挑战副本
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="dungeonId">副本ID</param>
        /// <returns>战斗结果</returns>
        /// <remarks>
        /// 副本挑战规则：
        /// 1. 每日奖励次数只影响奖励资格，不限制进入副本
        /// 2. 副本难度较高，奖励更丰厚
        /// 3. 可以组队挑战
        /// 4. 首次通关有额外奖励
        /// </remarks>
        Task<BattleResultDto> ChallengeDungeonAsync(string playerId, string dungeonId);

        /// <summary>
        /// 使用临时队伍挑战副本。
        /// </summary>
        /// <param name="playerId">当前发起挑战的玩家ID</param>
        /// <param name="partyId">临时队伍ID</param>
        /// <param name="dungeonId">副本ID</param>
        /// <param name="requestId">客户端请求幂等编号；为空时生成一次性编号。</param>
        /// <returns>组队副本挑战结果</returns>
        Task<BattleResultDto> ChallengeDungeonWithPartyAsync(string playerId, string partyId, string dungeonId, string? requestId = null);

        /// <summary>
        /// 按 BattleId 查询已持久化的组队副本摘要。
        /// </summary>
        /// <param name="battleId">战斗编号。</param>
        /// <returns>战斗摘要；不存在时返回 null。</returns>
        Task<PartyBattleRecordDto?> GetPartyBattleRecordAsync(string battleId);

        /// <summary>
        /// 开始单人普通地图离线挂机。
        /// </summary>
        Task<OfflineBattleStatusDto> StartOfflineBattleAsync(string playerId, StartOfflineBattleRequestDto request);

        /// <summary>
        /// 停止当前离线挂机并返回汇总。
        /// </summary>
        Task<OfflineBattleSummaryDto> StopOfflineBattleAsync(string playerId);

        /// <summary>
        /// 获取当前离线挂机状态。
        /// </summary>
        Task<OfflineBattleStatusDto> GetOfflineBattleStatusAsync(string playerId);

        /// <summary>
        /// 推进所有离线挂机账号的战斗逻辑。
        /// </summary>
        Task ProcessOfflineBattleTicksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 处理一条战斗任务/成就进度汇总事件。
        /// </summary>
        Task ProcessBattleProgressEventAsync(XXX.Application.Events.BattleProgressEvent progressEvent);
    }
}
