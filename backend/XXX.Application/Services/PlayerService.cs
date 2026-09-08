using AutoMapper;
using Microsoft.Extensions.Logging;
using XXX.Achievement;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    /// <summary>
    /// 玩家服务实现。
    /// </summary>
    /// <remarks>
    /// 这里故意不再使用进程内缓存读取玩家基础信息。
    /// 原因是当前项目里有多条业务链会直接改数据库中的金币、经验、装备和排行榜状态，
    /// 如果继续读取旧缓存，前端就会看到“接口成功但人物面板没变化”的假象。
    /// </remarks>
    public class PlayerService : IPlayerService
    {
        /// <summary>
        /// 数据库上下文。
        /// 用于执行事务、原子更新和轻量行锁操作。
        /// </summary>
        private readonly DbContext _dbContext;

        /// <summary>
        /// 玩家仓储。
        /// </summary>
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// 玩家属性点分配仓储。
        /// </summary>
        private readonly IRepository<PlayerAttributeAllocationEntity> _attributeAllocationRepository;

        /// <summary>
        /// 对象映射器。
        /// 负责把玩家实体转换成前端 DTO。
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// 游戏同步服务。
        /// 角色基础数据变化后会用它刷新运行态缓存。
        /// </summary>
        private readonly IGameSyncService _gameSyncService;

        /// <summary>
        /// 玩家属性服务。
        /// 等级、加点、突破变化后都会通过它重算最终属性。
        /// </summary>
        private readonly IPlayerAttributeService _playerAttributeService;

        /// <summary>
        /// 成就服务。
        /// 当前主要用于同步等级类成就进度。
        /// </summary>
        private readonly IAchievementService _achievementService;

        /// <summary>
        /// 玩家服务日志记录器。
        /// </summary>
        private readonly ILogger<PlayerService> _logger;

        /// <summary>
        /// 初始化玩家服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="attributeAllocationRepository">玩家属性点分配仓储。</param>
        /// <param name="mapper">对象映射器。</param>
        /// <param name="gameSyncService">游戏同步服务。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="achievementService">成就服务。</param>
        /// <param name="logger">日志记录器。</param>
        public PlayerService(
            DbContext dbContext,
            IRepository<UserEntity> userRepository,
            IRepository<PlayerAttributeAllocationEntity> attributeAllocationRepository,
            IMapper mapper,
            IGameSyncService gameSyncService,
            IPlayerAttributeService playerAttributeService,
            IAchievementService achievementService,
            ILogger<PlayerService> logger)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _attributeAllocationRepository = attributeAllocationRepository;
            _mapper = mapper;
            _gameSyncService = gameSyncService;
            _playerAttributeService = playerAttributeService;
            _achievementService = achievementService;
            _logger = logger;
        }

        /// <summary>
        /// 根据玩家编号获取玩家基础信息。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>命中的玩家 DTO；不存在时返回空。</returns>
        public async Task<PlayerDto?> GetByIdAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            await EnsurePlayerElementInitializedAsync(user);
            user = await EnsureLevelExpStateInitializedAsync(user);
            user = await EnsureAttributePointStateInitializedAsync(user);
            return await BuildPlayerDtoAsync(user);
        }

        /// <summary>
        /// 根据账号获取玩家基础信息。
        /// </summary>
        /// <param name="account">账号标识。</param>
        /// <returns>命中的玩家 DTO；不存在时返回空。</returns>
        public async Task<PlayerDto?> GetByAccountAsync(string account)
        {
            var user = await _userRepository.GetFirstAsync(u => u.Account == account && !u.IsDeleted);
            if (user == null)
            {
                return null;
            }

            await EnsurePlayerElementInitializedAsync(user);
            user = await EnsureLevelExpStateInitializedAsync(user);
            user = await EnsureAttributePointStateInitializedAsync(user);
            return await BuildPlayerDtoAsync(user);
        }

        /// <summary>
        /// 获取玩家详情信息。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>命中的玩家详情 DTO；不存在时返回空。</returns>
        public async Task<PlayerDetailDto?> GetDetailAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            await EnsurePlayerElementInitializedAsync(user);
            user = await EnsureLevelExpStateInitializedAsync(user);
            user = await EnsureAttributePointStateInitializedAsync(user);
            return await BuildPlayerDetailDtoAsync(user);
        }

        /// <summary>
        /// 创建新玩家。
        /// </summary>
        /// <param name="request">创建请求。</param>
        /// <returns>创建完成后的玩家基础信息。</returns>
        public async Task<PlayerDto> CreateAsync(CreatePlayerRequestDto request)
        {
            if (!PlayerProfessionCatalog.IsPlayable(request.Profession))
            {
                throw new InvalidOperationException("请选择有效职业。");
            }

            var playerId = Guid.NewGuid().ToString("N");
            var user = PlayerManager.CreateNewPlayer(playerId, request.Name, request.Profession);
            user.Account = playerId;
            user.PasswordHash = string.Empty;
            user.LastUpdateTime = DateTime.Now;

            await _userRepository.AddAsync(user);
            await _gameSyncService.SyncPlayerAsync(playerId);
            _logger.LogInformation("玩家 {PlayerId} 已创建，初始等级={Level}，初始境界={RealmAlias}", playerId, user.Level, RealmLevelCatalog.Get(user.Level).Alias);
            return await BuildPlayerDtoAsync(user);
        }

        /// <summary>
        /// 更新玩家可编辑信息。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">更新请求。</param>
        /// <returns>更新后的玩家 DTO；不存在时返回空。</returns>
        public async Task<PlayerDto?> UpdateAsync(string playerId, UpdatePlayerRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                user.Name = request.Name.Trim();
            }

            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            await _gameSyncService.SyncPlayerAsync(playerId);
            return await BuildPlayerDtoAsync(user);
        }

        public async Task<EquipmentAutoSellSettingsDto?> GetEquipmentAutoSellSettingsAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted) return null;
            return new EquipmentAutoSellSettingsDto
            {
                MinEquipmentLevel = user.EquipmentAutoSellMinLevel,
                MinEquipmentQuality = user.EquipmentAutoSellMinQuality
            };
        }

        public async Task<EquipmentAutoSellSettingsDto?> UpdateEquipmentAutoSellSettingsAsync(string playerId, UpdateEquipmentAutoSellSettingsRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted) return null;

            user.EquipmentAutoSellMinLevel = Math.Clamp(request?.MinEquipmentLevel ?? 0, 0, 100);
            user.EquipmentAutoSellMinQuality = Math.Clamp(request?.MinEquipmentQuality ?? 0, 0, 5);
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return new EquipmentAutoSellSettingsDto
            {
                MinEquipmentLevel = user.EquipmentAutoSellMinLevel,
                MinEquipmentQuality = user.EquipmentAutoSellMinQuality
            };
        }

        /// <summary>
        /// 更新玩家头像图片路径。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="avatarImagePath">头像相对路径。</param>
        /// <returns>更新后的玩家 DTO；不存在时返回空。</returns>
        public async Task<PlayerDto?> UpdateAvatarImageAsync(string playerId, string avatarImagePath)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            user.AvatarImagePath = string.IsNullOrWhiteSpace(avatarImagePath) ? null : avatarImagePath.Trim();
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return await BuildPlayerDtoAsync(user);
        }

        /// <summary>
        /// 为玩家增加经验。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">经验变更请求。</param>
        /// <returns>变更后的玩家 DTO；不存在时返回空。</returns>
        public async Task<PlayerDto?> AddExpAsync(string playerId, AddExpRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            var expToAdd = request.Exp > 0 ? request.Exp : request.Amount;
            if (expToAdd <= 0)
            {
                throw new InvalidOperationException("经验值必须大于 0");
            }

            var result = PlayerManager.AddExp(user, expToAdd);
            if (!result.Success)
            {
                throw new InvalidOperationException(result.Message);
            }

            user.XExp = PlayerManager.GetRequiredExp(user);
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            // 中文注释：
            // 升级后必须统一走一次完整属性重算，
            // 否则人物在后续穿戴装备、突破或切换宠物时会回落到另一套等级底板。
            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);

            var latestUser = await _userRepository.GetByIdAsync(playerId);
            if (latestUser != null)
            {
                user = latestUser;
            }

            await SyncLevelAchievementsAsync(user);
            _logger.LogInformation("玩家 {PlayerId} 获得经验 {Exp}，来源={Source}，当前等级={Level}", playerId, expToAdd, request.Source, user.Level);

            return await BuildPlayerDtoAsync(user);
        }

        /// <summary>
        /// 扣除玩家金币。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="amount">扣除数量。</param>
        /// <param name="reason">扣除原因。</param>
        /// <returns>扣除成功返回真。</returns>
        public async Task<bool> DeductGoldAsync(string playerId, long amount, string reason)
        {
            if (amount <= 0)
            {
                return false;
            }

            var now = DateTime.Now;
            var affectedRows = await _dbContext.Db.Updateable<UserEntity>()
                .SetColumns(u => u.Gold == u.Gold - amount)
                .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + amount)
                .SetColumns(u => u.LastUpdateTime == now)
                .Where(u => u.GID == playerId && !u.IsDeleted && u.Gold >= amount)
                .ExecuteCommandAsync();

            if (affectedRows > 0)
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 增加玩家金币。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="amount">增加数量。</param>
        /// <param name="reason">增加原因。</param>
        /// <returns>增加成功返回真。</returns>
        public async Task<bool> AddGoldAsync(string playerId, long amount, string reason)
        {
            if (amount <= 0)
            {
                return false;
            }

            var now = DateTime.Now;
            var affectedRows = await _dbContext.Db.Updateable<UserEntity>()
                .SetColumns(u => u.Gold == u.Gold + amount)
                .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + amount)
                .SetColumns(u => u.LastUpdateTime == now)
                .Where(u => u.GID == playerId && !u.IsDeleted)
                .ExecuteCommandAsync();

            if (affectedRows > 0)
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            return affectedRows > 0;
        }

        /// <summary>
        /// 更新玩家最后登录时间。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>更新成功返回真。</returns>
        public async Task<bool> UpdateLastLoginTimeAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return false;
            }

            user.LastLoginTime = DateTime.Now;
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// 检查玩家是否存在。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>存在且未删除时返回真。</returns>
        public async Task<bool> ExistsAsync(string playerId)
        {
            return await _userRepository.ExistsAsync(u => u.GID == playerId && !u.IsDeleted);
        }

        /// <summary>
        /// 获取玩家属性点总览。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>属性点总览；玩家不存在时返回空。</returns>
        public async Task<PlayerAttributePointOverviewDto?> GetAttributePointOverviewAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            user = await EnsureAttributePointStateInitializedAsync(user);
            return await BuildAttributePointOverviewAsync(user);
        }

        /// <summary>
        /// 分配玩家属性点。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">属性点分配请求。</param>
        /// <returns>分配后的属性点总览；玩家不存在时返回空。</returns>
        public async Task<PlayerAttributePointOverviewDto?> AllocateAttributePointAsync(string playerId, AdjustPlayerAttributePointRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            user = await EnsureAttributePointStateInitializedAsync(user);

            if (!AttributePointConfig.TryResolveDefinition(user.Profession, request.AttributeKey, out var definition) || definition == null)
            {
                throw new InvalidOperationException("不支持的属性类型");
            }

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                await LockAttributePointStateAsync(playerId);

                var latestUser = await _dbContext.Db.Queryable<UserEntity>()
                    .FirstAsync(item => item.GID == playerId && !item.IsDeleted);
                if (latestUser == null)
                {
                    throw new InvalidOperationException("玩家不存在");
                }

                var allocations = await _dbContext.Db.Queryable<PlayerAttributeAllocationEntity>()
                    .Where(item => item.PlayerId == playerId)
                    .ToListAsync();

                var totalEarnedPoints = AttributePointConfig.GetTotalEarnedPoints(latestUser.Level);
                var totalUsedPoints = allocations.Sum(item => item.AllocatedPoints);
                if (totalUsedPoints >= totalEarnedPoints)
                {
                    throw new InvalidOperationException("可用属性点不足");
                }

                var now = DateTime.Now;
                var currentAllocation = allocations.FirstOrDefault(item => item.AttributeKey == definition.Key);
                if (currentAllocation == null)
                {
                    await _dbContext.Db.Insertable(new PlayerAttributeAllocationEntity
                    {
                        AllocationId = Guid.NewGuid().ToString("N"),
                        PlayerId = playerId,
                        AttributeKey = definition.Key,
                        AllocatedPoints = 1,
                        CreateTime = now,
                        LastUpdateTime = now
                    }).ExecuteCommandAsync();
                }
                else
                {
                    currentAllocation.AllocatedPoints += 1;
                    currentAllocation.LastUpdateTime = now;
                    await _dbContext.Db.Updateable(currentAllocation).ExecuteCommandAsync();
                }
            });

            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);

            var latestUser = await _userRepository.GetByIdAsync(playerId);
            return latestUser == null ? null : await BuildAttributePointOverviewAsync(latestUser);
        }

        /// <summary>
        /// 返还玩家已分配的属性点。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">属性点返还请求。</param>
        /// <returns>返还后的属性点总览；玩家不存在时返回空。</returns>
        public async Task<PlayerAttributePointOverviewDto?> RefundAttributePointAsync(string playerId, AdjustPlayerAttributePointRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            user = await EnsureAttributePointStateInitializedAsync(user);

            if (!AttributePointConfig.TryResolveDefinition(user.Profession, request.AttributeKey, out var definition) || definition == null)
            {
                throw new InvalidOperationException("不支持的属性类型");
            }

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                await LockAttributePointStateAsync(playerId);

                var currentAllocation = await _dbContext.Db.Queryable<PlayerAttributeAllocationEntity>()
                    .FirstAsync(item => item.PlayerId == playerId && item.AttributeKey == definition.Key);

                if (currentAllocation == null || currentAllocation.AllocatedPoints <= 0)
                {
                    throw new InvalidOperationException("当前属性没有可返还的点数");
                }

                if (currentAllocation.AllocatedPoints == 1)
                {
                    await _dbContext.Db.Deleteable<PlayerAttributeAllocationEntity>()
                        .Where(item => item.AllocationId == currentAllocation.AllocationId)
                        .ExecuteCommandAsync();
                }
                else
                {
                    currentAllocation.AllocatedPoints -= 1;
                    currentAllocation.LastUpdateTime = DateTime.Now;
                    await _dbContext.Db.Updateable(currentAllocation).ExecuteCommandAsync();
                }
            });

            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);

            var latestUser = await _userRepository.GetByIdAsync(playerId);
            return latestUser == null ? null : await BuildAttributePointOverviewAsync(latestUser);
        }

        /// <summary>
        /// 执行玩家突破。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>突破结果；玩家不存在时返回空。</returns>
        public async Task<PlayerBreakthroughResultDto?> BreakthroughAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            user = await EnsureAttributePointStateInitializedAsync(user);
            var currentConfig = RealmLevelCatalog.Get(user.Level);
            var nextConfig = RealmLevelCatalog.GetNext(user.Level);
            var overview = await BuildBreakthroughOverviewAsync(user);

            if (!currentConfig.IsBreakthroughPoint)
            {
                throw new InvalidOperationException("当前等级无需突破。");
            }

            if (nextConfig == null)
            {
                throw new InvalidOperationException("当前已达到最高境界，无法继续突破。");
            }

            if (user.Exp < user.XExp)
            {
                throw new InvalidOperationException($"经验未满，当前需达到 {user.XExp} 点经验后才能突破至 {nextConfig.Alias}。");
            }

            var insufficientMaterial = overview.RequiredMaterials.FirstOrDefault(item => !item.IsEnough);
            if (insufficientMaterial != null)
            {
                throw new InvalidOperationException($"突破材料不足：{insufficientMaterial.Name} 需要 {insufficientMaterial.Count} 个，当前仅有 {insufficientMaterial.OwnedCount} 个。");
            }

            var breakthroughBonusPercent = Math.Max(0, user.BreakthroughBonusPercent);
            var finalSuccessRate = Math.Min(100, currentConfig.BreakthroughSuccessRate + breakthroughBonusPercent);
            var rollValue = Random.Shared.Next(1, 101);
            var previousLevel = user.Level;
            var succeeded = rollValue <= finalSuccessRate;
            var expLoss = 0;

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                await DeductBreakthroughMaterialsInTransactionAsync(playerId, currentConfig.BreakthroughMaterials);

                if (succeeded)
                {
                    user.Level = Math.Min(LevelConfig.MaxLevel, user.Level + 1);
                    user.Exp = 0;
                    user.XExp = PlayerManager.GetRequiredExp(user);
                }
                else
                {
                    expLoss = (int)Math.Ceiling(user.XExp * currentConfig.BreakthroughExpLossPercent / 100d);
                    user.Exp = Math.Max(0, user.Exp - expLoss);
                }

                user.BreakthroughBonusPercent = 0;

                user.LastUpdateTime = DateTime.Now;
                await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(entity => entity.Level == user.Level)
                    .SetColumns(entity => entity.Exp == user.Exp)
                    .SetColumns(entity => entity.XExp == user.XExp)
                    .SetColumns(entity => entity.BreakthroughBonusPercent == user.BreakthroughBonusPercent)
                    .SetColumns(entity => entity.LastUpdateTime == user.LastUpdateTime)
                    .Where(entity => entity.GID == user.GID && !entity.IsDeleted)
                    .ExecuteCommandAsync();
            });

            if (succeeded)
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            var latestUser = await _userRepository.GetByIdAsync(playerId);
            if (latestUser == null)
            {
                return null;
            }

            var breakthrough = await BuildBreakthroughOverviewAsync(latestUser);
            var result = new PlayerBreakthroughResultDto
            {
                Succeeded = succeeded,
                Message = succeeded
                    ? $"突破成功，已踏入 {breakthrough.CurrentAlias}。"
                    : $"突破失败，损失 {expLoss} 点经验，请重新凝聚修为后再试。",
                PreviousLevel = previousLevel,
                CurrentLevel = latestUser.Level,
                RollValue = rollValue,
                SuccessRate = finalSuccessRate,
                Player = await BuildPlayerDtoAsync(latestUser),
                Breakthrough = breakthrough
            };

            _logger.LogInformation(
                "玩家 {PlayerId} 已完成突破处理，成功={Succeeded}，掷值={RollValue}，成功率={SuccessRate}，等级 {PreviousLevel}->{CurrentLevel}",
                playerId,
                succeeded,
                rollValue,
                finalSuccessRate,
                previousLevel,
                latestUser.Level);

            return result;
        }

        /// <summary>
        /// 为早期缺失灵根字段的玩家补齐初始元素。
        /// 只对 <see cref="Element.None"/> 的历史数据生效，不会覆盖已存在的培养结果。
        /// </summary>
        /// <param name="user">待校验的玩家实体。</param>
        private async Task EnsurePlayerElementInitializedAsync(UserEntity user)
        {
            if (user.Element != Element.None)
            {
                return;
            }

            var availableElements = new[]
            {
                Element.Metal,
                Element.Wood,
                Element.Water,
                Element.Fire,
                Element.Earth
            };

            var index = Math.Abs((user.GID ?? string.Empty).GetHashCode()) % availableElements.Length;
            user.Element = availableElements[index];
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }

        /// <summary>
        /// 确保玩家属性点状态可直接使用。
        /// 当前版本的属性点已完全切到独立分配表，因此这里不再执行旧字段迁移。
        /// </summary>
        /// <param name="user">待处理的玩家实体。</param>
        /// <returns>原样返回的玩家实体。</returns>
        private Task<UserEntity> EnsureAttributePointStateInitializedAsync(UserEntity user)
        {
            return Task.FromResult(user);
        }

        /// <summary>
        /// 校准玩家当前等级对应的升级经验需求。
        /// 老库里可能保留旧版本的 <c>XExp</c>，这里在读取角色时懒同步到当前成长配置。
        /// </summary>
        /// <param name="user">待校准的玩家实体。</param>
        /// <returns>校准后的玩家实体。</returns>
        private async Task<UserEntity> EnsureLevelExpStateInitializedAsync(UserEntity user)
        {
            var expectedRequiredExp = PlayerManager.GetRequiredExp(user);
            if (user.XExp == expectedRequiredExp)
            {
                return user;
            }

            user.XExp = expectedRequiredExp;
            if (user.Exp > user.XExp)
            {
                user.Exp = user.XExp;
            }

            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return user;
        }

        /// <summary>
        /// 构建玩家基础信息 DTO。
        /// 这里会把属性点、突破状态和战斗冷却等衍生结构一并补齐，保证前端拿到的是完整人物面板。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <returns>可直接给前端使用的玩家基础信息 DTO。</returns>
        private async Task<PlayerDto> BuildPlayerDtoAsync(UserEntity user)
        {
            var dto = _mapper.Map<PlayerDto>(user);
            dto.AttributePoints = await BuildAttributePointOverviewAsync(user);
            dto.Breakthrough = await BuildBreakthroughOverviewAsync(user);
            dto.BattleCooldownUntilUtc = user.BattleCooldownUntilUtc;
            dto.BattleCooldownSeconds = CalculateBattleCooldownSeconds(user);
            await _playerAttributeService.ApplyAllBonusesToDtoAsync(user.GID, dto);
            return dto;
        }

        /// <summary>
        /// 构建玩家详细信息 DTO。
        /// 相比基础信息，会额外包含战斗统计和货币累计统计。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <returns>玩家详细信息 DTO。</returns>
        private async Task<PlayerDetailDto> BuildPlayerDetailDtoAsync(UserEntity user)
        {
            var dto = _mapper.Map<PlayerDetailDto>(user);
            dto.AttributePoints = await BuildAttributePointOverviewAsync(user);
            dto.Breakthrough = await BuildBreakthroughOverviewAsync(user);
            dto.BattleCooldownUntilUtc = user.BattleCooldownUntilUtc;
            dto.BattleCooldownSeconds = CalculateBattleCooldownSeconds(user);
            await _playerAttributeService.ApplyAllBonusesToDtoAsync(user.GID, dto);
            return dto;
        }

        /// <summary>
        /// 计算玩家当前剩余战斗冷却秒数。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <returns>剩余冷却秒数；没有冷却或已到期时返回 0。</returns>
        private static int CalculateBattleCooldownSeconds(UserEntity user)
        {
            if (!user.BattleCooldownUntilUtc.HasValue)
            {
                return 0;
            }

            var remaining = user.BattleCooldownUntilUtc.Value - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                return 0;
            }

            return (int)Math.Ceiling(remaining.TotalSeconds);
        }

        /// <summary>
        /// 构建玩家当前突破状态总览。
        /// 用于前端展示境界阶段、成功率、材料清单与提示文案。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <returns>突破状态 DTO。</returns>
        private async Task<PlayerBreakthroughDto> BuildBreakthroughOverviewAsync(UserEntity user)
        {
            var currentConfig = RealmLevelCatalog.Get(user.Level);
            var nextConfig = RealmLevelCatalog.GetNext(user.Level);
            var breakthroughSuccessRate = Math.Min(100, currentConfig.BreakthroughSuccessRate + Math.Max(0, user.BreakthroughBonusPercent));
            var ownedMaterialCounts = await GetOwnedMaterialCountsAsync(user.GID, currentConfig.BreakthroughMaterials);
            var requiredMaterials = currentConfig.BreakthroughMaterials
                .Select(material => new PlayerBreakthroughMaterialDto
                {
                    ItemId = material.ItemId,
                    Name = ResolveBreakthroughMaterialName(material.ItemId),
                    Count = material.Count,
                    OwnedCount = ownedMaterialCounts.TryGetValue(material.ItemId, out var ownedCount) ? ownedCount : 0,
                    IsEnough = (ownedMaterialCounts.TryGetValue(material.ItemId, out var actualCount) ? actualCount : 0) >= material.Count
                })
                .ToList();
            var expReady = user.XExp > 0 && user.Exp >= user.XExp;
            var materialsReady = requiredMaterials.All(item => item.IsEnough);
            var canBreakthrough = currentConfig.IsBreakthroughPoint && expReady && materialsReady;
            var nextRequiredLevel = nextConfig?.Level ?? 0;
            var remainingLevels = nextRequiredLevel > 0
                ? Math.Max(0, nextRequiredLevel - user.Level)
                : 0;

            return new PlayerBreakthroughDto
            {
                CompletedCount = currentConfig.RealmOrder,
                CurrentRealmName = currentConfig.RealmName,
                CurrentRealmLayer = currentConfig.Layer,
                CurrentAlias = currentConfig.Alias,
                NextRealmName = nextConfig?.RealmName ?? "圆满",
                NextAlias = nextConfig?.Alias ?? "境界圆满",
                CurrentLevel = user.Level,
                NextRequiredLevel = nextRequiredLevel,
                RemainingLevels = remainingLevels,
                CanBreakthrough = canBreakthrough,
                IsBreakthroughPoint = currentConfig.IsBreakthroughPoint,
                BreakthroughSuccessRate = breakthroughSuccessRate,
                BreakthroughExpLossPercent = currentConfig.BreakthroughExpLossPercent,
                RequiredMaterials = requiredMaterials,
                AttributeBonusPercent = currentConfig.AttributeBonusPercent,
                RequirementText = BuildBreakthroughRequirementText(currentConfig, nextConfig, expReady, materialsReady, requiredMaterials, user)
            };
        }

        /// <summary>
        /// 汇总玩家当前拥有的突破材料数量。
        /// 统计口径按物品编号分组，不区分来自哪一格背包。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="materials">当前突破所需材料列表。</param>
        /// <returns>物品编号到拥有数量的映射表。</returns>
        private async Task<Dictionary<string, int>> GetOwnedMaterialCountsAsync(string playerId, IEnumerable<RealmBreakthroughMaterial> materials)
        {
            var itemIds = materials
                .Where(material => !string.IsNullOrWhiteSpace(material.ItemId))
                .Select(material => material.ItemId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (itemIds.Count == 0)
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            var grouped = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && itemIds.Contains(item.ItemId))
                .GroupBy(item => item.ItemId)
                .Select(item => new { item.ItemId, Total = SqlSugar.SqlFunc.AggregateSum(item.Quantity) })
                .ToListAsync();

            return grouped.ToDictionary(item => item.ItemId, item => item.Total, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 生成当前突破阶段的提示文案。
        /// 会根据是否关口、经验是否满足、材料是否齐全返回不同说明。
        /// </summary>
        /// <param name="currentConfig">当前等级对应的境界配置。</param>
        /// <param name="nextConfig">下一境界配置。</param>
        /// <param name="expReady">经验是否已满足。</param>
        /// <param name="materialsReady">材料是否已满足。</param>
        /// <param name="requiredMaterials">突破材料清单。</param>
        /// <param name="user">玩家实体。</param>
        /// <returns>前端可直接展示的突破提示文案。</returns>
        private static string BuildBreakthroughRequirementText(
            RealmLevelConfigEntity currentConfig,
            RealmLevelConfigEntity? nextConfig,
            bool expReady,
            bool materialsReady,
            IReadOnlyCollection<PlayerBreakthroughMaterialDto> requiredMaterials,
            UserEntity user)
        {
            if (!currentConfig.IsBreakthroughPoint)
            {
                return $"当前为 {currentConfig.Alias}，继续修炼即可迈向 {nextConfig?.Alias ?? "更高境界"}。";
            }

            if (nextConfig == null)
            {
                return "当前已达到可配置的最高境界。";
            }

            if (!expReady)
            {
                return $"经验未满，当前 {user.Exp}/{user.XExp}，修满后可尝试突破至 {nextConfig.Alias}。";
            }

            if (!materialsReady)
            {
                var missing = requiredMaterials
                    .Where(item => !item.IsEnough)
                    .Select(item => $"{item.Name} {item.OwnedCount}/{item.Count}");
                return $"突破材料不足：{string.Join("、", missing)}。";
            }

            return $"已满足突破条件，成功率 {currentConfig.BreakthroughSuccessRate}% ，失败将损失 {currentConfig.BreakthroughExpLossPercent}% 当前经验。";
        }

        /// <summary>
        /// 在事务内扣除突破所需材料。
        /// 按背包记录逐条扣减，确保并发下数量变化时能及时报错回滚。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="materials">待扣除材料列表。</param>
        private async Task DeductBreakthroughMaterialsInTransactionAsync(string playerId, IEnumerable<RealmBreakthroughMaterial> materials)
        {
            foreach (var material in materials.Where(item => item.Count > 0 && !string.IsNullOrWhiteSpace(item.ItemId)))
            {
                var remaining = material.Count;
                var inventoryItems = await _dbContext.Db.Queryable<InventoryItemEntity>()
                    .Where(item => item.PlayerId == playerId && item.ItemId == material.ItemId)
                    .OrderBy(item => item.IsLocked)
                    .OrderBy(item => item.Id)
                    .ToListAsync();

                foreach (var inventoryItem in inventoryItems)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var deductCount = Math.Min(inventoryItem.Quantity, remaining);
                    if (deductCount == inventoryItem.Quantity)
                    {
                        var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                            .Where(item => item.Id == inventoryItem.Id && item.Quantity == inventoryItem.Quantity)
                            .ExecuteCommandAsync();
                        if (deleteRows == 0)
                        {
                            throw new InvalidOperationException($"突破材料{ResolveBreakthroughMaterialName(material.ItemId)}在扣除时发生变化，请稍后重试。");
                        }
                    }
                    else
                    {
                        var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                            .SetColumns(item => item.Quantity == item.Quantity - deductCount)
                            .Where(item => item.Id == inventoryItem.Id && item.Quantity >= deductCount)
                            .ExecuteCommandAsync();
                        if (updateRows == 0)
                        {
                            throw new InvalidOperationException($"突破材料{ResolveBreakthroughMaterialName(material.ItemId)}在扣除时发生变化，请稍后重试。");
                        }
                    }

                    remaining -= deductCount;
                }

                if (remaining > 0)
                {
                    throw new InvalidOperationException($"突破材料不足：{ResolveBreakthroughMaterialName(material.ItemId)}。");
                }
            }
        }

        /// <summary>
        /// 解析突破材料的展示名称。
        /// 物品模板不存在时回退显示"未知材料"，避免前端看到原始物品编号。
        /// </summary>
        /// <param name="itemId">物品编号。</param>
        /// <returns>材料名称。</returns>
        private static string ResolveBreakthroughMaterialName(string itemId)
        {
            if (XXX.GameData.Items.TryGetValue(itemId, out var item))
            {
                return item.Name;
            }

            return "未知材料";
        }

        /// <summary>
        /// 在升级后同步等级类成就进度。
        /// 当前主要覆盖 <c>level_10</c>、<c>level_30</c>、<c>level_50</c> 等等级要求链路。
        /// </summary>
        /// <param name="user">刚完成升级链路的玩家实体。</param>
        private async Task SyncLevelAchievementsAsync(UserEntity user)
        {
            try
            {
                await _achievementService.SyncRequirementStateAsync(user.GID, AchievementRequirementType.ReachLevel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "同步玩家 {PlayerId} 的等级成就进度失败。", user.GID);
            }
        }

        /// <summary>
        /// 基于玩家实体实时构建属性点总览。
        /// 会读取当前职业下的属性定义，并把独立分配表中的点数换算成展示值。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <returns>属性点总览 DTO。</returns>
        private async Task<PlayerAttributePointOverviewDto> BuildAttributePointOverviewAsync(UserEntity user)
        {
            var allocations = await _attributeAllocationRepository.GetListAsync(item => item.PlayerId == user.GID);
            return BuildAttributePointOverview(user, allocations);
        }

        /// <summary>
        /// 根据玩家实体和属性点分配记录生成属性点总览。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <param name="allocations">属性点分配记录集合。</param>
        /// <returns>属性点总览 DTO。</returns>
        private static PlayerAttributePointOverviewDto BuildAttributePointOverview(
            UserEntity user,
            IReadOnlyCollection<PlayerAttributeAllocationEntity> allocations)
        {
            var definitions = AttributePointConfig.GetDefinitions(user.Profession);
            var items = new List<PlayerAttributePointItemDto>();
            var totalUsedPoints = 0;

            foreach (var definition in definitions)
            {
                var allocatedPoints = allocations
                    .Where(item => item.AttributeKey == definition.Key)
                    .Sum(item => Math.Max(0, item.AllocatedPoints));
                var bonusValue = AttributePointConfig.GetBonusValue(allocatedPoints, definition);

                totalUsedPoints += allocatedPoints;
                items.Add(new PlayerAttributePointItemDto
                {
                    Key = definition.Key,
                    Name = definition.Name,
                    AttributeType = definition.AttributeType.ToString(),
                    AllocatedPoints = allocatedPoints,
                    BonusValue = bonusValue,
                    BonusPerPoint = definition.BonusPerPoint,
                    PointsPerBonus = definition.PointsPerBonus
                });
            }

            var totalEarnedPoints = AttributePointConfig.GetTotalEarnedPoints(user.Level);
            return new PlayerAttributePointOverviewDto
            {
                TotalEarnedPoints = totalEarnedPoints,
                TotalUsedPoints = totalUsedPoints,
                AvailablePoints = Math.Max(0, totalEarnedPoints - totalUsedPoints),
                Attributes = items
            };
        }

        /// <summary>
        /// 对玩家属性点状态加一个轻量锁。
        /// 当前通过触碰玩家行自身，避免并发加点时重复消费可用点数。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        private async Task LockAttributePointStateAsync(string playerId)
        {
            await _dbContext.Db.Ado.ExecuteCommandAsync(
                "UPDATE Users SET LastUpdateTime = LastUpdateTime WHERE GID = @gid AND IsDeleted = 0;",
                new
                {
                    gid = playerId
                });
        }
    }
}
