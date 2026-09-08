namespace XXX.Chat
{
    /// <summary>
    /// 聊天频道类型。
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// 世界频道，所有玩家可见。
        /// </summary>
        World,

        /// <summary>
        /// 宗门频道，仅同宗门成员可见。
        /// </summary>
        Guild
    }

    /// <summary>
    /// 聊天消息类型。
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 普通文本消息。
        /// </summary>
        Text,

        /// <summary>
        /// 系统消息。
        /// </summary>
        System,

        /// <summary>
        /// 公告消息。
        /// </summary>
        Notice
    }

    /// <summary>
    /// 聊天系统配置。
    /// </summary>
    public static class ChatConfig
    {
        /// <summary>
        /// 世界频道固定组名。
        /// </summary>
        public const string WorldChannelId = "world";

        /// <summary>
        /// 发送聊天的统一冷却时间，单位秒。
        /// </summary>
        public const int ChatCooldownSeconds = 5;

        /// <summary>
        /// 单条消息的最大长度。
        /// </summary>
        public const int MaxMessageLength = 200;

        /// <summary>
        /// 世界频道历史保留条数。
        /// </summary>
        public const int WorldChannelHistoryLimit = 100;

        /// <summary>
        /// 宗门频道历史保留条数。
        /// </summary>
        public const int GuildChannelHistoryLimit = 50;

        /// <summary>
        /// 心跳超时阈值，单位秒。
        /// </summary>
        public const int HeartbeatTimeoutSeconds = 60;

        /// <summary>
        /// 根据频道类型返回历史保留条数。
        /// </summary>
        public static int GetHistoryLimit(ChannelType channelType)
        {
            return channelType switch
            {
                ChannelType.World => WorldChannelHistoryLimit,
                ChannelType.Guild => GuildChannelHistoryLimit,
                _ => 50
            };
        }

        /// <summary>
        /// 根据频道类型生成频道 ID。
        /// </summary>
        public static string GetChannelId(ChannelType channelType, string? guildId = null)
        {
            return channelType switch
            {
                ChannelType.World => WorldChannelId,
                ChannelType.Guild => $"guild_{guildId ?? string.Empty}",
                _ => "unknown"
            };
        }

        /// <summary>
        /// 判断是否为当前支持的频道类型。
        /// </summary>
        public static bool IsValidChannel(ChannelType channelType)
        {
            return channelType == ChannelType.World || channelType == ChannelType.Guild;
        }
    }

    /// <summary>
    /// 聊天操作结果。
    /// </summary>
    public class ChatResult
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
        /// 结果代码。
        /// </summary>
        public ChatResultCode Code { get; set; }

        /// <summary>
        /// 额外数据。
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// 创建成功结果。
        /// </summary>
        public static ChatResult Ok(string message = "", object? data = null)
        {
            return new ChatResult
            {
                Success = true,
                Message = message,
                Code = ChatResultCode.Success,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        public static ChatResult Fail(ChatResultCode code, string message)
        {
            return new ChatResult
            {
                Success = false,
                Message = message,
                Code = code
            };
        }
    }

    /// <summary>
    /// 聊天结果代码。
    /// </summary>
    public enum ChatResultCode
    {
        /// <summary>
        /// 成功。
        /// </summary>
        Success,

        /// <summary>
        /// 无效频道。
        /// </summary>
        InvalidChannel,

        /// <summary>
        /// 消息为空。
        /// </summary>
        EmptyMessage,

        /// <summary>
        /// 消息过长。
        /// </summary>
        MessageTooLong,

        /// <summary>
        /// 包含敏感词。
        /// </summary>
        ContainsSensitiveWord,

        /// <summary>
        /// 冷却中。
        /// </summary>
        InCooldown,

        /// <summary>
        /// 未加入宗门。
        /// </summary>
        NotInGuild,

        /// <summary>
        /// 频道不存在。
        /// </summary>
        ChannelNotExist,

        /// <summary>
        /// 玩家离线。
        /// </summary>
        PlayerOffline
    }
}
