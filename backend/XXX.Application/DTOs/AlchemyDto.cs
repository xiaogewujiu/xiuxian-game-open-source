namespace XXX.Application.DTOs
{
    /// <summary>
    /// 炼丹系统总览数据。
    /// </summary>
    public class AlchemyDto
    {
        /// <summary>
        /// 丹炉等级。
        /// </summary>
        public int FurnaceLevel { get; set; }

        /// <summary>
        /// 炼丹师等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 炼丹师总经验。
        /// </summary>
        public int AlchemistExp { get; set; }

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
        /// 炼丹熟练度。
        /// </summary>
        public int Proficiency { get; set; }

        /// <summary>
        /// 成功率加成百分比。
        /// </summary>
        public int SuccessRateBonus { get; set; }

        /// <summary>
        /// 炼制时间缩短百分比。
        /// </summary>
        public int CraftTimeReduction { get; set; }

        /// <summary>
        /// 成品产出加成。
        /// </summary>
        public int YieldBonus { get; set; }

        /// <summary>
        /// 今日炼丹次数。
        /// </summary>
        public int TodayCraftCount { get; set; }

        /// <summary>
        /// 历史总炼丹次数。
        /// </summary>
        public int TotalCraftCount { get; set; }

        /// <summary>
        /// 历史成功炼丹次数。
        /// </summary>
        public int SuccessCraftCount { get; set; }

        /// <summary>
        /// 已学习丹方 ID 列表。
        /// </summary>
        public List<string> LearnedRecipes { get; set; } = [];

        /// <summary>
        /// 当前是否存在炼丹中的任务。
        /// </summary>
        public bool IsCrafting { get; set; }

        /// <summary>
        /// 当前炼丹中的丹方ID。
        /// </summary>
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 当前炼丹开始时间。
        /// </summary>
        public DateTime? ActiveCraftStartedAt { get; set; }

        /// <summary>
        /// 当前炼丹完成时间。
        /// </summary>
        public DateTime? ActiveCraftCompleteAt { get; set; }

        /// <summary>
        /// 当前是否可以领取炼丹结果。
        /// </summary>
        public bool CanCollect { get; set; }
    }

    /// <summary>
    /// 单个丹方的展示数据。
    /// </summary>
    public class AlchemyRecipeDto
    {
        /// <summary>
        /// 丹方 ID。
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 产出丹药模板 ID。
        /// </summary>
        public string PillTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 丹方名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 丹方描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 丹方等级。
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 学习或使用所需丹炉等级。
        /// </summary>
        public int RequiredFurnaceLevel { get; set; }

        /// <summary>
        /// 基础成功率。
        /// </summary>
        public int BaseSuccessRate { get; set; }

        /// <summary>
        /// 基础炼制时长。
        /// </summary>
        public int BaseCraftTime { get; set; }

        /// <summary>
        /// 当前实际炼制时长（秒）。
        /// </summary>
        public int ActualCraftTimeSeconds { get; set; }

        /// <summary>
        /// 配方材料列表。
        /// </summary>
        public List<RecipeMaterialDto> Materials { get; set; } = [];

        /// <summary>
        /// 当前玩家处理该丹方时的实际成功率。
        /// </summary>
        public int ActualSuccessRate { get; set; }

        /// <summary>
        /// 当前玩家处理低级丹方获得的额外成功率加成。
        /// </summary>
        public int SuccessRateBonus { get; set; }

        /// <summary>
        /// 当前玩家是否满足该丹方的炼丹师等级要求。
        /// </summary>
        public bool CanCraft { get; set; }

        /// <summary>
        /// 当前玩家无法处理该丹方时的原因。
        /// </summary>
        public string UnavailableReason { get; set; } = string.Empty;

        /// <summary>
        /// 当前玩家是否已学会。
        /// </summary>
        public bool IsLearned { get; set; }

        /// <summary>
        /// 解锁该丹方所需的卷轴道具编号。
        /// </summary>
        public string? UnlockItemId { get; set; }

        /// <summary>
        /// 解锁该丹方所需的卷轴道具名称。
        /// </summary>
        public string? UnlockItemName { get; set; }

        /// <summary>
        /// 丹方展示图。
        /// 通常复用产出丹药模板的图片路径。
        /// </summary>
        public string? IconPath { get; set; }
    }

    /// <summary>
    /// 丹方材料条目。
    /// </summary>
    public class RecipeMaterialDto
    {
        /// <summary>
        /// 材料物品 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 材料物品名称。
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 所需数量。
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 是否允许替代。
        /// </summary>
        public bool IsReplaceable { get; set; }

        /// <summary>
        /// 材料图标路径。
        /// </summary>
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// 发起炼丹请求。
    /// </summary>
    public class StartAlchemyRequestDto
    {
        /// <summary>
        /// 要炼制的丹方 ID。
        /// </summary>
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 炼制数量。
        /// </summary>
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 炼丹结算结果。
    /// </summary>
    public class AlchemyResultDto
    {
        /// <summary>
        /// 本次炼制是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结算提示文案。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 产出丹药物品 ID。
        /// </summary>
        public string? PillItemId { get; set; }

        /// <summary>
        /// 产出丹药名称。
        /// </summary>
        public string? PillName { get; set; }

        /// <summary>
        /// 产出数量。
        /// </summary>
        public int PillQuantity { get; set; }

        /// <summary>
        /// 本次采用的实际成功率。
        /// </summary>
        public int ActualSuccessRate { get; set; }

        /// <summary>
        /// 本次获得的炼丹师经验。
        /// </summary>
        public int AlchemistExpGained { get; set; }

        /// <summary>
        /// 结算后的炼丹师等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 本次获得熟练度。
        /// </summary>
        public int ProficiencyGained { get; set; }

        /// <summary>
        /// 本次开始炼丹的开始时间。
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 本次开始炼丹的完成时间。
        /// </summary>
        public DateTime? CompleteAt { get; set; }

        /// <summary>
        /// 是否需要等待完成后领取。
        /// </summary>
        public bool RequiresCollection { get; set; }
    }

    /// <summary>
    /// 丹药模板的前端展示结构。
    /// </summary>
    public class PillDto
    {
        /// <summary>
        /// 丹药模板 ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 丹药名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 丹药品阶。
        /// </summary>
        public string Rank { get; set; } = string.Empty;

        /// <summary>
        /// 丹药类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 效果类型。
        /// </summary>
        public string EffectType { get; set; } = string.Empty;

        /// <summary>
        /// 效果数值。
        /// </summary>
        public double EffectValue { get; set; }

        /// <summary>
        /// 效果持续时间。
        /// </summary>
        public int EffectDuration { get; set; }

        /// <summary>
        /// 使用冷却。
        /// </summary>
        public int Cooldown { get; set; }

        /// <summary>
        /// 每日使用上限。
        /// </summary>
        public int DailyLimit { get; set; }
    }
}
