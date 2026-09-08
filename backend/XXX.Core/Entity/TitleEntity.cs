using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 称号模板实体。
    /// </summary>
    [SugarTable("TitleTemplates")]
    public class TitleTemplateEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 称号唯一标识。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// 称号名称。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 描述。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 来源类型（Achievement/Arena/Tower/Admin）。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string Source { get; set; } = "Admin";

        /// <summary>
        /// 来源ID（成就ID/赛季编号等）。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SourceId { get; set; }

        /// <summary>
        /// 稀有度（Common/Rare/Epic/Legendary）。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "Common")]
        public string Rarity { get; set; } = "Common";

        /// <summary>
        /// 小图标路径。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? IconPath { get; set; }

        /// <summary>
        /// 称号图片路径（徽章/边框图）。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? ImagePath { get; set; }

        /// <summary>
        /// 是否在称号列表中可见。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 玩家称号实体。
    /// </summary>
    [SugarTable("PlayerTitles")]
    public class PlayerTitleEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 玩家GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player_title" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 称号ID。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new[] { "idx_player_title" })]
        public string TitleId { get; set; } = string.Empty;

        /// <summary>
        /// 解锁时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UnlockedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否佩戴中。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsEquipped { get; set; }
    }
}
