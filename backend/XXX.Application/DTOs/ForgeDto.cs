namespace XXX.Application.DTOs
{
    /// <summary>
    /// 中文注释：
    /// 锻造配方 DTO。
    /// 这里只返回当前前端锻造弹窗真正需要展示的字段：
    /// 配方ID、产出装备、材料需求、金币消耗和成功率。
    /// 这样可以先把“真实选图纸 -> 扣材料 -> 生成装备实例”这条链路跑通，
    /// 后续如果再补复杂的锻造台等级、锻造师等级，也能在这个结构上继续扩展。
    /// </summary>
    public class ForgeRecipeDto
    {
        /// <summary>
        /// 配方ID
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 装备模板ID
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 产出装备名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产出装备描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 装备部位
        /// </summary>
        public string SlotName { get; set; } = string.Empty;

        /// <summary>
        /// 装备品质
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 装备等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 消耗金币
        /// </summary>
        public long CostGold { get; set; }

        /// <summary>
        /// 成功率
        /// </summary>
        public int SuccessRate { get; set; }

        /// <summary>
        /// 当前玩家处理该图纸时的实际成功率。
        /// </summary>
        public int ActualSuccessRate { get; set; }

        /// <summary>
        /// 当前玩家处理低级图纸获得的额外成功率加成。
        /// </summary>
        public int SuccessRateBonus { get; set; }

        /// <summary>
        /// 当前玩家是否满足该图纸的锻造师等级要求。
        /// </summary>
        public bool CanForge { get; set; }

        /// <summary>
        /// 当前玩家无法处理该图纸时的原因。
        /// </summary>
        public string UnavailableReason { get; set; } = string.Empty;

        /// <summary>
        /// 当前实际打造时长（秒）。
        /// </summary>
        public int CraftTimeSeconds { get; set; }

        /// <summary>
        /// 所需材料
        /// </summary>
        public List<ForgeMaterialDto> Materials { get; set; } = [];

        /// <summary>
        /// 解锁该图纸所需的卷轴道具编号。
        /// </summary>
        public string? UnlockItemId { get; set; }

        /// <summary>
        /// 解锁该图纸所需的卷轴道具名称。
        /// </summary>
        public string? UnlockItemName { get; set; }
    }

    /// <summary>
    /// 锻造材料 DTO
    /// </summary>
    public class ForgeMaterialDto
    {
        /// <summary>
        /// 材料物品 ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 材料名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 材料图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 所需数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 获取锻造总览 DTO
    /// </summary>
    public class ForgeOverviewDto
    {
        /// <summary>
        /// 当前锻造师等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 当前锻造师总经验。
        /// </summary>
        public int BlacksmithExp { get; set; }

        /// <summary>
        /// 当前等级已积累经验。
        /// </summary>
        public int CurrentLevelExp { get; set; }

        /// <summary>
        /// 升到下一级所需经验。
        /// </summary>
        public int NextLevelExp { get; set; }

        /// <summary>
        /// 当前聚灵阵给予的等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前是否存在锻造中的任务。
        /// </summary>
        public bool IsForging { get; set; }

        /// <summary>
        /// 当前锻造中的图纸ID。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 当前锻造开始时间。
        /// </summary>
        public DateTime? ActiveForgeStartedAt { get; set; }

        /// <summary>
        /// 当前锻造完成时间。
        /// </summary>
        public DateTime? ActiveForgeCompleteAt { get; set; }

        /// <summary>
        /// 当前是否可以领取锻造结果。
        /// </summary>
        public bool CanCollect { get; set; }

        /// <summary>
        /// 当前可用配方
        /// </summary>
        public List<ForgeRecipeDto> Recipes { get; set; } = [];

        /// <summary>
        /// 玩家当前背包中的相关材料
        /// </summary>
        public List<InventoryItemDto> Materials { get; set; } = [];
    }

    /// <summary>
    /// 锻造请求 DTO
    /// </summary>
    public class ForgeEquipmentRequestDto
    {
        /// <summary>
        /// 配方ID
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 锻造结果 DTO
    /// </summary>
    public class ForgeEquipmentResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 本次采用的实际成功率。
        /// </summary>
        public int ActualSuccessRate { get; set; }

        /// <summary>
        /// 本次获得的锻造师经验。
        /// </summary>
        public int BlacksmithExpGained { get; set; }

        /// <summary>
        /// 结算后的锻造师等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 实际消耗金币
        /// </summary>
        public long CostGold { get; set; }

        /// <summary>
        /// 实际消耗材料
        /// </summary>
        public List<ForgeMaterialDto> ConsumedMaterials { get; set; } = [];

        /// <summary>
        /// 新生成的装备
        /// </summary>
        public EquipmentDto? Equipment { get; set; }

        /// <summary>
        /// 开始打造时间。
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 完成打造时间。
        /// </summary>
        public DateTime? CompleteAt { get; set; }

        /// <summary>
        /// 是否需要等待完成后领取。
        /// </summary>
        public bool RequiresCollection { get; set; }
    }
}
