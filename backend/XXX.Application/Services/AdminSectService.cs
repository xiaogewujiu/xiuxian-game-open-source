using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 后台宗门管理服务实现。
    /// </summary>
    public class AdminSectService : IAdminSectService
    {
        private readonly IRepository<SectTemplateEntity> _sectTemplateRepository;
        private readonly IRepository<HeartSutraTemplateEntity> _heartSutraRepository;
        private readonly IRepository<SectBossTemplateEntity> _bossTemplateRepository;
        private readonly IRepository<SectTournamentScheduleEntity> _tournamentScheduleRepository;
        private readonly IRepository<SectShopItemEntity> _shopItemRepository;
        private readonly IRepository<SectBlessingConfigEntity> _blessingRepository;
        private readonly IRepository<GuildEntity> _guildRepository;

        /// <summary>
        /// 初始化后台宗门管理服务。
        /// </summary>
        public AdminSectService(
            IRepository<SectTemplateEntity> sectTemplateRepository,
            IRepository<HeartSutraTemplateEntity> heartSutraRepository,
            IRepository<SectBossTemplateEntity> bossTemplateRepository,
            IRepository<SectTournamentScheduleEntity> tournamentScheduleRepository,
            IRepository<SectShopItemEntity> shopItemRepository,
            IRepository<SectBlessingConfigEntity> blessingRepository,
            IRepository<GuildEntity> guildRepository)
        {
            _sectTemplateRepository = sectTemplateRepository;
            _heartSutraRepository = heartSutraRepository;
            _bossTemplateRepository = bossTemplateRepository;
            _tournamentScheduleRepository = tournamentScheduleRepository;
            _shopItemRepository = shopItemRepository;
            _blessingRepository = blessingRepository;
            _guildRepository = guildRepository;
        }

        #region SectTemplate CRUD

        /// <inheritdoc />
        public async Task<List<AdminSectTemplateListItemDto>> GetSectTemplatesAsync()
        {
            var templates = await _sectTemplateRepository.Db.Queryable<SectTemplateEntity>()
                .OrderBy(t => t.SortOrder)
                .OrderBy(t => t.SectId)
                .ToListAsync();

            var allGuilds = await _guildRepository.GetListAsync(g => !g.IsDeleted);

            return templates.Select(t =>
            {
                var memberCount = allGuilds
                    .Where(g => g.SectTemplateId == t.SectId)
                    .Sum(g => g.MemberCount);

                return new AdminSectTemplateListItemDto
                {
                    SectId = t.SectId,
                    Name = t.Name,
                    IsEnabled = t.IsEnabled,
                    SortOrder = t.SortOrder,
                    MemberCount = memberCount
                };
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<AdminSectTemplateDetailDto?> GetSectTemplateAsync(string sectId)
        {
            if (string.IsNullOrWhiteSpace(sectId)) return null;
            var entity = await _sectTemplateRepository.GetByIdAsync(sectId.Trim());
            return entity == null ? null : MapSectTemplate(entity);
        }

        /// <inheritdoc />
        public async Task<AdminSectTemplateDetailDto> SaveSectTemplateAsync(AdminSectTemplateDetailDto request)
        {
            var sectId = (request.SectId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sectId))
            {
                throw new InvalidOperationException("宗门编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("宗门名称不能为空。");
            }

            var entity = await _sectTemplateRepository.GetByIdAsync(sectId) ?? new SectTemplateEntity
            {
                SectId = sectId
            };

            entity.Name = name;
            entity.Description = request.Description;
            entity.Icon = request.Icon;
            entity.PortraitPath = request.PortraitPath;
            entity.HeartSutraIdsJson = request.HeartSutraIdsJson;
            entity.IsEnabled = request.IsEnabled;
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (await _sectTemplateRepository.GetByIdAsync(sectId) == null)
            {
                await _sectTemplateRepository.AddAsync(entity);
            }
            else
            {
                await _sectTemplateRepository.UpdateAsync(entity);
            }

            return MapSectTemplate(entity);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSectTemplateAsync(string sectId)
        {
            if (string.IsNullOrWhiteSpace(sectId)) return false;
            var deleted = await _sectTemplateRepository.DeleteAsync(sectId.Trim());
            return deleted > 0;
        }

        #endregion

        #region HeartSutra CRUD

        /// <inheritdoc />
        public async Task<List<AdminHeartSutraListItemDto>> GetHeartSutrasAsync(string sectId = "")
        {
            var query = _heartSutraRepository.Db.Queryable<HeartSutraTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(sectId))
            {
                query = query.Where(s => s.SectId == sectId.Trim());
            }

            var sutras = await query
                .OrderBy(s => s.SortOrder)
                .OrderBy(s => s.SutraId)
                .ToListAsync();

            var sectIds = sutras.Select(s => s.SectId).Distinct().ToList();
            var sects = await _sectTemplateRepository.GetListAsync(s => sectIds.Contains(s.SectId));
            var sectMap = sects.ToDictionary(s => s.SectId, s => s.Name);

            return sutras.Select(s => new AdminHeartSutraListItemDto
            {
                SutraId = s.SutraId,
                Name = s.Name,
                SectId = s.SectId,
                SectName = sectMap.GetValueOrDefault(s.SectId, s.SectId),
                MaxLayer = s.MaxLayer
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<AdminHeartSutraDetailDto?> GetHeartSutraAsync(string sutraId)
        {
            if (string.IsNullOrWhiteSpace(sutraId)) return null;
            var entity = await _heartSutraRepository.GetByIdAsync(sutraId.Trim());
            return entity == null ? null : MapHeartSutra(entity);
        }

        /// <inheritdoc />
        public async Task<AdminHeartSutraDetailDto> SaveHeartSutraAsync(AdminHeartSutraDetailDto request)
        {
            var sutraId = (request.SutraId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sutraId))
            {
                throw new InvalidOperationException("心法编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("心法名称不能为空。");
            }

            var entity = await _heartSutraRepository.GetByIdAsync(sutraId) ?? new HeartSutraTemplateEntity
            {
                SutraId = sutraId
            };

            entity.Name = name;
            entity.Description = request.Description;
            entity.SectId = request.SectId;
            entity.MaxLayer = Math.Max(1, request.MaxLayer);
            entity.LayersJson = request.LayersJson;
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (await _heartSutraRepository.GetByIdAsync(sutraId) == null)
            {
                await _heartSutraRepository.AddAsync(entity);
            }
            else
            {
                await _heartSutraRepository.UpdateAsync(entity);
            }

            return MapHeartSutra(entity);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteHeartSutraAsync(string sutraId)
        {
            if (string.IsNullOrWhiteSpace(sutraId)) return false;
            var deleted = await _heartSutraRepository.DeleteAsync(sutraId.Trim());
            return deleted > 0;
        }

        #endregion

        #region SectBossTemplate CRUD

        /// <inheritdoc />
        public async Task<List<AdminSectBossTemplateListItemDto>> GetSectBossTemplatesAsync()
        {
            var templates = await _bossTemplateRepository.Db.Queryable<SectBossTemplateEntity>()
                .OrderBy(t => t.SortOrder)
                .OrderBy(t => t.BossId)
                .ToListAsync();

            return templates.Select(t => new AdminSectBossTemplateListItemDto
            {
                BossId = t.BossId,
                Name = t.Name,
                MonsterTemplateId = t.MonsterTemplateId,
                IsEnabled = t.IsEnabled
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<AdminSectBossTemplateDetailDto?> GetSectBossTemplateAsync(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId)) return null;
            var entity = await _bossTemplateRepository.GetByIdAsync(bossId.Trim());
            return entity == null ? null : MapBossTemplate(entity);
        }

        /// <inheritdoc />
        public async Task<AdminSectBossTemplateDetailDto> SaveSectBossTemplateAsync(AdminSectBossTemplateDetailDto request)
        {
            var bossId = (request.BossId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(bossId))
            {
                throw new InvalidOperationException("Boss编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Boss名称不能为空。");
            }

            var entity = await _bossTemplateRepository.GetByIdAsync(bossId) ?? new SectBossTemplateEntity
            {
                BossId = bossId
            };

            entity.Name = name;
            entity.MonsterTemplateId = request.MonsterTemplateId;
            entity.PortraitPath = request.PortraitPath;
            entity.IsEnabled = request.IsEnabled;
            entity.DurationMinutes = Math.Max(1, request.DurationMinutes);
            entity.ParticipationRewardContribution = Math.Max(0, request.ParticipationRewardContribution);
            entity.Rank1RewardContribution = Math.Max(0, request.Rank1RewardContribution);
            entity.Rank2RewardContribution = Math.Max(0, request.Rank2RewardContribution);
            entity.Rank3RewardContribution = Math.Max(0, request.Rank3RewardContribution);
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (await _bossTemplateRepository.GetByIdAsync(bossId) == null)
            {
                await _bossTemplateRepository.AddAsync(entity);
            }
            else
            {
                await _bossTemplateRepository.UpdateAsync(entity);
            }

            return MapBossTemplate(entity);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSectBossTemplateAsync(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId)) return false;
            var deleted = await _bossTemplateRepository.DeleteAsync(bossId.Trim());
            return deleted > 0;
        }

        #endregion

        #region SectTournamentSchedule

        /// <inheritdoc />
        public async Task<AdminSectTournamentScheduleDto> GetSectTournamentScheduleAsync()
        {
            var entity = await _tournamentScheduleRepository.GetByIdAsync("default");
            if (entity == null)
            {
                entity = new SectTournamentScheduleEntity();
                await _tournamentScheduleRepository.AddAsync(entity);
            }

            return MapTournamentSchedule(entity);
        }

        /// <inheritdoc />
        public async Task<AdminSectTournamentScheduleDto> SaveSectTournamentScheduleAsync(AdminSectTournamentScheduleDto request)
        {
            var entity = await _tournamentScheduleRepository.GetByIdAsync("default") ?? new SectTournamentScheduleEntity();
            entity.SpawnTimeText = string.IsNullOrWhiteSpace(request.SpawnTimeText) ? "20:00" : request.SpawnTimeText.Trim();
            entity.TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "China Standard Time" : request.TimeZoneId.Trim();
            entity.IsEnabled = request.IsEnabled;
            entity.DurationMinutes = Math.Max(1, request.DurationMinutes);
            entity.LastUpdateTime = DateTime.Now;

            if (await _tournamentScheduleRepository.GetByIdAsync(entity.ScheduleId) == null)
            {
                await _tournamentScheduleRepository.AddAsync(entity);
            }
            else
            {
                await _tournamentScheduleRepository.UpdateAsync(entity);
            }

            return MapTournamentSchedule(entity);
        }

        #endregion

        #region SectShopItem CRUD

        /// <inheritdoc />
        public async Task<List<AdminSectShopItemListItemDto>> GetSectShopItemsAsync()
        {
            var items = await _shopItemRepository.Db.Queryable<SectShopItemEntity>()
                .OrderBy(i => i.SortOrder)
                .OrderBy(i => i.GID)
                .ToListAsync();

            return items.Select(i => new AdminSectShopItemListItemDto
            {
                GID = i.GID,
                ShopId = i.ShopId,
                ItemId = i.ItemId,
                ItemType = i.ItemType,
                ContributionCost = i.ContributionCost
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<AdminSectShopItemDetailDto?> GetSectShopItemAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid)) return null;
            var entity = await _shopItemRepository.GetByIdAsync(gid.Trim());
            return entity == null ? null : MapShopItem(entity);
        }

        /// <inheritdoc />
        public async Task<AdminSectShopItemDetailDto> SaveSectShopItemAsync(AdminSectShopItemDetailDto request)
        {
            var gid = (request.GID ?? string.Empty).Trim();
            var isNew = string.IsNullOrWhiteSpace(gid);

            if (isNew)
            {
                gid = Guid.NewGuid().ToString("N");
            }

            var entity = await _shopItemRepository.GetByIdAsync(gid) ?? new SectShopItemEntity
            {
                GID = gid
            };

            entity.ShopId = request.ShopId;
            entity.ItemId = request.ItemId;
            entity.ItemType = request.ItemType;
            entity.ContributionCost = Math.Max(0, request.ContributionCost);
            entity.Stock = request.Stock;
            entity.DailyLimit = request.DailyLimit;
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (isNew)
            {
                await _shopItemRepository.AddAsync(entity);
            }
            else
            {
                await _shopItemRepository.UpdateAsync(entity);
            }

            return MapShopItem(entity);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSectShopItemAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid)) return false;
            var deleted = await _shopItemRepository.DeleteAsync(gid.Trim());
            return deleted > 0;
        }

        #endregion

        #region SectBlessing CRUD

        /// <inheritdoc />
        public async Task<List<AdminSectBlessingListItemDto>> GetSectBlessingsAsync()
        {
            var blessings = await _blessingRepository.Db.Queryable<SectBlessingConfigEntity>()
                .OrderBy(b => b.SortOrder)
                .OrderBy(b => b.BlessingId)
                .ToListAsync();

            return blessings.Select(b => new AdminSectBlessingListItemDto
            {
                BlessingId = b.BlessingId,
                Name = b.Name,
                RequiredGuildLevel = b.RequiredGuildLevel,
                BuffId = b.BuffId
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<AdminSectBlessingDetailDto?> GetSectBlessingAsync(string blessingId)
        {
            if (string.IsNullOrWhiteSpace(blessingId)) return null;
            var entity = await _blessingRepository.GetByIdAsync(blessingId.Trim());
            return entity == null ? null : MapBlessing(entity);
        }

        /// <inheritdoc />
        public async Task<AdminSectBlessingDetailDto> SaveSectBlessingAsync(AdminSectBlessingDetailDto request)
        {
            var blessingId = (request.BlessingId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(blessingId))
            {
                throw new InvalidOperationException("福利编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("福利名称不能为空。");
            }

            var entity = await _blessingRepository.GetByIdAsync(blessingId) ?? new SectBlessingConfigEntity
            {
                BlessingId = blessingId
            };

            entity.Name = name;
            entity.Description = request.Description;
            entity.RequiredGuildLevel = Math.Max(1, request.RequiredGuildLevel);
            entity.BuffId = request.BuffId;
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (await _blessingRepository.GetByIdAsync(blessingId) == null)
            {
                await _blessingRepository.AddAsync(entity);
            }
            else
            {
                await _blessingRepository.UpdateAsync(entity);
            }

            return MapBlessing(entity);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSectBlessingAsync(string blessingId)
        {
            if (string.IsNullOrWhiteSpace(blessingId)) return false;
            var deleted = await _blessingRepository.DeleteAsync(blessingId.Trim());
            return deleted > 0;
        }

        #endregion

        #region Guild admin

        /// <inheritdoc />
        public async Task<List<AdminGuildListItemDto>> GetGuildsAsync()
        {
            var guilds = await _guildRepository.GetListAsync(g => !g.IsDeleted);

            var sectIds = guilds.Where(g => !string.IsNullOrWhiteSpace(g.SectTemplateId))
                .Select(g => g.SectTemplateId!)
                .Distinct()
                .ToList();
            var sects = sectIds.Count > 0
                ? await _sectTemplateRepository.GetListAsync(s => sectIds.Contains(s.SectId))
                : [];
            var sectMap = sects.ToDictionary(s => s.SectId, s => s.Name);

            return guilds
                .OrderByDescending(g => g.Level)
                .ThenByDescending(g => g.MemberCount)
                .Select(g => new AdminGuildListItemDto
                {
                    GuildId = g.GID,
                    Name = g.Name,
                    SectName = g.SectTemplateId != null ? sectMap.GetValueOrDefault(g.SectTemplateId, string.Empty) : string.Empty,
                    Level = g.Level,
                    MemberCount = g.MemberCount,
                    LeaderName = g.LeaderName,
                    TotalDonation = g.TotalDonation
                })
                .ToList();
        }

        /// <inheritdoc />
        public async Task<AdminGuildDetailDto?> GetGuildAsync(string guildId)
        {
            if (string.IsNullOrWhiteSpace(guildId)) return null;
            var guild = await _guildRepository.GetByIdAsync(guildId.Trim());
            if (guild == null || guild.IsDeleted) return null;

            var sectName = string.Empty;
            if (!string.IsNullOrWhiteSpace(guild.SectTemplateId))
            {
                var sect = await _sectTemplateRepository.GetByIdAsync(guild.SectTemplateId);
                sectName = sect?.Name ?? string.Empty;
            }

            return new AdminGuildDetailDto
            {
                GuildId = guild.GID,
                Name = guild.Name,
                SectTemplateId = guild.SectTemplateId,
                SectName = sectName,
                Level = guild.Level,
                Exp = guild.Exp,
                MemberCount = guild.MemberCount,
                MaxMembers = guild.MaxMembers,
                Funds = guild.Funds,
                LeaderId = guild.LeaderId,
                LeaderName = guild.LeaderName,
                TotalDonation = guild.TotalDonation,
                Announcement = guild.Announcement,
                CreateTime = guild.CreateTime
            };
        }

        #endregion

        #region Mappers

        private static AdminSectTemplateDetailDto MapSectTemplate(SectTemplateEntity entity)
        {
            return new AdminSectTemplateDetailDto
            {
                SectId = entity.SectId,
                Name = entity.Name,
                Description = entity.Description,
                Icon = entity.Icon,
                PortraitPath = entity.PortraitPath,
                HeartSutraIdsJson = entity.HeartSutraIdsJson,
                IsEnabled = entity.IsEnabled,
                SortOrder = entity.SortOrder
            };
        }

        private static AdminHeartSutraDetailDto MapHeartSutra(HeartSutraTemplateEntity entity)
        {
            return new AdminHeartSutraDetailDto
            {
                SutraId = entity.SutraId,
                Name = entity.Name,
                Description = entity.Description,
                SectId = entity.SectId,
                MaxLayer = entity.MaxLayer,
                LayersJson = entity.LayersJson,
                SortOrder = entity.SortOrder
            };
        }

        private static AdminSectBossTemplateDetailDto MapBossTemplate(SectBossTemplateEntity entity)
        {
            return new AdminSectBossTemplateDetailDto
            {
                BossId = entity.BossId,
                Name = entity.Name,
                MonsterTemplateId = entity.MonsterTemplateId,
                PortraitPath = entity.PortraitPath,
                IsEnabled = entity.IsEnabled,
                DurationMinutes = entity.DurationMinutes,
                ParticipationRewardContribution = entity.ParticipationRewardContribution,
                Rank1RewardContribution = entity.Rank1RewardContribution,
                Rank2RewardContribution = entity.Rank2RewardContribution,
                Rank3RewardContribution = entity.Rank3RewardContribution,
                SortOrder = entity.SortOrder
            };
        }

        private static AdminSectTournamentScheduleDto MapTournamentSchedule(SectTournamentScheduleEntity entity)
        {
            return new AdminSectTournamentScheduleDto
            {
                ScheduleId = entity.ScheduleId,
                SpawnTimeText = entity.SpawnTimeText,
                TimeZoneId = entity.TimeZoneId,
                IsEnabled = entity.IsEnabled,
                DurationMinutes = entity.DurationMinutes
            };
        }

        private static AdminSectShopItemDetailDto MapShopItem(SectShopItemEntity entity)
        {
            return new AdminSectShopItemDetailDto
            {
                GID = entity.GID,
                ShopId = entity.ShopId,
                ItemId = entity.ItemId,
                ItemType = entity.ItemType,
                ContributionCost = entity.ContributionCost,
                Stock = entity.Stock,
                DailyLimit = entity.DailyLimit,
                SortOrder = entity.SortOrder
            };
        }

        private static AdminSectBlessingDetailDto MapBlessing(SectBlessingConfigEntity entity)
        {
            return new AdminSectBlessingDetailDto
            {
                BlessingId = entity.BlessingId,
                Name = entity.Name,
                Description = entity.Description,
                RequiredGuildLevel = entity.RequiredGuildLevel,
                BuffId = entity.BuffId,
                SortOrder = entity.SortOrder
            };
        }

        #endregion
    }
}
