using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 炼丹服务接口
    /// 负责管理玩家炼丹系统的学习丹方、炼制丹药等业务逻辑
    /// </summary>
    /// <remarks>
    /// 炼丹系统是游戏的丹药产出系统，与其他系统的关联：
    /// 1. 与背包系统关联：炼丹消耗材料，产出丹药存入背包
    /// 2. 与灵田系统关联：灵田产出的草药是炼丹材料
    /// 3. 与商店系统关联：部分材料可以从商店购买
    /// 4. 与战斗系统关联：丹药可以在战斗中使用
    /// 
    /// 炼丹规则：
    /// - 需要学习丹方后才能炼制
    /// - 炼丹成功率受熟练度影响
    /// - 炼丹炉等级影响可学习的丹方
    /// </remarks>
    public interface IAlchemyService
    {
        /// <summary>
        /// 获取炼丹系统信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>炼丹系统信息，包含炼丹炉等级、熟练度、已学丹方等</returns>
        /// <remarks>
        /// 如果玩家首次访问炼丹系统，会自动初始化
        /// 初始炼丹炉等级为1，熟练度为0
        /// </remarks>
        Task<AlchemyDto> GetAlchemyInfoAsync(string playerId);

        /// <summary>
        /// 获取已学习的丹方列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>已学习的丹方列表</returns>
        /// <remarks>
        /// 只返回已学习的丹方
        /// 丹方包含所需材料和炼制时间
        /// </remarks>
        Task<List<AlchemyRecipeDto>> GetLearnedRecipesAsync(string playerId);

        /// <summary>
        /// 获取当前已解锁的丹方列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>已通过配方卷轴解锁的丹方列表</returns>
        /// <remarks>
        /// 与锻造图纸接口保持一致，只返回已经解锁的丹方
        /// </remarks>
        Task<List<AlchemyRecipeDto>> GetAvailableRecipesAsync(string playerId);

        /// <summary>
        /// 学习丹方
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="recipeId">丹方ID</param>
        /// <returns>是否学习成功</returns>
        /// <remarks>
        /// 学习条件：
        /// 1. 炼丹炉等级达到要求
        /// 2. 未学习过该丹方
        /// 
        /// 学习消耗：
        /// - 金币或贡献点
        /// </remarks>
        Task<bool> LearnRecipeAsync(string playerId, string recipeId);

        /// <summary>
        /// 开始炼丹
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">炼丹请求，包含丹方ID和炼制数量</param>
        /// <returns>炼丹结果，包含是否成功、获得丹药数量、熟练度增加等</returns>
        /// <remarks>
        /// 炼丹流程：
        /// 1. 检查是否已学习该丹方
        /// 2. 检查材料是否足够（从背包扣除）
        /// 3. 计算成功率（基础成功率 + 熟练度加成）
        /// 4. 判断是否成功
        /// 5. 如果成功，产出丹药存入背包
        /// 6. 增加熟练度
        /// 
        /// 成功率计算：
        /// 实际成功率 = 基础成功率 + 熟练度加成 + 炼丹炉加成
        /// </remarks>
        Task<AlchemyResultDto> StartAlchemyAsync(string playerId, StartAlchemyRequestDto request);

        /// <summary>
        /// 领取炼制的丹药
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>领取结果，如果不存在待领取的丹药则返回null</returns>
        /// <remarks>
        /// 用于异步炼丹完成后领取丹药
        /// 炼丹完成后需要手动领取
        /// </remarks>
        Task<AlchemyResultDto?> CollectPillAsync(string playerId);
    }
}
