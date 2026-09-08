using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminCollectionService : IAdminCollectionService
    {
        private readonly IRepository<TextCollectionSeriesEntity> _textSeriesRepo;
        private readonly IRepository<TextCollectionItemEntity> _textItemRepo;
        private readonly IRepository<TextCollectionBonusEntity> _textBonusRepo;
        private readonly IRepository<ImageCollectionSeriesEntity> _imageSeriesRepo;
        private readonly IRepository<ImageCollectionItemEntity> _imageItemRepo;
        private readonly IRepository<ImageCollectionBonusEntity> _imageBonusRepo;
        private readonly ICollectionService _collectionService;
        private readonly ILogger<AdminCollectionService> _logger;

        public AdminCollectionService(
            IRepository<TextCollectionSeriesEntity> textSeriesRepo,
            IRepository<TextCollectionItemEntity> textItemRepo,
            IRepository<TextCollectionBonusEntity> textBonusRepo,
            IRepository<ImageCollectionSeriesEntity> imageSeriesRepo,
            IRepository<ImageCollectionItemEntity> imageItemRepo,
            IRepository<ImageCollectionBonusEntity> imageBonusRepo,
            ICollectionService collectionService,
            ILogger<AdminCollectionService> logger)
        {
            _textSeriesRepo = textSeriesRepo;
            _textItemRepo = textItemRepo;
            _textBonusRepo = textBonusRepo;
            _imageSeriesRepo = imageSeriesRepo;
            _imageItemRepo = imageItemRepo;
            _imageBonusRepo = imageBonusRepo;
            _collectionService = collectionService;
            _logger = logger;
        }

        // ===== 文字图鉴系列 =====
        public async Task<List<AdminTextCollectionSeriesListItemDto>> GetTextSeriesListAsync(string? keyword)
        {
            var query = _textSeriesRepo.Db.Queryable<TextCollectionSeriesEntity>();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Name.Contains(keyword));
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminTextCollectionSeriesListItemDto
            {
                SeriesId = x.SeriesId, Name = x.Name, IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminTextCollectionSeriesDetailDto?> GetTextSeriesDetailAsync(string seriesId)
        {
            var entity = await _textSeriesRepo.GetByIdAsync(seriesId);
            if (entity == null) return null;
            return MapToDetail(entity);
        }

        public async Task<AdminTextCollectionSeriesDetailDto> SaveTextSeriesAsync(AdminTextCollectionSeriesDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("系列名称不能为空。");

            var existing = await _textSeriesRepo.GetByIdAsync(dto.SeriesId);
            if (existing != null)
            {
                ApplyToEntity(existing, dto);
                await _textSeriesRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new TextCollectionSeriesEntity();
                ApplyToEntity(entity, dto);
                await _textSeriesRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeleteTextSeriesAsync(string seriesId)
        {
            var items = await _textItemRepo.Db.Queryable<TextCollectionItemEntity>().Where(x => x.SeriesId == seriesId).ToListAsync();
            if (items.Count > 0)
                throw new InvalidOperationException("该系列下还有图鉴项，请先删除图鉴项。");
            return await _textSeriesRepo.DeleteAsync(seriesId) > 0;
        }

        // ===== 文字图鉴项 =====
        public async Task<List<AdminTextCollectionItemListItemDto>> GetTextItemListAsync(string? seriesId)
        {
            var query = _textItemRepo.Db.Queryable<TextCollectionItemEntity>();
            if (!string.IsNullOrWhiteSpace(seriesId))
                query = query.Where(x => x.SeriesId == seriesId);
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminTextCollectionItemListItemDto
            {
                ItemId = x.ItemId, SeriesId = x.SeriesId, Character = x.Character,
                SlotIndex = x.SlotIndex, IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminTextCollectionItemDetailDto?> GetTextItemDetailAsync(string itemId)
        {
            var entity = await _textItemRepo.GetByIdAsync(itemId);
            if (entity == null) return null;
            return new AdminTextCollectionItemDetailDto
            {
                ItemId = entity.ItemId, SeriesId = entity.SeriesId, Character = entity.Character,
                SlotIndex = entity.SlotIndex, SortOrder = entity.SortOrder, IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn, SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminTextCollectionItemDetailDto> SaveTextItemAsync(AdminTextCollectionItemDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                throw new InvalidOperationException("图鉴项ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("所属系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.Character))
                throw new InvalidOperationException("汉字内容不能为空。");

            var seriesExists = await _textSeriesRepo.GetByIdAsync(dto.SeriesId);
            if (seriesExists == null)
                throw new InvalidOperationException("所属系列不存在。");

            var existing = await _textItemRepo.GetByIdAsync(dto.ItemId);
            if (existing != null)
            {
                existing.SeriesId = dto.SeriesId;
                existing.Character = dto.Character;
                existing.SlotIndex = dto.SlotIndex;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.LastUpdateTime = DateTime.Now;
                await _textItemRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new TextCollectionItemEntity
                {
                    ItemId = dto.ItemId, SeriesId = dto.SeriesId, Character = dto.Character,
                    SlotIndex = dto.SlotIndex, SortOrder = dto.SortOrder, IsEnabled = dto.IsEnabled,
                    SeedKey = dto.SeedKey, IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _textItemRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeleteTextItemAsync(string itemId) => await _textItemRepo.DeleteAsync(itemId) > 0;

        // ===== 文字图鉴属性加成 =====
        public async Task<List<AdminTextCollectionBonusListItemDto>> GetTextBonusListAsync(string? seriesId)
        {
            var query = _textBonusRepo.Db.Queryable<TextCollectionBonusEntity>();
            if (!string.IsNullOrWhiteSpace(seriesId))
                query = query.Where(x => x.SeriesId == seriesId);
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminTextCollectionBonusListItemDto
            {
                BonusId = x.BonusId, SeriesId = x.SeriesId, AttrType = x.AttrType,
                AttrValue = x.AttrValue, ValueType = x.ValueType,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminTextCollectionBonusDetailDto?> GetTextBonusDetailAsync(string bonusId)
        {
            var entity = await _textBonusRepo.GetByIdAsync(bonusId);
            if (entity == null) return null;
            return new AdminTextCollectionBonusDetailDto
            {
                BonusId = entity.BonusId, SeriesId = entity.SeriesId, AttrType = entity.AttrType,
                AttrValue = entity.AttrValue, ValueType = entity.ValueType, SortOrder = entity.SortOrder,
                IsBuiltIn = entity.IsBuiltIn, SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminTextCollectionBonusDetailDto> SaveTextBonusAsync(AdminTextCollectionBonusDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BonusId))
                throw new InvalidOperationException("加成ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("所属系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.AttrType))
                throw new InvalidOperationException("属性类型不能为空。");

            var existing = await _textBonusRepo.GetByIdAsync(dto.BonusId);
            if (existing != null)
            {
                existing.SeriesId = dto.SeriesId;
                existing.AttrType = dto.AttrType;
                existing.AttrValue = dto.AttrValue;
                existing.ValueType = dto.ValueType;
                existing.SortOrder = dto.SortOrder;
                existing.LastUpdateTime = DateTime.Now;
                await _textBonusRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new TextCollectionBonusEntity
                {
                    BonusId = dto.BonusId, SeriesId = dto.SeriesId, AttrType = dto.AttrType,
                    AttrValue = dto.AttrValue, ValueType = dto.ValueType, SortOrder = dto.SortOrder,
                    SeedKey = dto.SeedKey, IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _textBonusRepo.AddAsync(entity);
            }
            await _collectionService.InvalidateConfigCacheAsync();
            return dto;
        }

        public async Task<bool> DeleteTextBonusAsync(string bonusId)
        {
            var result = await _textBonusRepo.DeleteAsync(bonusId) > 0;
            if (result) await _collectionService.InvalidateConfigCacheAsync();
            return result;
        }

        // ===== 图片图鉴系列 =====
        public async Task<List<AdminImageCollectionSeriesListItemDto>> GetImageSeriesListAsync(string? keyword)
        {
            var query = _imageSeriesRepo.Db.Queryable<ImageCollectionSeriesEntity>();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Name.Contains(keyword));
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminImageCollectionSeriesListItemDto
            {
                SeriesId = x.SeriesId, Name = x.Name, IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminImageCollectionSeriesDetailDto?> GetImageSeriesDetailAsync(string seriesId)
        {
            var entity = await _imageSeriesRepo.GetByIdAsync(seriesId);
            if (entity == null) return null;
            return new AdminImageCollectionSeriesDetailDto
            {
                SeriesId = entity.SeriesId, Name = entity.Name, Description = entity.Description,
                Icon = entity.Icon, SortOrder = entity.SortOrder, IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn, SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminImageCollectionSeriesDetailDto> SaveImageSeriesAsync(AdminImageCollectionSeriesDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("系列名称不能为空。");

            var existing = await _imageSeriesRepo.GetByIdAsync(dto.SeriesId);
            if (existing != null)
            {
                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.Icon = dto.Icon;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.LastUpdateTime = DateTime.Now;
                await _imageSeriesRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new ImageCollectionSeriesEntity
                {
                    SeriesId = dto.SeriesId, Name = dto.Name, Description = dto.Description,
                    Icon = dto.Icon, SortOrder = dto.SortOrder, IsEnabled = dto.IsEnabled,
                    SeedKey = dto.SeedKey, IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _imageSeriesRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeleteImageSeriesAsync(string seriesId)
        {
            var items = await _imageItemRepo.Db.Queryable<ImageCollectionItemEntity>().Where(x => x.SeriesId == seriesId).ToListAsync();
            if (items.Count > 0)
                throw new InvalidOperationException("该系列下还有图鉴项，请先删除图鉴项。");
            return await _imageSeriesRepo.DeleteAsync(seriesId) > 0;
        }

        // ===== 图片图鉴项 =====
        public async Task<List<AdminImageCollectionItemListItemDto>> GetImageItemListAsync(string? seriesId)
        {
            var query = _imageItemRepo.Db.Queryable<ImageCollectionItemEntity>();
            if (!string.IsNullOrWhiteSpace(seriesId))
                query = query.Where(x => x.SeriesId == seriesId);
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminImageCollectionItemListItemDto
            {
                ItemId = x.ItemId, SeriesId = x.SeriesId, ImageName = x.ImageName,
                ThumbUrl = x.ThumbUrl, IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminImageCollectionItemDetailDto?> GetImageItemDetailAsync(string itemId)
        {
            var entity = await _imageItemRepo.GetByIdAsync(itemId);
            if (entity == null) return null;
            return new AdminImageCollectionItemDetailDto
            {
                ItemId = entity.ItemId, SeriesId = entity.SeriesId, ImageName = entity.ImageName,
                ThumbUrl = entity.ThumbUrl, OriginalUrl = entity.OriginalUrl,
                SortOrder = entity.SortOrder, IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn, SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminImageCollectionItemDetailDto> SaveImageItemAsync(AdminImageCollectionItemDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                throw new InvalidOperationException("图鉴项ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("所属系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.ImageName))
                throw new InvalidOperationException("图片名称不能为空。");

            var seriesExists = await _imageSeriesRepo.GetByIdAsync(dto.SeriesId);
            if (seriesExists == null)
                throw new InvalidOperationException("所属系列不存在。");

            var existing = await _imageItemRepo.GetByIdAsync(dto.ItemId);
            if (existing != null)
            {
                existing.SeriesId = dto.SeriesId;
                existing.ImageName = dto.ImageName;
                existing.ThumbUrl = dto.ThumbUrl;
                existing.OriginalUrl = dto.OriginalUrl;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.LastUpdateTime = DateTime.Now;
                await _imageItemRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new ImageCollectionItemEntity
                {
                    ItemId = dto.ItemId, SeriesId = dto.SeriesId, ImageName = dto.ImageName,
                    ThumbUrl = dto.ThumbUrl, OriginalUrl = dto.OriginalUrl,
                    SortOrder = dto.SortOrder, IsEnabled = dto.IsEnabled,
                    SeedKey = dto.SeedKey, IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _imageItemRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeleteImageItemAsync(string itemId) => await _imageItemRepo.DeleteAsync(itemId) > 0;

        // ===== 图片图鉴属性加成 =====
        public async Task<List<AdminImageCollectionBonusListItemDto>> GetImageBonusListAsync(string? seriesId)
        {
            var query = _imageBonusRepo.Db.Queryable<ImageCollectionBonusEntity>();
            if (!string.IsNullOrWhiteSpace(seriesId))
                query = query.Where(x => x.SeriesId == seriesId);
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminImageCollectionBonusListItemDto
            {
                BonusId = x.BonusId, SeriesId = x.SeriesId, AttrType = x.AttrType,
                AttrValue = x.AttrValue, ValueType = x.ValueType,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminImageCollectionBonusDetailDto?> GetImageBonusDetailAsync(string bonusId)
        {
            var entity = await _imageBonusRepo.GetByIdAsync(bonusId);
            if (entity == null) return null;
            return new AdminImageCollectionBonusDetailDto
            {
                BonusId = entity.BonusId, SeriesId = entity.SeriesId, AttrType = entity.AttrType,
                AttrValue = entity.AttrValue, ValueType = entity.ValueType, SortOrder = entity.SortOrder,
                IsBuiltIn = entity.IsBuiltIn, SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminImageCollectionBonusDetailDto> SaveImageBonusAsync(AdminImageCollectionBonusDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BonusId))
                throw new InvalidOperationException("加成ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.SeriesId))
                throw new InvalidOperationException("所属系列ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.AttrType))
                throw new InvalidOperationException("属性类型不能为空。");

            var existing = await _imageBonusRepo.GetByIdAsync(dto.BonusId);
            if (existing != null)
            {
                existing.SeriesId = dto.SeriesId;
                existing.AttrType = dto.AttrType;
                existing.AttrValue = dto.AttrValue;
                existing.ValueType = dto.ValueType;
                existing.SortOrder = dto.SortOrder;
                existing.LastUpdateTime = DateTime.Now;
                await _imageBonusRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new ImageCollectionBonusEntity
                {
                    BonusId = dto.BonusId, SeriesId = dto.SeriesId, AttrType = dto.AttrType,
                    AttrValue = dto.AttrValue, ValueType = dto.ValueType, SortOrder = dto.SortOrder,
                    SeedKey = dto.SeedKey, IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _imageBonusRepo.AddAsync(entity);
            }
            await _collectionService.InvalidateConfigCacheAsync();
            return dto;
        }

        public async Task<bool> DeleteImageBonusAsync(string bonusId)
        {
            var result = await _imageBonusRepo.DeleteAsync(bonusId) > 0;
            if (result) await _collectionService.InvalidateConfigCacheAsync();
            return result;
        }

        // ===== Helpers =====
        private static AdminTextCollectionSeriesDetailDto MapToDetail(TextCollectionSeriesEntity e) => new()
        {
            SeriesId = e.SeriesId, Name = e.Name, Description = e.Description,
            Icon = e.Icon, SortOrder = e.SortOrder, IsEnabled = e.IsEnabled,
            IsBuiltIn = e.IsBuiltIn, SeedKey = e.SeedKey, BuiltInVersion = e.BuiltInVersion,
            LastUpdateTime = e.LastUpdateTime
        };

        private static void ApplyToEntity(TextCollectionSeriesEntity entity, AdminTextCollectionSeriesDetailDto dto)
        {
            entity.SeriesId = dto.SeriesId;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Icon = dto.Icon;
            entity.SortOrder = dto.SortOrder;
            entity.IsEnabled = dto.IsEnabled;
            entity.SeedKey = dto.SeedKey;
            entity.IsBuiltIn = dto.IsBuiltIn;
            entity.BuiltInVersion = dto.BuiltInVersion;
            entity.LastUpdateTime = DateTime.Now;
        }
    }
}
