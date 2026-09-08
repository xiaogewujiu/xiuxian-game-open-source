#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台 Buff 模板服务。
    /// </summary>
    public class AdminBuffService : IAdminBuffService
    {
        private readonly IRepository<BuffTemplateEntity> _buffRepository;
        private readonly IRepository<SkillTemplateEntity> _skillRepository;
        private readonly IRepository<MonsterTemplateEntity> _monsterRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminBuffService(
            IRepository<BuffTemplateEntity> buffRepository,
            IRepository<SkillTemplateEntity> skillRepository,
            IRepository<MonsterTemplateEntity> monsterRepository,
            IRepository<UserEntity> userRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _buffRepository = buffRepository;
            _skillRepository = skillRepository;
            _monsterRepository = monsterRepository;
            _userRepository = userRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminBuffListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _buffRepository.Db.Queryable<BuffTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(buff =>
                    buff.BuffId.Contains(normalizedKeyword) ||
                    buff.Name.Contains(normalizedKeyword));
            }

            var buffs = await query.OrderBy(buff => buff.BuffId).ToListAsync();
            return buffs.Select(buff => new AdminBuffListItemDto
            {
                BuffId = buff.BuffId,
                Name = buff.Name,
                Duration = buff.Duration,
                MaxStack = buff.MaxStack,
                IsBuiltIn = buff.IsBuiltIn,
                BuiltInVersion = buff.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminBuffDetailDto?> GetDetailAsync(string buffId)
        {
            if (string.IsNullOrWhiteSpace(buffId))
            {
                return null;
            }

            var buff = await _buffRepository.GetByIdAsync(buffId.Trim());
            if (buff == null)
            {
                return null;
            }

            return new AdminBuffDetailDto
            {
                BuffId = buff.BuffId,
                Name = buff.Name,
                Description = buff.Description,
                Duration = buff.Duration,
                MaxStack = buff.MaxStack,
                StackRule = buff.StackRule,
                Effects = buff.Effects.ToList(),
                IsBuiltIn = buff.IsBuiltIn,
                SeedKey = buff.SeedKey,
                BuiltInVersion = buff.BuiltInVersion,
                LastUpdateTime = buff.LastUpdateTime
            };
        }

        public async Task<AdminBuffDetailDto> SaveAsync(AdminBuffDetailDto request)
        {
            var buffId = (request.BuffId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(buffId))
            {
                throw new InvalidOperationException("Buff 编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("Buff 名称不能为空。");
            }

            var existing = await _buffRepository.GetByIdAsync(buffId);
            if (existing == null)
            {
                existing = new BuffTemplateEntity
                {
                    BuffId = buffId
                };

                await _buffRepository.AddAsync(ApplyBuff(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetDetailAsync(buffId))!;
            }

            ApplyBuff(existing, request);
            await _buffRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(buffId))!;
        }

        public async Task<bool> DeleteAsync(string buffId)
        {
            var normalizedBuffId = (buffId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedBuffId))
            {
                return false;
            }

            if (await _skillRepository.Db.Queryable<SkillTemplateEntity>().Where(skill => skill.BuffIdsJson != null && skill.BuffIdsJson.Contains(normalizedBuffId)).AnyAsync())
            {
                throw new InvalidOperationException("当前 Buff 仍被技能模板引用，不能直接删除。");
            }

            if (await _monsterRepository.Db.Queryable<MonsterTemplateEntity>().Where(monster => monster.PassiveIdsJson != null && monster.PassiveIdsJson.Contains(normalizedBuffId)).AnyAsync())
            {
                throw new InvalidOperationException("当前 Buff 仍被怪物被动引用，不能直接删除。");
            }

            if (await _userRepository.Db.Queryable<UserEntity>().Where(user => user.PassiveIdsJson != null && user.PassiveIdsJson.Contains(normalizedBuffId)).AnyAsync())
            {
                throw new InvalidOperationException("当前 Buff 仍被玩家角色引用，不能直接删除。");
            }

            var deleteRows = await _buffRepository.DeleteAsync(normalizedBuffId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static BuffTemplateEntity ApplyBuff(BuffTemplateEntity entity, AdminBuffDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.Duration = request.Duration;
            entity.MaxStack = Math.Max(1, request.MaxStack);
            entity.StackRule = request.StackRule;
            entity.Effects = request.Effects.ToList();
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
