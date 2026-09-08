using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家属性服务接口
    /// 负责管理玩家属性的计算、更新和跨模块协调
    /// </summary>
    /// <remarks>
    /// 玩家属性服务是跨模块协调的核心服务，与其他系统的关联：
    /// 1. 与装备系统关联：穿戴装备影响玩家属性
    /// 2. 与灵宠系统关联：出战灵宠影响玩家属性
    /// 3. 与丹药系统关联：丹药buff影响玩家属性
    /// 4. 与战斗系统关联：战斗中使用属性计算
    ///
    /// 属性计算规则：
    /// 最终属性 = 基础属性 + 装备加成 + 灵宠加成 + 丹药加成 + 其他加成
    /// </remarks>
    public interface IPlayerAttributeService
    {
        /// <summary>
        /// 重新计算玩家总属性
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="syncLevelDrivenProgress">是否同步等级驱动的任务/成就进度</param>
        /// <returns>异步任务</returns>
        /// <remarks>
        /// 计算流程：
        /// 1. 获取玩家基础属性
        /// 2. 计算所有已装备装备的属性加成
        /// 3. 计算出战灵宠的属性加成（10%）
        /// 4. 计算丹药buff的加成
        /// 5. 汇总所有加成，更新玩家属性
        /// 
        /// 触发时机：
        /// - 穿戴/卸下装备时
        /// - 设置出战灵宠时
        /// - 使用属性丹药时
        /// - 升级时
        /// </remarks>
        Task RecalculatePlayerAttributesAsync(string playerId, bool syncLevelDrivenProgress = false);

        /// <summary>
        /// 仅重新计算玩家战斗属性并保存，不触发排行榜与等级进度同步。
        /// </summary>
        /// <remarks>
        /// 中文注释：
        /// 用于战斗结算等高频路径：未升级时玩家属性并未变化，
        /// 只需把"属性计算 + 玩家字段保存"做完，排行榜与等级任务/成就
        /// 由调用方决定是否投递后台事件，避免每场战斗都做完整重算。
        /// </remarks>
        Task RecalculateCombatAttributesAsync(string playerId);


        /// <summary>
        /// 恢复玩家生命值
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="amount">恢复数量</param>
        /// <returns>是否恢复成功</returns>
        /// <remarks>
        /// 恢复后HP不会超过最大HP
        /// 用于药水、技能、休息等恢复场景
        /// </remarks>
        Task<bool> RestoreHPAsync(string playerId, int amount);

        /// <summary>
        /// 恢复玩家法力值
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="amount">恢复数量</param>
        /// <returns>是否恢复成功</returns>
        /// <remarks>
        /// 恢复后MP不会超过最大MP
        /// 用于药水、技能、休息等恢复场景
        /// </remarks>
        Task<bool> RestoreMPAsync(string playerId, int amount);

        /// <summary>
        /// 增加玩家经验值
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="amount">经验值数量</param>
        /// <returns>是否增加成功</returns>
        /// <remarks>
        /// 经验处理逻辑：
        /// 1. 累加经验值到当前经验
        /// 2. 检查是否满足升级条件
        /// 3. 如果满足，自动升级并提升属性
        /// 4. 更新累计获得经验统计
        /// 
        /// 升级属性提升：
        /// - MaxHP +10
        /// - Attack +2
        /// - Defense +1
        /// </remarks>
        Task<bool> AddExpAsync(string playerId, long amount, bool recalculateAttributes = true);

        /// <summary>
        /// 扣除玩家金币
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="amount">扣除数量</param>
        /// <param name="reason">扣除原因，用于日志记录</param>
        /// <returns>是否扣除成功</returns>
        /// <remarks>
        /// 扣除失败的情况：
        /// - 玩家不存在
        /// - 金币不足
        /// 
        /// 扣除成功后会更新累计消耗金币统计
        /// </remarks>
        Task<bool> DeductGoldAsync(string playerId, long amount, string reason);

        /// <summary>
        /// 增加玩家金币
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="amount">增加数量</param>
        /// <param name="reason">增加原因，用于日志记录</param>
        /// <returns>是否增加成功</returns>
        /// <remarks>
        /// 增加成功后会更新累计获得金币统计
        /// 用于战斗奖励、任务奖励、出售物品等场景
        /// </remarks>
        Task<bool> AddGoldAsync(string playerId, long amount, string reason, bool syncRankings = true);

        /// <summary>
        /// 检查玩家资源是否足够
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="resourceType">资源类型：Gold/SpiritStone</param>
        /// <param name="amount">需要数量</param>
        /// <returns>是否足够</returns>
        /// <remarks>
        /// 支持检查的资源类型：
        /// - Gold：金币
        /// - SpiritStone：灵石
        /// </remarks>
        Task<bool> HasEnoughResourceAsync(string playerId, string resourceType, long amount);

        /// <summary>
        /// 获取玩家心法属性加成汇总（读取时动态计算）。
        /// </summary>
        Task<List<HeartSutraBonusSummaryDto>> GetHeartSutraBonusesAsync(string playerId);

        /// <summary>
        /// 将心法加成叠加到 PlayerDto（前端展示用）。
        /// </summary>
        Task ApplyHeartSutraBonusesToDtoAsync(string playerId, PlayerDto dto);

        /// <summary>
        /// 将心法加成叠加到 UserEntity（战斗计算用）。
        /// </summary>
        Task ApplyHeartSutraBonusesToEntityAsync(string playerId, UserEntity player);

        /// <summary>
        /// 同时叠加心法和图鉴加成到战斗实体，百分比从同一基值加算。
        /// </summary>
        Task ApplyAllBonusesToEntityAsync(string playerId, UserEntity player);

        /// <summary>
        /// 同时叠加心法和图鉴加成到 DTO，百分比从同一基值加算。
        /// </summary>
        Task ApplyAllBonusesToDtoAsync(string playerId, PlayerDto dto);
    }
}
