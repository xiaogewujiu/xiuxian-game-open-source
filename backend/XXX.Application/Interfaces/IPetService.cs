using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 灵宠服务接口
    /// 负责管理玩家灵宠的召唤、培养、出战等业务逻辑
    /// </summary>
    /// <remarks>
    /// 灵宠系统与其他系统的关联：
    /// 1. 与战斗系统关联：出战灵宠作为独立单位参与战斗
    /// 2. 与背包系统关联：召唤、喂养、进化材料来自背包
    /// 3. 与模板系统关联：灵宠实例由数据库模板生成，不再是固定预制宠物
    /// 
    /// 灵宠品质分级：
    /// - 1级：普通
    /// - 2级：优秀
    /// - 3级：精良
    /// - 4级：史诗
    /// - 5级：传说
    /// 
    /// </remarks>
    public interface IPetService
    {
        /// <summary>
        /// 获取玩家的所有灵宠列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>灵宠列表，包含所有已拥有的灵宠</returns>
        /// <remarks>
        /// 返回的灵宠按品质降序排列，同品质按等级降序排列
        /// 包含出战和休息状态的灵宠
        /// </remarks>
        Task<List<PetDto>> GetPlayerPetsAsync(string playerId);

        /// <summary>
        /// 获取灵宠详细信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="petId">灵宠实例ID</param>
        /// <returns>灵宠详细信息，包含属性、技能、忠诚度等</returns>
        /// <remarks>
        /// 如果灵宠不存在或不属于该玩家，返回null
        /// </remarks>
        Task<PetDto?> GetPetDetailAsync(string playerId, string petId);

        /// <summary>
        /// 获取当前出战的灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>出战灵宠信息，如果没有出战灵宠则返回null</returns>
        /// <remarks>
        /// 一个玩家同时只能有一只出战灵宠
        /// 出战灵宠会作为独立单位参与战斗
        /// </remarks>
        Task<PetDto?> GetActivePetAsync(string playerId);

        /// <summary>
        /// 设置出战灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="petId">灵宠实例ID</param>
        /// <returns>是否设置成功</returns>
        /// <remarks>
        /// 设置流程：
        /// 1. 检查灵宠是否存在且属于该玩家
        /// 2. 将当前出战灵宠设为休息状态
        /// 3. 将指定灵宠设为出战状态
        /// 4. 更新玩家的PetId字段
        /// 5. 不再给玩家面板叠加宠物属性
        /// 
        /// 设置失败的情况：
        /// - 灵宠不存在
        /// - 灵宠不属于该玩家
        /// </remarks>
        Task<bool> SetActivePetAsync(string playerId, string petId);

        /// <summary>
        /// 召回当前出战灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>是否召回成功</returns>
        Task<bool> ClearActivePetAsync(string playerId);

        /// <summary>
        /// 喂养灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">喂养请求，包含灵宠ID、食物ID和数量</param>
        /// <returns>是否喂养成功</returns>
        /// <remarks>
        /// 喂养效果：
        /// - 每个食物增加10点忠诚度
        /// - 忠诚度上限为100
        /// - 消耗背包中的食物物品
        /// 
        /// 忠诚度当前只作为培养状态展示和后续扩展钩子，暂不直接改写人物属性。
        /// </remarks>
        Task<bool> FeedPetAsync(string playerId, PetFeedRequestDto request);

        /// <summary>
        /// 进化灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">进化请求，包含灵宠ID</param>
        /// <returns>是否进化成功</returns>
        /// <remarks>
        /// 进化规则：
        /// 1. 品质上限为5级（传说）
        /// 2. 进化需要消耗特定材料
        /// 3. 进化后品质+1
        /// 4. 进化后基础属性提升20%
        /// 5. 进化后等级保持不变
        /// 
        /// 进化失败的情况：
        /// - 灵宠已达到最高品质
        /// - 材料不足
        /// </remarks>
        Task<bool> EvolvePetAsync(string playerId, PetEvolveRequestDto request);

        /// <summary>
        /// 给当前出战灵宠发放战斗经验。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="expGained">本次战斗提供给灵宠的经验值</param>
        /// <returns>实际发放到灵宠的经验值</returns>
        /// <remarks>
        /// 中文注释：
        /// 灵宠升级已经独立于人物面板，不再给玩家叠属性。
        /// 这里单独收口“战斗后给出战灵宠加经验”的逻辑，避免战斗服务自己直接操作宠物实例表。
        /// </remarks>
        Task<int> GrantBattleExpAsync(string playerId, int expGained);

        /// <summary>
        /// 放生灵宠
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="petId">灵宠实例ID</param>
        /// <returns>是否放生成功</returns>
        /// <remarks>
        /// 放生规则：
        /// 1. 出战状态的灵宠不能放生
        /// 2. 放生后灵宠永久删除
        /// 3. 放生不返还任何资源
        /// 
        /// 放生失败的情况：
        /// - 灵宠处于出战状态
        /// - 灵宠不存在
        /// </remarks>
        Task<bool> ReleasePetAsync(string playerId, string petId);
    }
}
