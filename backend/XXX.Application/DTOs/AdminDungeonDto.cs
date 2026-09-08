namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台副本列表项。
    /// </summary>
    public class AdminDungeonListItemDto
    {
        /// <summary>
        /// 副本编号。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 副本名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 日次数限制。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 需要队伍人数。
        /// </summary>
        public int RequiredTeamSize { get; set; }

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
    /// 后台副本详情。
    /// </summary>
    public class AdminDungeonDetailDto
    {
        /// <summary>
        /// 副本编号。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 副本名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 日次数限制。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 对应副本首层地图编号。
        /// </summary>
        public string FubenMapId { get; set; } = string.Empty;

        /// <summary>
        /// 需要队伍人数。
        /// </summary>
        public int RequiredTeamSize { get; set; }

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

        /// <summary>
        /// 副本地图链是否有效。
        /// </summary>
        public bool ChainValid { get; set; }

        /// <summary>
        /// 副本地图链校验错误。
        /// </summary>
        public string? ChainError { get; set; }

        /// <summary>
        /// 副本地图链层级。
        /// </summary>
        public List<AdminDungeonMapStageDto> ChainStages { get; set; } = [];
    }

    /// <summary>
    /// 后台副本地图链层级。
    /// </summary>
    public class AdminDungeonMapStageDto
    {
        /// <summary>层序号。</summary>
        public int StageIndex { get; set; }
        /// <summary>地图编号。</summary>
        public string MapId { get; set; } = string.Empty;
        /// <summary>地图名称。</summary>
        public string MapName { get; set; } = string.Empty;
        /// <summary>下一张地图编号。</summary>
        public string? NextMapId { get; set; }
    }
}
