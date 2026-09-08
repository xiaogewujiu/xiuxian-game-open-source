using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 公会服务实现。
    /// </summary>
    public class GuildService : IGuildService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<GuildEntity> _guildRepository;
        private readonly IRepository<GuildMemberEntity> _memberRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<GuildService> _logger;

        /// <summary>
        /// 初始化公会服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="guildRepository">公会仓储。</param>
        /// <param name="memberRepository">公会成员仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="logger">日志记录器。</param>
        public GuildService(
            DbContext dbContext,
            IRepository<GuildEntity> guildRepository,
            IRepository<GuildMemberEntity> memberRepository,
            IRepository<UserEntity> userRepository,
            ILogger<GuildService> logger)
        {
            _dbContext = dbContext;
            _guildRepository = guildRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取公会列表。
        /// </summary>
        /// <returns>按等级、人数和名称排序后的公会列表。</returns>
        public async Task<List<GuildDto>> GetGuildsAsync()
        {
            var guilds = await _guildRepository.GetListAsync(g => !g.IsDeleted);
            return guilds
                .OrderByDescending(g => g.Level)
                .ThenByDescending(g => g.MemberCount)
                .ThenBy(g => g.Name)
                .Select(MapToGuildDto)
                .ToList();
        }

        /// <summary>
        /// 创建公会。
        /// </summary>
        /// <param name="playerId">创建者玩家编号。</param>
        /// <param name="request">创建请求。</param>
        /// <returns>新建公会 DTO。</returns>
        public async Task<GuildDto> CreateGuildAsync(string playerId, CreateGuildRequestDto request)
        {
            var guildName = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(guildName))
            {
                throw new InvalidOperationException("公会名称不能为空");
            }

            var existingGuild = await _guildRepository.GetFirstAsync(g => g.Name == guildName && !g.IsDeleted);
            if (existingGuild != null)
            {
                throw new InvalidOperationException("公会名称已存在");
            }

            var player = await GetRequiredPlayerAsync(playerId);
            await ReconcileMembershipStateAsync(player);

            var existingMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (existingMember != null || !string.IsNullOrWhiteSpace(player.GuildId))
            {
                throw new InvalidOperationException("您已加入其他公会，请先退出");
            }

            var guildId = Guid.NewGuid().ToString("N");
            var now = DateTime.Now;
            var playerName = GetDisplayPlayerName(player);

            var guild = new GuildEntity
            {
                GID = guildId,
                Name = guildName,
                LeaderId = playerId,
                LeaderName = playerName,
                Level = 1,
                MemberCount = 1,
                MaxMembers = 20,
                CreateTime = now,
                LastUpdateTime = now
            };

            var member = new GuildMemberEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                GuildId = guildId,
                PlayerId = playerId,
                PlayerName = playerName,
                PlayerLevel = player.Level,
                Position = GuildPosition.Leader,
                JoinTime = now,
                LastActiveTime = now
            };

            try
            {
                _dbContext.BeginTransaction();

                await _guildRepository.AddAsync(guild);
                await _memberRepository.AddAsync(member);

                player.GuildId = guildId;
                player.LastUpdateTime = now;
                await _userRepository.UpdateAsync(player);

                _dbContext.CommitTransaction();
                _logger.LogInformation(
                    "Player {PlayerId} created guild {GuildId} ({GuildName})",
                    playerId,
                    guildId,
                    guildName);
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Create guild failed. PlayerId={PlayerId}, GuildName={GuildName}", playerId, guildName);
                throw;
            }

            return MapToGuildDto(guild);
        }

        /// <summary>
        /// 加入指定公会。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="guildId">公会编号。</param>
        /// <returns>加入成功返回真。</returns>
        public async Task<bool> JoinGuildAsync(string playerId, string guildId)
        {
            var guild = await _guildRepository.GetByIdAsync(guildId);
            if (guild == null || guild.IsDeleted)
            {
                throw new InvalidOperationException("公会不存在");
            }

            var player = await GetRequiredPlayerAsync(playerId);
            await ReconcileMembershipStateAsync(player);

            var existingMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (existingMember != null || !string.IsNullOrWhiteSpace(player.GuildId))
            {
                throw new InvalidOperationException("您已加入其他公会，请先退出");
            }

            if (guild.MemberCount >= guild.MaxMembers)
            {
                throw new InvalidOperationException("公会成员已满");
            }

            if (player.Level < guild.RequiredLevel)
            {
                throw new InvalidOperationException($"加入该公会至少需要等级 {guild.RequiredLevel}");
            }

            var now = DateTime.Now;
            var member = new GuildMemberEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                GuildId = guildId,
                PlayerId = playerId,
                PlayerName = GetDisplayPlayerName(player),
                PlayerLevel = player.Level,
                Position = GuildPosition.Member,
                JoinTime = now,
                LastActiveTime = now
            };

            try
            {
                _dbContext.BeginTransaction();

                await _memberRepository.AddAsync(member);

                guild.MemberCount += 1;
                guild.LastUpdateTime = now;
                await _guildRepository.UpdateAsync(guild);

                player.GuildId = guildId;
                player.LastUpdateTime = now;
                await _userRepository.UpdateAsync(player);

                _dbContext.CommitTransaction();
                _logger.LogInformation("Player {PlayerId} joined guild {GuildId}", playerId, guildId);
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Join guild failed. PlayerId={PlayerId}, GuildId={GuildId}", playerId, guildId);
                throw;
            }
        }

        /// <summary>
        /// 退出当前公会。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>退出成功返回真。</returns>
        public async Task<bool> LeaveGuildAsync(string playerId)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            await ReconcileMembershipStateAsync(player);

            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                if (!string.IsNullOrWhiteSpace(player.GuildId))
                {
                    var staleGuildId = player.GuildId;
                    player.GuildId = null;
                    player.LastUpdateTime = DateTime.Now;
                    await _userRepository.UpdateAsync(player);

                    _logger.LogWarning(
                        "Player {PlayerId} had stale Users.GuildId={GuildId} without membership record. Cleared during leave.",
                        playerId,
                        staleGuildId);
                    return true;
                }

                throw new InvalidOperationException("您未加入任何公会");
            }

            if (member.Position == GuildPosition.Leader)
            {
                throw new InvalidOperationException("会长无法退出公会，请先转让会长或解散公会");
            }

            var guild = await _guildRepository.GetByIdAsync(member.GuildId);
            var now = DateTime.Now;

            try
            {
                _dbContext.BeginTransaction();

                await _memberRepository.DeleteAsync(member);

                if (guild != null && !guild.IsDeleted)
                {
                    guild.MemberCount = Math.Max(0, guild.MemberCount - 1);
                    guild.LastUpdateTime = now;
                    await _guildRepository.UpdateAsync(guild);
                }
                else
                {
                    _logger.LogWarning(
                        "Player {PlayerId} left guild membership {GuildId}, but guild record was missing or deleted.",
                        playerId,
                        member.GuildId);
                }

                player.GuildId = null;
                player.LastUpdateTime = now;
                await _userRepository.UpdateAsync(player);

                _dbContext.CommitTransaction();
                _logger.LogInformation("Player {PlayerId} left guild {GuildId}", playerId, member.GuildId);
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Leave guild failed. PlayerId={PlayerId}", playerId);
                throw;
            }
        }

        /// <summary>
        /// 获取玩家所属公会。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>命中的公会 DTO；未加入公会时返回空。</returns>
        public async Task<GuildDto?> GetPlayerGuildAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                return null;
            }

            await ReconcileMembershipStateAsync(player);

            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            var guildId = member?.GuildId ?? player.GuildId;
            if (string.IsNullOrWhiteSpace(guildId))
            {
                return null;
            }

            var guild = await _guildRepository.GetByIdAsync(guildId);
            if (guild == null || guild.IsDeleted)
            {
                _logger.LogWarning(
                    "Player {PlayerId} references guild {GuildId}, but the guild record is missing or deleted.",
                    playerId,
                    guildId);
                return null;
            }

            return MapToGuildDto(guild);
        }

        /// <summary>
        /// 获取公会成员列表。
        /// </summary>
        /// <param name="guildId">公会编号。</param>
        /// <returns>按职位、贡献和加入时间排序后的成员列表。</returns>
        public async Task<List<GuildMemberDto>> GetGuildMembersAsync(string guildId)
        {
            var members = await _memberRepository.GetListAsync(m => m.GuildId == guildId);

            return members
                .OrderByDescending(m => m.Position)
                .ThenByDescending(m => m.Contribution)
                .ThenBy(m => m.JoinTime)
                .Select(m => new GuildMemberDto
                {
                    PlayerId = m.PlayerId,
                    PlayerName = m.PlayerName,
                    PlayerLevel = m.PlayerLevel,
                    Position = m.Position.ToString(),
                    Contribution = m.Contribution,
                    JoinTime = m.JoinTime
                })
                .ToList();
        }

        private async Task<UserEntity> GetRequiredPlayerAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在");
            }

            return player;
        }

        /// <summary>
        /// 收口 Users.GuildId 与 guild_member 的漂移。
        /// 当前以后者为真实成员关系来源，并把玩家表同步回一致状态。
        /// </summary>
        private async Task ReconcileMembershipStateAsync(UserEntity player)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == player.GID);
            if (member == null)
            {
                if (string.IsNullOrWhiteSpace(player.GuildId))
                {
                    return;
                }

                var staleGuildId = player.GuildId;
                player.GuildId = null;
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);

                _logger.LogWarning(
                    "Cleared stale Users.GuildId. PlayerId={PlayerId}, StaleGuildId={GuildId}",
                    player.GID,
                    staleGuildId);
                return;
            }

            if (string.Equals(player.GuildId, member.GuildId, StringComparison.Ordinal))
            {
                return;
            }

            var oldGuildId = player.GuildId;
            player.GuildId = member.GuildId;
            player.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(player);

            _logger.LogWarning(
                "Reconciled guild membership drift. PlayerId={PlayerId}, OldGuildId={OldGuildId}, MemberGuildId={MemberGuildId}",
                player.GID,
                oldGuildId,
                member.GuildId);
        }

        private static string GetDisplayPlayerName(UserEntity player)
        {
            return string.IsNullOrWhiteSpace(player.Name)
                ? $"玩家{player.GID[..Math.Min(6, player.GID.Length)]}"
                : player.Name;
        }

        private static GuildDto MapToGuildDto(GuildEntity guild)
        {
            return new GuildDto
            {
                GuildId = guild.GID,
                Name = guild.Name,
                Level = guild.Level,
                MemberCount = guild.MemberCount,
                MaxMembers = guild.MaxMembers,
                LeaderName = guild.LeaderName
            };
        }
    }
}
