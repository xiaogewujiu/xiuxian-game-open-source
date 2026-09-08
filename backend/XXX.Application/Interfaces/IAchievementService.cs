using XXX.Achievement;
using XXX.Entity;
using XXX.Inventory;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 成就服务接口
    /// 提供成就配置管理、进度追踪、奖励领取等成就系统功能
    /// </summary>
    /// <remarks>
    /// 成就系统是游戏的长期目标系统，提供：
    /// - 多类型成就（等级、战斗、装备、收集等）
    /// - 前置成就依赖
    /// - 成就点数统计
    /// - 丰富的奖励
    /// </remarks>
    public interface IAchievementService
    {
        /// <summary>
        /// 初始化成就系统
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 从数据库加载成就配置
        /// 2. 如果没有配置，加载默认成就
        /// 3. 构建成就依赖关系
        /// </remarks>
        Task InitializeAsync();

        /// <summary>
        /// 重新从数据库装载成就配置缓存。
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 获取所有成就配置
        /// </summary>
        /// <returns>成就配置列表，按排序顺序排列</returns>
        Task<List<AchievementConfig>> GetAllAchievementsAsync();

        /// <summary>
        /// 获取指定成就配置
        /// </summary>
        /// <param name="achievementId">成就ID</param>
        /// <returns>成就配置，如果不存在则返回null</returns>
        Task<AchievementConfig?> GetAchievementConfigAsync(string achievementId);

        /// <summary>
        /// 获取玩家所有成就进度
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>成就进度列表</returns>
        Task<List<AchievementProgress>> GetPlayerAchievementsAsync(string playerId);

        /// <summary>
        /// 获取玩家已完成但未领取的成就
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>已完成的成就进度列表</returns>
        Task<List<AchievementProgress>> GetPlayerCompletedAchievementsAsync(string playerId);

        /// <summary>
        /// 获取玩家指定成就的进度
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="achievementId">成就ID</param>
        /// <returns>成就进度，如果不存在则返回null</returns>
        Task<AchievementProgress?> GetAchievementProgressAsync(string playerId, string achievementId);

        /// <summary>
        /// 领取成就奖励
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="achievementId">成就ID</param>
        /// <param name="inventory">背包管理器（可选），用于发放物品奖励</param>
        /// <returns>领取结果，包含奖励详情</returns>
        /// <remarks>
        /// 领取流程：
        /// 1. 验证成就存在
        /// 2. 验证成就已完成
        /// 3. 发放奖励（金币、钻石、经验、称号、装备等）
        /// 4. 更新成就状态为已领取
        /// 5. 累加成就点数
        /// </remarks>
        Task<AchievementClaimResult> ClaimRewardAsync(UserEntity player, string achievementId, InventoryManager? inventory = null);

        /// <summary>
        /// 完成成就
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="achievementId">成就ID</param>
        /// <returns>是否完成成功</returns>
        /// <remarks>
        /// 当成就进度达到目标时调用此方法
        /// 更新成就状态为已完成
        /// </remarks>
        Task<bool> CompleteAchievementAsync(string playerId, string achievementId);

        /// <summary>
        /// 更新成就进度
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="achievementId">成就ID</param>
        /// <param name="progress">新的进度值</param>
        /// <remarks>
        /// 更新流程：
        /// 1. 获取或创建进度记录
        /// 2. 更新当前进度
        /// 3. 如果进度达到目标，自动完成成就
        /// </remarks>
        Task UpdateProgressAsync(string playerId, string achievementId, long progress);

        /// <summary>
        /// 记录一次成就统计事件。
        /// </summary>
        Task RecordRequirementEventAsync(string playerId, AchievementRequirementEvent requirementEvent);

        /// <summary>
        /// 同步快照型成就要求。
        /// </summary>
        Task SyncRequirementStateAsync(string playerId, AchievementRequirementType requirementType);

        /// <summary>
        /// 获取玩家成就统计
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>成就统计信息</returns>
        /// <remarks>
        /// 统计信息包含：
        /// - 总成就数量
        /// - 已完成数量
        /// - 进行中数量
        /// - 完成百分比
        /// - 总成就点数
        /// </remarks>
        Task<AchievementStats> GetAchievementStatsAsync(string playerId);

        /// <summary>
        /// 获取玩家成就点数
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>总成就点数</returns>
        Task<int> GetPlayerAchievementPointsAsync(string playerId);
    }
}
