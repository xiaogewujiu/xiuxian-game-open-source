#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台灵宠模板服务。
    /// </summary>
    public class AdminPetService : IAdminPetService
    {
        private readonly IRepository<PetTemplateEntity> _petRepository;
        private readonly IRepository<PetInstanceEntity> _petInstanceRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminPetService(
            IRepository<PetTemplateEntity> petRepository,
            IRepository<PetInstanceEntity> petInstanceRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _petRepository = petRepository;
            _petInstanceRepository = petInstanceRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminPetListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _petRepository.Db.Queryable<PetTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(pet => pet.TemplateId.Contains(normalizedKeyword) || pet.Name.Contains(normalizedKeyword));
            }

            var pets = await query.OrderBy(pet => pet.TemplateId).ToListAsync();
            return pets.Select(pet => new AdminPetListItemDto
            {
                TemplateId = pet.TemplateId,
                Name = pet.Name,
                Type = (int)pet.Type,
                InitialQualityMin = pet.InitialQualityMin,
                InitialQualityMax = pet.InitialQualityMax,
                IsBuiltIn = pet.IsBuiltIn,
                BuiltInVersion = pet.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminPetDetailDto?> GetDetailAsync(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return null;
            }

            var pet = await _petRepository.GetByIdAsync(templateId.Trim());
            if (pet == null)
            {
                return null;
            }

            return new AdminPetDetailDto
            {
                TemplateId = pet.TemplateId,
                Name = pet.Name,
                Description = pet.Description,
                Type = (int)pet.Type,
                InitialQualityMin = pet.InitialQualityMin,
                InitialQualityMax = pet.InitialQualityMax,
                MaxQuality = pet.MaxQuality,
                GrowthRateMin = pet.GrowthRateMin,
                GrowthRateMax = pet.GrowthRateMax,
                InitialSkillCount = pet.InitialSkillCount,
                Attributes = MapAttributes(pet),
                SkillIds = pet.SkillIds.ToList(),
                ObtainMethod = pet.ObtainMethod,
                IsTradable = pet.IsTradable,
                IsBuiltIn = pet.IsBuiltIn,
                SeedKey = pet.SeedKey,
                BuiltInVersion = pet.BuiltInVersion,
                LastUpdateTime = pet.LastUpdateTime
            };
        }

        public async Task<AdminPetDetailDto> SaveAsync(AdminPetDetailDto request)
        {
            var templateId = (request.TemplateId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new InvalidOperationException("灵宠模板编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("灵宠名称不能为空。");
            }

            if (request.InitialQualityMin <= 0 || request.InitialQualityMax <= 0)
            {
                throw new InvalidOperationException("初始品质范围必须大于 0。");
            }

            if (request.InitialQualityMin > request.InitialQualityMax)
            {
                throw new InvalidOperationException("初始品质下限不能大于上限。");
            }

            if (request.MaxQuality < request.InitialQualityMax)
            {
                throw new InvalidOperationException("最大品质不能小于初始品质上限。");
            }

            if (request.GrowthRateMin <= 0 || request.GrowthRateMax <= 0)
            {
                throw new InvalidOperationException("成长率范围必须大于 0。");
            }

            if (request.GrowthRateMin > request.GrowthRateMax)
            {
                throw new InvalidOperationException("成长率下限不能大于上限。");
            }

            if (!HasCoreAttributeTemplate(request.Attributes))
            {
                throw new InvalidOperationException("请至少配置一组核心属性区间（生命、法力、物攻、法攻、物防、法防、速度）。");
            }

            var existing = await _petRepository.GetByIdAsync(templateId);
            if (existing == null)
            {
                existing = new PetTemplateEntity
                {
                    TemplateId = templateId
                };

                await _petRepository.AddAsync(ApplyPet(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetDetailAsync(templateId))!;
            }

            ApplyPet(existing, request);
            await _petRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(templateId))!;
        }

        public async Task<bool> DeleteAsync(string templateId)
        {
            var normalizedTemplateId = (templateId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedTemplateId))
            {
                return false;
            }

            var hasInstances = await _petInstanceRepository.Db.Queryable<PetInstanceEntity>()
                .Where(pet => pet.TemplateId == normalizedTemplateId)
                .AnyAsync();
            if (hasInstances)
            {
                throw new InvalidOperationException("当前灵宠模板仍被灵宠实例引用，不能直接删除。");
            }

            var deleted = await _petRepository.DeleteAsync(normalizedTemplateId) > 0;
            if (deleted)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleted;
        }

        private static PetTemplateEntity ApplyPet(PetTemplateEntity entity, AdminPetDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.Type = (PetType)request.Type;
            entity.InitialQualityMin = Math.Max(1, request.InitialQualityMin);
            entity.InitialQualityMax = Math.Max(entity.InitialQualityMin, request.InitialQualityMax);
            entity.MaxQuality = Math.Max(entity.InitialQualityMax, request.MaxQuality);
            entity.GrowthRateMin = Math.Max(0.1, request.GrowthRateMin);
            entity.GrowthRateMax = Math.Max(entity.GrowthRateMin, request.GrowthRateMax);
            entity.InitialSkillCount = Math.Max(0, request.InitialSkillCount);
            ApplyAttributes(entity, request.Attributes);
            entity.SkillIds = request.SkillIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).ToList();
            entity.InitialSkillCount = Math.Min(entity.InitialSkillCount, entity.SkillIds.Count);
            entity.ObtainMethod = string.IsNullOrWhiteSpace(request.ObtainMethod) ? null : request.ObtainMethod.Trim();
            entity.IsTradable = request.IsTradable;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static void ApplyAttributes(PetTemplateEntity entity, BaseAttributesRangeDto attributes)
        {
            entity.MinType1 = attributes.MinType1;
            entity.MaxType1 = attributes.MaxType1;
            entity.MinType2 = attributes.MinType2;
            entity.MaxType2 = attributes.MaxType2;
            entity.MinType3 = attributes.MinType3;
            entity.MaxType3 = attributes.MaxType3;
            entity.MinType4 = attributes.MinType4;
            entity.MaxType4 = attributes.MaxType4;
            entity.MinType5 = attributes.MinType5;
            entity.MaxType5 = attributes.MaxType5;
            entity.MinType6 = attributes.MinType6;
            entity.MaxType6 = attributes.MaxType6;
            entity.MinType7 = attributes.MinType7;
            entity.MaxType7 = attributes.MaxType7;
            entity.MinType8 = attributes.MinType8;
            entity.MaxType8 = attributes.MaxType8;
            entity.MinType9 = attributes.MinType9;
            entity.MaxType9 = attributes.MaxType9;
            entity.MinType10 = attributes.MinType10;
            entity.MaxType10 = attributes.MaxType10;
            entity.MinType11 = attributes.MinType11;
            entity.MaxType11 = attributes.MaxType11;
            entity.MinType12 = attributes.MinType12;
            entity.MaxType12 = attributes.MaxType12;
            entity.MinType13 = attributes.MinType13;
            entity.MaxType13 = attributes.MaxType13;
            entity.MinType14 = attributes.MinType14;
            entity.MaxType14 = attributes.MaxType14;
            entity.MinType15 = attributes.MinType15;
            entity.MaxType15 = attributes.MaxType15;
            entity.Element = attributes.Element ?? Element.None;
        }

        private static BaseAttributesRangeDto MapAttributes(PetTemplateEntity pet)
        {
            return new BaseAttributesRangeDto
            {
                MinType1 = pet.MinType1,
                MaxType1 = pet.MaxType1,
                MinType2 = pet.MinType2,
                MaxType2 = pet.MaxType2,
                MinType3 = pet.MinType3,
                MaxType3 = pet.MaxType3,
                MinType4 = pet.MinType4,
                MaxType4 = pet.MaxType4,
                MinType5 = pet.MinType5,
                MaxType5 = pet.MaxType5,
                MinType6 = pet.MinType6,
                MaxType6 = pet.MaxType6,
                MinType7 = pet.MinType7,
                MaxType7 = pet.MaxType7,
                MinType8 = pet.MinType8,
                MaxType8 = pet.MaxType8,
                MinType9 = pet.MinType9,
                MaxType9 = pet.MaxType9,
                MinType10 = pet.MinType10,
                MaxType10 = pet.MaxType10,
                MinType11 = pet.MinType11,
                MaxType11 = pet.MaxType11,
                MinType12 = pet.MinType12,
                MaxType12 = pet.MaxType12,
                MinType13 = pet.MinType13,
                MaxType13 = pet.MaxType13,
                MinType14 = pet.MinType14,
                MaxType14 = pet.MaxType14,
                MinType15 = pet.MinType15,
                MaxType15 = pet.MaxType15,
                Element = pet.Element
            };
        }

        private static bool HasCoreAttributeTemplate(BaseAttributesRangeDto attributes)
        {
            return attributes.MinType1.HasValue || attributes.MaxType1.HasValue ||
                   attributes.MinType2.HasValue || attributes.MaxType2.HasValue ||
                   attributes.MinType3.HasValue || attributes.MaxType3.HasValue ||
                   attributes.MinType4.HasValue || attributes.MaxType4.HasValue ||
                   attributes.MinType5.HasValue || attributes.MaxType5.HasValue ||
                   attributes.MinType6.HasValue || attributes.MaxType6.HasValue ||
                   attributes.MinType7.HasValue || attributes.MaxType7.HasValue;
        }

    }
}
#pragma warning restore CS1591
