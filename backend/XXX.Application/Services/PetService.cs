using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 灵宠服务实现
    /// </summary>
    public class PetService : IPetService
    {
        private readonly IRepository<PetInstanceEntity> _petRepository;
        private readonly IRepository<PetTemplateEntity> _petTemplateRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<PetService> _logger;

        /// <summary>
        /// 初始化灵宠服务。
        /// </summary>
        /// <param name="petRepository">灵宠实例仓储。</param>
        /// <param name="petTemplateRepository">灵宠模板仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="inventoryService">背包服务。</param>
        /// <param name="logger">日志记录器。</param>
        public PetService(
            IRepository<PetInstanceEntity> petRepository,
            IRepository<PetTemplateEntity> petTemplateRepository,
            IRepository<UserEntity> userRepository,
            IInventoryService inventoryService,
            ILogger<PetService> logger)
        {
            _petRepository = petRepository;
            _petTemplateRepository = petTemplateRepository;
            _userRepository = userRepository;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// 获取玩家拥有的全部灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>按出战状态、品质和等级排序后的灵宠列表。</returns>
        public async Task<List<PetDto>> GetPlayerPetsAsync(string playerId)
        {
            await EnsurePetTemplatesSeededAsync();

            var pets = await _petRepository.GetListAsync(p => p.PlayerId == playerId);
            var templateIds = pets.Select(p => p.TemplateId).Distinct().ToList();
            var templates = await LoadTemplateMapAsync(templateIds);

            return pets
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.Quality)
                .ThenByDescending(p => p.Level)
                .ThenByDescending(p => p.AcquiredTime)
                .Select(pet =>
                {
                    templates.TryGetValue(pet.TemplateId, out var template);
                    return MapToDto(pet, template);
                })
                .ToList();
        }

        /// <summary>
        /// 获取单只灵宠详情。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="petId">灵宠实例编号。</param>
        /// <returns>命中的灵宠详情；不存在时返回空。</returns>
        public async Task<PetDto?> GetPetDetailAsync(string playerId, string petId)
        {
            await EnsurePetTemplatesSeededAsync();

            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.InstanceId == petId);
            if (pet == null)
            {
                return null;
            }

            var template = await _petTemplateRepository.GetByIdAsync(pet.TemplateId);
            return MapToDto(pet, template);
        }

        /// <summary>
        /// 获取当前出战灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>当前出战灵宠；未设置时返回空。</returns>
        public async Task<PetDto?> GetActivePetAsync(string playerId)
        {
            await EnsurePetTemplatesSeededAsync();

            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.IsActive);
            if (pet == null)
            {
                return null;
            }

            var template = await _petTemplateRepository.GetByIdAsync(pet.TemplateId);
            return MapToDto(pet, template);
        }

        /// <summary>
        /// 设置出战灵宠（事务一致性）。
        /// </summary>
        /// <remarks>
        /// 方法作用：切换玩家当前出战灵宠，并同步写入玩家 PetId。
        /// 关键逻辑：在同一事务内完成“清空旧出战状态 + 设置新出战状态 + 更新用户PetId”，任一步失败都回滚。
        /// </remarks>
        public async Task<bool> SetActivePetAsync(string playerId, string petId)
        {
            await EnsurePetTemplatesSeededAsync();

            var db = _petRepository.Db;
            var now = DateTime.Now;
            var targetExists = await db.Queryable<PetInstanceEntity>()
                .AnyAsync(p => p.PlayerId == playerId && p.InstanceId == petId);
            if (!targetExists)
            {
                return false;
            }

            try
            {
                // 中文注释：
                // 出战灵宠状态与用户 PetId 必须同事务提交。
                // 只要中间任一步失败，就会出现“人物面板显示有出战灵宠，但灵宠列表里没有出战标记”
                // 或者反过来的错配状态，所以这里必须整体回滚。
                db.Ado.BeginTran();

                await db.Updateable<PetInstanceEntity>()
                    .SetColumns(p => p.IsActive == false)
                    .SetColumns(p => p.LastUpdateTime == now)
                    .Where(p => p.PlayerId == playerId && p.IsActive)
                    .ExecuteCommandAsync();

                var activeRows = await db.Updateable<PetInstanceEntity>()
                    .SetColumns(p => p.IsActive == true)
                    .SetColumns(p => p.LastUpdateTime == now)
                    .Where(p => p.PlayerId == playerId && p.InstanceId == petId)
                    .ExecuteCommandAsync();
                if (activeRows == 0)
                {
                    db.Ado.RollbackTran();
                    return false;
                }

                var userRows = await db.Updateable<UserEntity>()
                    .SetColumns(u => u.PetId == petId)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted)
                    .ExecuteCommandAsync();
                if (userRows == 0)
                {
                    db.Ado.RollbackTran();
                    return false;
                }

                db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                _logger.LogError(ex, "玩家 {PlayerId} 设置出战灵宠 {PetId} 时发生异常", playerId, petId);
                return false;
            }

            _logger.LogInformation("玩家 {PlayerId} 设置出战灵宠 {PetId}", playerId, petId);

            return true;
        }

        /// <summary>
        /// 清空当前出战灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>清空成功返回真。</returns>
        public async Task<bool> ClearActivePetAsync(string playerId)
        {
            var db = _petRepository.Db;
            var now = DateTime.Now;

            try
            {
                // 中文注释：
                // “召回”本质上是把当前出战灵宠全部清空，并同步把玩家身上的 PetId 置空。
                // 这一步同样必须和用户表一起事务提交，否则前端会看到旧的出战残影。
                db.Ado.BeginTran();

                await db.Updateable<PetInstanceEntity>()
                    .SetColumns(p => p.IsActive == false)
                    .SetColumns(p => p.LastUpdateTime == now)
                    .Where(p => p.PlayerId == playerId && p.IsActive)
                    .ExecuteCommandAsync();

                await db.Updateable<UserEntity>()
                    .SetColumns(u => u.PetId == null)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == playerId && !u.IsDeleted)
                    .ExecuteCommandAsync();

                db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                _logger.LogError(ex, "玩家 {PlayerId} 召回出战灵宠时发生异常", playerId);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 喂养灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">喂养请求。</param>
        /// <returns>喂养成功返回真。</returns>
        public async Task<bool> FeedPetAsync(string playerId, PetFeedRequestDto request)
        {
            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.InstanceId == request.PetId);
            if (pet == null)
            {
                return false;
            }

            // 中文注释：
            // 喂养消耗直接从背包里扣真实道具。
            // 这样宠物忠诚度的提升和玩家资产变化能保持一致，不会出现页面上“喂养成功”但背包没变化的假操作。
            var deductSuccess = await _inventoryService.DeductItemAsync(playerId, request.FoodItemId, request.Quantity, "喂养灵宠");
            if (!deductSuccess)
            {
                _logger.LogWarning("玩家 {PlayerId} 喂养灵宠失败，食物不足", playerId);
                return false;
            }

            pet.Loyalty = Math.Min(100, pet.Loyalty + 10 * request.Quantity);
            pet.LastUpdateTime = DateTime.Now;
            await _petRepository.UpdateAsync(pet);

            _logger.LogInformation("玩家 {PlayerId} 喂养灵宠 {PetId}，忠诚度提升至 {Loyalty}",
                playerId, request.PetId, pet.Loyalty);

            return true;
        }

        /// <summary>
        /// 进化灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">进化请求。</param>
        /// <returns>进化成功返回真。</returns>
        public async Task<bool> EvolvePetAsync(string playerId, PetEvolveRequestDto request)
        {
            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.InstanceId == request.PetId);
            if (pet == null)
            {
                return false;
            }

            var template = await _petTemplateRepository.GetByIdAsync(pet.TemplateId);
            var maxQuality = Math.Max(1, template?.MaxQuality ?? 5);
            if (pet.Quality >= maxQuality)
            {
                return false;
            }

            var requiredStones = pet.Quality * 5;
            var hasEnoughStones = await _inventoryService.GetItemCountAsync(playerId, "evolution_stone") >= requiredStones;
            if (!hasEnoughStones)
            {
                _logger.LogWarning("玩家 {PlayerId} 进化灵宠失败，材料不足", playerId);
                return false;
            }

            var deductSuccess = await _inventoryService.DeductItemAsync(playerId, "evolution_stone", requiredStones, "灵宠进化");
            if (!deductSuccess)
            {
                _logger.LogWarning("玩家 {PlayerId} 灵宠进化扣材料失败，所需 {RequiredStones} 个进化石", playerId, requiredStones);
                return false;
            }

            pet.Quality++;
            pet.Type1 = ScaleStat(pet.Type1, 1.18);
            pet.Type2 = ScaleStat(pet.Type2, 1.12);
            pet.Type3 = ScaleStat(pet.Type3, 1.15);
            pet.Type4 = ScaleStat(pet.Type4, 1.15);
            pet.Type5 = ScaleStat(pet.Type5, 1.12);
            pet.Type6 = ScaleStat(pet.Type6, 1.12);
            pet.Type7 = ScaleStat(pet.Type7, 1.08);
            pet.Type8 = MathF.Min(1f, pet.Type8 + 0.01f);
            pet.Type9 = MathF.Min(1f, pet.Type9 + 0.01f);
            pet.Type10 = MathF.Min(1f, pet.Type10 + 0.01f);
            pet.Type11 = MathF.Max(1f, pet.Type11 + 0.08f);
            pet.Type12 = MathF.Min(1f, pet.Type12 + 0.01f);
            pet.Type13 = MathF.Min(1f, pet.Type13 + 0.01f);
            pet.Type14 = MathF.Min(0.7f, pet.Type14 + 0.01f);
            pet.Type15 = MathF.Min(1f, pet.Type15 + 0.01f);
            pet.LastUpdateTime = DateTime.Now;
            await _petRepository.UpdateAsync(pet);

            _logger.LogInformation("玩家 {PlayerId} 灵宠 {PetId} 进化成功，新品质：{Quality}",
                playerId, request.PetId, pet.Quality);

            return true;
        }

        /// <summary>
        /// 给当前出战灵宠发放战斗经验。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="expGained">本次发放的灵宠经验。</param>
        /// <returns>实际发放到出战灵宠的经验值。</returns>
        public async Task<int> GrantBattleExpAsync(string playerId, int expGained)
        {
            if (expGained <= 0)
            {
                return 0;
            }

            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.IsActive);
            if (pet == null)
            {
                return 0;
            }

            var template = await _petTemplateRepository.GetByIdAsync(pet.TemplateId);
            var player = await _userRepository.GetByIdAsync(playerId);
            var maxPetLevel = Math.Max(1, Math.Min(60, (player?.Level ?? pet.Level) + 5));

            var grantedExp = Math.Max(1, expGained);
            pet.Exp += grantedExp;

            while (pet.Level < maxPetLevel && pet.Exp >= pet.XExp)
            {
                pet.Exp -= pet.XExp;
                pet.Level++;
                ApplyLevelUpGrowth(pet, template);
                pet.XExp = PetGenerationHelper.CalculatePetRequiredExp(pet.Level);
            }

            if (pet.Level >= maxPetLevel && pet.Exp > pet.XExp)
            {
                pet.Exp = pet.XExp;
            }

            pet.LastUpdateTime = DateTime.Now;
            await _petRepository.UpdateAsync(pet);

            return grantedExp;
        }

        /// <summary>
        /// 放生灵宠。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="petId">灵宠实例编号。</param>
        /// <returns>放生成功返回真。</returns>
        public async Task<bool> ReleasePetAsync(string playerId, string petId)
        {
            var pet = await _petRepository.GetFirstAsync(p => p.PlayerId == playerId && p.InstanceId == petId);
            if (pet == null || pet.IsActive)
            {
                return false;
            }

            await _petRepository.DeleteAsync(pet);
            _logger.LogInformation("玩家 {PlayerId} 放生灵宠 {PetId}", playerId, petId);

            return true;
        }

        /// <summary>
        /// 中文注释：
        /// 如果数据库里的灵宠模板表是空的，就在第一次使用灵宠系统时补写一份最小可玩模板。
        /// 这样既能保证 SQLite 开发库冷启动后立刻可用，也能避免前端“打开弹窗只有空白和加号”的假死体验。
        /// </summary>
        private async Task EnsurePetTemplatesSeededAsync()
        {
            var count = await _petTemplateRepository.CountAsync(_ => true);
            if (count > 0)
            {
                return;
            }

            _logger.LogError("PetTemplates table is empty. Runtime pet seeding has been disabled; database templates are required.");
            throw new InvalidOperationException("PetTemplates table is empty. Please run startup seed sync before using the pet system.");
        }

        private async Task<Dictionary<string, PetTemplateEntity>> LoadTemplateMapAsync(List<string> templateIds)
        {
            if (templateIds.Count == 0)
            {
                return [];
            }

            var templates = await _petTemplateRepository.GetListAsync(template => templateIds.Contains(template.TemplateId));
            return templates.ToDictionary(template => template.TemplateId, template => template);
        }

        private PetDto MapToDto(PetInstanceEntity entity, PetTemplateEntity? template)
        {
            var effectiveSkillIds = entity.SkillIds.Count > 0
                ? entity.SkillIds
                : template?.SkillIds ?? [];

            return new PetDto
            {
                InstanceId = entity.InstanceId,
                TemplateId = entity.TemplateId,
                Name = string.IsNullOrWhiteSpace(entity.Name) ? template?.Name ?? "未知灵宠" : entity.Name,
                Description = template?.Description ?? "暂无描述",
                Avatar = GetPetAvatar(entity.TemplateId, template?.Type ?? PetType.Balanced),
                Level = entity.Level,
                Exp = entity.Exp,
                XExp = entity.XExp,
                Quality = entity.Quality,
                MaxQuality = Math.Max(entity.Quality, template?.MaxQuality ?? entity.Quality),
                QualityText = GetQualityText(entity.Quality),
                Attack = entity.Type3,
                Defense = entity.Type5,
                HP = entity.Type1,
                MP = entity.Type2,
                Speed = entity.Type7,
                Loyalty = entity.Loyalty,
                Type = template?.Type ?? PetType.Balanced,
                TypeText = GetPetTypeText(template?.Type ?? PetType.Balanced),
                Element = entity.Element,
                ElementText = ElementRelation.GetElementName(entity.Element),
                GrowthRate = entity.GrowthRate > 0 ? entity.GrowthRate : template?.GrowthRateMax ?? 1.0,
                GrowthText = GetGrowthText(entity.GrowthRate > 0 ? entity.GrowthRate : template?.GrowthRateMax ?? 1.0),
                HitRate = entity.Type8,
                DodgeRate = entity.Type9,
                CritRate = entity.Type10,
                CritDamage = entity.Type11,
                ComboRate = entity.Type12,
                CounterRate = entity.Type13,
                ArmorBreak = entity.Type14,
                BonusDamage = entity.Type15,
                IsActive = entity.IsActive,
                SkillIds = effectiveSkillIds,
                Skills = effectiveSkillIds.Select(MapSkillToDto).ToList(),
                AcquiredTime = entity.AcquiredTime
            };
        }

        /// <summary>
        /// 中文注释：
        /// 宠物升级只作用于宠物实例本身，不再给人物面板叠属性。
        /// 升级时按模板底板和实例成长率增量提升 Type1-Type7，保持“模板生成个体，再由个体成长”的模型。
        /// </summary>
        private static void ApplyLevelUpGrowth(PetInstanceEntity pet, PetTemplateEntity? template)
        {
            var growth = Math.Max(0.6, pet.GrowthRate <= 0 ? 1.0 : pet.GrowthRate);
            var qualityMultiplier = 1.0 + Math.Max(0, pet.Quality - 1) * 0.08;

            pet.Type1 += CalculateGrowthDelta(template?.MinType1, template?.MaxType1, pet.Type1, growth, 0.12, qualityMultiplier);
            pet.Type2 += CalculateGrowthDelta(template?.MinType2, template?.MaxType2, pet.Type2, growth, 0.10, 1.0);
            pet.Type3 += CalculateGrowthDelta(template?.MinType3, template?.MaxType3, pet.Type3, growth, 0.10, qualityMultiplier);
            pet.Type4 += CalculateGrowthDelta(template?.MinType4, template?.MaxType4, pet.Type4, growth, 0.10, qualityMultiplier);
            pet.Type5 += CalculateGrowthDelta(template?.MinType5, template?.MaxType5, pet.Type5, growth, 0.08, qualityMultiplier);
            pet.Type6 += CalculateGrowthDelta(template?.MinType6, template?.MaxType6, pet.Type6, growth, 0.08, qualityMultiplier);
            pet.Type7 += CalculateGrowthDelta(template?.MinType7, template?.MaxType7, pet.Type7, growth, 0.05, 1.0);
        }

        private static int CalculateGrowthDelta(int? min, int? max, int currentValue, double growthRate, double ratio, double qualityMultiplier)
        {
            var templateBase = 0d;
            if (min.HasValue || max.HasValue)
            {
                templateBase = ((min ?? max ?? 0) + (max ?? min ?? 0)) / 2d;
            }

            if (templateBase <= 0)
            {
                templateBase = currentValue;
            }

            var delta = templateBase * growthRate * ratio * qualityMultiplier;
            return Math.Max(1, (int)Math.Round(delta, MidpointRounding.AwayFromZero));
        }

        private static int ScaleStat(int value, double multiplier)
        {
            return Math.Max(1, (int)Math.Round(value * multiplier, MidpointRounding.AwayFromZero));
        }

        private static PetSkillDto MapSkillToDto(string skillId)
        {
            if (int.TryParse(skillId, out var parsedSkillId) &&
                XXX.SkillData.Skills.TryGetValue(parsedSkillId, out var skill))
            {
                return new PetSkillDto
                {
                    SkillId = skillId,
                    Name = skill.Name,
                    Description = skill.Description,
                    Icon = GetSkillIcon(skill.DamageType)
                };
            }

            return new PetSkillDto
            {
                SkillId = skillId,
                Name = skillId,
                Description = "未找到对应技能描述。",
                Icon = "❓"
            };
        }

        private static string GetPetTypeText(PetType petType)
        {
            return petType switch
            {
                PetType.Attack => "攻击型",
                PetType.Defense => "防御型",
                PetType.Support => "辅助型",
                _ => "平衡型"
            };
        }

        private static string GetQualityText(int quality)
        {
            return quality switch
            {
                1 => "普通",
                2 => "优秀",
                3 => "精良",
                4 => "史诗",
                _ => "传说"
            };
        }

        private static string GetGrowthText(double growthRate)
        {
            if (growthRate >= 1.18)
            {
                return "完美";
            }

            if (growthRate >= 1.14)
            {
                return "优秀";
            }

            if (growthRate >= 1.1)
            {
                return "良好";
            }

            return "普通";
        }

        private static string GetPetAvatar(string templateId, PetType petType)
        {
            return templateId switch
            {
                "pet_fox_fire" => "🦊",
                "pet_wolf_wind" => "🐺",
                "pet_turtle_guard" => "🐢",
                _ => petType switch
                {
                    PetType.Attack => "🦁",
                    PetType.Defense => "🛡️",
                    PetType.Support => "🕊️",
                    _ => "🐉"
                }
            };
        }

        private static string GetSkillIcon(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Heal => "💚",
                DamageType.Buff => "🌀",
                DamageType.Magic => "🔥",
                DamageType.True => "⚡",
                DamageType.Revive => "✨",
                _ => "🗡️"
            };
        }
    }
}
