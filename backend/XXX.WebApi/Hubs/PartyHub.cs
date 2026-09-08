using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Hubs
{
    /// <summary>
    /// 临时队伍实时同步 Hub。
    /// </summary>
    /// <remarks>
    /// 这一层只负责把在线连接和当前临时队伍做绑定，
    /// 让组队副本结算结果可以按队伍维度广播给所有在线成员。
    /// </remarks>
    [Authorize]
    public class PartyHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string?> ConnectionPartyMap = new();

        private readonly IRepository<PartyMemberEntity> _partyMemberRepository;
        private readonly ILogger<PartyHub> _logger;

        /// <summary>
        /// 初始化队伍实时 Hub。
        /// </summary>
        public PartyHub(IRepository<PartyMemberEntity> partyMemberRepository, ILogger<PartyHub> logger)
        {
            _partyMemberRepository = partyMemberRepository;
            _logger = logger;
        }

        /// <summary>
        /// 连接建立后同步当前队伍归属。
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var playerId = Context.User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Context.Abort();
                return;
            }

            await SyncPartyGroupInternalAsync(playerId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 连接断开时移除旧队伍组绑定。
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionPartyMap.TryRemove(Context.ConnectionId, out var previousPartyId) &&
                !string.IsNullOrWhiteSpace(previousPartyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetPartyGroupName(previousPartyId));
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 主动同步当前连接的队伍归属。
        /// 当前玩家创建、加入、退出或解散队伍后，前端应调用一次。
        /// </summary>
        public async Task<string?> SyncPartyGroups()
        {
            var playerId = Context.User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new HubException("未登录");
            }

            return await SyncPartyGroupInternalAsync(playerId);
        }

        /// <summary>
        /// 统一生成队伍组名。
        /// </summary>
        public static string GetPartyGroupName(string partyId) => $"party:{partyId}";

        private async Task<string?> SyncPartyGroupInternalAsync(string playerId)
        {
            var membership = await _partyMemberRepository.GetFirstAsync(member => member.PlayerId == playerId);
            var nextPartyId = membership?.PartyId;
            ConnectionPartyMap.TryGetValue(Context.ConnectionId, out var previousPartyId);

            if (!string.IsNullOrWhiteSpace(previousPartyId) &&
                !string.Equals(previousPartyId, nextPartyId, StringComparison.OrdinalIgnoreCase))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetPartyGroupName(previousPartyId));
            }

            if (!string.IsNullOrWhiteSpace(nextPartyId) &&
                !string.Equals(previousPartyId, nextPartyId, StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetPartyGroupName(nextPartyId));
            }

            ConnectionPartyMap[Context.ConnectionId] = nextPartyId;
            _logger.LogInformation(
                "Party hub synced connection {ConnectionId} for player {PlayerId} to party {PartyId}",
                Context.ConnectionId,
                playerId,
                nextPartyId ?? "(none)");
            return nextPartyId;
        }
    }

    /// <summary>
    /// 队伍副本战斗结算广播事件。
    /// </summary>
    public sealed class PartyBattleResolvedEvent
    {
        /// <summary>
        /// 战斗唯一编号。
        /// </summary>
        public string BattleId { get; set; } = string.Empty;

        /// <summary>
        /// 队伍编号。
        /// </summary>
        public string PartyId { get; set; } = string.Empty;

        /// <summary>
        /// 副本编号。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 副本名称。
        /// </summary>
        public string DungeonName { get; set; } = string.Empty;

        /// <summary>
        /// 发起战斗的玩家编号。
        /// </summary>
        public string StartedByPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 战斗结算结果。
        /// </summary>
        public Application.DTOs.BattleResultDto Result { get; set; } = new();
    }
}
