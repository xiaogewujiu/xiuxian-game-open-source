using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 中文注释：
    /// 聊天消息持久化实体。
    /// 当前项目原本只有聊天 DTO，没有真实数据库表，导致世界聊天刷新后消息全部消失。
    /// 这里单独落一张最小可用表，只存频道、发送者、内容和时间，
    /// 先把“玩家发言可落库、切页后还能看到历史记录”这条真实链路跑通。
    /// </summary>
    [SugarTable("ChatMessages")]
    public class ChatMessageEntity
    {
        /// <summary>
        /// 消息唯一 ID。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string MessageId { get; set; } = string.Empty;

        /// <summary>
        /// 频道类型。
        /// world / sect / system
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false)]
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>
        /// 发送者玩家 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者名称。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 消息正文。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 发送时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime SendTime { get; set; } = DateTime.Now;
    }
}
