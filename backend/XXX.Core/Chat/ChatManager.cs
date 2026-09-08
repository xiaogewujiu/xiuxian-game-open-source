using XXX.Entity;

namespace XXX.Chat
{
    /// <summary>
    /// 聊天管理器
    /// 负责聊天系统的核心功能，包括玩家管理、消息处理、频道管理等
    /// </summary>
    public class ChatManager
    {
        /// <summary>
        /// 在线玩家列表
        /// Key: ConnectionId, Value: OnlinePlayer
        /// </summary>
        private static Dictionary<string, OnlinePlayer> onlinePlayers = [];

        /// <summary>
        /// 玩家ID到连接ID的映射
        /// Key: PlayerId, Value: ConnectionId
        /// </summary>
        private static Dictionary<string, string> playerConnectionMap = [];

        /// <summary>
        /// 聊天频道列表
        /// Key: ChannelId, Value: ChatChannel
        /// </summary>
        private static Dictionary<string, ChatChannel> channels = [];

        /// <summary>
        /// 每个玩家最近发送的消息（用于刷屏检测）
        /// Key: PlayerId, Value: 消息队列
        /// </summary>
        private static Dictionary<string, Queue<ChatMessage>> playerMessageHistory =
            [];

        /// <summary>
        /// 初始化聊天系统
        /// </summary>
        public static void Initialize()
        {
            onlinePlayers.Clear();
            playerConnectionMap.Clear();
            playerMessageHistory.Clear();
            channels.Clear();

            // 创建世界频道
            CreateWorldChannel();
        }

        /// <summary>
        /// 创建世界频道
        /// </summary>
        private static void CreateWorldChannel()
        {
            var worldChannel = new ChatChannel
            {
                ChannelId = ChatConfig.WorldChannelId,
                ChannelType = ChannelType.World,
                ChannelName = "世界频道",
                MaxMessages = ChatConfig.WorldChannelHistoryLimit,
                OnlineCount = 0
            };

            channels[worldChannel.ChannelId] = worldChannel;
        }

        /// <summary>
        /// 创建公会频道
        /// </summary>
        /// <param name="guildId">公会ID</param>
        /// <param name="guildName">公会名称</param>
        public static void CreateGuildChannel(string guildId, string guildName)
        {
            if (string.IsNullOrEmpty(guildId))
            {
                return;
            }

            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);

            if (channels.ContainsKey(channelId))
            {
                return; // 频道已存在
            }

            var guildChannel = new ChatChannel
            {
                ChannelId = channelId,
                ChannelType = ChannelType.Guild,
                ChannelName = $"{guildName}公会",
                MaxMessages = ChatConfig.GuildChannelHistoryLimit,
                OnlineCount = 0
            };

            channels[channelId] = guildChannel;
        }

