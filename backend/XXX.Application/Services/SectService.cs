using System.Text.Json;
using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Quest;

namespace XXX.Application.Services
{
    /// <summary>
    /// 宗门服务实现。
    /// </summary>
    public class SectService : ISectService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<SectTemplateEntity> _sectTemplateRepository;
        private readonly IRepository<HeartSutraTemplateEntity> _heartSutraRepository;
        private readonly IRepository<PlayerHeartSutraEntity> _playerSutraRepository;
        private readonly IRepository<SectDonationRecordEntity> _donationRecordRepository;
        private readonly IRepository<SectTournamentEntity> _tournamentRepository;
        private readonly IRepository<SectTournamentMatchEntity> _tournamentMatchRepository;
        private readonly IRepository<GeniusTournamentEntity> _geniusTournamentRepository;
        private readonly IRepository<GeniusTournamentMatchEntity> _geniusTournamentMatchRepository;
        private readonly IRepository<SectBossTemplateEntity> _bossTemplateRepository;
        private readonly IRepository<SectBossInstanceEntity> _bossInstanceRepository;
        private readonly IRepository<SectShopConfigEntity> _shopConfigRepository;
        private readonly IRepository<SectShopItemEntity> _shopItemRepository;
        private readonly IRepository<SectBlessingConfigEntity> _blessingRepository;
        private readonly IRepository<GuildEntity> _guildRepository;
        private readonly IRepository<GuildMemberEntity> _memberRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IQuestService _questService;
        private readonly ILogger<SectService> _logger;

        /// <summary>
        /// 初始化宗门服务。
        /// </summary>
        public SectService(
            DbContext dbContext,
            IRepository<SectTemplateEntity> sectTemplateRepository,
            IRepository<HeartSutraTemplateEntity> heartSutraRepository,
            IRepository<PlayerHeartSutraEntity> playerSutraRepository,
            IRepository<SectDonationRecordEntity> donationRecordRepository,
            IRepository<SectTournamentEntity> tournamentRepository,
            IRepository<SectTournamentMatchEntity> tournamentMatchRepository,
            IRepository<GeniusTournamentEntity> geniusTournamentRepository,
            IRepository<GeniusTournamentMatchEntity> geniusTournamentMatchRepository,
            IRepository<SectBossTemplateEntity> bossTemplateRepository,
            IRepository<SectBossInstanceEntity> bossInstanceRepository,
            IRepository<SectShopConfigEntity> shopConfigRepository,
            IRepository<SectShopItemEntity> shopItemRepository,
            IRepository<SectBlessingConfigEntity> blessingRepository,
            IRepository<GuildEntity> guildRepository,
            IRepository<GuildMemberEntity> memberRepository,
            IRepository<UserEntity> userRepository,
            IQuestService questService,
            ILogger<SectService> logger)
        {
            _dbContext = dbContext;
            _sectTemplateRepository = sectTemplateRepository;
            _heartSutraRepository = heartSutraRepository;
            _playerSutraRepository = playerSutraRepository;
            _donationRecordRepository = donationRecordRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentMatchRepository = tournamentMatchRepository;
            _geniusTournamentRepository = geniusTournamentRepository;
            _geniusTournamentMatchRepository = geniusTournamentMatchRepository;
            _bossTemplateRepository = bossTemplateRepository;
            _bossInstanceRepository = bossInstanceRepository;
            _shopConfigRepository = shopConfigRepository;
            _shopItemRepository = shopItemRepository;
            _blessingRepository = blessingRepository;
            _guildRepository = guildRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _questService = questService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有可用宗门列表。
        /// </summary>
        public async Task<List<SectTemplateDto>> GetAvailableSectsAsync(string playerId)
        {
            var templates = await _sectTemplateRepository.GetListAsync(t => t.IsEnabled);
            var allGuilds = await _guildRepository.GetListAsync(g => !g.IsDeleted);
            var playerMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            string? playerSectTemplateId = null;

            if (playerMember != null)
            {
                var playerGuild = await _guildRepository.GetByIdAsync(playerMember.GuildId);
                playerSectTemplateId = playerGuild?.SectTemplateId;
            }

            var result = new List<SectTemplateDto>();
            foreach (var template in templates)
            {
                var memberCount = allGuilds
                    .Where(g => g.SectTemplateId == template.SectId)
                    .Sum(g => g.MemberCount);

                result.Add(new SectTemplateDto
                {
                    SectId = template.SectId,
                    Name = template.Name,
                    Description = template.Description,
                    Icon = template.Icon,
                    MemberCount = memberCount,
                    IsJoined = playerSectTemplateId == template.SectId
                });
            }

            return result.OrderBy(t => t.SectId).ToList();
        }

        /// <summary>
        /// 获取宗门详情。
        /// </summary>
        public async Task<SectDetailDto?> GetSectDetailAsync(string playerId, string sectId)
        {
            var template = await _sectTemplateRepository.GetByIdAsync(sectId);
            if (template == null || !template.IsEnabled)
            {
                return null;
            }

            var allGuilds = await _guildRepository.GetListAsync(g => !g.IsDeleted && g.SectTemplateId == sectId);
            var memberCount = allGuilds.Sum(g => g.MemberCount);

            var playerMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            string? playerSectTemplateId = null;
            int guildLevel = 1;
            string? announcement = null;

            if (playerMember != null)
            {
                var playerGuild = await _guildRepository.GetByIdAsync(playerMember.GuildId);
                if (playerGuild != null)
                {
                    playerSectTemplateId = playerGuild.SectTemplateId;
                    guildLevel = playerGuild.Level;
                    announcement = playerGuild.Announcement;
                }
            }

            var heartSutraIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(template.HeartSutraIdsJson))
            {
                try
                {
                    heartSutraIds = JsonSerializer.Deserialize<List<string>>(template.HeartSutraIdsJson) ?? [];
                }
                catch
                {
                    // ignore parse errors
                }
            }

            var heartSutras = new List<HeartSutraDto>();
            if (heartSutraIds.Count > 0)
            {
                var allSutras = await _heartSutraRepository.GetListAsync(s => heartSutraIds.Contains(s.SutraId));
                var playerSutras = await _playerSutraRepository.GetListAsync(s => s.PlayerId == playerId && heartSutraIds.Contains(s.SutraId));
                var playerSutraMap = playerSutras.ToDictionary(s => s.SutraId, s => s.CurrentLayer);

                foreach (var sutra in allSutras.OrderBy(s => s.SortOrder))
                {
                    var layers = new List<SutraLayerDto>();
                    if (!string.IsNullOrWhiteSpace(sutra.LayersJson))
                    {
                        try
                        {
                            var layerConfigs = JsonSerializer.Deserialize<List<SutraLayerConfig>>(sutra.LayersJson) ?? [];
                            var currentLayer = playerSutraMap.GetValueOrDefault(sutra.SutraId, 0);
                            layers = layerConfigs.Select(lc => new SutraLayerDto
                            {
                                Layer = lc.Layer,
                                Name = lc.Name,
                                ContributionCost = lc.ContributionCost,
                                GoldCost = lc.GoldCost,
                                Bonuses = lc.Bonuses.Select(b => new SutraAttributeBonusDto
                                {
                                    AttributeName = b.AttributeName,
                                    Value = b.Value,
                                    IsPercentage = b.IsPercentage
                                }).ToList(),
                                UnlockSkillId = lc.UnlockSkillId,
                                UnlockSkillName = !string.IsNullOrWhiteSpace(lc.UnlockSkillId)
                                    && int.TryParse(lc.UnlockSkillId, out var skillId)
                                    && SkillData.Skills.TryGetValue(skillId, out var skill) ? skill.Name : null,
                                UnlockBuffId = lc.UnlockBuffId,
                                UnlockBuffName = !string.IsNullOrWhiteSpace(lc.UnlockBuffId)
                                    && BuffDataTemplates.BuffTemplates.TryGetValue(lc.UnlockBuffId, out var buff) ? buff.Name : null,
                                IsUnlocked = currentLayer >= lc.Layer
                            }).ToList();
                        }
                        catch
                        {
                            // ignore parse errors
                        }
                    }

                    heartSutras.Add(new HeartSutraDto
                    {
                        SutraId = sutra.SutraId,
                        Name = sutra.Name,
                        Description = sutra.Description,
                        MaxLayer = sutra.MaxLayer,
                        Layers = layers
                    });
                }
            }

            return new SectDetailDto
            {
                SectId = template.SectId,
                Name = template.Name,
                Description = template.Description,
                Icon = template.Icon,
                MemberCount = memberCount,
                IsJoined = playerSectTemplateId == template.SectId,
                HeartSutras = heartSutras,
                GuildLevel = guildLevel,
                Announcement = announcement
            };
        }

