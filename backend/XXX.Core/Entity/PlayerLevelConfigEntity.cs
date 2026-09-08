using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家等级成长配置。
    /// </summary>
    [SugarTable("PlayerLevelConfigs")]
    public class PlayerLevelConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Level { get; set; }

        [SugarColumn]
        public long RequiredExp { get; set; }

        [SugarColumn]
        public int BaseHp { get; set; }

        [SugarColumn]
        public int BaseMp { get; set; }

        [SugarColumn]
        public int BasePhysicalAttack { get; set; }

        [SugarColumn]
        public int BaseMagicAttack { get; set; }

        [SugarColumn]
        public int BasePhysicalDefense { get; set; }

        [SugarColumn]
        public int BaseMagicDefense { get; set; }

        [SugarColumn]
        public int BaseSpeed { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
