using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 五行聚灵阵服务接口
    /// 负责管理玩家五行聚灵阵的升级、灵气收集等业务逻辑
    /// </summary>
    /// <remarks>
    /// 五行聚灵阵是游戏的灵气产出系统，与其他系统的关联：
    /// 1. 与玩家修炼系统关联：灵气用于提升修为
    /// 2. 与灵宠系统关联：灵气可用于培养灵宠
    /// 3. 与炼丹系统关联：灵气是高级丹药的炼制条件
    /// 
    /// 五行规则：
    /// - 金、木、水、火、土五种元素
    /// - 每种元素可独立升级
    /// - 元素等级影响灵气产出速率
    /// - 特定元素组合可激活额外加成
    /// </remarks>
    public interface IFiveElementService
    {
        /// <summary>
        /// 获取五行聚灵阵信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>五行聚灵阵详细信息，包含各元素等级、灵气总量、产出速率等</returns>
        /// <remarks>
        /// 如果玩家首次访问五行聚灵阵，会自动初始化
        /// 初始各元素等级为1，灵气产出速率为10/小时
        /// </remarks>
        Task<FiveElementDto> GetFiveElementInfoAsync(string playerId);

        /// <summary>
        /// 升级聚灵阵主等级。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>是否升级成功</returns>
        Task<bool> UpgradeArrayAsync(string playerId);

        /// <summary>
        /// 升级五行元素
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">升级请求，包含要升级的元素类型</param>
        /// <returns>是否升级成功</returns>
        /// <remarks>
        /// 升级条件：
        /// 1. 拥有足够的元素经验
        /// 2. 元素等级未达到上限
        /// 
        /// 升级效果：
        /// - 元素等级+1
        /// - 灵气产出速率+2/小时
        /// 
        /// 元素经验获取：
        /// - 收集灵气时获得
        /// - 完成特定任务获得
        /// </remarks>
        Task<bool> UpgradeElementAsync(string playerId, UpgradeElementRequestDto request);

        /// <summary>
        /// 收集灵气
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>收集结果，包含收集的灵气数量、当前灵气总量等</returns>
        /// <remarks>
        /// 收集规则：
        /// 1. 根据离线时间计算可收集的灵气数量
        /// 2. 收集数量 = 产出速率 * 离线小时数
        /// 3. 有每日收集次数限制
        /// 4. 收集后增加对应元素的经验
        /// 
        /// 灵气用途：
        /// - 提升修为境界
        /// - 培养灵宠
        /// - 炼制高级丹药
        /// </remarks>
        Task<CollectSpiritPowerResultDto> CollectSpiritPowerAsync(string playerId);

        /// <summary>
        /// 计算灵气产出速率
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>当前灵气产出速率（每小时）</returns>
        /// <remarks>
        /// 产出速率计算：
        /// 基础速率 + 金元素加成 + 木元素加成 + 水元素加成 + 火元素加成 + 土元素加成
        /// 
        /// 每种元素每级提供2/小时的加成
        /// </remarks>
        Task<int> CalculateSpiritPowerProductionAsync(string playerId);
    }
}
