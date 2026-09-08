using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services;

public class AdminEquipmentDecomposeRuleService : IAdminEquipmentDecomposeRuleService
{
    private readonly DbContext _dbContext;

    public AdminEquipmentDecomposeRuleService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminEquipmentDecomposeRuleDto>> GetRulesAsync()
    {
        var rules = await _dbContext.Db.Queryable<EquipmentDecomposeRuleEntity>()
            .OrderBy(rule => rule.Quality)
            .OrderBy(rule => rule.SortOrder)
            .ToListAsync();
        return rules.Select(MapToDto).ToList();
    }

    public async Task SaveRuleAsync(AdminEquipmentDecomposeRuleDto dto)
    {
        if (dto.Quality is < 1 or > 5)
        {
            throw new InvalidOperationException("装备品质必须为 1 到 5。 ");
        }

        if (string.IsNullOrWhiteSpace(dto.MaterialItemId))
        {
            throw new InvalidOperationException("必须配置产出道具。 ");
        }

        if (dto.MinQuantity <= 0 || dto.MaxQuantity < dto.MinQuantity)
        {
            throw new InvalidOperationException("产出数量区间无效。 ");
        }

        var itemExists = await _dbContext.Db.Queryable<ItemTemplateEntity>()
            .AnyAsync(item => item.ItemId == dto.MaterialItemId);
        if (!itemExists)
        {
            throw new InvalidOperationException("产出道具不存在。 ");
        }

        var duplicate = await _dbContext.Db.Queryable<EquipmentDecomposeRuleEntity>()
            .Where(rule => rule.Quality == dto.Quality && rule.GID != dto.GID && rule.IsEnabled && dto.IsEnabled)
            .AnyAsync();
        if (duplicate)
        {
            throw new InvalidOperationException($"品质 {dto.Quality} 已存在启用的分解规则。 ");
        }

        var now = DateTime.Now;
        if (dto.GID <= 0)
        {
            await _dbContext.Db.Insertable(new EquipmentDecomposeRuleEntity
            {
                Quality = dto.Quality,
                MaterialItemId = dto.MaterialItemId.Trim(),
                MinQuantity = dto.MinQuantity,
                MaxQuantity = dto.MaxQuantity,
                SortOrder = dto.SortOrder,
                IsEnabled = dto.IsEnabled,
                LastUpdateTime = now
            }).ExecuteCommandAsync();
            return;
        }

        var existing = await _dbContext.Db.Queryable<EquipmentDecomposeRuleEntity>()
            .FirstAsync(rule => rule.GID == dto.GID);
        if (existing == null)
        {
            throw new InvalidOperationException("分解规则不存在。 ");
        }

        existing.Quality = dto.Quality;
        existing.MaterialItemId = dto.MaterialItemId.Trim();
        existing.MinQuantity = dto.MinQuantity;
        existing.MaxQuantity = dto.MaxQuantity;
        existing.SortOrder = dto.SortOrder;
        existing.IsEnabled = dto.IsEnabled;
        existing.LastUpdateTime = now;
        await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
    }

    public async Task DeleteRuleAsync(long gid)
    {
        await _dbContext.Db.Deleteable<EquipmentDecomposeRuleEntity>()
            .Where(rule => rule.GID == gid)
            .ExecuteCommandAsync();
    }

    private static AdminEquipmentDecomposeRuleDto MapToDto(EquipmentDecomposeRuleEntity rule) => new()
    {
        GID = rule.GID,
        Quality = rule.Quality,
        MaterialItemId = rule.MaterialItemId,
        MinQuantity = rule.MinQuantity,
        MaxQuantity = rule.MaxQuantity,
        SortOrder = rule.SortOrder,
        IsEnabled = rule.IsEnabled,
        IsBuiltIn = rule.IsBuiltIn,
        BuiltInVersion = rule.BuiltInVersion,
        SeedKey = rule.SeedKey,
        LastUpdateTime = rule.LastUpdateTime
    };
}