        /// <summary>
        /// 加入宗门。
        /// </summary>
        public async Task<bool> JoinSectAsync(string playerId, string sectId)
        {
            var template = await _sectTemplateRepository.GetByIdAsync(sectId);
            if (template == null || !template.IsEnabled)
            {
                throw new InvalidOperationException("宗门不存在或未启用");
            }

            var player = await GetRequiredPlayerAsync(playerId);
            var existingMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (existingMember != null)
            {
                // 检查是否是旧公会系统数据（无宗门模板ID），如果是则自动退出
                var oldGuild = await _guildRepository.GetByIdAsync(existingMember.GuildId);
                if (oldGuild == null || string.IsNullOrWhiteSpace(oldGuild.SectTemplateId))
                {
                    // 旧公会数据，自动清理
                    await _memberRepository.DeleteAsync(existingMember);
                    if (oldGuild != null && oldGuild.MemberCount > 0)
                    {
                        oldGuild.MemberCount--;
                        await _guildRepository.UpdateAsync(oldGuild);
                    }
                }
                else
                {
                    throw new InvalidOperationException("您已加入其他宗门，请先退出当前宗门");
                }
            }

            // 清除旧的 GuildId 残留（兼容旧公会系统数据）
            if (!string.IsNullOrWhiteSpace(player.GuildId))
            {
                player.GuildId = null;
                await _userRepository.UpdateAsync(player);
            }

            var guilds = await _guildRepository.GetListAsync(g => !g.IsDeleted && g.SectTemplateId == sectId);
            var availableGuild = guilds.FirstOrDefault(g => g.MemberCount < g.MaxMembers);

            if (availableGuild == null)
            {
                var guildId = Guid.NewGuid().ToString("N");
                var now = DateTime.Now;
                var playerName = GetDisplayPlayerName(player);

                availableGuild = new GuildEntity
                {
                    GID = guildId,
                    Name = $"{template.Name}一殿",
                    LeaderId = playerId,
                    LeaderName = playerName,
                    Level = 1,
                    MemberCount = 0,
                    MaxMembers = 20,
                    SectTemplateId = sectId,
                    CreateTime = now,
                    LastUpdateTime = now
                };

                try
                {
                    _dbContext.BeginTransaction();
                    await _guildRepository.AddAsync(availableGuild);
                    _dbContext.CommitTransaction();
                    _logger.LogInformation("Auto-created guild {GuildId} for sect {SectId}", guildId, sectId);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    _logger.LogError(ex, "Auto-create guild failed for sect {SectId}", sectId);
                    throw;
                }
            }

            var now2 = DateTime.Now;
            var member = new GuildMemberEntity
            {
                GID = Guid.NewGuid().ToString("N"),
                GuildId = availableGuild.GID,
                PlayerId = playerId,
                PlayerName = GetDisplayPlayerName(player),
                PlayerLevel = player.Level,
                Position = GuildPosition.Member,
                JoinTime = now2,
                LastActiveTime = now2
            };

            try
            {
                _dbContext.BeginTransaction();

                await _memberRepository.AddAsync(member);

                availableGuild.MemberCount += 1;
                availableGuild.LastUpdateTime = now2;
                await _guildRepository.UpdateAsync(availableGuild);

                player.GuildId = availableGuild.GID;
                player.LastUpdateTime = now2;
                await _userRepository.UpdateAsync(player);

                _dbContext.CommitTransaction();
                _logger.LogInformation("Player {PlayerId} joined sect {SectId} via guild {GuildId}", playerId, sectId, availableGuild.GID);
                return true;
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Join sect failed. PlayerId={PlayerId}, SectId={SectId}", playerId, sectId);
                throw;
            }
        }

