#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminCropService : IAdminCropService
    {
        private readonly IRepository<CropTemplateEntity> _cropRepository;
        private readonly IRepository<ItemTemplateEntity> _itemRepository;
        private readonly IRepository<SpiritFieldPlotEntity> _plotRepository;

        public AdminCropService(
            IRepository<CropTemplateEntity> cropRepository,
            IRepository<ItemTemplateEntity> itemRepository,
            IRepository<SpiritFieldPlotEntity> plotRepository)
        {
            _cropRepository = cropRepository;
            _itemRepository = itemRepository;
            _plotRepository = plotRepository;
        }

        public async Task<List<AdminCropListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _cropRepository.Db.Queryable<CropTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(crop => crop.TemplateId.Contains(normalizedKeyword) || crop.Name.Contains(normalizedKeyword));
            }

            var crops = await query.OrderBy(crop => crop.TemplateId).ToListAsync();
            return crops.Select(crop => new AdminCropListItemDto
            {
                TemplateId = crop.TemplateId,
                Name = crop.Name,
                Type = (int)crop.Type,
                UnlockLevel = crop.UnlockLevel,
                IsBuiltIn = crop.IsBuiltIn,
                BuiltInVersion = crop.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminCropDetailDto?> GetDetailAsync(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return null;
            }

            var crop = await _cropRepository.GetByIdAsync(templateId.Trim());
            if (crop == null)
            {
                return null;
            }

            return new AdminCropDetailDto
            {
                TemplateId = crop.TemplateId,
                Name = crop.Name,
                Description = crop.Description,
                Type = (int)crop.Type,
                GrowthCycle = crop.GrowthCycle,
                Yield = crop.Yield,
                SeedId = crop.SeedId,
                SeedAmount = crop.SeedAmount,
                OutputItemId = crop.OutputItemId,
                OutputAmount = crop.OutputAmount,
                MinQuality = crop.MinQuality,
                MaxQuality = crop.MaxQuality,
                UnlockLevel = crop.UnlockLevel,
                IsBuiltIn = crop.IsBuiltIn,
                SeedKey = crop.SeedKey,
                BuiltInVersion = crop.BuiltInVersion,
                LastUpdateTime = crop.LastUpdateTime
            };
        }

        public async Task<AdminCropDetailDto> SaveAsync(AdminCropDetailDto request)
        {
            var templateId = (request.TemplateId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(templateId))
            {
                throw new InvalidOperationException("作物模板编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidOperationException("作物名称不能为空。");
            }

            var seedId = (request.SeedId ?? string.Empty).Trim();
            var outputItemId = (request.OutputItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(seedId) || string.IsNullOrWhiteSpace(outputItemId))
            {
                throw new InvalidOperationException("种子编号和产出物品编号不能为空。");
            }

            if (!await _itemRepository.ExistsAsync(item => item.ItemId == seedId))
            {
                throw new InvalidOperationException("种子道具不存在，请先创建对应道具模板。");
            }

            var seedTemplate = await _itemRepository.GetByIdAsync(seedId);
            if (seedTemplate == null || seedTemplate.Type != (int)ItemType.Seed)
            {
                throw new InvalidOperationException("种子道具必须是 Seed 类型。");
            }

            if (!await _itemRepository.ExistsAsync(item => item.ItemId == outputItemId))
            {
                throw new InvalidOperationException("产出道具不存在，请先创建对应道具模板。");
            }

            var existing = await _cropRepository.GetByIdAsync(templateId);
            if (existing == null)
            {
                existing = new CropTemplateEntity { TemplateId = templateId };
                await _cropRepository.AddAsync(ApplyCrop(existing, request));
                return (await GetDetailAsync(templateId))!;
            }

            ApplyCrop(existing, request);
            await _cropRepository.UpdateAsync(existing);
            return (await GetDetailAsync(templateId))!;
        }

        public async Task<bool> DeleteAsync(string templateId)
        {
            var normalizedTemplateId = (templateId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedTemplateId))
            {
                return false;
            }

            var hasActivePlots = await _plotRepository.Db.Queryable<SpiritFieldPlotEntity>()
                .Where(plot => plot.CropTemplateId == normalizedTemplateId)
                .AnyAsync();
            if (hasActivePlots)
            {
                throw new InvalidOperationException("当前作物模板仍被灵田地块引用，不能直接删除。");
            }

            return await _cropRepository.DeleteAsync(normalizedTemplateId) > 0;
        }

        private static CropTemplateEntity ApplyCrop(CropTemplateEntity entity, AdminCropDetailDto request)
        {
            entity.Name = request.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.Type = (CropType)request.Type;
            entity.GrowthCycle = Math.Max(1, request.GrowthCycle);
            entity.Yield = Math.Max(1, request.Yield);
            entity.SeedId = (request.SeedId ?? string.Empty).Trim();
            entity.SeedAmount = Math.Max(1, request.SeedAmount);
            entity.OutputItemId = (request.OutputItemId ?? string.Empty).Trim();
            entity.OutputAmount = Math.Max(1, request.OutputAmount);
            entity.MinQuality = Math.Max(1, request.MinQuality);
            entity.MaxQuality = Math.Max(entity.MinQuality, request.MaxQuality);
            entity.UnlockLevel = Math.Max(1, request.UnlockLevel);
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
