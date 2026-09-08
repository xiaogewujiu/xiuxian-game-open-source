namespace XXX.Application.DTOs
{
    /// <summary>
    /// 聊天消息DTO
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者ID
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者名称
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 频道类型
        /// </summary>
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// 发送者称号名称
        /// </summary>
        public string? SenderTitle { get; set; }

        /// <summary>
        /// 发送者称号稀有度
        /// </summary>
        public string? SenderTitleRarity { get; set; }

        /// <summary>
        /// 发送者称号图标路径
        /// </summary>
        public string? SenderTitleIcon { get; set; }
    }

    /// <summary>
    /// 发送消息请求DTO
    /// </summary>
    public class SendMessageRequestDto
    {
        /// <summary>
        /// 频道类型
        /// </summary>
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 目标ID（私聊时使用）
        /// </summary>
        public string? TargetId { get; set; }
    }
}