        /// <summary>
        /// 获取玩家心法进度列表。
        /// </summary>
        public async Task<List<PlayerSutraProgressDto>> GetPlayerSutrasAsync(string playerId)
        {
            var playerSutras = await _playerSutraRepository.GetListAsync(s => s.PlayerId == playerId);
            if (playerSutras.Count == 0)
            {
                return [];
            }

            var sutraIds = playerSutras.Select(s => s.SutraId).ToList();
            var templates = await _heartSutraRepository.GetListAsync(t => sutraIds.Contains(t.SutraId));
            var templateMap = templates.ToDictionary(t => t.SutraId);

            return playerSutras.Select(ps =>
            {
                templateMap.TryGetValue(ps.SutraId, out var template);
                return new PlayerSutraProgressDto
                {
                    SutraId = ps.SutraId,
                    SutraName = template?.Name ?? ps.SutraId,
                    CurrentLayer = ps.CurrentLayer,
                    MaxLayer = template?.MaxLayer ?? 10
                };
            }).ToList();
        }

        /// <summary>
        /// 升级心法层级。
        /// </summary>
        public async Task<HeartSutraUpgradeResultDto> UpgradeSutraLayerAsync(string playerId, string sutraId)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                throw new InvalidOperationException("您未加入任何宗门");
            }

            var guild = await _guildRepository.GetByIdAsync(member.GuildId);
            if (guild == null || guild.IsDeleted)
            {
                throw new InvalidOperationException("公会不存在");
            }

            var sutra = await _heartSutraRepository.GetByIdAsync(sutraId);
            if (sutra == null)
            {
                throw new InvalidOperationException("心法不存在");
            }

            var playerSutra = await _playerSutraRepository.GetFirstAsync(s => s.PlayerId == playerId && s.SutraId == sutraId);
            var currentLayer = playerSutra?.CurrentLayer ?? 0;

            if (currentLayer >= sutra.MaxLayer)
            {
                return new HeartSutraUpgradeResultDto
                {
                    Success = false,
                    NewLayer = currentLayer,
                    Message = "心法已达到最高层"
                };
            }

            var nextLayer = currentLayer + 1;
            List<SutraLayerConfig> layerConfigs = [];
            if (!string.IsNullOrWhiteSpace(sutra.LayersJson))
            {
                try
                {
                    layerConfigs = JsonSerializer.Deserialize<List<SutraLayerConfig>>(sutra.LayersJson) ?? [];
                }
                catch
                {
                    // ignore
                }
            }

            var layerConfig = layerConfigs.FirstOrDefault(l => l.Layer == nextLayer);
            if (layerConfig == null)
            {
                return new HeartSutraUpgradeResultDto
                {
                    Success = false,
                    NewLayer = currentLayer,
                    Message = "心法层级配置不存在"
                };
            }

            if (member.Contribution < layerConfig.ContributionCost)
            {
                return new HeartSutraUpgradeResultDto
                {
                    Success = false,
                    NewLayer = currentLayer,
                    Message = $"贡献值不足，需要 {layerConfig.ContributionCost}，当前 {member.Contribution}"
                };
            }

            if (player.Gold < layerConfig.GoldCost)
            {
                return new HeartSutraUpgradeResultDto
                {
                    Success = false,
                    NewLayer = currentLayer,
                    Message = $"金币不足，需要 {layerConfig.GoldCost}，当前 {player.Gold}"
                };
            }

            try
            {
                _dbContext.BeginTransaction();

                member.Contribution -= layerConfig.ContributionCost;
                member.LastActiveTime = DateTime.Now;
                await _memberRepository.UpdateAsync(member);

                player.Gold -= layerConfig.GoldCost;
                player.LastUpdateTime = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(layerConfig.UnlockBuffId))
                {
                    var passiveIds = new List<string>();
                    if (!string.IsNullOrWhiteSpace(player.PassiveIdsJson))
                    {
                        try
                        {
                            passiveIds = JsonSerializer.Deserialize<List<string>>(player.PassiveIdsJson) ?? [];
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    if (!passiveIds.Contains(layerConfig.UnlockBuffId))
                    {
                        passiveIds.Add(layerConfig.UnlockBuffId);
                        player.PassiveIdsJson = JsonSerializer.Serialize(passiveIds);
                    }
                }

                var unlockedSkillName = (string?)null;
                var unlockedSkillId = (int?)null;

                if (!string.IsNullOrWhiteSpace(layerConfig.UnlockSkillId)
                    && int.TryParse(layerConfig.UnlockSkillId, out var parsedSkillId)
                    && SkillData.Skills.ContainsKey(parsedSkillId))
                {
                    var ownedSkillIds = (player.OwnedSkillIds ?? [])
                        .Select(id => id?.Trim())
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.Ordinal)
                        .Select(id => id!)
                        .ToList();

                    var normalizedSkillId = parsedSkillId.ToString();
                    if (!ownedSkillIds.Contains(normalizedSkillId, StringComparer.Ordinal))
                    {
                        ownedSkillIds.Add(normalizedSkillId);
                        player.OwnedSkillIds = ownedSkillIds;

                        var equippedIds = (player.SkillIds ?? []).ToList();
                        if (equippedIds.Count < 6 && !equippedIds.Contains(normalizedSkillId, StringComparer.Ordinal))
                        {
                            equippedIds.Add(normalizedSkillId);
                            player.SkillIds = equippedIds;
                        }
                    }

                    unlockedSkillId = parsedSkillId;
                    unlockedSkillName = SkillData.Skills[parsedSkillId].Name;
                }

                await _userRepository.UpdateAsync(player);

                if (playerSutra == null)
                {
                    playerSutra = new PlayerHeartSutraEntity
                    {
                        GID = Guid.NewGuid().ToString("N"),
                        PlayerId = playerId,
                        SutraId = sutraId,
                        CurrentLayer = nextLayer,
                        LastUpdateTime = DateTime.Now
                    };
                    await _playerSutraRepository.AddAsync(playerSutra);
                }
                else
                {
                    playerSutra.CurrentLayer = nextLayer;
                    playerSutra.LastUpdateTime = DateTime.Now;
                    await _playerSutraRepository.UpdateAsync(playerSutra);
                }

                _dbContext.CommitTransaction();

                return new HeartSutraUpgradeResultDto
                {
                    Success = true,
                    NewLayer = nextLayer,
                    Message = unlockedSkillName != null ? $"心法升级成功，习得技能：{unlockedSkillName}" : "心法升级成功",
                    UnlockedSkillId = unlockedSkillId,
                    UnlockedSkillName = unlockedSkillName
                };
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Upgrade sutra failed. PlayerId={PlayerId}, SutraId={SutraId}", playerId, sutraId);
                throw;
            }
        }

        /// <summary>
        /// 获取捐献状态。
        /// </summary>
        public async Task<DonationStatusDto> GetDonationStatusAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                return new DonationStatusDto { CanDonate = false, AlreadyDonatedToday = false };
            }

            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var existingRecord = await _donationRecordRepository.GetFirstAsync(r => r.PlayerId == playerId && r.DonateDate == today);

            var goldAmount = existingRecord?.GoldDonated ?? 10000L;
            var contributionReward = existingRecord != null
                ? existingRecord.ContributionEarned
                : (int)(goldAmount / 1000 * 10);

            return new DonationStatusDto
            {
                CanDonate = existingRecord == null,
                GoldAmount = goldAmount,
                ContributionReward = contributionReward,
                AlreadyDonatedToday = existingRecord != null
            };
        }

        /// <summary>
        /// 执行捐献。
        /// </summary>
        public async Task<DonationResultDto> DonateAsync(string playerId, long goldAmount = 0)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                throw new InvalidOperationException("您未加入任何宗门");
            }

            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var existingRecord = await _donationRecordRepository.GetFirstAsync(r => r.PlayerId == playerId && r.DonateDate == today);
            if (existingRecord != null)
            {
                throw new InvalidOperationException("今日已捐献，请明日再来");
            }

            // 允许的捐献金额
            var allowedAmounts = new[] { 1000L, 5000L, 10000L, 50000L };
            if (goldAmount <= 0) goldAmount = 10000;
            if (!allowedAmounts.Contains(goldAmount))
            {
                throw new InvalidOperationException($"捐献金额只能是 {string.Join("/", allowedAmounts)}");
            }

            // 贡献比例：每1000金币=10贡献
            var contributionReward = (int)(goldAmount / 1000 * 10);

            if (player.Gold < goldAmount)
            {
                throw new InvalidOperationException($"金币不足，需要 {goldAmount}，当前 {player.Gold}");
            }

            var now = DateTime.Now;

            try
            {
                _dbContext.BeginTransaction();

                player.Gold -= goldAmount;
                player.GuildContribution += contributionReward;
                player.LastUpdateTime = now;
                await _userRepository.UpdateAsync(player);

                member.Contribution += contributionReward;
                member.TotalContribution += contributionReward;
                member.LastActiveTime = now;
                await _memberRepository.UpdateAsync(member);

                var guild = await _guildRepository.GetByIdAsync(member.GuildId);
                if (guild != null && !guild.IsDeleted)
                {
                    guild.TotalDonation += (int)goldAmount;
                    guild.LastUpdateTime = now;
                    await _guildRepository.UpdateAsync(guild);
                }

                var record = new SectDonationRecordEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PlayerId = playerId,
                    GuildId = member.GuildId,
                    GoldDonated = goldAmount,
                    ContributionEarned = contributionReward,
                    DonateTime = now,
                    DonateDate = today
                };
                await _donationRecordRepository.AddAsync(record);

                _dbContext.CommitTransaction();

                // 更新宗门任务进度
                await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
                {
                    ObjectiveType = ObjectiveType.Donate,
                    Delta = 1
                });

                return new DonationResultDto
                {
                    Success = true,
                    GoldSpent = goldAmount,
                    ContributionEarned = contributionReward,
                    Message = "捐献成功"
                };
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Donate failed. PlayerId={PlayerId}", playerId);
                throw;
            }
        }

        /// <summary>
        /// 获取宗门弟子列表。
        /// </summary>
        public async Task<List<SectDiscipleDto>> GetDisciplesAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                return [];
            }

            var members = await _memberRepository.GetListAsync(m => m.GuildId == member.GuildId);
            var playerIds = members.Select(m => m.PlayerId).ToList();
            var users = await _userRepository.GetListAsync(u => playerIds.Contains(u.GID));
            var userMap = users.ToDictionary(u => u.GID);

            return members
                .OrderByDescending(m => m.Position)
                .ThenByDescending(m => m.Contribution)
                .ThenBy(m => m.JoinTime)
                .Select(m =>
                {
                    userMap.TryGetValue(m.PlayerId, out var user);
                    var element = user?.Element ?? Element.None;
                    return new SectDiscipleDto
                    {
                        PlayerId = m.PlayerId,
                        PlayerName = m.PlayerName,
                        PlayerLevel = m.PlayerLevel,
                        Position = m.Position.ToString(),
                        Contribution = m.Contribution,
                        Element = element.ToString().ToLowerInvariant(),
                        ElementName = element switch
                        {
                            Element.None => "无",
                            Element.Metal => "金",
                            Element.Wood => "木",
                            Element.Water => "水",
                            Element.Fire => "火",
                            Element.Earth => "土",
                            Element.Wind => "风",
                            Element.Ice => "冰",
                            Element.Thunder => "雷",
                            _ => "无"
                        },
                        Profession = user?.Profession ?? "warrior",
                        ProfessionName = PlayerProfessionCatalog.GetDisplayName(user?.Profession),
                        TotalBattles = user?.TotalBattles ?? 0,
                        WinBattles = user?.WinBattles ?? 0,
                        IsSelf = m.PlayerId == playerId
                    };
                })
                .ToList();
        }

        /// <summary>
        /// 切磋。
        /// </summary>
        public async Task<SparResultDto> SparAsync(string playerId, string targetPlayerId)
        {
            if (playerId == targetPlayerId)
            {
                throw new InvalidOperationException("不能与自己切磋");
            }

            var player = await GetRequiredPlayerAsync(playerId);
            var target = await GetRequiredPlayerAsync(targetPlayerId);

            var playerMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            var targetMember = await _memberRepository.GetFirstAsync(m => m.PlayerId == targetPlayerId);

            if (playerMember == null || targetMember == null || playerMember.GuildId != targetMember.GuildId)
            {
                throw new InvalidOperationException("双方必须在同一公会");
            }

            // Simplified PvP: use level and random factor to determine winner
            var random = new Random();
            var playerPower = player.Level * 100 + random.Next(0, 50);
            var targetPower = target.Level * 100 + random.Next(0, 50);

            var winnerId = playerPower >= targetPower ? playerId : targetPlayerId;
            var winnerName = playerPower >= targetPower ? GetDisplayPlayerName(player) : GetDisplayPlayerName(target);

            var battleLog = new
            {
                Player1 = new { Id = playerId, Name = GetDisplayPlayerName(player), Power = playerPower },
                Player2 = new { Id = targetPlayerId, Name = GetDisplayPlayerName(target), Power = targetPower },
                Rounds = new[]
                {
                    new { Round = 1, Attacker = playerId, Damage = Math.Max(1, playerPower / 10), TargetHp = Math.Max(0, 100 - playerPower / 10) },
                    new { Round = 2, Attacker = targetPlayerId, Damage = Math.Max(1, targetPower / 10), TargetHp = Math.Max(0, 100 - targetPower / 10) }
                }
            };

            return new SparResultDto
            {
                WinnerId = winnerId,
                WinnerName = winnerName,
                BattleLogJson = JsonSerializer.Serialize(battleLog)
            };
        }

        /// <summary>
        /// 获取宗门大比状态。
        /// </summary>
        public async Task<SectTournamentStatusDto?> GetSectTournamentStatusAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                return null;
            }

            var tournament = await _tournamentRepository.GetFirstAsync(t => t.GuildId == member.GuildId && t.State != 2);
            if (tournament == null)
            {
                return null;
            }

            var matches = await _tournamentMatchRepository.GetListAsync(m => m.TournamentId == tournament.TournamentId);
            var participants = matches
                .SelectMany(m => new[] { m.Player1Name, m.Player2Name })
                .Distinct()
                .ToList();

            var myRank = 0;
            if (!string.IsNullOrWhiteSpace(tournament.ResultsJson))
            {
                try
                {
                    var results = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(tournament.ResultsJson) ?? [];
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].TryGetValue("PlayerId", out var pid) && pid?.ToString() == playerId)
                        {
                            myRank = i + 1;
                            break;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return new SectTournamentStatusDto
            {
                TournamentId = tournament.TournamentId,
                State = tournament.State,
                StartTime = tournament.StartTime,
                EndTime = tournament.EndTime,
                MyRank = myRank,
                Participants = participants
            };
        }

        /// <summary>
        /// 获取宗门大比对战记录。
        /// </summary>
        public async Task<List<SectTournamentMatchDto>> GetSectTournamentMatchesAsync(string tournamentId)
        {
            var matches = await _tournamentMatchRepository.GetListAsync(m => m.TournamentId == tournamentId);

            return matches
                .OrderBy(m => m.Round)
                .ThenBy(m => m.MatchTime)
                .Select(m => new SectTournamentMatchDto
                {
                    MatchId = m.MatchId,
                    Player1Name = m.Player1Name,
                    Player2Name = m.Player2Name,
                    WinnerName = m.WinnerId == m.Player1Id ? m.Player1Name : m.WinnerId == m.Player2Id ? m.Player2Name : null,
                    Round = m.Round,
                    BattleLogJson = m.BattleLogJson
                })
                .ToList();
        }

        /// <summary>
        /// 领取宗门大比奖励。
        /// </summary>
        public Task<SectTournamentRewardDto?> ClaimSectTournamentRewardAsync(string playerId, string tournamentId)
        {
            // Placeholder: reward claiming logic would go here
            _logger.LogInformation("ClaimSectTournamentReward called. PlayerId={PlayerId}, TournamentId={TournamentId}", playerId, tournamentId);
            return Task.FromResult<SectTournamentRewardDto?>(null);
        }

        /// <summary>
        /// 获取天骄赛状态。
        /// </summary>
        public async Task<GeniusTournamentStatusDto?> GetGeniusTournamentStatusAsync()
        {
            var tournament = await _geniusTournamentRepository.GetFirstAsync(t => t.State != 2);
            if (tournament == null)
            {
                return null;
            }

            var matches = await _geniusTournamentMatchRepository.GetListAsync(m => m.TournamentId == tournament.TournamentId);
            var participants = matches
                .SelectMany(m => new[] { m.Player1Name, m.Player2Name })
                .Distinct()
                .ToList();

            return new GeniusTournamentStatusDto
            {
                TournamentId = tournament.TournamentId,
                Season = tournament.Season,
                State = tournament.State,
                StartTime = tournament.StartTime,
                EndTime = tournament.EndTime,
                Participants = participants
            };
        }

        /// <summary>
        /// 获取天骄赛对战记录。
        /// </summary>
        public async Task<List<GeniusTournamentMatchDto>> GetGeniusTournamentMatchesAsync(string tournamentId)
        {
            var matches = await _geniusTournamentMatchRepository.GetListAsync(m => m.TournamentId == tournamentId);

            return matches
                .OrderBy(m => m.Round)
                .ThenBy(m => m.MatchTime)
                .Select(m => new GeniusTournamentMatchDto
                {
                    MatchId = m.MatchId,
                    Player1Name = m.Player1Name,
                    Player1SectName = m.Player1SectName,
                    Player2Name = m.Player2Name,
                    Player2SectName = m.Player2SectName,
                    WinnerName = m.WinnerId == m.Player1Id ? m.Player1Name : m.WinnerId == m.Player2Id ? m.Player2Name : null,
                    Round = m.Round
                })
                .ToList();
        }

        /// <summary>
        /// 领取天骄赛奖励。
        /// </summary>
        public Task<GeniusTournamentRewardDto?> ClaimGeniusTournamentRewardAsync(string playerId, string tournamentId)
        {
            // Placeholder: reward claiming logic would go here
            _logger.LogInformation("ClaimGeniusTournamentReward called. PlayerId={PlayerId}, TournamentId={TournamentId}", playerId, tournamentId);
            return Task.FromResult<GeniusTournamentRewardDto?>(null);
        }

        /// <summary>
        /// 获取宗门Boss状态。
        /// </summary>
        public async Task<SectBossStatusDto?> GetSectBossStatusAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                return null;
            }

            var instance = await _bossInstanceRepository.GetFirstAsync(i => i.GuildId == member.GuildId && i.State == 1);
            if (instance == null)
            {
                return null;
            }

            return new SectBossStatusDto
            {
                BossName = instance.BossName,
                CurrentHp = instance.CurrentHp,
                MaxHp = instance.MaxHp,
                EndTime = instance.EndAtUtc,
                MyDamage = 0,
                MyRank = 0,
                CanClaim = instance.CurrentHp <= 0
            };
        }

        /// <summary>
        /// 攻击宗门Boss。
        /// </summary>
        public async Task<SectBossActionResultDto> AttackSectBossAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                throw new InvalidOperationException("您未加入任何宗门");
            }

            var instance = await _bossInstanceRepository.GetFirstAsync(i => i.GuildId == member.GuildId && i.State == 1);
            if (instance == null)
            {
                throw new InvalidOperationException("当前没有可挑战的宗门Boss");
            }

            if (instance.CurrentHp <= 0)
            {
                throw new InvalidOperationException("Boss已被击败");
            }

            if (instance.EndAtUtc < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Boss挑战已结束");
            }

            var player = await GetRequiredPlayerAsync(playerId);
            var random = new Random();
            var baseDamage = player.Level * 50 + (player.TotalBattles > 0 ? player.WinBattles * 10 : 0);
            var isCrit = random.Next(100) < 20;
            var damage = isCrit ? baseDamage * 2 : baseDamage;
            damage = Math.Max(1, damage + random.Next(-10, 10));

            instance.CurrentHp = Math.Max(0, instance.CurrentHp - damage);
            instance.LastUpdateTime = DateTime.Now;
            await _bossInstanceRepository.UpdateAsync(instance);

            // 更新宗门任务进度
            await _questService.RecordObjectiveEventAsync(playerId, new QuestObjectiveEvent
            {
                ObjectiveType = ObjectiveType.SectBossDamage,
                Delta = damage
            });

            return new SectBossActionResultDto
            {
                Damage = damage,
                IsCrit = isCrit,
                BossCurrentHp = instance.CurrentHp,
                BossDefeated = instance.CurrentHp <= 0
            };
        }

        /// <summary>
        /// 领取宗门Boss奖励。
        /// </summary>
        public Task<SectBossRewardDto?> ClaimSectBossRewardAsync(string playerId)
        {
            // Placeholder: reward claiming logic would go here
            _logger.LogInformation("ClaimSectBossReward called. PlayerId={PlayerId}", playerId);
            return Task.FromResult<SectBossRewardDto?>(null);
        }

        /// <summary>
        /// 获取宗门商店商品列表。
        /// </summary>
        public async Task<List<SectShopItemDto>> GetSectShopItemsAsync(string playerId)
        {
            var items = await _shopItemRepository.GetAllAsync();
            var today = DateTime.Now.ToString("yyyy-MM-dd");

            // Note: daily purchase tracking would require a separate record table
            // For now, PurchasedToday is always 0
            return items
                .OrderBy(i => i.SortOrder)
                .Select(i => new SectShopItemDto
                {
                    GID = i.GID,
                    ItemId = i.ItemId,
                    ItemName = i.ItemId, // Would resolve from ItemTemplate in production
                    ItemType = i.ItemType,
                    ContributionCost = i.ContributionCost,
                    Stock = i.Stock,
                    DailyLimit = i.DailyLimit,
                    PurchasedToday = 0
                })
                .ToList();
        }

        /// <summary>
        /// 购买宗门商店商品。
        /// </summary>
        public async Task<SectShopPurchaseResultDto> PurchaseSectShopItemAsync(string playerId, string shopItemId)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null)
            {
                throw new InvalidOperationException("您未加入任何宗门");
            }

            var item = await _shopItemRepository.GetByIdAsync(shopItemId);
            if (item == null)
            {
                throw new InvalidOperationException("商品不存在");
            }

            if (player.GuildContribution < item.ContributionCost)
            {
                return new SectShopPurchaseResultDto
                {
                    Success = false,
                    Message = $"贡献值不足，需要 {item.ContributionCost}，当前 {player.GuildContribution}"
                };
            }

            if (item.Stock == 0)
            {
                return new SectShopPurchaseResultDto
                {
                    Success = false,
                    Message = "商品已售罄"
                };
            }

            try
            {
                _dbContext.BeginTransaction();

                player.GuildContribution -= item.ContributionCost;
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);

                if (item.Stock > 0)
                {
                    item.Stock -= 1;
                    item.LastUpdateTime = DateTime.Now;
                    await _shopItemRepository.UpdateAsync(item);
                }

                _dbContext.CommitTransaction();

                return new SectShopPurchaseResultDto
                {
                    Success = true,
                    Message = "购买成功"
                };
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "Purchase sect shop item failed. PlayerId={PlayerId}, ItemId={ItemId}", playerId, shopItemId);
                throw;
            }
        }

        /// <summary>
        /// 获取宗门福利列表。
        /// </summary>
        public async Task<List<SectBlessingDto>> GetSectBlessingsAsync(string playerId)
        {
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            var guildLevel = 0;
            if (member != null)
            {
                var guild = await _guildRepository.GetByIdAsync(member.GuildId);
                guildLevel = guild?.Level ?? 0;
            }

            var blessings = await _blessingRepository.GetAllAsync();

            return blessings
                .OrderBy(b => b.SortOrder)
                .Select(b => new SectBlessingDto
                {
                    BlessingId = b.BlessingId,
                    Name = b.Name,
                    Description = b.Description,
                    RequiredGuildLevel = b.RequiredGuildLevel,
                    IsActive = member != null && guildLevel >= b.RequiredGuildLevel
                })
                .ToList();
        }

        /// <summary>
        /// 领取宗门福利。
        /// </summary>
        public Task<bool> ClaimSectBlessingAsync(string playerId, string blessingId)
        {
            // Placeholder: buff application logic would go here
            _logger.LogInformation("ClaimSectBlessing called. PlayerId={PlayerId}, BlessingId={BlessingId}", playerId, blessingId);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 获取宗门任务列表。
        /// </summary>
        public async Task<List<SectTaskDto>> GetSectTasksAsync(string playerId)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var member = await _memberRepository.GetFirstAsync(m => m.PlayerId == playerId);
            if (member == null) return [];

            // 触发自动接取逻辑，确保每日重置后宗门任务自动进入"进行中"状态。
            await _questService.GetPlayerInProgressQuestsAsync(playerId);

            // 查询宗门任务配置（QuestType=5）
            var sectQuests = await _dbContext.Db.Queryable<QuestConfigEntity>()
                .Where(q => q.QuestType == 5 && q.IsEnabled)
                .OrderBy(q => q.SortOrder)
                .ToListAsync();

            if (sectQuests.Count == 0) return [];

            var questIds = sectQuests.Select(q => q.QuestId).ToList();

            // 查询玩家进度
            var progressList = await _dbContext.Db.Queryable<QuestProgressEntity>()
                .Where(p => p.PlayerId == playerId && questIds.Contains(p.QuestId))
                .ToListAsync();
            var progressMap = progressList.ToDictionary(p => p.QuestId);

            // 查询已完成记录
            var completedIds = await _dbContext.Db.Queryable<QuestCompletedRecordEntity>()
                .Where(c => c.PlayerId == playerId && questIds.Contains(c.QuestId))
                .Select(c => c.QuestId)
                .ToListAsync();
            var completedSet = completedIds.ToHashSet();

            var result = new List<SectTaskDto>();
            foreach (var quest in sectQuests)
            {
                progressMap.TryGetValue(quest.QuestId, out var progress);

                // 解析目标数量
                var targetCount = 1;
                if (!string.IsNullOrEmpty(quest.ObjectivesJson))
                {
                    try
                    {
                        var objectives = JsonSerializer.Deserialize<List<JsonElement>>(quest.ObjectivesJson) ?? [];
                        if (objectives.Count > 0)
                        {
                            targetCount = objectives[0].GetProperty("Count").GetInt32();
                        }
                    }
                    catch { }
                }

                var currentProgress = 0;
                var isCompleted = false;
                var rewardClaimed = false;

                if (progress != null)
                {
                    // 解析目标进度
                    if (!string.IsNullOrEmpty(progress.ObjectiveProgressJson))
                    {
                        try
                        {
                            var objProgress = JsonSerializer.Deserialize<List<int>>(progress.ObjectiveProgressJson) ?? [];
                            currentProgress = objProgress.Count > 0 ? objProgress[0] : 0;
                        }
                        catch { }
                    }
                    isCompleted = progress.Status == 2 || progress.Status == 3;
                }
                else if (completedSet.Contains(quest.QuestId))
                {
                    rewardClaimed = true;
                    currentProgress = targetCount;
                }

                // 贡献奖励：默认100，可在 RewardItemsJson 中配置
                var contributionReward = 100;
                if (!string.IsNullOrEmpty(quest.RewardItemsJson))
                {
                    try
                    {
                        var rewardJson = JsonSerializer.Deserialize<JsonElement>(quest.RewardItemsJson);
                        if (rewardJson.TryGetProperty("contribution", out var cVal))
                            contributionReward = cVal.GetInt32();
                    }
                    catch { }
                }

                result.Add(new SectTaskDto
                {
                    QuestId = quest.QuestId,
                    Name = quest.QuestName,
                    Icon = "📜",
                    Description = quest.Description,
                    TypeText = "宗门日常",
                    CurrentProgress = currentProgress,
                    TargetProgress = targetCount,
                    IsCompleted = isCompleted,
                    RewardClaimed = rewardClaimed,
                    ContributionReward = contributionReward,
                    GoldReward = quest.RewardGold
                });
            }

            return result;
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

        private static string GetDisplayPlayerName(UserEntity player)
        {
            return string.IsNullOrWhiteSpace(player.Name)
                ? $"玩家{player.GID[..Math.Min(6, player.GID.Length)]}"
                : player.Name;
        }
    }
}
