using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Chat;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Hubs
{
    /// <summary>
    /// 聊天实时通信 Hub。
    /// </summary>
    /// <remarks>
    /// 与旧版 Core 层 ChatHub 不同，这里在连接时直接查数据库获取玩家实时状态，
    /// 只信任 JWT 里的玩家 ID，不把等级、宗门等运行态字段固化进令牌。
    /// </remarks>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<ChatHub> _logger;

        /// <summary>
        /// 初始化聊天 Hub。
        /// </summary>
        public ChatHub(IChatService chatService, IRepository<UserEntity> userRepository, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 连接建立时加载玩家实时资料并加入默认频道。
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var player = await GetCurrentPlayerAsync();
            if (player == null)
            {
                _logger.LogWarning("Chat hub connection aborted because player claims or player record is missing. connectionId={ConnectionId}", Context.ConnectionId);
                Context.Abort();
                return;
            }

            await CleanupPreviousConnectionAsync(player.GID);

            var result = ChatManager.PlayerConnect(Context.ConnectionId, player);
            if (!result.Success)
            {
                _logger.LogWarning("Chat hub failed to bind online player state. connectionId={ConnectionId}, playerId={PlayerId}", Context.ConnectionId, player.GID);
                Context.Abort();
                return;
            }

            // 中文注释：
            // ChatManager 维护的是内存频道归属；
            // SignalR 这边还要把连接加入对应组，广播消息才能真正推送到客户端。
            await Groups.AddToGroupAsync(Context.ConnectionId, ChatConfig.WorldChannelId);
            if (!string.IsNullOrWhiteSpace(player.GuildId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatConfig.GetChannelId(ChannelType.Guild, player.GuildId));
            }

            await Clients.All.SendAsync("OnlineCountChanged", ChatManager.GetOnlineCount());
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 连接断开时清理在线态。
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ChatManager.PlayerDisconnect(Context.ConnectionId);
            await Clients.All.SendAsync("OnlineCountChanged", ChatManager.GetOnlineCount());
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 发送聊天消息。
        /// </summary>
        public async Task<ChatMessageDto> SendMessage(string channelType, string content)
        {
            var playerId = Context.User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new HubException("未登录");
            }

            var player = ChatManager.GetOnlinePlayer(Context.ConnectionId);
            if (player == null)
            {
                throw new HubException("聊天连接未初始化");
            }

            var dto = await _chatService.SendMessageAsync(playerId, new SendMessageRequestDto
            {
                ChannelType = channelType,
                Content = content
            });

            var groupName = ResolveBroadcastGroup(dto.ChannelType, player.GuildId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", dto);
            return dto;
        }

        /// <summary>
        /// 心跳保活。
        /// </summary>
        public void Heartbeat()
        {
            ChatManager.PlayerHeartbeat(Context.ConnectionId);
        }

        /// <summary>
        /// 主动加入宗门频道。
        /// </summary>
        public async Task<ChatResult> JoinGuildChannel(string guildId)
        {
            var player = ChatManager.GetOnlinePlayer(Context.ConnectionId);
            if (player == null)
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "玩家离线");
            }

            if (!string.Equals(player.GuildId, guildId, StringComparison.Ordinal))
            {
                return ChatResult.Fail(ChatResultCode.NotInGuild, "你不是该宗门成员");
            }

            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);
            if (ChatManager.GetChannel(channelId) == null)
            {
                ChatManager.CreateGuildChannel(guildId, "未知道");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            player.JoinChannel(channelId);
            ChatManager.ChatManager_UpdateChannelOnlineCount();

            return ChatResult.Ok("加入宗门频道成功");
        }

        /// <summary>
        /// 主动离开宗门频道。
        /// </summary>
        public async Task<ChatResult> LeaveGuildChannel(string guildId)
        {
            var player = ChatManager.GetOnlinePlayer(Context.ConnectionId);
            if (player == null)
            {
                return ChatResult.Fail(ChatResultCode.PlayerOffline, "玩家离线");
            }

            var channelId = ChatConfig.GetChannelId(ChannelType.Guild, guildId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
            player.LeaveChannel(channelId);
            ChatManager.ChatManager_UpdateChannelOnlineCount();

            return ChatResult.Ok("离开宗门频道成功");
        }

        /// <summary>
        /// 获取在线玩家列表。
        /// </summary>
        public List<OnlinePlayer> GetOnlinePlayers(ChannelType? channelType = null)
        {
            var allPlayers = ChatManager.GetAllOnlinePlayers();
            if (!channelType.HasValue)
            {
                return allPlayers;
            }

            return allPlayers
                .Where(player => player.IsInChannel(ChatConfig.GetChannelId(channelType.Value, player.GuildId)))
                .ToList();
        }

        /// <summary>
        /// 获取最近消息。
        /// </summary>
        public List<ChatMessage> GetRecentMessages(ChannelType channelType, string? guildId = null, int count = 50)
        {
            var channelId = ResolveChannelId(channelType, guildId);
            return ChatManager.GetChannelMessages(channelId, count);
        }

        /// <summary>
        /// 获取频道摘要信息。
        /// </summary>
        public object GetChannelInfo(ChannelType channelType, string? guildId = null)
        {
            var channelId = ResolveChannelId(channelType, guildId);
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

        /// <summary>
        /// 从当前鉴权上下文读取真实玩家。
        /// </summary>
        private async Task<UserEntity?> GetCurrentPlayerAsync()
        {
            var playerId = Context.User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                return null;
            }

            return player;
        }

        /// <summary>
        /// 如果同一账号已有旧连接，先从组里移除并通知旧连接下线。
        /// </summary>
        private async Task CleanupPreviousConnectionAsync(string playerId)
        {
            var previous = ChatManager.GetOnlinePlayerByPlayerId(playerId);
            if (previous == null || string.Equals(previous.ConnectionId, Context.ConnectionId, StringComparison.Ordinal))
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(previous.ConnectionId, ChatConfig.WorldChannelId);
            if (!string.IsNullOrWhiteSpace(previous.GuildId))
            {
                await Groups.RemoveFromGroupAsync(previous.ConnectionId, ChatConfig.GetChannelId(ChannelType.Guild, previous.GuildId));
            }

            await Clients.Client(previous.ConnectionId).SendAsync("ForceDisconnect", "账号已在其他位置登录。");
        }

        /// <summary>
        /// 统一解析频道 ID，宗门频道默认回退到当前在线玩家的宗门。
        /// </summary>
        private string ResolveChannelId(ChannelType channelType, string? guildId)
        {
            if (channelType != ChannelType.Guild)
            {
                return ChatConfig.GetChannelId(channelType);
            }

            var effectiveGuildId = !string.IsNullOrWhiteSpace(guildId)
                ? guildId
                : ChatManager.GetOnlinePlayer(Context.ConnectionId)?.GuildId;

            return ChatConfig.GetChannelId(ChannelType.Guild, effectiveGuildId ?? string.Empty);
        }

        /// <summary>
        /// 将聊天频道类型映射为 SignalR 组名。
        /// </summary>
        private static string ResolveBroadcastGroup(string channelType, string? guildId)
        {
            return channelType switch
            {
                "sect" => ChatConfig.GetChannelId(ChannelType.Guild, guildId ?? string.Empty),
                _ => ChatConfig.WorldChannelId
            };
        }
    }
}
