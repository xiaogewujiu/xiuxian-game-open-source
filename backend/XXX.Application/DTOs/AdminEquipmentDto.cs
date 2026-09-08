namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台装备列表项。
    /// </summary>
    public class AdminEquipmentListItemDto
    {
        /// <summary>
        /// 装备编号。
        /// </summary>
        public int EquipmentId { get; set; }

        /// <summary>
        /// 装备名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 装备等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 槽位。
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// 图片路径。
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 是否可交易。
        /// </summary>
        public bool IsTradeable { get; set; } = true;
    }

    /// <summary>
    /// 后台装备详情。
    /// </summary>
    public class AdminEquipmentDetailDto
    {
        /// <summary>
        /// 装备编号。
        /// </summary>
        public int EquipmentId { get; set; }

        /// <summary>
        /// 装备名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 装备等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 槽位。
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// 战斗风格。
        /// </summary>
        public int CombatStyle { get; set; }

        /// <summary>
        /// 武器分类。
        /// </summary>
        public int WeaponCategory { get; set; }

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 图片路径。
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary>
        /// 是否可交易。
        /// </summary>
        public bool IsTradeable { get; set; } = true;

        /// <summary>
        /// 属性区间。
        /// </summary>
        public BaseAttributesRangeDto Attributes { get; set; } = new();
    }
}
