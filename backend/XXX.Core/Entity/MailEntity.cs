using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 邮件消息实体。
    /// </summary>
    [SugarTable("MailMessages")]
    public class MailMessageEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 收件人GID（全服邮件时为空）。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, IndexGroupNameList = new[] { "idx_recipient" })]
        public string? RecipientId { get; set; }

        /// <summary>
        /// 发件人类型（System/GM）。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string SenderType { get; set; } = "System";

        /// <summary>
        /// 发件人名称。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string SenderName { get; set; } = "系统";

        /// <summary>
        /// 邮件标题。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 邮件正文。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 附件JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? AttachmentsJson { get; set; }

        /// <summary>
        /// 是否已读。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsRead { get; set; }

        /// <summary>
        /// 附件是否已领取。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsClaimed { get; set; }

        /// <summary>
        /// 是否全服邮件。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsGlobal { get; set; }

        /// <summary>
        /// 发送时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 过期时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime ExpireAt { get; set; } = DateTime.Now.AddDays(30);
    }

    /// <summary>
    /// 全服邮件领取记录实体。
    /// </summary>
    [SugarTable("MailGlobalClaimRecords")]
    public class MailGlobalClaimRecordEntity
    {
        /// <summary>
        /// 主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 全服邮件ID。
        /// </summary>
        [SugarColumn(IsNullable = false, IndexGroupNameList = new[] { "idx_mail_player" })]
        public long MailId { get; set; }

        /// <summary>
        /// 玩家GID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_mail_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 领取时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime ClaimedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已读。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsRead { get; set; }

        /// <summary>
        /// 是否已领取附件。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool HasClaimed { get; set; }
    }
}
