namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台新手礼包列表项。
    /// </summary>
    public class AdminStarterPackageListItemDto
    {
        /// <summary>礼包编号。</summary>
        public string PackageId { get; set; } = string.Empty;

        /// <summary>礼包名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>当前礼包是否启用。</summary>
        public bool IsEnabled { get; set; }

        /// <summary>是否在注册时自动发放。</summary>
        public bool AutoGrantOnRegister { get; set; }

        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; }

        /// <summary>物品奖励条目数量。</summary>
        public int ItemGrantCount { get; set; }

        /// <summary>技能奖励条目数量。</summary>
        public int SkillGrantCount { get; set; }

        /// <summary>配置版本号。</summary>
        public string? ConfigVersion { get; set; }

        /// <summary>是否为内置种子礼包。</summary>
        public bool IsBuiltIn { get; set; }
    }

    /// <summary>
    /// 后台新手礼包详情。
    /// </summary>
    public class AdminStarterPackageDetailDto
    {
        /// <summary>礼包编号。</summary>
        public string PackageId { get; set; } = string.Empty;

        /// <summary>礼包名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>礼包描述。</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>当前礼包是否启用。</summary>
        public bool IsEnabled { get; set; }

        /// <summary>是否在注册时自动发放。</summary>
        public bool AutoGrantOnRegister { get; set; }

        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; }

        /// <summary>配置版本号。</summary>
        public string? ConfigVersion { get; set; }

        /// <summary>内置种子键。</summary>
        public string? SeedKey { get; set; }

        /// <summary>是否为内置种子礼包。</summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>内置版本号。</summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>备注信息。</summary>
        public string? Remark { get; set; }

        /// <summary>创建人。</summary>
        public string? CreatedBy { get; set; }

        /// <summary>最后更新人。</summary>
        public string? UpdatedBy { get; set; }

        /// <summary>创建时间。</summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>最后更新时间。</summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary>物品奖励明细。</summary>
        public List<AdminStarterPackageItemGrantDto> ItemGrants { get; set; } = [];

        /// <summary>技能奖励明细。</summary>
        public List<AdminStarterPackageSkillGrantDto> SkillGrants { get; set; } = [];
    }

    /// <summary>
    /// 后台新手礼包物品奖励明细。
    /// </summary>
    public class AdminStarterPackageItemGrantDto
    {
        /// <summary>奖励记录主键。</summary>
        public string? GID { get; set; }

        /// <summary>物品编号。</summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>奖励数量。</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>是否绑定。</summary>
        public bool IsBound { get; set; } = true;

        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// 后台新手礼包技能奖励明细。
    /// </summary>
    public class AdminStarterPackageSkillGrantDto
    {
        /// <summary>奖励记录主键。</summary>
        public string? GID { get; set; }

        /// <summary>技能编号。</summary>
        public int SkillId { get; set; }

        /// <summary>后台展示排序。</summary>
        public int SortOrder { get; set; } = 0;
    }
}
