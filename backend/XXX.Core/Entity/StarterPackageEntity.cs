using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 新手礼包配置实体。
    /// </summary>
    [SugarTable("StarterPackageConfigs")]
    public class StarterPackageConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string PackageId { get; set; } = string.Empty;

        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        [SugarColumn]
        public bool IsEnabled { get; set; } = true;

        [SugarColumn]
        public bool AutoGrantOnRegister { get; set; } = false;

        [SugarColumn]
        public int SortOrder { get; set; } = 0;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? ConfigVersion { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Remark { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? CreatedBy { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? UpdatedBy { get; set; }

        [SugarColumn]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [SugarColumn]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 新手礼包道具发放项。
    /// </summary>
    [SugarTable("StarterPackageGrantItems")]
    public class StarterPackageGrantItemEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 50)]
        public string PackageId { get; set; } = string.Empty;

        [SugarColumn(Length = 50)]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn]
        public int Quantity { get; set; } = 1;

        [SugarColumn]
        public bool IsBound { get; set; } = true;

        [SugarColumn]
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// 新手礼包技能发放项。
    /// </summary>
    [SugarTable("StarterPackageGrantSkills")]
    public class StarterPackageGrantSkillEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string GID { get; set; } = string.Empty;

        [SugarColumn(Length = 50)]
        public string PackageId { get; set; } = string.Empty;

        [SugarColumn]
        public int SkillId { get; set; }

        [SugarColumn]
        public int SortOrder { get; set; } = 0;
    }
}
