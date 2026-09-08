using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class TitleService : ITitleService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<TitleTemplateEntity> _titleTemplateRepository;
        private readonly IRepository<PlayerTitleEntity> _playerTitleRepository;
        private readonly ILogger<TitleService> _logger;

        public TitleService(
            DbContext dbContext,
            IRepository<TitleTemplateEntity> titleTemplateRepository,
            IRepository<PlayerTitleEntity> playerTitleRepository,
            ILogger<TitleService> logger)
        {
            _dbContext = dbContext;
            _titleTemplateRepository = titleTemplateRepository;
            _playerTitleRepository = playerTitleRepository;
            _logger = logger;
        }

        public async Task<PlayerTitleOverviewDto> GetOverviewAsync(string playerId)
        {
            var playerTitles = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.UnlockedAt)
                .ToListAsync();

            // 佩戴中的排前面
            playerTitles = playerTitles.OrderByDescending(t => t.IsEquipped).ThenByDescending(t => t.UnlockedAt).ToList();

            if (playerTitles.Count == 0)
            {
                return new PlayerTitleOverviewDto();
            }

            var titleIds = playerTitles.Select(t => t.TitleId).Distinct().ToList();
            var templates = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => titleIds.Contains(t.TitleId) && t.IsVisible)
                .ToListAsync();

            var templateMap = templates.ToDictionary(t => t.TitleId);

            var titles = new List<TitleListItemDto>();
            CurrentTitleDto? currentTitle = null;

            foreach (var pt in playerTitles)
            {
                if (!templateMap.TryGetValue(pt.TitleId, out var template)) continue;

                var item = new TitleListItemDto
                {
                    TitleId = pt.TitleId,
                    Name = template.Name,
                    Description = template.Description,
                    Rarity = template.Rarity,
                    IconPath = template.IconPath,
                    ImagePath = template.ImagePath,
                    UnlockedAt = pt.UnlockedAt,
                    IsEquipped = pt.IsEquipped
                };

                titles.Add(item);

                if (pt.IsEquipped)
                {
                    currentTitle = new CurrentTitleDto
                    {
                        TitleId = template.TitleId,
                        Name = template.Name,
                        Rarity = template.Rarity,
                        IconPath = template.IconPath,
                        ImagePath = template.ImagePath
                    };
                }
            }

            return new PlayerTitleOverviewDto
            {
                CurrentTitle = currentTitle,
                Titles = titles
            };
        }

        public async Task<bool> EquipTitleAsync(string playerId, string titleId)
        {
            var playerTitle = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId && t.TitleId == titleId)
                .FirstAsync();

            if (playerTitle == null)
            {
                _logger.LogWarning("玩家未拥有称号。PlayerId={PlayerId}, TitleId={TitleId}", playerId, titleId);
                return false;
            }

            _dbContext.BeginTransaction();
            try
            {
                // 先取消当前佩戴
                var currentEquipped = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                    .Where(t => t.PlayerId == playerId && t.IsEquipped)
                    .ToListAsync();

                foreach (var ct in currentEquipped)
                {
                    ct.IsEquipped = false;
                }

                if (currentEquipped.Count > 0)
                {
                    await _playerTitleRepository.UpdateRangeAsync(currentEquipped);
                }

                // 佩戴新称号
                playerTitle.IsEquipped = true;
                await _playerTitleRepository.UpdateAsync(playerTitle);

                _dbContext.CommitTransaction();
                return true;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task<bool> UnequipTitleAsync(string playerId)
        {
            var currentEquipped = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId && t.IsEquipped)
                .ToListAsync();

            if (currentEquipped.Count == 0) return true;

            _dbContext.BeginTransaction();
            try
            {
                foreach (var ct in currentEquipped)
                {
                    ct.IsEquipped = false;
                }

                await _playerTitleRepository.UpdateRangeAsync(currentEquipped);
                _dbContext.CommitTransaction();
                return true;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task<CurrentTitleDto?> GetCurrentTitleAsync(string playerId)
        {
            var equipped = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId && t.IsEquipped)
                .FirstAsync();

            if (equipped == null) return null;

            var template = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => t.TitleId == equipped.TitleId)
                .FirstAsync();

            if (template == null) return null;

            return new CurrentTitleDto
            {
                TitleId = template.TitleId,
                Name = template.Name,
                Rarity = template.Rarity,
                IconPath = template.IconPath,
                ImagePath = template.ImagePath
            };
        }

        public async Task<bool> GrantTitleAsync(string playerId, string titleId)
        {
            var existing = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId && t.TitleId == titleId)
                .FirstAsync();

            if (existing != null) return false; // 已拥有

            var template = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => t.TitleId == titleId)
                .FirstAsync();

            // 自动创建称号模板（如果不存在）
            if (template == null)
            {
                template = new TitleTemplateEntity
                {
                    TitleId = titleId,
                    Name = titleId,
                    Description = $"系统自动创建的称号: {titleId}",
                    Source = "System",
                    Rarity = "Common"
                };
                await _dbContext.Db.Insertable(template).ExecuteCommandAsync();
                _logger.LogInformation("自动创建称号模板。TitleId={TitleId}", titleId);
            }

            var entity = new PlayerTitleEntity
            {
                PlayerId = playerId,
                TitleId = titleId,
                UnlockedAt = DateTime.Now,
                IsEquipped = false
            };

            await _playerTitleRepository.AddAsync(entity);

            // 同步更新 UserEntity.CurrentTitle 以保持一致
            await _dbContext.Db.Updateable<UserEntity>()
                .SetColumns(u => u.CurrentTitle == template.Name)
                .Where(u => u.GID == playerId)
                .ExecuteCommandAsync();

            return true;
        }

        public async Task<bool> RevokeTitleAsync(string playerId, string titleId)
        {
            var entity = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(t => t.PlayerId == playerId && t.TitleId == titleId)
                .FirstAsync();

            if (entity == null) return false;

            _dbContext.BeginTransaction();
            try
            {
                // 如果称号正在佩戴，先取消佩戴
                if (entity.IsEquipped)
                {
                    entity.IsEquipped = false;
                    await _playerTitleRepository.UpdateAsync(entity);
                }

                await _playerTitleRepository.DeleteAsync(entity.Id);
                _dbContext.CommitTransaction();
                return true;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }
    }
}
