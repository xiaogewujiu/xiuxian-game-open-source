namespace XXX.Application.DTOs
{
    /// <summary>
    /// 邮件列表项DTO。
    /// </summary>
    public class MailListItemDto
    {
        /// <summary>
        /// 邮件ID。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 邮件标题。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 发件人名称。
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 发件人类型。
        /// </summary>
        public string SenderType { get; set; } = string.Empty;

        /// <summary>
        /// 是否已读。
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 附件是否已领取。
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// 是否有附件。
        /// </summary>
        public bool HasAttachments { get; set; }

        /// <summary>
        /// 是否全服邮件。
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// 发送时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 过期时间。
        /// </summary>
        public DateTime ExpireAt { get; set; }
    }

    /// <summary>
    /// 邮件详情DTO。
    /// </summary>
    public class MailDetailDto
    {
        /// <summary>
        /// 邮件ID。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 邮件标题。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 邮件正文。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 发件人名称。
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 发件人类型。
        /// </summary>
        public string SenderType { get; set; } = string.Empty;

        /// <summary>
        /// 附件JSON（原始）。
        /// </summary>
        public string? AttachmentsJson { get; set; }

        /// <summary>
        /// 附件JSON（含名称，用于前端展示）。
        /// </summary>
        public string? ResolvedAttachmentsJson { get; set; }

        /// <summary>
        /// 是否已读。
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 附件是否已领取。
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// 是否有附件。
        /// </summary>
        public bool HasAttachments { get; set; }

        /// <summary>
        /// 是否全服邮件。
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// 发送时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 过期时间。
        /// </summary>
        public DateTime ExpireAt { get; set; }
    }

    /// <summary>
    /// 邮件附件领取结果DTO。
    /// </summary>
    public class MailClaimResultDto
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获得的奖励列表。
        /// </summary>
        public List<RewardItemDto> Rewards { get; set; } = new();
    }

    /// <summary>
    /// 邮件分页结果DTO。
    /// </summary>
    public class MailPagedResultDto
    {
        /// <summary>
        /// 邮件列表。
        /// </summary>
        public List<MailListItemDto> Items { get; set; } = new();

        /// <summary>
        /// 总数。
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 未读数量。
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// 当前页码。
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页大小。
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 一键领取结果DTO。
    /// </summary>
    public class MailClaimAllResultDto
    {
        /// <summary>
        /// 成功领取的邮件数。
        /// </summary>
        public int ClaimedCount { get; set; }

        /// <summary>
        /// 获得的所有奖励。
        /// </summary>
        public List<RewardItemDto> TotalRewards { get; set; } = new();

        /// <summary>
        /// 失败的邮件ID和原因。
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
