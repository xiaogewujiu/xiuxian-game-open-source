using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 灵田服务接口
    /// 负责管理玩家灵田的种植、收获、升级等业务逻辑
    /// </summary>
    /// <remarks>
    /// 灵田系统是游戏的资源产出系统，与其他系统的关联：
    /// 1. 与背包系统关联：种植消耗种子，收获获得物品
    /// 2. 与炼丹系统关联：灵田产出的草药是炼丹材料
    /// 3. 与商店系统关联：种子可以从商店购买
    /// 
    /// 灵田规则：
    /// - 初始解锁3块地块
    /// - 最多可扩展到9块地块
    /// - 每块地块可以独立升级
    /// - 地块等级影响产量和生长速度
    /// </remarks>
    public interface ISpiritFieldService
    {
        /// <summary>
        /// 获取灵田信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>灵田详细信息，包含所有地块状态和全局加成</returns>
        /// <remarks>
        /// 如果玩家首次访问灵田，会自动初始化灵田系统
        /// 初始化时创建3块1级地块
        /// </remarks>
        Task<SpiritFieldDto> GetSpiritFieldAsync(string playerId);

        /// <summary>
        /// 获取可种植的作物列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>可种植的作物模板列表</returns>
        /// <remarks>
        /// 根据灵田等级筛选可种植的作物
        /// 每种作物有解锁等级要求
        /// </remarks>
        Task<List<CropTemplateDto>> GetAvailableCropsAsync(string playerId);

        /// <summary>
        /// 种植作物
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">种植请求，包含地块编号和作物模板ID</param>
        /// <returns>是否种植成功</returns>
        /// <remarks>
        /// 种植流程：
        /// 1. 检查地块是否为空
        /// 2. 检查种子是否足够（从背包扣除）
        /// 3. 设置地块状态为生长中
        /// 4. 记录种植时间和预计收获时间
        /// 
        /// 种植失败的情况：
        /// - 地块不为空
        /// - 种子不足
        /// - 作物模板不存在
        /// </remarks>
        Task<bool> PlantCropAsync(string playerId, PlantCropRequestDto request);

        /// <summary>
        /// 收获作物
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">收获请求，包含地块编号</param>
        /// <returns>收获结果，包含获得的作物名称、数量、品质</returns>
        /// <remarks>
        /// 收获流程：
        /// 1. 检查地块是否可收获
        /// 2. 计算产量（基础产量 + 地块加成 + 全局加成）
        /// 3. 随机生成品质
        /// 4. 将产物添加到背包
        /// 5. 清空地块状态
        /// 
        /// 产量计算公式：
        /// 实际产量 = 基础产量 * (1 + 地块加成% + 全局加成%)
        /// </remarks>
        Task<HarvestResultDto?> HarvestCropAsync(string playerId, HarvestCropRequestDto request);

        /// <summary>
        /// 加速作物生长
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">加速请求，包含地块编号和使用的道具ID</param>
        /// <returns>是否加速成功</returns>
        /// <remarks>
        /// 加速效果：
        /// - 每个加速道具按数据库规则减少对应的生长时间
        /// 加速道具从背包扣除
        /// </remarks>
        Task<bool> SpeedUpCropAsync(string playerId, SpeedUpCropRequestDto request);

        /// <summary>
        /// 升级地块
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="plotNumber">地块编号（1-9）</param>
        /// <returns>是否升级成功</returns>
        /// <remarks>
        /// 升级效果：
        /// - 产量加成 +5%
        /// - 生长速度 +5%
        /// 
        /// 升级消耗：
        /// - 金币和灵石
        /// - 消耗随等级提升而增加
        /// </remarks>
        Task<bool> UpgradePlotAsync(string playerId, int plotNumber);
    }

    /// <summary>
    /// 收获结果DTO
    /// </summary>
    public class HarvestResultDto
    {
        /// <summary>
        /// 作物名称
        /// </summary>
        public string CropName { get; set; } = string.Empty;

        /// <summary>
        /// 收获数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 作物品质（1-5）
        /// </summary>
        public int Quality { get; set; }
    }
}
