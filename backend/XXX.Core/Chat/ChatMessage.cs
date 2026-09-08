namespace XXX.Chat
{
    /// <summary>
    /// 聊天消息实体。
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 消息唯一 ID。
        /// </summary>
        public string MessageId { get; set; } = string.Empty;

        /// <summary>
        /// 频道类型。
        /// </summary>
        public ChannelType ChannelType { get; set; }

        /// <summary>
        /// 频道 ID。
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者玩家 ID。
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者名称。
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// 发送者等级。
        /// </summary>
        public int SenderLevel { get; set; }

        /// <summary>
        /// 发送者宗门 ID。
        /// </summary>
        public string SenderGuildId { get; set; } = string.Empty;

        /// <summary>
        /// 发送者宗门名称。
        /// </summary>
        public string SenderGuildName { get; set; } = string.Empty;

        /// <summary>
        /// 消息正文。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型。
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// 发送时间。
        /// </summary>
        public DateTime SendTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 扩展数据。
        /// </summary>
        public Dictionary<string, string> ExtraData { get; set; } = [];

        /// <summary>
        /// 是否为系统类消息。
        /// </summary>
        public bool IsSystemMessage => MessageType == MessageType.System || MessageType == MessageType.Notice;

        /// <summary>
        /// 获取用于界面显示的消息文本。
        /// </summary>
        public string GetDisplayText()
        {
            if (IsSystemMessage)
            {
                return $"【系统】{Content}";
            }

            var guildPrefix = !string.IsNullOrEmpty(SenderGuildName)
                ? $"<{SenderGuildName}> "
                : string.Empty;

            return $"{guildPrefix}[Lv.{SenderLevel}] {SenderName}：{Content}";
        }

        /// <summary>
        /// 获取用于日志输出的摘要文本。
        /// </summary>
        public string GetSummary()
        {
            return $"[{ChannelType}] {SenderName}: {Content}";
        }

        /// <summary>
        /// 创建一条系统消息。
        /// </summary>
        public static ChatMessage CreateSystemMessage(string content, ChannelType channelType, string? channelId = null)
        {
            return new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                ChannelType = channelType,
                ChannelId = channelId ?? ChatConfig.GetChannelId(channelType),
                SenderId = "system",
                SenderName = "系统",
                SenderLevel = 0,
                Content = content,
                MessageType = MessageType.System,
                SendTime = DateTime.Now
            };
        }

        /// <summary>
        /// 创建一条公告消息。
        /// </summary>
        public static ChatMessage CreateNoticeMessage(string content, ChannelType channelType, string? channelId = null)
        {
            return new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                ChannelType = channelType,
                ChannelId = channelId ?? ChatConfig.GetChannelId(channelType),
                SenderId = "system",
                SenderName = "公告",
                SenderLevel = 0,
                Content = content,
                MessageType = MessageType.Notice,
                SendTime = DateTime.Now
            };
        }

        /// <summary>
        /// 复制当前消息，常用于广播前生成安全副本。
        /// </summary>
        public ChatMessage Clone()
        {
            return new ChatMessage
            {
                MessageId = MessageId,
                ChannelType = ChannelType,
                ChannelId = ChannelId,
                SenderId = SenderId,
                SenderName = SenderName,
                SenderLevel = SenderLevel,
                SenderGuildId = SenderGuildId,
                SenderGuildName = SenderGuildName,
                Content = Content,
                MessageType = MessageType,
                SendTime = SendTime,
                ExtraData = new Dictionary<string, string>(ExtraData)
            };
        }
    }

    /// <summary>
    /// 在线玩家信息。
    /// </summary>
    public class OnlinePlayer
    {
        /// <summary>
        /// 玩家 ID。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 宗门 ID。
        /// </summary>
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string GuildName { get; set; } = string.Empty;

        /// <summary>
        /// SignalR 连接 ID。
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 最后一次心跳时间。
        /// </summary>
        public DateTime LastHeartbeat { get; set; } = DateTime.Now;

        /// <summary>
        /// 连接建立时间。
        /// </summary>
        public DateTime ConnectTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 当前所在频道 ID 列表。
        /// </summary>
        public List<string> CurrentChannels { get; set; } = [];

        /// <summary>
        /// 每个频道最后一次发言时间，用于冷却检查。
        /// </summary>
        private readonly Dictionary<string, DateTime> _lastMessageTime = [];

        /// <summary>
        /// 判断是否心跳超时。
        /// </summary>
        public bool IsTimeout()
        {
            var elapsed = DateTime.Now - LastHeartbeat;
            return elapsed.TotalSeconds > ChatConfig.HeartbeatTimeoutSeconds;
        }

        /// <summary>
        /// 刷新心跳时间。
        /// </summary>
        public void UpdateHeartbeat()
        {
            LastHeartbeat = DateTime.Now;
        }

        /// <summary>
        /// 加入频道。
        /// </summary>
        public void JoinChannel(string channelId)
        {
            if (!CurrentChannels.Contains(channelId))
            {
                CurrentChannels.Add(channelId);
            }
        }

        /// <summary>
        /// 离开频道。
        /// </summary>
        public void LeaveChannel(string channelId)
        {
            CurrentChannels.Remove(channelId);
            _lastMessageTime.Remove(channelId);
        }

        /// <summary>
        /// 判断当前是否在指定频道中。
        /// </summary>
        public bool IsInChannel(string channelId)
        {
            return CurrentChannels.Contains(channelId);
        }

        /// <summary>
        /// 获取某个频道剩余的发言冷却时间。
        /// </summary>
        public int GetCooldownRemaining(string channelId)
        {
            if (!_lastMessageTime.TryGetValue(channelId, out var lastTime))
            {
                return 0;
            }

            var elapsed = DateTime.Now - lastTime;
            var remaining = ChatConfig.ChatCooldownSeconds - (int)elapsed.TotalSeconds;
            return Math.Max(0, remaining);
        }

        /// <summary>
        /// 记录一次发言时间。
        /// </summary>
        public void RecordMessageTime(string channelId)
        {
            _lastMessageTime[channelId] = DateTime.Now;
        }

        /// <summary>
        /// 获取在线时长，单位秒。
        /// </summary>
        public int GetOnlineSeconds()
        {
            return (int)(DateTime.Now - ConnectTime).TotalSeconds;
        }

        /// <summary>
        /// 获取在线时长的可读文本。
        /// </summary>
        public string GetOnlineDurationText()
        {
            var seconds = GetOnlineSeconds();

            if (seconds < 60)
            {
                return $"{seconds}秒";
            }

            if (seconds < 3600)
            {
                return $"{seconds / 60}分钟";
            }

            var hours = seconds / 3600;
            var minutes = (seconds % 3600) / 60;
            return $"{hours}小时{minutes}分钟";
        }
    }

    /// <summary>
    /// 聊天频道信息。
    /// </summary>
    public class ChatChannel
    {
        /// <summary>
        /// 频道 ID。
        /// </summary>
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// 频道类型。
        /// </summary>
        public ChannelType ChannelType { get; set; }

        /// <summary>
        /// 频道名称。
        /// </summary>
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 当前频道内的消息历史。
        /// </summary>
        private readonly LinkedList<ChatMessage> _messages = new();

        /// <summary>
        /// 最大消息保留条数。
        /// </summary>
        public int MaxMessages { get; set; }

        /// <summary>
        /// 当前在线人数。
        /// </summary>
        public int OnlineCount { get; set; }

        /// <summary>
        /// 添加一条消息到频道历史。
        /// </summary>
        public void AddMessage(ChatMessage message)
        {
            _messages.AddLast(message);

            var maxMessages = MaxMessages > 0 ? MaxMessages : ChatConfig.GetHistoryLimit(ChannelType);
            while (_messages.Count > maxMessages)
            {
                _messages.RemoveFirst();
            }
        }

        /// <summary>
        /// 获取最近的若干条消息。
        /// </summary>
        public List<ChatMessage> GetRecentMessages(int count)
        {
            return _messages.TakeLast(count).ToList();
        }

        /// <summary>
        /// 获取全部消息。
        /// </summary>
        public List<ChatMessage> GetAllMessages()
        {
            return _messages.ToList();
        }

        /// <summary>
        /// 清空频道消息历史。
        /// </summary>
        public void ClearMessages()
        {
            _messages.Clear();
        }

        /// <summary>
        /// 获取当前消息总数。
        /// </summary>
        public int GetMessageCount()
        {
            return _messages.Count;
        }
    }
}
