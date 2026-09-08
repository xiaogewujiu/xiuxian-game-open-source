using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台 Buff 模板列表项。
    /// </summary>
    public class AdminBuffListItemDto
    {
        /// <summary>
        /// Buff 编号。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// Buff 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 持续回合。
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台 Buff 模板详情。
    /// </summary>
    public class AdminBuffDetailDto
    {
        /// <summary>
        /// Buff 编号。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// Buff 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 持续回合。
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// 叠层规则。
        /// </summary>
        public int StackRule { get; set; }

        /// <summary>
        /// 效果列表。
        /// </summary>
        public List<BuffEffect> Effects { get; set; } = [];

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
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
    }
}
