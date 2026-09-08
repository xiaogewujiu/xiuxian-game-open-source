using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class GemService : IGemService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly ILogger<GemService> _logger;

        public GemService(
            DbContext dbContext,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IInventoryService inventoryService,
            IPlayerAttributeService playerAttributeService,
            ILogger<GemService> logger)
        {
            _dbContext = dbContext;
            _inventoryRepository = inventoryRepository;
            _equipmentRepository = equipmentRepository;
            _inventoryService = inventoryService;
            _playerAttributeService = playerAttributeService;
            _logger = logger;
        }

        public async Task<List<GemTemplateDto>> GetTemplatesAsync()
        {
            var templates = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .OrderBy(g => g.AttributeType)
                .OrderBy(g => g.Level)
                .ToListAsync();

            return templates.Select(MapToDto).ToList();
        }

        public async Task<List<GemInventoryDto>> GetInventoryGemsAsync(string playerId)
        {
            var gemTemplates = await _dbContext.Db.Queryable<GemTemplateEntity>().ToListAsync();
            var gemIdSet = gemTemplates.Select(g => g.GemId).ToHashSet();
            var templateMap = gemTemplates.ToDictionary(g => g.GemId);

            var items = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(i => i.PlayerId == playerId && gemIdSet.Contains(i.ItemId))
                .ToListAsync();

            return items.Select(i =>
            {
                templateMap.TryGetValue(i.ItemId, out var t);
                return new GemInventoryDto
                {
                    InventoryItemId = i.Id,
                    GemId = i.ItemId,
                    Name = t?.Name ?? i.ItemId,
                    Level = t?.Level ?? 0,
                    AttributeType = t?.AttributeType ?? "",
                    BonusValue = t?.BonusValue ?? 0,
                    BonusMode = t?.BonusMode ?? "Flat",
                    Quality = t?.Quality ?? 0,
                    Quantity = i.Quantity
                };
            }).ToList();
        }

        public async Task SocketGemAsync(string playerId, SocketGemDto dto)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(
                e => e.PlayerId == playerId && e.InstanceId == dto.EquipmentInstanceId);
            if (equipment == null)
                throw new InvalidOperationException("装备不存在。");

            var maxSlots = GetMaxSlots(equipment.Quality);
            if (dto.SlotIndex < 0 || dto.SlotIndex >= maxSlots)
                throw new InvalidOperationException("无效的孔位索引。");

            var invItem = await _inventoryRepository.GetFirstAsync(
                i => i.Id == dto.InventoryItemId && i.PlayerId == playerId);
            if (invItem == null)
                throw new InvalidOperationException("宝石不存在。");

            var gemTemplate = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.GemId == invItem.ItemId)
                .FirstAsync();
            if (gemTemplate == null)
                throw new InvalidOperationException("该物品不是宝石。");

            var slots = ParseGemSlots(equipment.GemSlotsJson);
            if (slots.Any(s => s.SlotIndex == dto.SlotIndex && s.GemId != null))
                throw new InvalidOperationException("该孔位已有宝石，请先取下。");

            _dbContext.BeginTransaction();
            try
            {
                // 从背包移除宝石
                if (invItem.Quantity <= 1)
                    await _inventoryRepository.DeleteAsync(invItem.Id);
                else
                {
                    invItem.Quantity--;
                    await _inventoryRepository.UpdateAsync(invItem);
                }

                // 更新装备宝石孔
                var existing = slots.FirstOrDefault(s => s.SlotIndex == dto.SlotIndex);
                if (existing != null)
                {
                    existing.GemId = gemTemplate.GemId;
                    existing.AttributeType = gemTemplate.AttributeType;
                    existing.BonusValue = gemTemplate.BonusValue;
                    existing.BonusMode = gemTemplate.BonusMode;
                }
                else
                    slots.Add(new GemSlotEntry
                    {
                        SlotIndex = dto.SlotIndex,
                        GemId = gemTemplate.GemId,
                        AttributeType = gemTemplate.AttributeType,
                        BonusValue = gemTemplate.BonusValue,
                        BonusMode = gemTemplate.BonusMode
                    });

                equipment.GemSlotsJson = JsonSerializer.Serialize(slots);
                equipment.LastUpdateTime = DateTime.Now;
                await _equipmentRepository.UpdateAsync(equipment);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            // 重算属性
            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
        }

        public async Task UnsocketGemAsync(string playerId, UnsocketGemDto dto)
        {
            var equipment = await _equipmentRepository.GetFirstAsync(
                e => e.PlayerId == playerId && e.InstanceId == dto.EquipmentInstanceId);
            if (equipment == null)
                throw new InvalidOperationException("装备不存在。");

            var slots = ParseGemSlots(equipment.GemSlotsJson);
            var slot = slots.FirstOrDefault(s => s.SlotIndex == dto.SlotIndex);
            if (slot == null || slot.GemId == null)
                throw new InvalidOperationException("该孔位没有宝石。");

            var gemId = slot.GemId;

            _dbContext.BeginTransaction();
            try
            {
                // 宝石返还背包
                await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                {
                    ItemId = gemId,
                    Quantity = 1,
                    Source = "GemUnsocket"
                });

                // 清空孔位
                slot.GemId = null;
                slot.AttributeType = null;
                slot.BonusValue = 0;
                slot.BonusMode = "Flat";
                equipment.GemSlotsJson = JsonSerializer.Serialize(slots);
                equipment.LastUpdateTime = DateTime.Now;
                await _equipmentRepository.UpdateAsync(equipment);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId);
        }

        public async Task SynthesizeGemAsync(string playerId, SynthesizeGemDto dto)
        {
            var gemTemplate = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.GemId == dto.GemId)
                .FirstAsync();
            if (gemTemplate == null)
                throw new InvalidOperationException("宝石模板不存在。");

            if (string.IsNullOrEmpty(gemTemplate.SynthFromGemId))
                throw new InvalidOperationException("该宝石不能合成。");

            var requiredCount = gemTemplate.SynthCount;

            // 查找源宝石模板（合成材料）
            var sourceTemplate = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.GemId == gemTemplate.SynthFromGemId)
                .FirstAsync();
            if (sourceTemplate == null)
                throw new InvalidOperationException("合成材料宝石模板不存在。");

            // 校验并扣减源宝石（自动按数量扣减，前端只需传一个物品ID）
            var sourceItem = await _inventoryRepository.GetFirstAsync(
                i => i.Id == dto.InventoryItemId && i.PlayerId == playerId && i.ItemId == gemTemplate.SynthFromGemId);
            if (sourceItem == null)
                throw new InvalidOperationException("宝石不足或不匹配。");

            if (sourceItem.Quantity < requiredCount)
                throw new InvalidOperationException($"需要 {requiredCount} 个宝石进行合成，当前只有 {sourceItem.Quantity} 个。");

            // 概率判定
            var successRate = gemTemplate.SynthSuccessRate;
            var roll = Random.Shared.Next(1, 101); // 1-100
            var isSuccess = roll <= successRate;

            _dbContext.BeginTransaction();
            try
            {
                // 扣除材料宝石（无论成功失败都扣除）
                if (sourceItem.Quantity <= requiredCount)
                    await _inventoryRepository.DeleteAsync(sourceItem.Id);
                else
                {
                    sourceItem.Quantity -= requiredCount;
                    await _inventoryRepository.UpdateAsync(sourceItem);
                }

                if (isSuccess)
                {
                    // 成功：添加合成目标宝石
                    await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                    {
                        ItemId = dto.GemId,
                        Quantity = 1,
                        Source = "GemSynthesize"
                    });
                }

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }

            if (!isSuccess)
                throw new InvalidOperationException($"合成失败！（成功率 {successRate}%）材料已消耗。");
        }

        public static int GetMaxSlots(int quality) => quality switch
        {
            <= 2 => 1,  // Common, Uncommon
            <= 4 => 2,  // Rare, Epic
            _ => 3      // Legendary
        };

        private static List<GemSlotEntry> ParseGemSlots(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];
            try { return JsonSerializer.Deserialize<List<GemSlotEntry>>(json) ?? []; }
            catch { return []; }
        }

        private static GemTemplateDto MapToDto(GemTemplateEntity e) => new()
        {
            GemId = e.GemId,
            Name = e.Name,
            Level = e.Level,
            AttributeType = e.AttributeType,
            BonusValue = e.BonusValue,
            BonusMode = e.BonusMode,
            IconPath = e.IconPath,
            Quality = e.Quality,
            SynthCount = e.SynthCount,
            SynthFromGemId = e.SynthFromGemId,
            SynthSuccessRate = e.SynthSuccessRate
        };
    }

    /// <summary>
    /// 宝石孔位条目（JSON序列化用）
    /// </summary>
    public class GemSlotEntry
    {
        public int SlotIndex { get; set; }
        public string? GemId { get; set; }
        public string? AttributeType { get; set; }
        public int BonusValue { get; set; }
        public string BonusMode { get; set; } = "Flat";
    }

    public class AdminGemService : IAdminGemService
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<AdminGemService> _logger;

        public AdminGemService(DbContext dbContext, ILogger<AdminGemService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<AdminGemListItemDto>> GetListAsync()
        {
            var list = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .OrderBy(g => g.AttributeType)
                .OrderBy(g => g.Level)
                .ToListAsync();

            return list.Select(e => new AdminGemListItemDto
            {
                Id = e.Id,
                GemId = e.GemId,
                Name = e.Name,
                Level = e.Level,
                AttributeType = e.AttributeType,
                BonusValue = e.BonusValue,
                BonusMode = e.BonusMode,
                Quality = e.Quality,
                SynthCount = e.SynthCount,
                SynthFromGemId = e.SynthFromGemId,
                IsBuiltIn = e.IsBuiltIn,
                SynthSuccessRate = e.SynthSuccessRate
            }).ToList();
        }

        public async Task<AdminGemDetailDto?> GetDetailAsync(long id)
        {
            var e = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.Id == id).FirstAsync();
            if (e == null) return null;

            return new AdminGemDetailDto
            {
                Id = e.Id,
                GemId = e.GemId,
                Name = e.Name,
                Level = e.Level,
                AttributeType = e.AttributeType,
                BonusValue = e.BonusValue,
                BonusMode = e.BonusMode,
                IconPath = e.IconPath,
                Quality = e.Quality,
                SynthCount = e.SynthCount,
                SynthFromGemId = e.SynthFromGemId,
                IsBuiltIn = e.IsBuiltIn,
                SynthSuccessRate = e.SynthSuccessRate
            };
        }

        public async Task CreateAsync(AdminGemSaveDto dto)
        {
            var entity = new GemTemplateEntity
            {
                GemId = dto.GemId,
                Name = dto.Name,
                Level = dto.Level,
                AttributeType = dto.AttributeType,
                BonusValue = dto.BonusValue,
                BonusMode = dto.BonusMode,
                IconPath = dto.IconPath,
                Quality = dto.Quality,
                SynthCount = dto.SynthCount,
                SynthFromGemId = dto.SynthFromGemId,
                SynthSuccessRate = dto.SynthSuccessRate
            };
            await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task UpdateAsync(long id, AdminGemSaveDto dto)
        {
            var entity = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.Id == id).FirstAsync();
            if (entity == null)
                throw new InvalidOperationException("宝石模板不存在。");

            entity.GemId = dto.GemId;
            entity.Name = dto.Name;
            entity.Level = dto.Level;
            entity.AttributeType = dto.AttributeType;
            entity.BonusValue = dto.BonusValue;
            entity.BonusMode = dto.BonusMode;
            entity.IconPath = dto.IconPath;
            entity.Quality = dto.Quality;
            entity.SynthCount = dto.SynthCount;
            entity.SynthFromGemId = dto.SynthFromGemId;
            entity.SynthSuccessRate = dto.SynthSuccessRate;

            await _dbContext.Db.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task DeleteAsync(long id)
        {
            await _dbContext.Db.Deleteable<GemTemplateEntity>()
                .Where(g => g.Id == id).ExecuteCommandAsync();
        }

        public async Task<int> BatchGenerateAsync(AdminGemBatchGenerateDto dto)
        {
            var existing = await _dbContext.Db.Queryable<GemTemplateEntity>()
                .Where(g => g.AttributeType == dto.AttributeType && g.BonusMode == dto.BonusMode)
                .ToListAsync();
            var existingLevels = existing.Select(g => g.Level).ToHashSet();

            var attrLower = dto.AttributeType.ToLower();
            var modeLower = dto.BonusMode.ToLower();
            var toInsert = new List<GemTemplateEntity>();

            for (int level = 1; level <= dto.MaxLevel; level++)
            {
                if (existingLevels.Contains(level)) continue;

                var bonusValue = dto.BaseBonusValue + (level - 1) * dto.BonusValueGrowth;
                var gemId = $"gem_{attrLower}_lv{level}_{modeLower}";
                var prevGemId = level > 1
                    ? $"gem_{attrLower}_lv{level - 1}_{modeLower}"
                    : null;

                toInsert.Add(new GemTemplateEntity
                {
                    GemId = gemId,
                    Name = $"{dto.NamePrefix} Lv.{level}",
                    Level = level,
                    AttributeType = dto.AttributeType,
                    BonusValue = bonusValue,
                    BonusMode = dto.BonusMode,
                    Quality = Math.Min(level, 5),
                    SynthCount = 3,
                    SynthFromGemId = prevGemId,
                    SynthSuccessRate = dto.SynthSuccessRate
                });
            }

            if (toInsert.Count > 0)
                await _dbContext.Db.Insertable(toInsert).ExecuteCommandAsync();

            return toInsert.Count;
        }
    }
}
