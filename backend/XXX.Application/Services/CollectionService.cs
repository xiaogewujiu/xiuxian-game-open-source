using Microsoft.Extensions.Logging;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    public class CollectionService : ICollectionService
    {
        private const string ConfigCacheKey = "collection:config";
        private const string BonusCacheKeyPrefix = "collection:bonuses:";
        private const int ConfigCacheMinutes = 60;
        private const int BonusCacheMinutes = 10;

        private readonly ISqlSugarClient _db;
        private readonly Infrastructure.Cache.ICacheService _cache;
        private readonly ILogger<CollectionService> _logger;

        public CollectionService(DbContext dbContext, Infrastructure.Cache.ICacheService cache, ILogger<CollectionService> logger)
        {
            _db = dbContext.Db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<PlayerCollectionSeriesDto>> GetCollectionConfigAsync()
        {
            return await _cache.GetOrSetAsync(ConfigCacheKey, LoadCollectionConfigAsync, ConfigCacheMinutes);
        }

        private async Task<List<PlayerCollectionSeriesDto>> LoadCollectionConfigAsync()
        {
            var result = new List<PlayerCollectionSeriesDto>();

            // 文字图鉴
            var textSeries = await _db.Queryable<TextCollectionSeriesEntity>()
                .Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToListAsync();
            var textItems = await _db.Queryable<TextCollectionItemEntity>()
                .Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToListAsync();
            var textBonuses = await _db.Queryable<TextCollectionBonusEntity>()
                .OrderBy(x => x.SortOrder).ToListAsync();

            foreach (var series in textSeries)
            {
                var items = textItems.Where(x => x.SeriesId == series.SeriesId).ToList();
                var bonuses = textBonuses.Where(x => x.SeriesId == series.SeriesId).ToList();
                result.Add(new PlayerCollectionSeriesDto
                {
                    SeriesId = series.SeriesId,
                    Name = series.Name,
                    Description = series.Description,
                    Icon = series.Icon,
                    CollectionType = 0,
                    TotalCount = items.Count,
                    Items = items.Select(i => new PlayerCollectionItemDto
                    {
                        ItemId = i.ItemId,
                        DisplayName = i.Character,
                        SlotIndex = i.SlotIndex
                    }).ToList(),
                    Bonuses = bonuses.Select(b => new CollectionBonusDto
                    {
                        SeriesId = b.SeriesId,
                        SeriesName = series.Name,
                        AttrType = b.AttrType,
                        AttrValue = b.AttrValue,
                        ValueType = b.ValueType
                    }).ToList()
                });
            }

            // 图片图鉴
            var imageSeries = await _db.Queryable<ImageCollectionSeriesEntity>()
                .Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToListAsync();
            var imageItems = await _db.Queryable<ImageCollectionItemEntity>()
                .Where(x => x.IsEnabled).OrderBy(x => x.SortOrder).ToListAsync();
            var imageBonuses = await _db.Queryable<ImageCollectionBonusEntity>()
                .OrderBy(x => x.SortOrder).ToListAsync();

            foreach (var series in imageSeries)
            {
                var items = imageItems.Where(x => x.SeriesId == series.SeriesId).ToList();
                var bonuses = imageBonuses.Where(x => x.SeriesId == series.SeriesId).ToList();
                result.Add(new PlayerCollectionSeriesDto
                {
                    SeriesId = series.SeriesId,
                    Name = series.Name,
                    Description = series.Description,
                    Icon = series.Icon,
                    CollectionType = 1,
                    TotalCount = items.Count,
                    Items = items.Select(i => new PlayerCollectionItemDto
                    {
                        ItemId = i.ItemId,
                        DisplayName = i.ImageName,
                        ThumbUrl = i.ThumbUrl,
                        OriginalUrl = i.OriginalUrl,
                        SlotIndex = i.SortOrder
                    }).ToList(),
                    Bonuses = bonuses.Select(b => new CollectionBonusDto
                    {
                        SeriesId = b.SeriesId,
                        SeriesName = series.Name,
                        AttrType = b.AttrType,
                        AttrValue = b.AttrValue,
                        ValueType = b.ValueType
                    }).ToList()
                });
            }

            return result;
        }

        public async Task<List<PlayerCollectionSeriesDto>> GetPlayerCollectionAsync(string playerId)
        {
            var config = await GetCollectionConfigAsync();
            var playerRecords = await _db.Queryable<PlayerCollectionEntity>()
                .Where(x => x.PlayerId == playerId && !x.IsDeleted).ToListAsync();

            foreach (var series in config)
            {
                foreach (var item in series.Items)
                {
                    var record = playerRecords.FirstOrDefault(x =>
                        x.SeriesId == series.SeriesId && x.ItemId == item.ItemId);
                    if (record != null)
                    {
                        item.IsOwned = true;
                        item.OwnedCount = record.OwnedCount;
                    }
                }
                series.OwnedCount = series.Items.Count(x => x.IsOwned);
                series.IsComplete = series.OwnedCount >= series.TotalCount;
            }

            return config;
        }

        public async Task<List<CollectionBonusSummaryDto>> GetPlayerBonusesAsync(string playerId)
        {
            var cacheKey = $"{BonusCacheKeyPrefix}{playerId}";
            return await _cache.GetOrSetAsync(cacheKey, () => CalculatePlayerBonusesAsync(playerId), BonusCacheMinutes);
        }

        private async Task<List<CollectionBonusSummaryDto>> CalculatePlayerBonusesAsync(string playerId)
        {
            var playerCollections = await GetPlayerCollectionAsync(playerId);
            var bonusMap = new Dictionary<string, CollectionBonusSummaryDto>();

            foreach (var series in playerCollections)
            {
                if (!series.IsComplete) continue;

                foreach (var bonus in series.Bonuses)
                {
                    if (!bonusMap.TryGetValue(bonus.AttrType, out var summary))
                    {
                        summary = new CollectionBonusSummaryDto { AttrType = bonus.AttrType };
                        bonusMap[bonus.AttrType] = summary;
                    }

                    if (bonus.ValueType == 0)
                        summary.FixedValue += bonus.AttrValue;
                    else
                        summary.PercentValue += bonus.AttrValue;
                }
            }

            return bonusMap.Values.ToList();
        }

        public async Task InvalidatePlayerBonusCacheAsync(string playerId)
        {
            await _cache.RemoveAsync($"{BonusCacheKeyPrefix}{playerId}");
        }

        public async Task InvalidateConfigCacheAsync()
        {
            await _cache.RemoveAsync(ConfigCacheKey);
            await _cache.RemoveByPrefixAsync(BonusCacheKeyPrefix);
        }

        public async Task ApplyCollectionBonusesToEntityAsync(string playerId, UserEntity player)
        {
            var bonuses = await GetPlayerBonusesAsync(playerId);
            if (bonuses.Count == 0) return;

            foreach (var bonus in bonuses)
            {
                if (bonus.FixedValue != 0)
                    ApplyFixedBonus(player, bonus.AttrType, bonus.FixedValue);
                if (bonus.PercentValue != 0)
                    ApplyPercentBonus(player, bonus.AttrType, bonus.PercentValue);
            }
        }

        public async Task GrantRandomCollectionItemAsync(string playerId, string seriesId, int collectionType)
        {
            List<string> itemIds;
            if (collectionType == 0)
            {
                itemIds = await _db.Queryable<TextCollectionItemEntity>()
                    .Where(x => x.SeriesId == seriesId && x.IsEnabled)
                    .Select(x => x.ItemId)
                    .ToListAsync();
            }
            else
            {
                itemIds = await _db.Queryable<ImageCollectionItemEntity>()
                    .Where(x => x.SeriesId == seriesId && x.IsEnabled)
                    .Select(x => x.ItemId)
                    .ToListAsync();
            }

            if (itemIds.Count == 0) return;

            var selectedItemId = itemIds[Random.Shared.Next(itemIds.Count)];

            var existing = await _db.Queryable<PlayerCollectionEntity>()
                .Where(x => x.PlayerId == playerId && x.CollectionType == collectionType
                    && x.SeriesId == seriesId && x.ItemId == selectedItemId)
                .FirstAsync();

            if (existing != null)
            {
                existing.OwnedCount += 1;
                existing.LastGetTime = DateTime.Now;
                await _db.Updateable(existing)
                    .UpdateColumns(x => new { x.OwnedCount, x.LastGetTime })
                    .ExecuteCommandAsync();
            }
            else
            {
                await _db.Insertable(new PlayerCollectionEntity
                {
                    PlayerId = playerId,
                    CollectionType = collectionType,
                    SeriesId = seriesId,
                    ItemId = selectedItemId,
                    OwnedCount = 1,
                    FirstGetTime = DateTime.Now,
                    LastGetTime = DateTime.Now
                }).ExecuteCommandAsync();
            }

            await InvalidatePlayerBonusCacheAsync(playerId);
        }

        private static void ApplyFixedBonus(UserEntity player, string attrType, float value)
        {
            switch (attrType)
            {
                case "Type1": player.Type1 += (int)Math.Round(value); break;
                case "Type2": player.Type2 += (int)Math.Round(value); break;
                case "Type3": player.Type3 += (int)Math.Round(value); break;
                case "Type4": player.Type4 += (int)Math.Round(value); break;
                case "Type5": player.Type5 += (int)Math.Round(value); break;
                case "Type6": player.Type6 += (int)Math.Round(value); break;
                case "Type7": player.Type7 += (int)Math.Round(value); break;
                case "Type8": player.Type8 += value; break;
                case "Type9": player.Type9 += value; break;
                case "Type10": player.Type10 += value; break;
                case "Type11": player.Type11 += value; break;
                case "Type12": player.Type12 += value; break;
                case "Type13": player.Type13 += value; break;
                case "Type14": player.Type14 += value; break;
                case "Type15": player.Type15 += value; break;
            }
        }

        private static void ApplyPercentBonus(UserEntity player, string attrType, float value)
        {
            switch (attrType)
            {
                case "Type1": player.Type1 = (int)Math.Round(player.Type1 * (1 + value / 100f)); break;
                case "Type2": player.Type2 = (int)Math.Round(player.Type2 * (1 + value / 100f)); break;
                case "Type3": player.Type3 = (int)Math.Round(player.Type3 * (1 + value / 100f)); break;
                case "Type4": player.Type4 = (int)Math.Round(player.Type4 * (1 + value / 100f)); break;
                case "Type5": player.Type5 = (int)Math.Round(player.Type5 * (1 + value / 100f)); break;
                case "Type6": player.Type6 = (int)Math.Round(player.Type6 * (1 + value / 100f)); break;
                case "Type7": player.Type7 = (int)Math.Round(player.Type7 * (1 + value / 100f)); break;
                case "Type8": player.Type8 += value / 100f; break;
                case "Type9": player.Type9 += value / 100f; break;
                case "Type10": player.Type10 += value / 100f; break;
                case "Type11": player.Type11 += value / 100f; break;
                case "Type12": player.Type12 += value / 100f; break;
                case "Type13": player.Type13 += value / 100f; break;
                case "Type14": player.Type14 += value / 100f; break;
                case "Type15": player.Type15 += value / 100f; break;
            }
        }
    }
}
