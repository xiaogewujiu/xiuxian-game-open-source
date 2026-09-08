#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminEquipmentService : IAdminEquipmentService
    {
        private readonly IRepository<EquipmentTemplateEntity> _equipmentRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentInstanceRepository;
        private readonly IRepository<ForgeRecipeEntity> _forgeRepository;
        private readonly IRepository<ShopItemEntity> _shopItemRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminEquipmentService(
            IRepository<EquipmentTemplateEntity> equipmentRepository,
            IRepository<EquipmentInstanceEntity> equipmentInstanceRepository,
            IRepository<ForgeRecipeEntity> forgeRepository,
            IRepository<ShopItemEntity> shopItemRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _equipmentRepository = equipmentRepository;
            _equipmentInstanceRepository = equipmentInstanceRepository;
            _forgeRepository = forgeRepository;
            _shopItemRepository = shopItemRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminEquipmentListItemDto>> GetListAsync(string? keyword = null, int? slot = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _equipmentRepository.Db.Queryable<EquipmentTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(equipment => equipment.Name.Contains(normalizedKeyword) || equipment.EquipmentId.ToString().Contains(normalizedKeyword));
            }

            query = query.WhereIF(slot.HasValue, equipment => equipment.Slot == slot!.Value);

            var equipments = await query.OrderBy(equipment => equipment.Level).OrderBy(equipment => equipment.EquipmentId).ToListAsync();
            return equipments.Select(equipment => new AdminEquipmentListItemDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Level = equipment.Level,
                Quality = equipment.Quality,
                Slot = equipment.Slot,
                IconPath = equipment.IconPath,
                IsBuiltIn = equipment.IsBuiltIn,
                BuiltInVersion = equipment.BuiltInVersion,
                IsTradeable = equipment.IsTradeable
            }).ToList();
        }

        public async Task<AdminEquipmentDetailDto?> GetDetailAsync(int equipmentId)
        {
            if (equipmentId <= 0) return null;
            var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
            if (equipment == null) return null;
            return MapDetail(equipment);
        }

        public async Task<AdminEquipmentDetailDto> SaveAsync(AdminEquipmentDetailDto request)
        {
            if (request.EquipmentId <= 0) throw new InvalidOperationException("装备编号必须大于 0。");
            if (string.IsNullOrWhiteSpace(request.Name)) throw new InvalidOperationException("装备名称不能为空。");

            var existing = await _equipmentRepository.GetByIdAsync(request.EquipmentId);
            if (existing == null)
            {
                existing = new EquipmentTemplateEntity { EquipmentId = request.EquipmentId };
                await _equipmentRepository.AddAsync(ApplyEquipment(existing, request));
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
                return (await GetDetailAsync(request.EquipmentId))!;
            }

            ApplyEquipment(existing, request);
            await _equipmentRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            return (await GetDetailAsync(request.EquipmentId))!;
        }

        public async Task<bool> DeleteAsync(int equipmentId)
        {
            if (equipmentId <= 0) return false;
            var equipmentIdText = equipmentId.ToString();

            if (await _equipmentInstanceRepository.Db.Queryable<EquipmentInstanceEntity>().Where(item => item.TemplateId == equipmentIdText).AnyAsync())
            {
                throw new InvalidOperationException("当前装备模板仍被玩家装备实例引用，不能直接删除。");
            }

            if (await _forgeRepository.Db.Queryable<ForgeRecipeEntity>().Where(recipe => recipe.TemplateId == equipmentIdText).AnyAsync())
            {
                throw new InvalidOperationException("当前装备模板仍被锻造配方引用，不能直接删除。");
            }

            if (await _shopItemRepository.Db.Queryable<ShopItemEntity>().Where(item => item.ItemType == 1 && item.ItemId == equipmentIdText).AnyAsync())
            {
                throw new InvalidOperationException("当前装备模板仍被商店商品引用，不能直接删除。");
            }

            var deleteRows = await _equipmentRepository.DeleteAsync(equipmentId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRuntimeTemplatesAsync();
            }

            return deleteRows > 0;
        }

        private static EquipmentTemplateEntity ApplyEquipment(EquipmentTemplateEntity entity, AdminEquipmentDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Level = Math.Max(1, request.Level);
            entity.Quality = Math.Max(1, request.Quality);
            entity.Slot = request.Slot;
            entity.CombatStyle = request.CombatStyle;
            entity.WeaponCategory = request.WeaponCategory;
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.IconPath = string.IsNullOrWhiteSpace(request.IconPath) ? null : request.IconPath.Trim();
            entity.MinType1 = request.Attributes.MinType1;
            entity.MaxType1 = request.Attributes.MaxType1;
            entity.MinType2 = request.Attributes.MinType2;
            entity.MaxType2 = request.Attributes.MaxType2;
            entity.MinType3 = request.Attributes.MinType3;
            entity.MaxType3 = request.Attributes.MaxType3;
            entity.MinType4 = request.Attributes.MinType4;
            entity.MaxType4 = request.Attributes.MaxType4;
            entity.MinType5 = request.Attributes.MinType5;
            entity.MaxType5 = request.Attributes.MaxType5;
            entity.MinType6 = request.Attributes.MinType6;
            entity.MaxType6 = request.Attributes.MaxType6;
            entity.MinType7 = request.Attributes.MinType7;
            entity.MaxType7 = request.Attributes.MaxType7;
            entity.MinType8 = request.Attributes.MinType8;
            entity.MaxType8 = request.Attributes.MaxType8;
            entity.MinType9 = request.Attributes.MinType9;
            entity.MaxType9 = request.Attributes.MaxType9;
            entity.MinType10 = request.Attributes.MinType10;
            entity.MaxType10 = request.Attributes.MaxType10;
            entity.MinType11 = request.Attributes.MinType11;
            entity.MaxType11 = request.Attributes.MaxType11;
            entity.MinType12 = request.Attributes.MinType12;
            entity.MaxType12 = request.Attributes.MaxType12;
            entity.MinType13 = request.Attributes.MinType13;
            entity.MaxType13 = request.Attributes.MaxType13;
            entity.MinType14 = request.Attributes.MinType14;
            entity.MaxType14 = request.Attributes.MaxType14;
            entity.MinType15 = request.Attributes.MinType15;
            entity.MaxType15 = request.Attributes.MaxType15;
            entity.Element = request.Attributes.Element;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            entity.IsTradeable = request.IsTradeable;
            return entity;
        }

        private static AdminEquipmentDetailDto MapDetail(EquipmentTemplateEntity equipment)
        {
            return new AdminEquipmentDetailDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Level = equipment.Level,
                Quality = equipment.Quality,
                Slot = equipment.Slot,
                CombatStyle = equipment.CombatStyle,
                WeaponCategory = equipment.WeaponCategory,
                Description = equipment.Description,
                IconPath = equipment.IconPath,
                IsBuiltIn = equipment.IsBuiltIn,
                SeedKey = equipment.SeedKey,
                BuiltInVersion = equipment.BuiltInVersion,
                LastUpdateTime = equipment.LastUpdateTime,
                Attributes = new BaseAttributesRangeDto
                {
                    MinType1 = equipment.MinType1,
                    MaxType1 = equipment.MaxType1,
                    MinType2 = equipment.MinType2,
                    MaxType2 = equipment.MaxType2,
                    MinType3 = equipment.MinType3,
                    MaxType3 = equipment.MaxType3,
                    MinType4 = equipment.MinType4,
                    MaxType4 = equipment.MaxType4,
                    MinType5 = equipment.MinType5,
                    MaxType5 = equipment.MaxType5,
                    MinType6 = equipment.MinType6,
                    MaxType6 = equipment.MaxType6,
                    MinType7 = equipment.MinType7,
                    MaxType7 = equipment.MaxType7,
                    MinType8 = equipment.MinType8,
                    MaxType8 = equipment.MaxType8,
                    MinType9 = equipment.MinType9,
                    MaxType9 = equipment.MaxType9,
                    MinType10 = equipment.MinType10,
                    MaxType10 = equipment.MaxType10,
                    MinType11 = equipment.MinType11,
                    MaxType11 = equipment.MaxType11,
                    MinType12 = equipment.MinType12,
                    MaxType12 = equipment.MaxType12,
                    MinType13 = equipment.MinType13,
                    MaxType13 = equipment.MaxType13,
                    MinType14 = equipment.MinType14,
                    MaxType14 = equipment.MaxType14,
                    MinType15 = equipment.MinType15,
                    MaxType15 = equipment.MaxType15,
                    Element = equipment.Element
                },
                IsTradeable = equipment.IsTradeable
            };
        }
    }
}
#pragma warning restore CS1591
