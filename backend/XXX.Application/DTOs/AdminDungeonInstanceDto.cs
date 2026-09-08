namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台秘境模板列表项。
    /// </summary>
    public class AdminDungeonInstanceTemplateListItemDto
    {
        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 每日进入次数限制。
        /// </summary>
        public int DailyEnterLimit { get; set; }

        /// <summary>
        /// tick 间隔秒数。
        /// </summary>
        public int TickIntervalSeconds { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 关联的事件组 ID。
        /// </summary>
        public string? EventGroupId { get; set; }
    }

    /// <summary>
    /// 后台秘境模板详情。
    /// </summary>
    public class AdminDungeonInstanceTemplateDetailDto
    {
        /// <summary>
        /// 秘境模板 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 秘境名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 每日进入次数限制。
        /// </summary>
        public int DailyEnterLimit { get; set; } = 1;

        /// <summary>
        /// tick 间隔秒数。
        /// </summary>
        public int TickIntervalSeconds { get; set; }

        /// <summary>
        /// 开放时间配置 JSON。
        /// </summary>
        public string? OpenScheduleJson { get; set; }

        /// <summary>
        /// 进入消耗配置 JSON。
        /// </summary>
        public string? EntryCostsJson { get; set; }

        /// <summary>
        /// 关联的事件组 ID。
        /// </summary>
        public string? EventGroupId { get; set; }

        /// <summary>
        /// 自动用药配置 JSON。
        /// </summary>
        public string? AutoMedicineConfigJson { get; set; }

        /// <summary>
        /// 玩家偶遇配置 JSON。
        /// </summary>
        public string? EncounterConfigJson { get; set; }

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

    /// <summary>
    /// 后台秘境事件配置列表项。
    /// </summary>
    public class AdminDungeonEventConfigListItemDto
    {
        /// <summary>
        /// 事件 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 事件名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 关联的秘境模板 ID。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 事件大类。
        /// </summary>
        public int EventType { get; set; }

        /// <summary>
        /// 抽取权重。
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 是否为内置数据。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台秘境事件配置详情。
    /// </summary>
    public class AdminDungeonEventConfigDetailDto
    {
        /// <summary>
        /// 事件 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 事件名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 事件描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 关联的秘境模板 ID。
        /// </summary>
        public string DungeonId { get; set; } = "*";

        /// <summary>
        /// 事件大类。
        /// </summary>
        public int EventType { get; set; }

        /// <summary>
        /// 抽取权重。
        /// </summary>
        public int Weight { get; set; } = 10;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 事件参数 JSON。
        /// </summary>
        public string? EventDataJson { get; set; }

        /// <summary>
        /// 死亡时是否保留收益。
        /// </summary>
        public bool? DeathKeep { get; set; }

        /// <summary>
        /// 是否为内置数据。
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

    /// <summary>
    /// 后台秘境事件组列表项。
    /// </summary>
    public class AdminDungeonEventGroupListItemDto
    {
        /// <summary>
        /// 事件组 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 事件组名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 组内事件数量。
        /// </summary>
        public int EventCount { get; set; }

        /// <summary>
        /// 是否为内置数据。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台秘境事件组详情。
    /// </summary>
    public class AdminDungeonEventGroupDetailDto
    {
        /// <summary>
        /// 事件组 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 事件组名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 组内事件列表 JSON（EventGroupItem[]）。
        /// </summary>
        public string? GroupItemsJson { get; set; }

        /// <summary>
        /// 是否为内置数据。
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
