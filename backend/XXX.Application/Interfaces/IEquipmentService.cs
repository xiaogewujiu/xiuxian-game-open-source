using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 装备服务接口
    /// 负责管理玩家装备的获取、穿戴、卸下、强化等业务逻辑
    /// </summary>
    /// <remarks>
    /// 装备系统是玩家战力提升的核心系统，与其他系统的关联：
    /// 1. 与玩家系统关联：装备属于特定玩家，穿戴后影响玩家属性
    /// 2. 与背包系统关联：装备存储在背包中，穿戴后标记为已装备
    /// 3. 与商店系统关联：装备可以出售获得金币
    /// 4. 与战斗系统关联：装备属性直接影响战斗结果
    /// 
    /// 装备品质分级：
    /// - 1级：普通（白色）
    /// - 2级：优秀（绿色）
    /// - 3级：精良（蓝色）
    /// - 4级：史诗（紫色）
    /// - 5级：传说（橙色）
    /// </remarks>
    public interface IEquipmentService
    {
        /// <summary>
        /// 获取玩家的所有装备列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>装备列表，包含已装备和未装备的所有装备</returns>
        /// <remarks>
        /// 返回的装备按品质降序排列，同品质按强化等级降序排列
        /// 不包含已出售的装备
        /// </remarks>
        Task<List<EquipmentDto>> GetPlayerEquipmentsAsync(string playerId);

        /// <summary>
        /// 获取玩家已装备的装备列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>已装备的装备列表</returns>
        /// <remarks>
        /// 每个装备部位最多只有一件装备
        /// 装备部位包括：武器、头盔、护甲、护腿、鞋子、项链、戒指、手镯
        /// </remarks>
        Task<List<EquipmentDto>> GetEquippedItemsAsync(string playerId);

        /// <summary>
        /// 获取装备详细信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="equipmentId">装备实例ID</param>
        /// <returns>装备详细信息，包含基础属性、强化等级、附加属性等</returns>
        /// <remarks>
        /// 如果装备不存在或不属于该玩家，返回null
        /// </remarks>
        Task<EquipmentDto?> GetEquipmentDetailAsync(string playerId, string equipmentId);

        /// <summary>
        /// 穿戴装备
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">穿戴请求，包含装备实例ID</param>
        /// <returns>是否穿戴成功</returns>
        /// <remarks>
        /// 穿戴流程：
        /// 1. 检查装备是否存在且属于该玩家
        /// 2. 检查装备等级要求是否满足
        /// 3. 如果该部位已有装备，先卸下旧装备
        /// 4. 标记新装备为已装备状态
        /// 5. 重新计算玩家总属性（触发PlayerAttributeService）
        /// 
        /// 穿戴失败的情况：
        /// - 装备不存在
        /// - 装备不属于该玩家
        /// - 玩家等级不足
        /// </remarks>
        Task<bool> EquipItemAsync(string playerId, EquipItemRequestDto request);

        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">卸下请求，包含装备部位</param>
        /// <returns>是否卸下成功</returns>
        /// <remarks>
        /// 卸下流程：
        /// 1. 查找该部位已装备的装备
        /// 2. 标记装备为未装备状态
        /// 3. 重新计算玩家总属性（触发PlayerAttributeService）
        /// 
        /// 卸下失败的情况：
        /// - 该部位没有装备
        /// </remarks>
        Task<bool> UnequipItemAsync(string playerId, UnequipItemRequestDto request);

        /// <summary>
        /// 强化装备
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="request">强化请求，包含装备ID和强化材料</param>
        /// <returns>强化结果，包含是否成功、新的强化等级、消耗的货币等</returns>
        /// <remarks>
        /// 强化规则：
        /// 1. 强化等级上限为+15
        /// 2. 强化成功率随等级提升而降低（基础成功率 = 100 - 等级*5）
        /// 3. 强化消耗金币和强化石
        /// 4. 强化失败不会降级
        /// 5. 强化成功提升装备基础属性（每次+10%）
        /// 
        /// 强化消耗：
        /// - 金币：1000 * (当前等级 + 1)
        /// - 强化石：1个
        /// </remarks>
        Task<EnhanceResultDto> EnhanceEquipmentAsync(string playerId, EnhanceEquipmentRequestDto request);

        /// <summary>
        /// 对比两件装备
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="newEquipmentId">新装备ID（要对比的装备）</param>
        /// <returns>装备对比结果，包含当前装备和新装备的属性差异</returns>
        /// <remarks>
        /// 用于在获得新装备时，快速判断是否比当前装备更好
        /// 如果该部位没有装备，CurrentEquipment为null
        /// </remarks>
        Task<EquipmentCompareDto?> CompareEquipmentAsync(string playerId, string newEquipmentId);

        /// <summary>
        /// 出售装备
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="equipmentId">装备实例ID</param>
        /// <returns>获得的金币数量，如果出售失败返回0</returns>
        /// <remarks>
        /// 出售规则：
        /// 1. 已装备的装备不能出售
        /// 2. 绑定的装备不能出售
        /// 3. 出售价格 = 基础价格 * 品质 * (强化等级 + 1)
        /// 4. 出售后装备永久删除
        /// 
        /// 出售后会自动增加玩家金币
        /// </remarks>
        Task<long> SellEquipmentAsync(string playerId, string equipmentId);

        Task<BatchSellEquipmentResultDto> SellEquipmentsAsync(string playerId, BatchSellEquipmentRequestDto request);

        /// <summary>
        /// 分解一件或多件未穿戴、未锁定装备，并按品质发放配置产出。
        /// </summary>
        Task<EquipmentDecomposeResultDto> DecomposeEquipmentsAsync(string playerId, DecomposeEquipmentRequestDto request);

        Task<EquipmentAutoSellResultDto> AutoSellUnqualifiedEquipmentAsync(
            string playerId,
            IEnumerable<string>? equipmentInstanceIds = null,
            DateTime? acquiredAfter = null,
            string reason = "自动出售装备",
            bool joinCurrentTransaction = false);

        /// <summary>
        /// 获取可强化的装备列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <returns>可强化的装备列表（强化等级小于15的装备）</returns>
        /// <remarks>
        /// 用于在强化界面显示可强化的装备
        /// 包含已装备和未装备的装备
        /// </remarks>
        Task<List<EquipmentDto>> GetEnhanceableEquipmentsAsync(string playerId);

        /// <summary>
        /// 计算装备强化成功率
        /// </summary>
        /// <param name="playerId">玩家唯一标识符（GID）</param>
        /// <param name="equipmentId">装备实例ID</param>
        /// <returns>成功率（0-100的整数）</returns>
        /// <remarks>
        /// 成功率计算公式：max(10, 100 - 当前强化等级 * 5)
        /// 最低成功率为10%
        /// </remarks>
        Task<int> CalculateEnhanceSuccessRateAsync(string playerId, string equipmentId);

        /// <summary>
        /// 锁定装备。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">锁定请求</param>
        /// <returns>锁定结果</returns>
        Task<BindEquipmentResultDto> BindEquipmentAsync(string playerId, BindEquipmentRequestDto request);

        /// <summary>
        /// 解锁装备。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">解锁请求</param>
        /// <returns>解锁结果</returns>
        Task<BindEquipmentResultDto> UnlockEquipmentAsync(string playerId, BindEquipmentRequestDto request);

        /// <summary>
        /// 捐献装备。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="equipmentId">装备实例ID</param>
        /// <returns>捐献结果</returns>
        Task<DonateEquipmentResultDto> DonateEquipmentAsync(string playerId, string equipmentId);

        /// <summary>
        /// 获取装备洗练预览。
        /// </summary>
        Task<EquipmentRerollPreviewDto?> GetEquipmentRerollPreviewAsync(string playerId, string equipmentId);

        /// <summary>
        /// 执行装备洗练（生成候选词条）。
        /// </summary>
        Task<RerollEquipmentResultDto> RollEquipmentRerollAsync(string playerId, RerollEquipmentRollRequestDto request);

        /// <summary>
        /// 接受洗练候选结果。
        /// </summary>
        Task<RerollEquipmentResultDto> AcceptEquipmentRerollAsync(string playerId, RerollEquipmentAcceptRequestDto request);

        /// <summary>
        /// 丢弃洗练候选结果。
        /// </summary>
        Task<RerollEquipmentResultDto> DiscardEquipmentRerollAsync(string playerId, RerollEquipmentDiscardRequestDto request);
    }
}
