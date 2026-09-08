using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家初始资源配置。
    /// </summary>
    [SugarTable("PlayerInitialResourceConfigs")]
    public class PlayerInitialResourceConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string ConfigId { get; set; } = "default";

        [SugarColumn]
        public int StartLevel { get; set; } = 1;

        [SugarColumn]
        public long StartExp { get; set; }

        [SugarColumn]
        public long StartGold { get; set; }

        [SugarColumn]
        public long StartSpiritStone { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
