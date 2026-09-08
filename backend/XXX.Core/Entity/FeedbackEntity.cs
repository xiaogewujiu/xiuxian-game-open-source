using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家建议反馈实体。
    /// 保存玩家提交的反馈正文、处理状态以及管理员处理结果。
    /// </summary>
    [SugarTable("PlayerFeedbacks")]
    public class PlayerFeedbackEntity : BaseEntity
    {
        /// <summary>
        /// 提交反馈的玩家编号。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_feedback_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 反馈类型：Suggestion、Bug、Gameplay、Account 或 Other。
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false, IndexGroupNameList = new[] { "idx_feedback_status" })]
        public string Type { get; set; } = "Suggestion";

        /// <summary>
        /// 反馈标题。
        /// </summary>
        [SugarColumn(Length = 80, IsNullable = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 反馈正文。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 当前处理状态：Pending、Processing、Resolved、Rejected 或 Closed。
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false, IndexGroupNameList = new[] { "idx_feedback_status" })]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 玩家可见的管理员回复。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? AdminReply { get; set; }

        /// <summary>
        /// 仅管理员可见的内部备注。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? InternalNote { get; set; }

        /// <summary>
        /// 最后处理反馈的管理员编号。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? HandledByAdminId { get; set; }

        /// <summary>
        /// 最后处理时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? HandledAt { get; set; }
    }

    /// <summary>
    /// 建议反馈图片附件实体。
    /// </summary>
    [SugarTable("PlayerFeedbackAttachments")]
    public class PlayerFeedbackAttachmentEntity : BaseEntity
    {
        /// <summary>
        /// 所属反馈编号。
        /// </summary>
        [SugarColumn(IsNullable = false, IndexGroupNameList = new[] { "idx_feedback_attachment" })]
        public long FeedbackId { get; set; }

        /// <summary>
        /// 图片相对路径。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名。
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// 文件 MIME 类型。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// 文件大小，单位为字节。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long FileSize { get; set; }

        /// <summary>
        /// 图片展示顺序。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 建议反馈状态历史实体。
    /// </summary>
    [SugarTable("PlayerFeedbackStatusHistories")]
    public class PlayerFeedbackStatusHistoryEntity : BaseEntity
    {
        /// <summary>
        /// 所属反馈编号。
        /// </summary>
        [SugarColumn(IsNullable = false, IndexGroupNameList = new[] { "idx_feedback_history" })]
        public long FeedbackId { get; set; }

        /// <summary>
        /// 变更前状态。
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string? FromStatus { get; set; }

        /// <summary>
        /// 变更后状态。
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false)]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>
        /// 执行操作的管理员编号。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// 操作时保存的玩家可见回复快照。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? ReplySnapshot { get; set; }

        /// <summary>
        /// 操作时保存的内部备注快照。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? NoteSnapshot { get; set; }
    }
}
