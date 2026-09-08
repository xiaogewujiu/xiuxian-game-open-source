using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 秘境事件配置。
    /// 每条记录代表一个具体的秘境随机事件，可通过后台自由配置。
    /// </summary>
    [SugarTable("DungeonEventConfigs")]
    public class DungeonEventConfigEntity : ConfigEntity
    {
        /// <summary>
        /// 关联的秘境模板 ID。"*" 表示所有秘境通用。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string DungeonId { get; set; } = "*";

        /// <summary>
        /// 事件大类（DungeonEventType 枚举值）。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int EventType { get; set; }

        /// <summary>
        /// 在该类型事件池中的抽取权重。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Weight { get; set; } = 10;

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 事件参数 JSON（怪物 ID、恢复百分比、buff 效果等）。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? EventDataJson { get; set; }

        /// <summary>
        /// 死亡时是否保留收益。仅收益类事件有效。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool? DeathKeep { get; set; }

        /// <summary>
        /// 种子数据标识。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置种子数据。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置数据版本。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
