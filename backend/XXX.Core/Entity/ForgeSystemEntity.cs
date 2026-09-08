using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 锻造系统数据实体。
    /// </summary>
    [SugarTable("ForgeSystems")]
    public class ForgeSystemEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int BlacksmithLevel { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BlacksmithExp { get; set; } = 0;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TodayForgeCount { get; set; } = 0;

        [SugarColumn(IsNullable = false)]
        public DateTime DailyResetTime { get; set; } = DateTime.Now.Date;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalForgeCount { get; set; } = 0;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int SuccessForgeCount { get; set; } = 0;

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ActiveRecipeId { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ActiveForgeStartedAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ActiveForgeCompleteAt { get; set; }

        /// <summary>
        /// 已解锁锻造配方 ID 列表（JSON格式）。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
        public string? LearnedRecipesJson { get; set; }

        /// <summary>
        /// 已解锁锻造配方 ID 列表（非持久化）。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> LearnedRecipes
        {
            get => string.IsNullOrWhiteSpace(LearnedRecipesJson)
                ? []
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(LearnedRecipesJson) ?? [];
            set => LearnedRecipesJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
        public string? PendingResultJson { get; set; }
    }
}
