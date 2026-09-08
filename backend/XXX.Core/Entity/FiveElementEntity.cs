using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 五行聚灵阵数据实体
    /// 对应数据库五行聚灵阵表
    /// </summary>
    [SugarTable("FiveElementArrays")]
    public class FiveElementArrayEntity
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
        /// 聚灵阵等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int ArrayLevel { get; set; } = 1;

        /// <summary>
        /// 金元素等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int MetalLevel { get; set; } = 0;

        /// <summary>
        /// 木元素等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int WoodLevel { get; set; } = 0;

        /// <summary>
        /// 水元素等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int WaterLevel { get; set; } = 0;

        /// <summary>
        /// 火元素等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int FireLevel { get; set; } = 0;

        /// <summary>
        /// 土元素等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int EarthLevel { get; set; } = 0;

        /// <summary>
        /// 金元素经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long MetalExp { get; set; } = 0;

        /// <summary>
        /// 木元素经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long WoodExp { get; set; } = 0;

        /// <summary>
        /// 水元素经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long WaterExp { get; set; } = 0;

        /// <summary>
        /// 火元素经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long FireExp { get; set; } = 0;

        /// <summary>
        /// 土元素经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long EarthExp { get; set; } = 0;

        /// <summary>
        /// 总灵气值
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalSpiritPower { get; set; } = 0;

        /// <summary>
        /// 灵气产出速率（每小时）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "10")]
        public int SpiritPowerRate { get; set; } = 10;

        /// <summary>
        /// 最后收集灵气时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastCollectTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 今日收集次数
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TodayCollectCount { get; set; } = 0;

        /// <summary>
        /// 今日统计重置时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime DailyResetTime { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// 累计收集灵气
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalCollectedSpiritPower { get; set; } = 0;

        /// <summary>
        /// 已激活的五行组合（JSON格式）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? ActiveCombinationsJson { get; set; }

        /// <summary>
        /// 已激活的五行组合（非持久化）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> ActiveCombinations
        {
            get => string.IsNullOrEmpty(ActiveCombinationsJson)
                ? []
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(ActiveCombinationsJson) ?? [];
            set => ActiveCombinationsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