        /// <summary>
        /// 移除公会频道
        /// </summary>
        /// <param name="guildId">公会ID</param>
        public static void RemoveGuildChannel(string guildId)
        {
            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);
            channels.Remove(channelId);
        }

        /// <summary>
        /// 玩家连接
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        /// <param name="player">玩家实体</param>
        /// <returns>连接结果</returns>
        public static ChatResult PlayerConnect(string connectionId, UserEntity player)
        {
            if (string.IsNullOrEmpty(connectionId) || player == null)
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "连接失败");
            }

            // 如果玩家已有连接，先断开旧连接
            if (playerConnectionMap.ContainsKey(player.GID))
            {
                PlayerDisconnect(playerConnectionMap[player.GID]);
            }

            // 创建在线玩家
            var onlinePlayer = new OnlinePlayer
            {
                PlayerId = player.GID,
                PlayerName = player.Name,
                Level = player.Level,
                GuildId = player.GuildId ?? string.Empty,
                ConnectionId = connectionId,
                ConnectTime = DateTime.Now,
                LastHeartbeat = DateTime.Now
            };

            onlinePlayers[connectionId] = onlinePlayer;
            playerConnectionMap[player.GID] = connectionId;

            // 初始化消息历史
            if (!playerMessageHistory.ContainsKey(player.GID))
            {
                playerMessageHistory[player.GID] = new Queue<ChatMessage>();
            }

            // 自动加入世界频道
            onlinePlayer.JoinChannel(ChatConfig.WorldChannelId);

            // 如果有公会，加入公会频道
            if (!string.IsNullOrEmpty(player.GuildId))
            {
                onlinePlayer.JoinChannel(ChatConfig.GetChannelId(ChannelType.Guild, player.GuildId));

                // 确保公会频道存在
                var guildChannelId = ChatConfig.GetChannelId(ChannelType.Guild, player.GuildId);
                if (!channels.ContainsKey(guildChannelId))
                {
                    // TODO: 从公会系统获取公会名称
                    CreateGuildChannel(player.GuildId, "未知道");
                }
            }

            // 更新频道在线人数
            UpdateChannelOnlineCount();

            return ChatResult.Ok("连接成功", onlinePlayer);
        }

        /// <summary>
        /// 玩家断开连接
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        public static void PlayerDisconnect(string connectionId)
        {
            if (!onlinePlayers.ContainsKey(connectionId))
            {
                return;
            }

            var player = onlinePlayers[connectionId];

            // 从所有频道移除
            foreach (var channelId in player.CurrentChannels)
            {
                // TODO: 通知频道内其他玩家玩家离开
            }

            // 移除记录
            onlinePlayers.Remove(connectionId);
            playerConnectionMap.Remove(player.PlayerId);
            playerMessageHistory.Remove(player.PlayerId);

            // 更新频道在线人数
            UpdateChannelOnlineCount();

            // 如果公会频道没人了，可以考虑删除（可选）
        }

        /// <summary>
        /// 玩家心跳
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        public static void PlayerHeartbeat(string connectionId)
        {
            if (onlinePlayers.ContainsKey(connectionId))
            {
                onlinePlayers[connectionId].UpdateHeartbeat();
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        /// <param name="channelType">频道类型</param>
        /// <param name="content">消息内容</param>
        /// <returns>发送结果</returns>
        public static ChatResult SendMessage(string connectionId, ChannelType channelType, string content)
        {
            // 验证频道类型
            if (!ChatConfig.IsValidChannel(channelType))
            {
                return ChatResult.Fail(ChatResultCode.InvalidChannel, "无效的频道类型");
            }

            // 获取玩家
            if (!onlinePlayers.ContainsKey(connectionId))
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "玩家离线");
            }

            var player = onlinePlayers[connectionId];
            var channelId = ChatConfig.GetChannelId(channelType, player.GuildId);

            // 验证频道存在
            if (!channels.ContainsKey(channelId))
            {
                return ChatResult.Fail(ChatResultCode.ChannelNotExist, "频道不存在");
            }

            // 公会频道检查
            if (channelType == ChannelType.Guild)
            {
                if (string.IsNullOrEmpty(player.GuildId))
                {
                    return ChatResult.Fail(ChatResultCode.NotInGuild, "你还没有加入公会");
                }
            }

            // 检查是否在频道中
            if (!player.IsInChannel(channelId))
            {
                return ChatResult.Fail(ChatResultCode.ChannelNotExist, "未加入该频道");
            }

            // 检查冷却
            int cooldownRemaining = player.GetCooldownRemaining(channelId);
            if (cooldownRemaining > 0)
            {
                return ChatResult.Fail(ChatResultCode.InCooldown, $"冷却中，还需{cooldownRemaining}秒");
            }

            // 验证消息
            var validationResult = ChatFilter.ValidateMessage(content);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 检查刷屏
            var messageHistory = playerMessageHistory[player.PlayerId];
            if (ChatFilter.CheckSpam(player.PlayerId, messageHistory))
            {
                return ChatResult.Fail(ChatResultCode.InCooldown, "发送消息过快，请稍后再试");
            }

            // 获取过滤后的内容
            string filteredContent = validationResult.Data as string ?? content;

            // 创建消息
            var message = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                ChannelType = channelType,
                ChannelId = channelId,
                SenderId = player.PlayerId,
                SenderName = player.PlayerName,
                SenderLevel = player.Level,
                SenderGuildId = player.GuildId,
                SenderGuildName = "", // TODO: 从公会系统获取
                Content = filteredContent,
                MessageType = MessageType.Text,
                SendTime = DateTime.Now
            };

            // 保存消息到频道
            channels[channelId].AddMessage(message);

            // 记录到玩家消息历史
            messageHistory.Enqueue(message);
            if (messageHistory.Count > 10) // 只保留最近10条
            {
                messageHistory.Dequeue();
            }

            // 记录发送时间
            player.RecordMessageTime(channelId);

            return ChatResult.Ok("发送成功", message);
        }

        /// <summary>
        /// 发送系统消息
        /// </summary>
        /// <param name="channelType">频道类型</param>
        /// <param name="content">消息内容</param>
        /// <param name="channelId">频道ID（可选，用于公会频道）</param>
        /// <returns>发送结果</returns>
        public static ChatResult SendSystemMessage(ChannelType channelType, string content, string? channelId = null)
        {
            if (!ChatConfig.IsValidChannel(channelType))
            {
                return ChatResult.Fail(ChatResultCode.InvalidChannel, "无效的频道类型");
            }

            // 确定频道ID
            string targetChannelId = channelId ?? ChatConfig.GetChannelId(channelType);

            // 验证频道存在
            if (!channels.ContainsKey(targetChannelId))
            {
                return ChatResult.Fail(ChatResultCode.ChannelNotExist, "频道不存在");
            }

            // 创建系统消息
            var message = ChatMessage.CreateSystemMessage(content, channelType, targetChannelId);

            // 保存消息
            channels[targetChannelId].AddMessage(message);

            return ChatResult.Ok("发送成功", message);
        }

        /// <summary>
        /// 发送公告消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="channelType">频道类型</param>
        /// <returns>发送结果</returns>
        public static ChatResult SendNoticeMessage(string content, ChannelType channelType = ChannelType.World)
        {
            if (!ChatConfig.IsValidChannel(channelType))
            {
                return ChatResult.Fail(ChatResultCode.InvalidChannel, "无效的频道类型");
            }

            var channelId = ChatConfig.GetChannelId(channelType);

            // 验证频道存在
            if (!channels.ContainsKey(channelId))
            {
                return ChatResult.Fail(ChatResultCode.ChannelNotExist, "频道不存在");
            }

            // 创建公告消息
            var message = ChatMessage.CreateNoticeMessage(content, channelType, channelId);

            // 保存消息
            channels[channelId].AddMessage(message);

            return ChatResult.Ok("发送成功", message);
        }

        /// <summary>
        /// 获取频道
        /// </summary>
        /// <param name="channelId">频道ID</param>
        /// <returns>频道对象</returns>
        public static ChatChannel? GetChannel(string channelId)
        {
            return channels.ContainsKey(channelId) ? channels[channelId] : null;
        }

        /// <summary>
        /// 获取所有频道
        /// </summary>
        /// <returns>频道列表</returns>
        public static List<ChatChannel> GetAllChannels()
        {
            return channels.Values.ToList();
        }

        /// <summary>
        /// 获取在线玩家
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        /// <returns>在线玩家</returns>
        public static OnlinePlayer? GetOnlinePlayer(string connectionId)
        {
            return onlinePlayers.ContainsKey(connectionId) ? onlinePlayers[connectionId] : null;
        }

        /// <summary>
        /// 根据玩家ID获取在线玩家
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>在线玩家</returns>
        public static OnlinePlayer? GetOnlinePlayerByPlayerId(string playerId)
        {
            if (!playerConnectionMap.ContainsKey(playerId))
            {
                return null;
            }

            var connectionId = playerConnectionMap[playerId];
            return GetOnlinePlayer(connectionId);
        }

        /// <summary>
        /// 获取所有在线玩家
        /// </summary>
        /// <returns>在线玩家列表</returns>
        public static List<OnlinePlayer> GetAllOnlinePlayers()
        {
            return onlinePlayers.Values.ToList();
        }

        /// <summary>
        /// 获取在线人数
        /// </summary>
        /// <returns>在线人数</returns>
        public static int GetOnlineCount()
        {
            return onlinePlayers.Count;
        }

        /// <summary>
        /// 获取频道的最近消息
        /// </summary>
        /// <param name="channelId">频道ID</param>
        /// <param name="count">数量</param>
        /// <returns>消息列表</returns>
        public static List<ChatMessage> GetChannelMessages(string channelId, int count = 50)
        {
            if (!channels.ContainsKey(channelId))
            {
                return [];
            }

            return channels[channelId].GetRecentMessages(count);
        }

        /// <summary>
        /// 清理超时玩家
        /// </summary>
        /// <returns>清理的玩家数量</returns>
        public static int CleanupTimeoutPlayers()
        {
            var timeoutConnections = new List<string>();

            foreach (var kvp in onlinePlayers)
            {
                if (kvp.Value.IsTimeout())
                {
                    timeoutConnections.Add(kvp.Key);
                }
            }

            foreach (var connectionId in timeoutConnections)
            {
                PlayerDisconnect(connectionId);
            }

            return timeoutConnections.Count;
        }

        /// <summary>
        /// 更新频道在线人数
        /// </summary>
        public static void UpdateChannelOnlineCount()
        {
            foreach (var channel in channels.Values)
            {
                int count = 0;

                if (channel.ChannelType == ChannelType.World)
                {
                    // 世界频道：所有在线玩家
                    count = onlinePlayers.Count;
                }
                else if (channel.ChannelType == ChannelType.Guild)
                {
                    // 公会频道：该公会的在线玩家
                    count = onlinePlayers.Values.Count(p => p.GuildId == channel.ChannelId.Replace("guild_", ""));
                }

                channel.OnlineCount = count;
            }
        }

        /// <summary>
        /// 获取玩家可以访问的频道列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>频道列表</returns>
        public static List<ChatChannel> GetAvailableChannels(string playerId)
        {
            var result = new List<ChatChannel>();

            foreach (var channel in channels.Values)
            {
                if (channel.ChannelType == ChannelType.World)
                {
                    // 世界频道所有人可访问
                    result.Add(channel);
                }
                else if (channel.ChannelType == ChannelType.Guild)
                {
                    // 公会频道需要公会成员
                    var player = GetOnlinePlayerByPlayerId(playerId);
                    if (player != null && !string.IsNullOrEmpty(player.GuildId))
                    {
                        var guildId = channel.ChannelId.Replace("guild_", "");
                        if (player.GuildId == guildId)
                        {
                            result.Add(channel);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取玩家连接映射（供ChatHub使用）
        /// </summary>
        public static Dictionary<string, string> GetPlayerConnectionMap()
        {
            return new Dictionary<string, string>(playerConnectionMap);
        }

        /// <summary>
        /// 更新频道在线人数（供ChatHub使用）
        /// </summary>
        public static void ChatManager_UpdateChannelOnlineCount()
        {
            UpdateChannelOnlineCount();
        }

        /// <summary>
        /// 暴露 playerConnectionMap 供 ChatHub 使用
        /// </summary>
        public static string? GetConnectionIdByPlayerId(string playerId)
        {
            return playerConnectionMap.ContainsKey(playerId) ? playerConnectionMap[playerId] : null;
        }
    }
}
