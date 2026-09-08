using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 丹方实体
    /// 对应数据库丹方表
    /// </summary>
    [SugarTable("AlchemyRecipes")]
    public class AlchemyRecipeEntity
    {
        /// <summary>
        /// 丹方ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string RecipeId { get; set; } = string.Empty;

        /// <summary>
        /// 丹药模板ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string PillTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 丹方名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 丹方描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 所需炼丹炉等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int RequiredFurnaceLevel { get; set; } = 1;

        /// <summary>
        /// 基础成功率（百分比）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "50")]
        public int BaseSuccessRate { get; set; } = 50;

        /// <summary>
        /// 基础炼制时间（秒）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "300")]
        public int BaseCraftTime { get; set; } = 300;

        /// <summary>
        /// 所需材料JSON
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? MaterialsJson { get; set; }

        /// <summary>
        /// 所需材料列表（非持久化）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<RecipeMaterial> Materials
        {
            get => string.IsNullOrEmpty(MaterialsJson)
                ? []
                : System.Text.Json.JsonSerializer.Deserialize<List<RecipeMaterial>>(MaterialsJson) ?? [];
            set => MaterialsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// 解锁条件
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? UnlockCondition { get; set; }

        /// <summary>
        /// 是否默认已学习
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsDefaultLearned { get; set; } = false;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置配方。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置版本号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 炼丹系统数据实体
    /// 对应数据库炼丹系统表
    /// </summary>
    [SugarTable("AlchemySystems")]
    public class AlchemySystemEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 炼丹炉等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int FurnaceLevel { get; set; } = 1;

        /// <summary>
        /// 炼丹师等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int AlchemistLevel { get; set; } = 1;

        /// <summary>
        /// 炼丹师总经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int AlchemistExp { get; set; } = 0;

        /// <summary>
        /// 炼丹熟练度
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Proficiency { get; set; } = 0;

        /// <summary>
        /// 已学习丹方ID列表（JSON格式）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? LearnedRecipesJson { get; set; }

        /// <summary>
        /// 已学习丹方ID列表（非持久化）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> LearnedRecipes
        {
            get => string.IsNullOrEmpty(LearnedRecipesJson)
                ? []
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(LearnedRecipesJson) ?? [];
            set => LearnedRecipesJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// 成功率加成（百分比）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SuccessRateBonus { get; set; } = 0;

        /// <summary>
        /// 炼制时间减少（百分比）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int CraftTimeReduction { get; set; } = 0;

        /// <summary>
        /// 产量加成（百分比）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int YieldBonus { get; set; } = 0;

        /// <summary>
        /// 今日炼制次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TodayCraftCount { get; set; } = 0;

        /// <summary>
        /// 今日统计重置时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime DailyResetTime { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// 累计炼制次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalCraftCount { get; set; } = 0;

        /// <summary>
        /// 成功炼制次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SuccessCraftCount { get; set; } = 0;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 当前正在炼制的丹方ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? ActiveRecipeId { get; set; }

        /// <summary>
        /// 当前炼制开始时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ActiveCraftStartedAt { get; set; }

        /// <summary>
        /// 当前炼制完成时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ActiveCraftCompleteAt { get; set; }

        /// <summary>
        /// 当前炼制待领取结果（JSON）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? PendingResultJson { get; set; }
    }

    /// <summary>
    /// 丹方材料
    /// </summary>
    public class RecipeMaterial
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 所需数量
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 是否可替代
        /// </summary>
        public bool IsReplaceable { get; set; }

        /// <summary>
        /// 替代物品ID列表
        /// </summary>
        public List<string>? AlternativeItemIds { get; set; }
    }
}
