namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台元素克制矩阵条目。
    /// </summary>
    public class AdminElementRelationEntryDto
    {
        /// <summary>
        /// 克制规则主键。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 攻击方元素枚举值。
        /// </summary>
        public int AttackerElement { get; set; }

        /// <summary>
        /// 防御方元素枚举值。
        /// </summary>
        public int DefenderElement { get; set; }

        /// <summary>
        /// 伤害修正倍率。
        /// 例如 1.2 表示增伤 20%。
        /// </summary>
        public double Modifier { get; set; }

        /// <summary>
        /// 后台展示与加载顺序。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 当前规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
