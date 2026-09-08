namespace XXX.Application.DTOs
{
    /// <summary>
    /// 管理后台邮件列表项DTO。
    /// </summary>
    public class AdminMailListItemDto
    {
        /// <summary>
        /// 邮件ID。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 收件人GID（全服邮件为空）。
        /// </summary>
        public string? RecipientId { get; set; }

        /// <summary>
        /// 发件人类型。
        /// </summary>
        public string SenderType { get; set; } = string.Empty;

        /// <summary>
        /// 发件人名称。
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 邮件标题。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 是否全服邮件。
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// 是否有附件。
        /// </summary>
        public bool HasAttachments { get; set; }

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
    /// 管理后台邮件详情DTO。
    /// </summary>
    public class AdminMailDetailDto
    {
        /// <summary>
        /// 邮件ID。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 收件人GID。
        /// </summary>
        public string? RecipientId { get; set; }

        /// <summary>
        /// 发件人类型。
        /// </summary>
        public string SenderType { get; set; } = string.Empty;

        /// <summary>
        /// 发件人名称。
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 邮件标题。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 邮件正文。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 附件JSON。
        /// </summary>
        public string? AttachmentsJson { get; set; }

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
    /// 管理后台发送邮件请求DTO。
    /// </summary>
    public class AdminSendMailDto
    {
        /// <summary>
        /// 收件人GID（全服邮件时为空）。
        /// </summary>
        public string? RecipientId { get; set; }

        /// <summary>
        /// 是否全服邮件。
        /// </summary>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// 发件人名称。
        /// </summary>
        public string SenderName { get; set; } = "GM";

        /// <summary>
        /// 邮件标题。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 邮件正文。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 附件JSON。
        /// </summary>
        public string? AttachmentsJson { get; set; }
    }

    /// <summary>
    /// 附件JSON结构。
    /// </summary>
    public class MailAttachmentsDto
    {
        /// <summary>
        /// 金币数量。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 灵石数量。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 道具列表。
        /// </summary>
        public List<MailAttachmentItemDto>? Items { get; set; }

        /// <summary>
        /// 装备列表。
        /// </summary>
        public List<MailAttachmentEquipmentDto>? Equipment { get; set; }

        /// <summary>
        /// 称号列表。
        /// </summary>
        public List<MailAttachmentTitleDto>? Titles { get; set; }
    }

    /// <summary>
    /// 附件道具项。
    /// </summary>
    public class MailAttachmentItemDto
    {
        /// <summary>
        /// 道具模板ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 数量。
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 附件装备项。
    /// </summary>
    public class MailAttachmentEquipmentDto
    {
        /// <summary>
        /// 装备模板ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }
    }

    /// <summary>
    /// 附件称号项。
    /// </summary>
    public class MailAttachmentTitleDto
    {
        /// <summary>
        /// 称号ID。
        /// </summary>
        public string TitleId { get; set; } = string.Empty;
    }
}
