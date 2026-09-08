using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Dungeon;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    /// <summary>
    /// 临时副本队伍服务。
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly IRepository<PartyEntity> _partyRepository;
        private readonly IRepository<PartyMemberEntity> _partyMemberRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<TeamService> _logger;

        /// <summary>
        /// 初始化临时队伍服务。
        /// </summary>
        public TeamService(
            IRepository<PartyEntity> partyRepository,
            IRepository<PartyMemberEntity> partyMemberRepository,
            IRepository<UserEntity> userRepository,
            ILogger<TeamService> logger)
        {
            _partyRepository = partyRepository;
            _partyMemberRepository = partyMemberRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前可加入的队伍列表。
        /// </summary>
        public async Task<List<PartySummaryDto>> GetAvailablePartiesAsync(string playerId)
        {
            await CleanupExpiredPartiesAsync();

            var currentPlayer = await _userRepository.GetByIdAsync(playerId)
                ?? throw new InvalidOperationException("玩家不存在");

            var parties = await _partyRepository.Db.Queryable<PartyEntity>()
                .Where(p => p.IsRecruiting && p.ExpiresAt > DateTime.Now)
                .OrderBy(p => p.CreateTime, SqlSugar.OrderByType.Desc)
                .ToListAsync();

            if (parties.Count == 0)
            {
                return [];
            }

            var partyIds = parties.Select(p => p.PartyId).Distinct().ToList();
            var members = await _partyMemberRepository.Db.Queryable<PartyMemberEntity>()
                .Where(m => partyIds.Contains(m.PartyId))
                .ToListAsync();
            var players = await LoadPlayersAsync(members.Select(m => m.PlayerId));
            var playerCurrentParty = await GetCurrentPartyAsync(playerId);

            return parties.Select(party =>
            {
                var partyMembers = members.Where(m => m.PartyId == party.PartyId).ToList();
                var leaderName = players.TryGetValue(party.LeaderPlayerId, out var leader)
                    ? leader.Name
                    : "未知队长";
                var unavailableReason = BuildJoinUnavailableReason(currentPlayer, party, partyMembers.Count, playerCurrentParty?.PartyId);

                return new PartySummaryDto
                {
                    PartyId = party.PartyId,
                    Name = party.Name,
                    LeaderPlayerId = party.LeaderPlayerId,
                    LeaderName = leaderName,
                    TargetDungeonId = party.TargetDungeonId,
                    TargetDungeonName = party.TargetDungeonName,
                    MinLevel = party.MinLevel,
                    MaxMembers = party.MaxMembers,
                    CurrentMembers = partyMembers.Count,
                    RequiredTeamSize = party.MaxMembers,
                    IsRecruiting = party.IsRecruiting,
                    CanJoin = string.IsNullOrWhiteSpace(unavailableReason),
                    CanChallenge = partyMembers.Count >= party.MaxMembers,
                    StatusText = BuildStatusText(party, partyMembers.Count),
                    UnavailableReason = unavailableReason,
                    CreateTime = party.CreateTime,
                    ExpiresAt = party.ExpiresAt
                };
            }).ToList();
        }

        /// <summary>
        /// 获取玩家当前所在队伍。
        /// </summary>
        public async Task<PartyDetailDto?> GetCurrentPartyAsync(string playerId)
        {
            await CleanupExpiredPartiesAsync();

            var membership = await _partyMemberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (membership == null)
            {
                return null;
            }

            var party = await _partyRepository.GetByIdAsync(membership.PartyId);
            if (party == null)
            {
                await _partyMemberRepository.DeleteAsync(m => m.PartyId == membership.PartyId);
                return null;
            }

            return await BuildPartyDetailAsync(party, playerId);
        }

        /// <summary>
        /// 创建一个新的临时队伍。
        /// </summary>
        public async Task<PartyDetailDto> CreatePartyAsync(string playerId, CreatePartyRequestDto request)
        {
            await CleanupExpiredPartiesAsync();

            var currentPlayer = await _userRepository.GetByIdAsync(playerId)
                ?? throw new InvalidOperationException("玩家不存在");

            if (request == null)
            {
                throw new InvalidOperationException("创建队伍请求不能为空。");
            }

            if (await GetCurrentPartyAsync(playerId) != null)
            {
                throw new InvalidOperationException("你已在临时队伍中，请先退出当前队伍。");
            }

            var partyName = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(partyName))
            {
                throw new InvalidOperationException("请输入队伍名称。");
            }

            var requestedTeamSize = request.TeamSize;
            if (requestedTeamSize is not (1 or 2 or 3))
            {
                throw new InvalidOperationException("队伍人数只能是 1、2 或 3 人。");
            }

            var minLevel = GetMinimumTeamLevel(requestedTeamSize);

            if (currentPlayer.Level < minLevel)
            {
                throw new InvalidOperationException($"需要达到 Lv.{minLevel} 才能创建该人数队伍。");
            }

            var now = DateTime.Now;
            var party = new PartyEntity
            {
                PartyId = Guid.NewGuid().ToString("N"),
                Name = partyName.Length > 20 ? partyName[..20] : partyName,
                LeaderPlayerId = playerId,
                // 中文注释：历史字段保留用于兼容旧数据库，但新队伍不再绑定具体副本。
                TargetDungeonId = string.Empty,
                TargetDungeonName = string.Empty,
                MinLevel = minLevel,
                MaxMembers = requestedTeamSize,
                IsRecruiting = true,
                CreateTime = now,
                LastUpdateTime = now,
                ExpiresAt = now.AddHours(12)
            };

            var membership = new PartyMemberEntity
            {
                MembershipId = Guid.NewGuid().ToString("N"),
                PartyId = party.PartyId,
                PlayerId = playerId,
                IsLeader = true,
                JoinTime = now
            };

            var db = _partyRepository.Db;
            try
            {
                db.Ado.BeginTran();
                await db.Insertable(party).ExecuteCommandAsync();
                await db.Insertable(membership).ExecuteCommandAsync();
                db.Ado.CommitTran();
            }
            catch (Exception ex) when (IsPartyMembershipDuplicate(ex))
            {
                db.Ado.RollbackTran();
                throw new InvalidOperationException("你已在临时队伍中，请先退出当前队伍。");
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

            _logger.LogInformation("Player {PlayerId} created party {PartyId} for dungeon {DungeonId}", playerId, party.PartyId, party.TargetDungeonId);
            return await BuildPartyDetailAsync(party, playerId);
        }

        /// <summary>
        /// 加入指定队伍。
        /// </summary>
        public async Task<PartyDetailDto> JoinPartyAsync(string playerId, string partyId)
        {
            await CleanupExpiredPartiesAsync();

            var currentPlayer = await _userRepository.GetByIdAsync(playerId)
                ?? throw new InvalidOperationException("玩家不存在");

            if (await GetCurrentPartyAsync(playerId) != null)
            {
                throw new InvalidOperationException("你已在临时队伍中，请先退出当前队伍。");
            }

            var party = await _partyRepository.GetByIdAsync(partyId)
                ?? throw new InvalidOperationException("队伍不存在或已解散。");

            var members = await GetPartyMembersAsync(party.PartyId);
            var unavailableReason = BuildJoinUnavailableReason(currentPlayer, party, members.Count, null);
            if (!string.IsNullOrWhiteSpace(unavailableReason))
            {
                throw new InvalidOperationException(unavailableReason);
            }

            var membership = new PartyMemberEntity
            {
                MembershipId = Guid.NewGuid().ToString("N"),
                PartyId = party.PartyId,
                PlayerId = playerId,
                IsLeader = false,
                JoinTime = DateTime.Now
            };

            var db = _partyRepository.Db;
            try
            {
                db.Ado.BeginTran();

                var latestParty = await db.Queryable<PartyEntity>()
                    .Where(p => p.PartyId == partyId)
                    .FirstAsync();
                if (latestParty == null)
                {
                    throw new InvalidOperationException("队伍不存在或已解散。");
                }

                var latestMemberCount = await db.Queryable<PartyMemberEntity>()
                    .Where(m => m.PartyId == partyId)
                    .CountAsync();

                var latestUnavailableReason = BuildJoinUnavailableReason(currentPlayer, latestParty, latestMemberCount, null);
                if (!string.IsNullOrWhiteSpace(latestUnavailableReason))
                {
                    throw new InvalidOperationException(latestUnavailableReason);
                }

                await db.Insertable(membership).ExecuteCommandAsync();
                await db.Updateable<PartyEntity>()
                    .SetColumns(p => p.LastUpdateTime == DateTime.Now)
                    .Where(p => p.PartyId == partyId)
                    .ExecuteCommandAsync();

                db.Ado.CommitTran();
                party = latestParty;
                party.LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex) when (IsPartyMembershipDuplicate(ex))
            {
                db.Ado.RollbackTran();
                throw new InvalidOperationException("你已在临时队伍中，请先退出当前队伍。");
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

            _logger.LogInformation("Player {PlayerId} joined party {PartyId}", playerId, partyId);
            return await BuildPartyDetailAsync(party, playerId);
        }

        /// <summary>
        /// 离开当前所在队伍。
        /// </summary>
        public async Task<PartyDetailDto?> LeaveCurrentPartyAsync(string playerId)
        {
            await CleanupExpiredPartiesAsync();

            var membership = await _partyMemberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (membership == null)
            {
                return null;
            }

            var party = await _partyRepository.GetByIdAsync(membership.PartyId);
            if (party == null)
            {
                await _partyMemberRepository.DeleteAsync(membership);
                return null;
            }

            var members = await GetPartyMembersAsync(party.PartyId);
            var otherMembers = members.Where(m => !string.Equals(m.PlayerId, playerId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.JoinTime)
                .ToList();

            var db = _partyRepository.Db;
            try
            {
                db.Ado.BeginTran();
                await db.Deleteable<PartyMemberEntity>().Where(m => m.MembershipId == membership.MembershipId).ExecuteCommandAsync();

                if (otherMembers.Count == 0)
                {
                    await db.Deleteable<PartyEntity>().Where(p => p.PartyId == party.PartyId).ExecuteCommandAsync();
                }
                else if (membership.IsLeader || string.Equals(party.LeaderPlayerId, playerId, StringComparison.OrdinalIgnoreCase))
                {
                    var nextLeader = otherMembers[0];
                    await db.Updateable<PartyMemberEntity>()
                        .SetColumns(m => m.IsLeader == true)
                        .Where(m => m.MembershipId == nextLeader.MembershipId)
                        .ExecuteCommandAsync();
                    await db.Updateable<PartyMemberEntity>()
                        .SetColumns(m => m.IsLeader == false)
                        .Where(m => m.PartyId == party.PartyId && m.MembershipId != nextLeader.MembershipId)
                        .ExecuteCommandAsync();
                    await db.Updateable<PartyEntity>()
                        .SetColumns(p => p.LeaderPlayerId == nextLeader.PlayerId)
                        .SetColumns(p => p.LastUpdateTime == DateTime.Now)
                        .Where(p => p.PartyId == party.PartyId)
                        .ExecuteCommandAsync();
                }
                else
                {
                    await db.Updateable<PartyEntity>()
                        .SetColumns(p => p.LastUpdateTime == DateTime.Now)
                        .Where(p => p.PartyId == party.PartyId)
                        .ExecuteCommandAsync();
                }

                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

            _logger.LogInformation("Player {PlayerId} left party {PartyId}", playerId, party.PartyId);
            return null;
        }

        /// <summary>
        /// 切换队伍招募状态。
        /// </summary>
        public async Task<PartyDetailDto> ToggleRecruitingAsync(string playerId, string partyId)
        {
            await CleanupExpiredPartiesAsync();

            var party = await _partyRepository.GetByIdAsync(partyId)
                ?? throw new InvalidOperationException("队伍不存在或已解散。");

            if (!string.Equals(party.LeaderPlayerId, playerId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("只有队长可以调整招募状态。");
            }

            party.IsRecruiting = !party.IsRecruiting;
            party.LastUpdateTime = DateTime.Now;
            await _partyRepository.UpdateAsync(party);

            _logger.LogInformation("Player {PlayerId} toggled recruiting for party {PartyId} to {Recruiting}", playerId, partyId, party.IsRecruiting);
            return await BuildPartyDetailAsync(party, playerId);
        }

        /// <summary>
        /// 解散指定队伍。
        /// </summary>
        public async Task<bool> DismissPartyAsync(string playerId, string partyId)
        {
            await CleanupExpiredPartiesAsync();

            var party = await _partyRepository.GetByIdAsync(partyId);
            if (party == null)
            {
                return false;
            }

            if (!string.Equals(party.LeaderPlayerId, playerId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("只有队长可以解散队伍。");
            }

            var db = _partyRepository.Db;
            try
            {
                db.Ado.BeginTran();
                await db.Deleteable<PartyMemberEntity>().Where(m => m.PartyId == partyId).ExecuteCommandAsync();
                await db.Deleteable<PartyEntity>().Where(p => p.PartyId == partyId).ExecuteCommandAsync();
                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

            _logger.LogInformation("Player {PlayerId} dismissed party {PartyId}", playerId, partyId);
            return true;
        }

        private async Task<PartyDetailDto> BuildPartyDetailAsync(PartyEntity party, string currentPlayerId)
        {
            var members = await GetPartyMembersAsync(party.PartyId);
            var players = await LoadPlayersAsync(members.Select(m => m.PlayerId));
            var currentMember = members.FirstOrDefault(m => string.Equals(m.PlayerId, currentPlayerId, StringComparison.OrdinalIgnoreCase));
            var currentMembers = members.Count;
            var underLevelMember = players.Values
                .Where(player => player != null && !player.IsDeleted)
                .FirstOrDefault(player => player.Level < party.MinLevel);
            var canChallenge = currentMembers >= party.MaxMembers && underLevelMember == null;
            var unavailableReason = canChallenge
                ? null
                : currentMembers < party.MaxMembers
                    ? $"队伍人数不足，当前 {currentMembers}/{party.MaxMembers}。"
                    : $"{underLevelMember?.Name ?? "有队员"} 尚未达到 Lv.{party.MinLevel}。";

            var detail = new PartyDetailDto
            {
                PartyId = party.PartyId,
                Name = party.Name,
                LeaderPlayerId = party.LeaderPlayerId,
                LeaderName = players.TryGetValue(party.LeaderPlayerId, out var leader) ? leader.Name : "未知队长",
                TargetDungeonId = party.TargetDungeonId,
                TargetDungeonName = party.TargetDungeonName,
                MinLevel = party.MinLevel,
                MaxMembers = party.MaxMembers,
                CurrentMembers = currentMembers,
                RequiredTeamSize = party.MaxMembers,
                IsRecruiting = party.IsRecruiting,
                CanJoin = false,
                CanChallenge = canChallenge,
                StatusText = BuildStatusText(party, currentMembers),
                UnavailableReason = unavailableReason,
                CreateTime = party.CreateTime,
                ExpiresAt = party.ExpiresAt,
                IsLeader = string.Equals(party.LeaderPlayerId, currentPlayerId, StringComparison.OrdinalIgnoreCase),
                IsMember = currentMember != null,
                Members = members.Select(member => MapPartyMember(member, players, currentPlayerId)).ToList()
            };

            return detail;
        }

        private async Task CleanupExpiredPartiesAsync()
        {
            var now = DateTime.Now;
            var expiredPartyIds = await _partyRepository.Db.Queryable<PartyEntity>()
                .Where(p => p.ExpiresAt <= now)
                .Select(p => p.PartyId)
                .ToListAsync();

            if (expiredPartyIds.Count == 0)
            {
                return;
            }

            var db = _partyRepository.Db;
            try
            {
                db.Ado.BeginTran();
                await db.Deleteable<PartyMemberEntity>().Where(m => expiredPartyIds.Contains(m.PartyId)).ExecuteCommandAsync();
                await db.Deleteable<PartyEntity>().Where(p => expiredPartyIds.Contains(p.PartyId)).ExecuteCommandAsync();
                db.Ado.CommitTran();
            }
            catch
            {
                db.Ado.RollbackTran();
                throw;
            }

            _logger.LogInformation("Cleaned up {Count} expired temporary parties", expiredPartyIds.Count);
        }

        private async Task<List<PartyMemberEntity>> GetPartyMembersAsync(string partyId)
        {
            return await _partyMemberRepository.Db.Queryable<PartyMemberEntity>()
                .Where(m => m.PartyId == partyId)
                .OrderBy(m => m.JoinTime)
                .ToListAsync();
        }

        private async Task<Dictionary<string, UserEntity>> LoadPlayersAsync(IEnumerable<string> playerIds)
        {
            var ids = playerIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<string, UserEntity>(StringComparer.OrdinalIgnoreCase);
            }

            var players = await _userRepository.Db.Queryable<UserEntity>()
                .Where(u => ids.Contains(u.GID) && !u.IsDeleted)
                .ToListAsync();

            return players.ToDictionary(u => u.GID, u => u, StringComparer.OrdinalIgnoreCase);
        }

        private static PartyMemberDto MapPartyMember(PartyMemberEntity member, IReadOnlyDictionary<string, UserEntity> players, string currentPlayerId)
        {
            var player = players.TryGetValue(member.PlayerId, out var matchedPlayer) ? matchedPlayer : null;
            var realmConfig = RealmLevelCatalog.Get(player?.Level ?? 1);

            return new PartyMemberDto
            {
                PlayerId = member.PlayerId,
                Name = player?.Name ?? "未知修士",
                Level = player?.Level ?? 1,
                RealmName = realmConfig.RealmName,
                RealmLayer = realmConfig.Layer,
                IsLeader = member.IsLeader,
                IsSelf = string.Equals(member.PlayerId, currentPlayerId, StringComparison.OrdinalIgnoreCase),
                SpiritRootName = GetSpiritRootName(player?.Element ?? Element.None),
                SpiritRootIcon = GetSpiritRootIcon(player?.Element ?? Element.None),
                JoinTime = member.JoinTime
            };
        }

        private static string BuildStatusText(PartyEntity party, int currentMembers)
        {
            if (!party.IsRecruiting)
            {
                return "已关闭";
            }

            if (currentMembers >= party.MaxMembers)
            {
                return "已满员";
            }

            return "招募中";
        }

        /// <summary>
        /// 生成队伍创建所需的最低等级。
        /// </summary>
        private static int GetMinimumTeamLevel(int teamSize)
        {
            return teamSize switch
            {
                1 => 1,
                2 => 1,
                3 => 1,
                _ => int.MaxValue
            };
        }

        private static string? BuildJoinUnavailableReason(UserEntity currentPlayer, PartyEntity party, int currentMembers, string? currentPartyId)
        {
            if (!string.IsNullOrWhiteSpace(currentPartyId))
            {
                return "你已在临时队伍中，请先退出当前队伍。";
            }

            if (!party.IsRecruiting)
            {
                return "队伍已停止招募。";
            }

            if (party.ExpiresAt <= DateTime.Now)
            {
                return "队伍已过期。";
            }

            if (currentMembers >= party.MaxMembers)
            {
                return "队伍人数已满。";
            }

            if (currentPlayer.Level < party.MinLevel)
            {
                return $"需要达到 Lv.{party.MinLevel} 才能加入该队伍。";
            }

            return null;
        }

        private static bool IsPartyMembershipDuplicate(Exception exception)
        {
            const string duplicateMessage = "UNIQUE constraint failed: PartyMembers.PlayerId";

            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current.Message.Contains(duplicateMessage, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetSpiritRootName(Element element)
        {
            return element switch
            {
                Element.Metal => "金",
                Element.Wood => "木",
                Element.Water => "水",
                Element.Fire => "火",
                Element.Earth => "土",
                Element.Wind => "风",
                Element.Ice => "冰",
                Element.Thunder => "雷",
                _ => "无"
            };
        }

        private static string GetSpiritRootIcon(Element element)
        {
            return element switch
            {
                Element.Metal => "⚔️",
                Element.Wood => "🌿",
                Element.Water => "💧",
                Element.Fire => "🔥",
                Element.Earth => "🏔️",
                Element.Wind => "🌪️",
                Element.Ice => "❄️",
                Element.Thunder => "⚡",
                _ => "○"
            };
        }
    }
}
