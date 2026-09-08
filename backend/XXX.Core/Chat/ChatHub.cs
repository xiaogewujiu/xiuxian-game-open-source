using Microsoft.AspNetCore.SignalR;
using XXX.Entity;

namespace XXX.Chat
{
    /// <summary>
    /// 聊天 SignalR Hub
    /// 处理客户端的实时通信
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// 连接时调用
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"[ChatHub] 客户端连接：{connectionId}");

            // TODO: 从客户端获取玩家信息进行验证
            // 这里简化处理，实际应该从认证信息中获取
            // PlayerConnect 需要玩家实体，这里先跳过
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开连接时调用
        /// </summary>
        /// <param name="exception">异常信息</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"[ChatHub] 客户端断开：{connectionId}");

            // 清理玩家连接
            ChatManager.PlayerDisconnect(connectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channelType">频道类型</param>
        /// <param name="content">消息内容</param>
        /// <returns>发送结果</returns>
        public async Task<ChatResult> SendMessage(ChannelType channelType, string content)
        {
            var connectionId = Context.ConnectionId;

            // 发送消息
            var result = ChatManager.SendMessage(connectionId, channelType, content);

            if (result.Success)
            {
                var message = result.Data as ChatMessage;

                // 广播消息到频道
                var channelId = ChatConfig.GetChannelId(channelType,
                    ChatManager.GetOnlinePlayer(connectionId)?.GuildId);

                await Clients.Group(channelId).SendAsync("ReceiveMessage", message);

                // 更新在线人数
                await Clients.All.SendAsync("OnlineCountChanged", ChatManager.GetOnlineCount());
            }

            return result;
        }

        /// <summary>
        /// 心跳
        /// </summary>
        public void Heartbeat()
        {
            var connectionId = Context.ConnectionId;
            ChatManager.PlayerHeartbeat(connectionId);
        }

        /// <summary>
        /// 加入公会频道
        /// </summary>
        /// <param name="guildId">公会ID</param>
        /// <returns>操作结果</returns>
        public async Task<ChatResult> JoinGuildChannel(string guildId)
        {
            var connectionId = Context.ConnectionId;
            var player = ChatManager.GetOnlinePlayer(connectionId);

            if (player == null)
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "玩家离线");
            }

            // 检查玩家是否在该公会
            if (player.GuildId != guildId)
            {
                return ChatResult.Fail(ChatResultCode.NotInGuild, "你不是该公会成员");
            }

            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);

            // 确保公会频道存在
            if (ChatManager.GetChannel(channelId) == null)
            {
                // TODO: 从公会系统获取公会名称
                ChatManager.CreateGuildChannel(guildId, "未知道");
            }

            // 加入SignalR组
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);

            // 加入频道
            player.JoinChannel(channelId);

            // 发送系统通知
            var noticeMessage = ChatMessage.CreateSystemMessage(
                $"{player.PlayerName} 加入了公会频道",
                ChannelType.Guild,
                channelId
            );
            ChatManager.GetChannel(channelId)?.AddMessage(noticeMessage);

            // 广播到公会频道
            await Clients.Group(channelId).SendAsync("ReceiveMessage", noticeMessage);

            // 更新在线人数
            ChatManager.ChatManager_UpdateChannelOnlineCount();

            return ChatResult.Ok("加入公会频道成功");
        }

        /// <summary>
        /// 离开公会频道
        /// </summary>
        /// <param name="guildId">公会ID</param>
        /// <returns>操作结果</returns>
        public async Task<ChatResult> LeaveGuildChannel(string guildId)
        {
            var connectionId = Context.ConnectionId;
            var player = ChatManager.GetOnlinePlayer(connectionId);

            if (player == null)
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "玩家离线");
            }

            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);

            // 离开SignalR组
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);

            // 离开频道
            player.LeaveChannel(channelId);

            // 发送系统通知
            var noticeMessage = ChatMessage.CreateSystemMessage(
                $"{player.PlayerName} 离开了公会频道",
                ChannelType.Guild,
                channelId
            );
            ChatManager.GetChannel(channelId)?.AddMessage(noticeMessage);

            // 广播到公会频道
            await Clients.Group(channelId).SendAsync("ReceiveMessage", noticeMessage);

            // 更新在线人数
            ChatManager.ChatManager_UpdateChannelOnlineCount();

            return ChatResult.Ok("离开公会频道成功");
        }

        /// <summary>
        /// 获取在线玩家列表
        /// </summary>
        /// <param name="channelType">频道类型（可选）</param>
        /// <returns>在线玩家列表</returns>
        public List<OnlinePlayer> GetOnlinePlayers(ChannelType? channelType = null)
        {
            var allPlayers = ChatManager.GetAllOnlinePlayers();

            if (!channelType.HasValue)
            {
                return allPlayers;
            }

            // 根据频道类型筛选
            return allPlayers.Where(p =>
            {
                var channelId = ChatConfig.GetChannelId(channelType.Value, p.GuildId);
                return p.IsInChannel(channelId);
            }).ToList();
        }

        /// <summary>
        /// 获取最近消息
        /// </summary>
        /// <param name="channelType">频道类型</param>
        /// <param name="guildId">公会ID（公会频道需要）</param>
        /// <param name="count">消息数量</param>
        /// <returns>消息列表</returns>
        public List<ChatMessage> GetRecentMessages(ChannelType channelType, string? guildId = null, int count = 50)
        {
            var channelId = ChatConfig.GetChannelId(channelType, guildId);
            return ChatManager.GetChannelMessages(channelId, count);
        }

        /// <summary>
        /// 玩家登录（从游戏服务器调用）
        /// </summary>
        /// <param name="connectionId">SignalR连接ID</param>
        /// <param name="player">玩家实体</param>
        public void PlayerLogin(string connectionId, UserEntity player)
        {
            var result = ChatManager.PlayerConnect(connectionId, player);

            if (result.Success)
            {
                // 广播玩家加入世界频道
                var noticeMessage = ChatMessage.CreateSystemMessage(
                    $"{player.Name} 加入了游戏",
                    ChannelType.World
                );
                ChatManager.GetChannel(ChatConfig.WorldChannelId)?.AddMessage(noticeMessage);

                // TODO: 广播到世界频道
                // await Clients.All.SendAsync("ReceiveMessage", noticeMessage);

                // 广播在线人数变化
                // await Clients.All.SendAsync("OnlineCountChanged", ChatManager.GetOnlineCount());
            }
        }

        /// <summary>
        /// 玩家登出（从游戏服务器调用）
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        public void PlayerLogout(string playerId)
        {
            var connectionId = ChatManager.GetConnectionIdByPlayerId(playerId);

            if (!string.IsNullOrEmpty(connectionId))
            {
                ChatManager.PlayerDisconnect(connectionId);
            }
        }

        /// <summary>
        /// 获取频道信息
        /// </summary>
        /// <param name="channelType">频道类型</param>
        /// <param name="guildId">公会ID（公会频道需要）</param>
        /// <returns>频道信息</returns>
        public object GetChannelInfo(ChannelType channelType, string? guildId = null)
        {
            var channelId = ChatConfig.GetChannelId(channelType, guildId);
            var channel = ChatManager.GetChannel(channelId);

            if (channel == null)
            {
                return new
                {
                    Exists = false,
                    ChannelId = channelId
                };
            }

            return new
            {
                Exists = true,
                ChannelId = channel.ChannelId,
                ChannelType = channel.ChannelType,
                ChannelName = channel.ChannelName,
                OnlineCount = channel.OnlineCount,
                MessageCount = channel.GetMessageCount()
            };
        }
    }
}
