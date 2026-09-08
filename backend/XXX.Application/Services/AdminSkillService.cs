#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台技能模板服务。
    /// </summary>
    public class AdminSkillService : IAdminSkillService
    {
        private readonly IRepository<SkillTemplateEntity> _skillRepository;
        private readonly IRepository<MonsterTemplateEntity> _monsterRepository;
        private readonly IRepository<PetTemplateEntity> _petTemplateRepository;
        private readonly IRepository<PetInstanceEntity> _petInstanceRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;
        private readonly DbContext _dbContext;

        public AdminSkillService(
            IRepository<SkillTemplateEntity> skillRepository,
            IRepository<MonsterTemplateEntity> monsterRepository,
            IRepository<PetTemplateEntity> petTemplateRepository,
            IRepository<PetInstanceEntity> petInstanceRepository,
            IRepository<UserEntity> userRepository,
            IAdminRuntimeRefreshService runtimeRefreshService,
            DbContext dbContext)
        {
            _skillRepository = skillRepository;
            _monsterRepository = monsterRepository;
            _petTemplateRepository = petTemplateRepository;
            _petInstanceRepository = petInstanceRepository;
            _userRepository = userRepository;
            _runtimeRefreshService = runtimeRefreshService;
            _dbContext = dbContext;
        }

        public async Task<List<AdminSkillListItemDto>> GetListAsync(string? keyword = null, string? catalog = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _skillRepository.Db.Queryable<SkillTemplateEntity>()
                .Where(skill => skill.SkillLevel == 1);

            var normalizedCatalog = (catalog ?? SkillTemplateEntity.CurrentCatalog).Trim().ToLowerInvariant();
            if (normalizedCatalog is SkillTemplateEntity.CurrentCatalog or SkillTemplateEntity.LegacyCatalog)
            {
                query = query.Where(skill => skill.SkillCatalog == normalizedCatalog);
            }

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(skill =>
                    skill.Name.Contains(normalizedKeyword) ||
                    skill.SkillId.ToString().Contains(normalizedKeyword));
            }

            var skills = await query.OrderBy(skill => skill.SkillId).ToListAsync();
            return skills.Select(skill => new AdminSkillListItemDto
            {
                SkillCatalog = skill.SkillCatalog,
                SkillId = skill.SkillId,
                SkillLevel = skill.SkillLevel,
                NextSkillId = skill.NextSkillId,
                Name = skill.Name,
                TargetType = skill.TargetType,
                Cooldown = skill.Cooldown,
                AllowedProfessionText = PlayerProfessionCatalog.GetAllowedDisplayName(skill.AllowedProfessions),
                IsBuiltIn = skill.IsBuiltIn,
                BuiltInVersion = skill.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminSkillDetailDto?> GetDetailAsync(int skillId)
        {
            if (skillId <= 0)
            {
                return null;
            }

            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null)
            {
                return null;
            }

            return new AdminSkillDetailDto
            {
                SkillCatalog = skill.SkillCatalog,
                SkillId = skill.SkillId,
                SkillLevel = skill.SkillLevel,
                PreviousSkillId = skill.PreviousSkillId,
                NextSkillId = skill.NextSkillId,
                NextSkillName = skill.NextSkillId.HasValue
                    ? (await _skillRepository.GetByIdAsync(skill.NextSkillId.Value))?.Name
                    : null,
                UpgradeConditions = skill.UpgradeConditions.ToList(),
                Name = skill.Name,
                Description = skill.Description,
                TargetType = skill.TargetType,
                ManaCost = skill.ManaCost,
                Cooldown = skill.Cooldown,
                DamageType = skill.DamageType,
                HitCount = skill.HitCount,
                RangeType = skill.RangeType,
                DamageMultiplier = skill.DamageMultiplier,
                TriggerChance = skill.TriggerChance,
                Hits = skill.Hits.ToList(),
                BuffIds = skill.BuffIds.ToList(),
                AllowedProfessions = skill.AllowedProfessions.ToList(),
                IsBuiltIn = skill.IsBuiltIn,
                SeedKey = skill.SeedKey,
                BuiltInVersion = skill.BuiltInVersion,
                LastUpdateTime = skill.LastUpdateTime
            };
        }

        /// <summary>
        /// 获取指定技能的全部后续等级。
        /// </summary>
        public async Task<List<AdminSkillNextLevelDto>> GetNextLevelsAsync(int skillId)
        {
            var source = await _skillRepository.GetByIdAsync(skillId) ?? throw new InvalidOperationException("技能模板不存在。");
            var skills = await _skillRepository.GetAllAsync();
            var result = new List<AdminSkillNextLevelDto>();
            var current = source;
            while (current.NextSkillId.HasValue)
            {
                var next = skills.FirstOrDefault(skill => skill.SkillId == current.NextSkillId.Value);
                if (next == null) break;
                result.Add(new AdminSkillNextLevelDto
                {
                    PreviousSkillId = current.SkillId,
                    SkillId = next.SkillId,
                    SkillLevel = next.SkillLevel,
                    Name = next.Name,
                    DamageType = next.DamageType,
                    ManaCost = next.ManaCost,
                    Cooldown = next.Cooldown,
                    UpgradeConditions = current.UpgradeConditions.ToList(),
                    UpgradeConditionSummary = FormatUpgradeConditions(current.UpgradeConditions)
                });
                current = next;
            }
            return result;
        }

        private static string FormatUpgradeConditions(IEnumerable<SkillUpgradeCondition> conditions)
        {
            var items = conditions.Where(condition => condition.Amount > 0).Select(condition => condition.Type switch
            {
                SkillUpgradeConditionType.Gold => $"金币 {condition.Amount}",
                SkillUpgradeConditionType.SpiritStone => $"灵石 {condition.Amount}",
                SkillUpgradeConditionType.Item => $"道具 {condition.ItemId}×{condition.Amount}",
                SkillUpgradeConditionType.PlayerLevel => $"等级 {condition.Amount}",
                _ => "未知条件"
            }).ToList();
            return items.Count == 0 ? "无条件" : string.Join("、", items);
        }

        /// <summary>
        /// 更新当前技能升级到下一级的条件。
        /// </summary>
        public async Task<AdminSkillDetailDto> UpdateUpgradeConditionsAsync(int skillId, List<SkillUpgradeCondition> conditions)
        {
            var source = await _skillRepository.GetByIdAsync(skillId) ?? throw new InvalidOperationException("技能模板不存在。");
            if (!source.NextSkillId.HasValue)
            {
                throw new InvalidOperationException("当前技能尚未配置下一级技能，不能设置升级条件。");
            }

            source.UpgradeConditions = (conditions ?? [])
                .Where(condition => condition.Amount > 0)
                .Select(condition => new SkillUpgradeCondition
                {
                    Type = condition.Type,
                    Amount = condition.Amount,
                    ItemId = condition.Type == SkillUpgradeConditionType.Item ? condition.ItemId?.Trim() : null
                })
                .ToList();
            source.LastUpdateTime = DateTime.Now;
            await _skillRepository.UpdateAsync(source);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(skillId))!;
        }
        /// <summary>
        /// 保存技能模板。
        /// </summary>
        public async Task<AdminSkillDetailDto> SaveAsync(AdminSkillDetailDto request)
        {
            if (request.SkillId <= 0)
            {
                throw new InvalidOperationException("技能编号必须大于 0。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("技能名称不能为空。");
            }

            var existing = await _skillRepository.GetByIdAsync(request.SkillId);
            if (request.SkillLevel <= 0)
            {
                request.SkillLevel = existing?.SkillLevel ?? 1;
            }
            if (existing == null)
            {
                existing = new SkillTemplateEntity
                {
                    SkillId = request.SkillId
                };

                await _skillRepository.AddAsync(ApplySkill(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetDetailAsync(request.SkillId))!;
            }

            ApplySkill(existing, request);
            await _skillRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return await GetDetailAsync(request.SkillId);
        }

        /// <summary>
        /// 复制当前技能创建下一级技能。
        /// </summary>
        public async Task<AdminSkillDetailDto> CopyNextAsync(int skillId)
        {
            var source = await _skillRepository.GetByIdAsync(skillId) ?? throw new InvalidOperationException("技能模板不存在。");
            if (source.NextSkillId.HasValue)
            {
                throw new InvalidOperationException("当前技能已经配置下一级技能。");
            }

            var nextId = (await _skillRepository.Db.Queryable<SkillTemplateEntity>().MaxAsync(skill => (int?)skill.SkillId) ?? 0) + 1;
            var next = new SkillTemplateEntity
            {
                SkillId = nextId,
                SkillLevel = Math.Max(1, source.SkillLevel) + 1,
                PreviousSkillId = source.SkillId,
                Name = source.Name,
                Description = source.Description,
                TargetType = source.TargetType,
                ManaCost = source.ManaCost,
                Cooldown = source.Cooldown,
                DamageType = source.DamageType,
                HitCount = source.HitCount,
                RangeType = source.RangeType,
                DamageMultiplier = source.DamageMultiplier,
                TriggerChance = source.TriggerChance,
                Hits = source.Hits.ToList(),
                BuffIds = source.BuffIds.ToList(),
                AllowedProfessions = source.AllowedProfessions.ToList(),
                LastUpdateTime = DateTime.Now
            };

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                source.NextSkillId = nextId;
                source.UpgradeConditions = [];
                source.LastUpdateTime = DateTime.Now;
                await _skillRepository.UpdateAsync(source);
                await _skillRepository.AddAsync(next);
            });
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(nextId))!;
        }

        /// <summary>
        /// 将已有独立技能关联为下一级。
        /// </summary>
        public async Task<AdminSkillDetailDto> LinkNextAsync(int skillId, int nextSkillId, List<SkillUpgradeCondition> conditions)
        {
            var source = await _skillRepository.GetByIdAsync(skillId) ?? throw new InvalidOperationException("当前技能模板不存在。");
            var next = await _skillRepository.GetByIdAsync(nextSkillId) ?? throw new InvalidOperationException("目标技能模板不存在。");
            if (source.NextSkillId.HasValue || source.SkillId == next.SkillId || next.PreviousSkillId.HasValue || next.NextSkillId.HasValue)
            {
                throw new InvalidOperationException("目标技能已存在升级关系，不能重复关联。");
            }

            next.SkillLevel = Math.Max(1, source.SkillLevel) + 1;
            next.PreviousSkillId = source.SkillId;
            source.NextSkillId = next.SkillId;
            source.UpgradeConditions = conditions ?? [];
            source.LastUpdateTime = DateTime.Now;
            next.LastUpdateTime = DateTime.Now;

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                await _skillRepository.UpdateAsync(source);
                await _skillRepository.UpdateAsync(next);
            });
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(source.SkillId))!;
        }

        /// <summary>
        /// 删除当前技能及其后续技能。
        /// </summary>
        public async Task DeleteNextChainAsync(int skillId)
        {
            var source = await _skillRepository.GetByIdAsync(skillId) ?? throw new InvalidOperationException("技能模板不存在。");
            var allSkills = await _skillRepository.GetAllAsync();
            var ids = new List<int>();
            var currentId = skillId;
            while (currentId > 0 && !ids.Contains(currentId))
            {
                ids.Add(currentId);
                currentId = allSkills.FirstOrDefault(skill => skill.SkillId == currentId)?.NextSkillId ?? 0;
            }

            foreach (var id in ids)
            {
                if (await _monsterRepository.Db.Queryable<MonsterTemplateEntity>().Where(monster => monster.SkillIdsJson != null && monster.SkillIdsJson.Contains(id.ToString())).AnyAsync()
                    || await _petTemplateRepository.Db.Queryable<PetTemplateEntity>().Where(pet => pet.SkillIdsJson != null && pet.SkillIdsJson.Contains(id.ToString())).AnyAsync()
                    || await _petInstanceRepository.Db.Queryable<PetInstanceEntity>().Where(pet => pet.SkillIdsJson != null && pet.SkillIdsJson.Contains(id.ToString())).AnyAsync()
                    || await _userRepository.Db.Queryable<UserEntity>().Where(user => (user.SkillIdsJson != null && user.SkillIdsJson.Contains(id.ToString())) || (user.OwnedSkillIdsJson != null && user.OwnedSkillIdsJson.Contains(id.ToString()))).AnyAsync())
                {
                    throw new InvalidOperationException($"技能 #{id} 已被使用，不能删除当前及后续技能。");
                }
            }

            var previous = source.PreviousSkillId.HasValue ? await _skillRepository.GetByIdAsync(source.PreviousSkillId.Value) : null;
            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                if (previous != null)
                {
                    previous.NextSkillId = null;
                    previous.UpgradeConditions = [];
                    previous.LastUpdateTime = DateTime.Now;
                    await _skillRepository.UpdateAsync(previous);
                }
                foreach (var id in ids)
                {
                    await _skillRepository.DeleteAsync(id);
                }
            });
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
        }

        public async Task<bool> DeleteAsync(int skillId)
        {
            if (skillId <= 0)
            {
                return false;
            }

            var normalizedSkillId = skillId.ToString();
            if (await _monsterRepository.Db.Queryable<MonsterTemplateEntity>().Where(monster => monster.SkillIdsJson != null && monster.SkillIdsJson.Contains(normalizedSkillId)).AnyAsync())
            {
                throw new InvalidOperationException("当前技能仍被怪物模板引用，不能直接删除。");
            }

            if (await _petTemplateRepository.Db.Queryable<PetTemplateEntity>().Where(pet => pet.SkillIdsJson != null && pet.SkillIdsJson.Contains(normalizedSkillId)).AnyAsync())
            {
                throw new InvalidOperationException("当前技能仍被灵宠模板引用，不能直接删除。");
            }

            if (await _petInstanceRepository.Db.Queryable<PetInstanceEntity>().Where(pet => pet.SkillIdsJson != null && pet.SkillIdsJson.Contains(normalizedSkillId)).AnyAsync())
            {
                throw new InvalidOperationException("当前技能仍被灵宠实例引用，不能直接删除。");
            }

            if (await _userRepository.Db.Queryable<UserEntity>().Where(user =>
                (user.SkillIdsJson != null && user.SkillIdsJson.Contains(normalizedSkillId)) ||
                (user.OwnedSkillIdsJson != null && user.OwnedSkillIdsJson.Contains(normalizedSkillId))).AnyAsync())
            {
                throw new InvalidOperationException("当前技能仍被玩家角色引用，不能直接删除。");
            }

            var deleteRows = await _skillRepository.DeleteAsync(skillId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static SkillTemplateEntity ApplySkill(SkillTemplateEntity entity, AdminSkillDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.SkillCatalog = string.Equals(request.SkillCatalog, SkillTemplateEntity.LegacyCatalog, StringComparison.OrdinalIgnoreCase)
                ? SkillTemplateEntity.LegacyCatalog
                : SkillTemplateEntity.CurrentCatalog;
            entity.SkillLevel = Math.Max(1, request.SkillLevel);
            entity.PreviousSkillId = request.PreviousSkillId;
            entity.NextSkillId = request.NextSkillId;
            entity.UpgradeConditions = request.UpgradeConditions.ToList();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.TargetType = request.TargetType;
            entity.ManaCost = Math.Max(0, request.ManaCost);
            entity.Cooldown = Math.Max(0, request.Cooldown);
            entity.DamageType = request.DamageType;
            entity.HitCount = Math.Max(0, request.HitCount);
            entity.RangeType = request.RangeType;
            entity.DamageMultiplier = request.DamageMultiplier;
            entity.TriggerChance = request.TriggerChance;
            entity.Hits = request.Hits.ToList();
            entity.BuffIds = request.BuffIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList();
            entity.AllowedProfessions = PlayerProfessionCatalog.NormalizeAllowedProfessions(request.AllowedProfessions);
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
