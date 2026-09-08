namespace XXX.Application.DTOs
{
    /// <summary>
    /// 玩家建议反馈列表项。
    /// </summary>
    public class FeedbackListItemDto
    {
        /// <summary>反馈编号。</summary>
        public long Id { get; set; }
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;
        /// <summary>玩家名称。</summary>
        public string PlayerName { get; set; } = string.Empty;
        /// <summary>反馈类型。</summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>反馈标题。</summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>附件数量。</summary>
        public int AttachmentCount { get; set; }
        /// <summary>处理状态。</summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>创建时间。</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>处理管理员编号。</summary>
        public string? HandledByAdminId { get; set; }
        /// <summary>处理时间。</summary>
        public DateTime? HandledAt { get; set; }
    }

    /// <summary>
    /// 建议反馈图片附件。
    /// </summary>
    public class FeedbackAttachmentDto
    {
        /// <summary>附件编号。</summary>
        public long Id { get; set; }
        /// <summary>图片相对路径。</summary>
        public string RelativePath { get; set; } = string.Empty;
        /// <summary>原始文件名。</summary>
        public string? OriginalFileName { get; set; }
        /// <summary>文件类型。</summary>
        public string ContentType { get; set; } = string.Empty;
        /// <summary>文件大小。</summary>
        public long FileSize { get; set; }
        /// <summary>展示顺序。</summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 建议反馈状态历史。
    /// </summary>
    public class FeedbackStatusHistoryDto
    {
        /// <summary>变更前状态。</summary>
        public string? FromStatus { get; set; }
        /// <summary>变更后状态。</summary>
        public string ToStatus { get; set; } = string.Empty;
        /// <summary>管理员编号。</summary>
        public string AdminId { get; set; } = string.Empty;
        /// <summary>玩家可见回复快照。</summary>
        public string? ReplySnapshot { get; set; }
        /// <summary>内部备注快照。</summary>
        public string? NoteSnapshot { get; set; }
        /// <summary>创建时间。</summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 建议反馈详情。
    /// </summary>
    public class FeedbackDetailDto : FeedbackListItemDto
    {
        /// <summary>反馈正文。</summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>玩家可见回复。</summary>
        public string? AdminReply { get; set; }
        /// <summary>管理员内部备注，仅后台返回。</summary>
        public string? InternalNote { get; set; }
        /// <summary>反馈附件。</summary>
        public List<FeedbackAttachmentDto> Attachments { get; set; } = [];
        /// <summary>状态历史，仅后台返回。</summary>
        public List<FeedbackStatusHistoryDto> StatusHistory { get; set; } = [];
    }

    /// <summary>玩家提交反馈请求。</summary>
    public class CreateFeedbackRequestDto
    {
        /// <summary>反馈类型。</summary>
        public string Type { get; set; } = "Suggestion";
        /// <summary>反馈标题。</summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>反馈正文。</summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>已上传附件路径。</summary>
        public List<CreateFeedbackAttachmentDto> Attachments { get; set; } = [];
    }

    /// <summary>反馈附件提交信息。</summary>
    public class CreateFeedbackAttachmentDto
    {
        /// <summary>图片相对路径。</summary>
        public string RelativePath { get; set; } = string.Empty;
        /// <summary>原始文件名。</summary>
        public string? OriginalFileName { get; set; }
        /// <summary>内容类型。</summary>
        public string ContentType { get; set; } = string.Empty;
        /// <summary>文件大小。</summary>
        public long FileSize { get; set; }
    }

    /// <summary>管理员反馈处理请求。</summary>
    public class ProcessFeedbackRequestDto
    {
        /// <summary>目标状态。</summary>
        public string Status { get; set; } = "Processing";
        /// <summary>玩家可见回复。</summary>
        public string? AdminReply { get; set; }
        /// <summary>内部备注。</summary>
        public string? InternalNote { get; set; }
    }

    /// <summary>反馈图片上传结果。</summary>
    public class FeedbackAttachmentUploadResultDto : CreateFeedbackAttachmentDto
    {
        /// <summary>可访问的图片地址。</summary>
        public string Url { get; set; } = string.Empty;
    }
}
