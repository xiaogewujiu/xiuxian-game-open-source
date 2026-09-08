using XXX.Entity;
using XXX.Inventory;
using XXX.Quest;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 任务服务接口
    /// 提供任务配置管理、接取、进度追踪、提交等任务系统功能
    /// </summary>
    /// <remarks>
    /// 任务系统是游戏的核心玩法引导系统，提供：
    /// - 多类型任务（主线、支线、日常、周常、活动）
    /// - 前置任务依赖
    /// - 多目标追踪
    /// - 丰富的奖励
    /// </remarks>
    public interface IQuestService
    {
        /// <summary>
        /// 初始化任务系统
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 从数据库加载任务配置
        /// 2. 如果没有配置，加载默认任务
        /// 3. 构建任务依赖关系
        /// </remarks>
        Task InitializeAsync();

        /// <summary>
        /// 重新从数据库装载任务配置缓存。
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 获取所有任务配置
        /// </summary>
        /// <returns>任务配置列表，按排序顺序排列</returns>
        Task<List<QuestConfig>> GetAllQuestConfigsAsync();

        /// <summary>
        /// 获取指定任务配置
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <returns>任务配置，如果不存在则返回null</returns>
        Task<QuestConfig?> GetQuestConfigAsync(string questId);

        /// <summary>
        /// 获取玩家可接取的任务列表
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>可接取的任务配置列表</returns>
        /// <remarks>
        /// 过滤条件：
        /// - 玩家等级满足要求
        /// - 前置任务已完成
        /// - 玩家当前未接取该任务
        /// - 玩家未完成该任务（非重复任务）
        /// </remarks>
        Task<List<QuestConfig>> GetAvailableQuestsAsync(UserEntity player);

        /// <summary>
        /// 获取玩家进行中的任务列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>进行中的任务进度列表</returns>
        Task<List<QuestProgress>> GetPlayerInProgressQuestsAsync(string playerId);

        /// <summary>
        /// 获取玩家已完成待提交的任务列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>已完成的任务进度列表</returns>
        Task<List<QuestProgress>> GetPlayerCompletedQuestsAsync(string playerId);

        /// <summary>
        /// 获取玩家指定任务的进度
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="questId">任务ID</param>
        /// <returns>任务进度，如果不存在则返回null</returns>
        Task<QuestProgress?> GetQuestProgressAsync(string playerId, string questId);

        /// <summary>
        /// 接取任务
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="questId">任务ID</param>
        /// <returns>接取结果</returns>
        /// <remarks>
        /// 接取流程：
        /// 1. 验证任务存在
        /// 2. 验证玩家满足接取条件
        /// 3. 验证玩家未接取该任务
        /// 4. 创建任务进度记录
        /// </remarks>
        Task<QuestOperationResult> AcceptQuestAsync(UserEntity player, string questId);

        /// <summary>
        /// 放弃任务
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="questId">任务ID</param>
        /// <returns>放弃结果</returns>
        /// <remarks>
        /// 主线任务不能放弃
        /// 只能放弃进行中的任务
        /// </remarks>
        Task<QuestOperationResult> AbandonQuestAsync(string playerId, string questId);

        /// <summary>
        /// 提交任务并领取奖励
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="questId">任务ID</param>
        /// <param name="inventory">背包管理器（可选），用于发放物品奖励</param>
        /// <returns>提交结果，包含奖励详情</returns>
        /// <remarks>
        /// 提交流程：
        /// 1. 验证任务已完成
        /// 2. 发放奖励
        /// 3. 删除进度记录
        /// 4. 记录完成历史
        /// </remarks>
        Task<QuestSubmitResult> SubmitQuestAsync(UserEntity player, string questId, InventoryManager? inventory = null);

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="questId">任务ID</param>
        /// <returns>是否完成成功</returns>
        /// <remarks>
        /// 当所有目标完成时调用此方法
        /// 更新任务状态为已完成
        /// </remarks>
        Task<bool> CompleteQuestAsync(string playerId, string questId);

        /// <summary>
        /// 更新任务目标进度
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="questId">任务ID</param>
        /// <param name="objectiveIndex">目标索引</param>
        /// <param name="delta">进度变化量</param>
        /// <returns>是否更新成功</returns>
        /// <remarks>
        /// 更新流程：
        /// 1. 获取进行中的任务进度
        /// 2. 更新指定目标的进度
        /// 3. 检查是否所有目标完成
        /// 4. 如果全部完成，自动完成任务
        /// </remarks>
        Task<bool> UpdateObjectiveProgressAsync(string playerId, string questId, int objectiveIndex, int delta);

        /// <summary>
        /// 记录一次任务事件。
        /// </summary>
        /// <param name="playerId">玩家ID。</param>
        /// <param name="objectiveEvent">事件内容。</param>
        Task RecordObjectiveEventAsync(string playerId, QuestObjectiveEvent objectiveEvent);

        /// <summary>
        /// 同步指定类型的快照型任务目标。
        /// </summary>
        /// <param name="playerId">玩家ID。</param>
        /// <param name="objectiveType">目标类型。</param>
        Task SyncObjectiveStateAsync(string playerId, ObjectiveType objectiveType);

        /// <summary>
        /// 检查并执行日常任务重置
        /// </summary>
        /// <remarks>
        /// 每日0点执行：
        /// - 清除所有日常任务的进度
        /// - 清除日常任务的完成记录
        /// </remarks>
        Task CheckDailyResetAsync();

        /// <summary>
        /// 检查并执行周常任务重置
        /// </summary>
        /// <remarks>
        /// 每周一0点执行：
        /// - 清除所有周常任务的进度
        /// - 清除周常任务的完成记录
        /// </remarks>
        Task CheckWeeklyResetAsync();
    }
}
