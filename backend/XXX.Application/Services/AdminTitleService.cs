using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminTitleService : IAdminTitleService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<TitleTemplateEntity> _titleTemplateRepository;
        private readonly ITitleService _titleService;
        private readonly ILogger<AdminTitleService> _logger;

        public AdminTitleService(
            DbContext dbContext,
            IRepository<TitleTemplateEntity> titleTemplateRepository,
            ITitleService titleService,
            ILogger<AdminTitleService> logger)
        {
            _dbContext = dbContext;
            _titleTemplateRepository = titleTemplateRepository;
            _titleService = titleService;
            _logger = logger;
        }

        public async Task<List<AdminTitleListItemDto>> GetListAsync(string? keyword = null, int take = 200)
        {
            var query = _dbContext.Db.Queryable<TitleTemplateEntity>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(t => t.TitleId.Contains(kw) || t.Name.Contains(kw));
            }

            var list = await query
                .OrderByDescending(t => t.CreatedAt)
                .Take(Math.Max(1, take))
                .ToListAsync();

            return list.Select(t => new AdminTitleListItemDto
            {
                Id = t.Id,
                TitleId = t.TitleId,
                Name = t.Name,
                Source = t.Source,
                Rarity = t.Rarity,
                IsVisible = t.IsVisible
            }).ToList();
        }

        public async Task<AdminTitleDetailDto?> GetDetailAsync(long id)
        {
            var entity = await _titleTemplateRepository.GetByIdAsync(id);
            if (entity == null) return null;

            return new AdminTitleDetailDto
            {
                Id = entity.Id,
                TitleId = entity.TitleId,
                Name = entity.Name,
                Description = entity.Description,
                Source = entity.Source,
                SourceId = entity.SourceId,
                Rarity = entity.Rarity,
                IconPath = entity.IconPath,
                ImagePath = entity.ImagePath,
                IsVisible = entity.IsVisible,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<AdminTitleDetailDto> CreateAsync(AdminTitleDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("称号名称不能为空。");

            if (string.IsNullOrWhiteSpace(dto.TitleId))
            {
                dto.TitleId = "title_" + Guid.NewGuid().ToString("N")[..8];
            }

            var exists = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => t.TitleId == dto.TitleId)
                .AnyAsync();

            if (exists)
                throw new InvalidOperationException($"称号ID '{dto.TitleId}' 已存在。");

            var entity = new TitleTemplateEntity
            {
                TitleId = dto.TitleId,
                Name = dto.Name,
                Description = dto.Description,
                Source = dto.Source,
                SourceId = dto.SourceId,
                Rarity = dto.Rarity,
                IconPath = dto.IconPath,
                ImagePath = dto.ImagePath,
                IsVisible = dto.IsVisible,
                CreatedAt = DateTime.Now
            };

            await _titleTemplateRepository.AddAsync(entity);

            _logger.LogInformation("称号模板已创建。TitleId={TitleId}", entity.TitleId);

            return new AdminTitleDetailDto
            {
                Id = entity.Id,
                TitleId = entity.TitleId,
                Name = entity.Name,
                Description = entity.Description,
                Source = entity.Source,
                SourceId = entity.SourceId,
                Rarity = entity.Rarity,
                IconPath = entity.IconPath,
                ImagePath = entity.ImagePath,
                IsVisible = entity.IsVisible,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<AdminTitleDetailDto?> UpdateAsync(long id, AdminTitleDetailDto dto)
        {
            var entity = await _titleTemplateRepository.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Source = dto.Source;
            entity.SourceId = dto.SourceId;
            entity.Rarity = dto.Rarity;
            entity.IconPath = dto.IconPath;
            entity.ImagePath = dto.ImagePath;
            entity.IsVisible = dto.IsVisible;

            await _titleTemplateRepository.UpdateAsync(entity);

            _logger.LogInformation("称号模板已更新。Id={Id}, TitleId={TitleId}", id, entity.TitleId);

            return new AdminTitleDetailDto
            {
                Id = entity.Id,
                TitleId = entity.TitleId,
                Name = entity.Name,
                Description = entity.Description,
                Source = entity.Source,
                SourceId = entity.SourceId,
                Rarity = entity.Rarity,
                IconPath = entity.IconPath,
                ImagePath = entity.ImagePath,
                IsVisible = entity.IsVisible,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _titleTemplateRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _titleTemplateRepository.DeleteAsync(id);
            _logger.LogInformation("称号模板已删除。Id={Id}, TitleId={TitleId}", id, entity.TitleId);
            return true;
        }

        public async Task<bool> GrantTitleAsync(AdminGrantTitleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PlayerId))
                throw new InvalidOperationException("玩家ID不能为空。");

            if (string.IsNullOrWhiteSpace(dto.TitleId))
                throw new InvalidOperationException("称号ID不能为空。");

            return await _titleService.GrantTitleAsync(dto.PlayerId, dto.TitleId);
        }

        public async Task<bool> RevokeTitleAsync(AdminGrantTitleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PlayerId))
                throw new InvalidOperationException("玩家ID不能为空。");

            if (string.IsNullOrWhiteSpace(dto.TitleId))
                throw new InvalidOperationException("称号ID不能为空。");

            return await _titleService.RevokeTitleAsync(dto.PlayerId, dto.TitleId);
        }
    }
}
