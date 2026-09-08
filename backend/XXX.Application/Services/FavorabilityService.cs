using Microsoft.Extensions.Configuration;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Favorability;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 好感度服务实现。
    /// </summary>
    public class FavorabilityService : IFavorabilityService
    {
        private readonly ISqlSugarClient _db;
        private readonly DbContext _dbContext;
        private readonly int _partyBattleValue;

        public FavorabilityService(DbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _db = dbContext.Db;
            _partyBattleValue = configuration.GetValue<int>("Favorability:PartyBattleValue", 10);
        }

        public async Task<GiftFavorabilityResponse> GiftItemAsync(string playerId, GiftFavorabilityRequest request)
        {
            // 1. 获取道具模板
            var itemTemplate = await _db.Queryable<ItemTemplateEntity>()
                .Where(x => x.ItemId == request.ItemId)
                .FirstAsync();

            // 2. 获取背包中该道具数量
            var inventoryItem = await _db.Queryable<InventoryItemEntity>()
                .Where(x => x.PlayerId == playerId && x.ItemId == request.ItemId)
                .FirstAsync();
            int inventoryCount = inventoryItem?.Quantity ?? 0;

            // 3. 获取当日赠送记录
            // 好感度是我(A)给你(B)送礼物，增加的是你(B)对我的(A)好感度
            // 所以查询的是 PlayerId=目标方(B), TargetPlayerId=赠送方(A)
            var fav = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => x.PlayerId == request.TargetPlayerId && x.TargetPlayerId == playerId && !x.IsDeleted)
                .FirstAsync();

            var todayGifts = FavorabilityManager.ParseTodayGifts(fav?.TodayGiftJson);

            // 4. 校验
            var (valid, error, config) = FavorabilityManager.ValidateGift(
                itemTemplate != null ? MapToItemTable(itemTemplate) : null,
                inventoryCount,
                todayGifts,
                request.ItemId);

            if (!valid || config == null)
            {
                return new GiftFavorabilityResponse { Success = false, Message = error };
            }

            // 5. 事务操作
            return await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                // 扣除道具
                if (inventoryItem!.Quantity <= 1)
                {
                    await _db.Deleteable(inventoryItem).ExecuteCommandAsync();
                }
                else
                {
                    inventoryItem.Quantity -= 1;
                    await _db.Updateable(inventoryItem)
                        .UpdateColumns(x => new { x.Quantity })
                        .ExecuteCommandAsync();
                }

                // 更新好感度
                // 好感度记录：PlayerId=目标方(B), TargetPlayerId=赠送方(A)
                // 表示 B 对 A 的好感度
                if (fav == null)
                {
                    fav = new PlayerFavorabilityEntity
                    {
                        PlayerId = request.TargetPlayerId,
                        TargetPlayerId = playerId,
                        Value = config.FavorabilityValue,
                        TodayGiftJson = FavorabilityManager.UpdateTodayGiftJson(null, request.ItemId),
                        LastGiftTime = DateTime.Now
                    };
                    await _db.Insertable(fav).ExecuteCommandAsync();
                }
                else
                {
                    fav.Value += config.FavorabilityValue;
                    fav.TodayGiftJson = FavorabilityManager.UpdateTodayGiftJson(fav.TodayGiftJson, request.ItemId);
                    fav.LastGiftTime = DateTime.Now;
                    await _db.Updateable(fav)
                        .UpdateColumns(x => new { x.Value, x.TodayGiftJson, x.LastGiftTime })
                        .ExecuteCommandAsync();
                }

                // 写入赠送日志
                // 日志记录：PlayerId=赠送方(A), TargetPlayerId=目标方(B)
                // 表示 A 送给 B 礼物
                var log = new FavorabilityGiftLogEntity
                {
                    PlayerId = playerId,
                    TargetPlayerId = request.TargetPlayerId,
                    ItemId = request.ItemId,
                    ItemName = itemTemplate!.Name,
                    FavorabilityChange = config.FavorabilityValue,
                    GiftTime = DateTime.Now
                };
                await _db.Insertable(log).ExecuteCommandAsync();

                // 获取等级信息
                var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                    .OrderBy(x => x.SortOrder)
                    .ToListAsync();
                var levelInfo = FavorabilityManager.GetLevelInfo(fav.Value, levels);

                string changeText = config.FavorabilityValue >= 0
                    ? $"+{config.FavorabilityValue}"
                    : config.FavorabilityValue.ToString();

                return new GiftFavorabilityResponse
                {
                    Success = true,
                    NewValue = fav.Value,
                    LevelName = levelInfo.Name,
                    LevelColor = levelInfo.Color,
                    Message = $"赠送成功，好感度{changeText}"
                };
            });
        }

        public async Task<List<FavorabilityListItemDto>> GetFavorabilityListAsync(string playerId, string direction)
        {
            List<PlayerFavorabilityEntity> relations;

            if (direction == "FromOthers")
            {
                relations = await _db.Queryable<PlayerFavorabilityEntity>()
                    .Where(x => x.TargetPlayerId == playerId && !x.IsDeleted)
                    .OrderByDescending(x => x.Value)
                    .ToListAsync();
            }
            else
            {
                relations = await _db.Queryable<PlayerFavorabilityEntity>()
                    .Where(x => x.PlayerId == playerId && !x.IsDeleted)
                    .OrderByDescending(x => x.Value)
                    .ToListAsync();
            }

            if (relations.Count == 0) return [];

            // 获取对方玩家信息
            var otherPlayerIds = direction == "FromOthers"
                ? relations.Select(x => x.PlayerId).Distinct().ToList()
                : relations.Select(x => x.TargetPlayerId).Distinct().ToList();

            var players = await _db.Queryable<UserEntity>()
                .Where(x => otherPlayerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name, x.AvatarImagePath })
                .ToListAsync();

            var playerDict = players.ToDictionary(x => x.GID, x => x);

            // 获取等级配置
            var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            // 获取最近赠送记录
            List<FavorabilityGiftLogEntity> allRecentLogs;
            if (direction == "FromOthers")
            {
                allRecentLogs = await _db.Queryable<FavorabilityGiftLogEntity>()
                    .Where(x => !x.IsDeleted && otherPlayerIds.Contains(x.PlayerId) && x.TargetPlayerId == playerId)
                    .OrderByDescending(x => x.GiftTime)
                    .Take(otherPlayerIds.Count * 5)
                    .ToListAsync();
            }
            else
            {
                allRecentLogs = await _db.Queryable<FavorabilityGiftLogEntity>()
                    .Where(x => !x.IsDeleted && x.PlayerId == playerId && otherPlayerIds.Contains(x.TargetPlayerId))
                    .OrderByDescending(x => x.GiftTime)
                    .Take(otherPlayerIds.Count * 5)
                    .ToListAsync();
            }

            var result = new List<FavorabilityListItemDto>();

            foreach (var rel in relations)
            {
                var otherId = direction == "FromOthers" ? rel.PlayerId : rel.TargetPlayerId;
                playerDict.TryGetValue(otherId, out var player);

                var levelInfo = FavorabilityManager.GetLevelInfo(rel.Value, levels);

                var recentGifts = allRecentLogs
                    .Where(x => direction == "FromOthers"
                        ? x.PlayerId == otherId && x.TargetPlayerId == playerId
                        : x.PlayerId == playerId && x.TargetPlayerId == otherId)
                    .OrderByDescending(x => x.GiftTime)
                    .Take(5)
                    .Select(x => new FavorabilityGiftRecordDto
                    {
                        ItemName = x.ItemName,
                        Change = x.FavorabilityChange,
                        Time = x.GiftTime
                    })
                    .ToList();

                result.Add(new FavorabilityListItemDto
                {
                    PlayerId = otherId,
                    PlayerName = player?.Name ?? "未知玩家",
                    AvatarPath = player?.AvatarImagePath,
                    Value = rel.Value,
                    LevelName = levelInfo.Name,
                    LevelColor = levelInfo.Color,
                    LastGiftTime = rel.LastGiftTime,
                    RecentGifts = recentGifts
                });
            }

            return result;
        }

        public async Task<List<FavorabilityGiftRecordDto>> GetGiftLogAsync(string playerId, FavorabilityLogQueryRequest request)
        {
            var query = _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => !x.IsDeleted && (x.PlayerId == playerId || x.TargetPlayerId == playerId));

            if (!string.IsNullOrEmpty(request.TargetPlayerId))
            {
                query = query.Where(x =>
                    (x.PlayerId == playerId && x.TargetPlayerId == request.TargetPlayerId) ||
                    (x.PlayerId == request.TargetPlayerId && x.TargetPlayerId == playerId));
            }

            var logs = await query
                .OrderByDescending(x => x.GiftTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            if (logs.Count == 0) return [];

            var playerIds = logs.SelectMany(x => new[] { x.PlayerId, x.TargetPlayerId }).Distinct().ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => playerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x.Name);

            return logs.Select(x => new FavorabilityGiftRecordDto
            {
                ItemName = x.ItemName,
                Change = x.FavorabilityChange,
                Time = x.GiftTime,
                FromPlayerName = playerDict.GetValueOrDefault(x.PlayerId, "未知"),
                ToPlayerName = playerDict.GetValueOrDefault(x.TargetPlayerId, "未知")
            }).ToList();
        }

        public async Task<List<FavorabilityLevelDto>> GetLevelsAsync()
        {
            var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return levels.Select(MapToLevelDto).ToList();
        }

        public async Task ResetDailyGiftLimitsAsync()
        {
            await _db.Updateable<PlayerFavorabilityEntity>()
                .SetColumns(x => x.TodayGiftJson == null)
                .Where(x => x.TodayGiftJson != null && !x.IsDeleted)
                .ExecuteCommandAsync();

            await _db.Updateable<PlayerFavorabilityEntity>()
                .SetColumns(x => x.TodayPartyBattleJson == null)
                .Where(x => x.TodayPartyBattleJson != null && !x.IsDeleted)
                .ExecuteCommandAsync();
        }

        public async Task AwardPartyBattleFavorabilityAsync(IList<string> playerIds)
        {
            if (playerIds.Count < 2) return;

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // 生成所有不重复的玩家对
            var pairs = new List<(string A, string B)>();
            for (var i = 0; i < playerIds.Count; i++)
            {
                for (var j = i + 1; j < playerIds.Count; j++)
                {
                    pairs.Add((playerIds[i], playerIds[j]));
                }
            }

            // 查询涉及的所有好感度记录
            var allIds = playerIds.Distinct().ToList();
            var existingRecords = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted && allIds.Contains(x.PlayerId) && allIds.Contains(x.TargetPlayerId))
                .ToListAsync();

            var recordDict = existingRecords.ToDictionary(x => $"{x.PlayerId}|{x.TargetPlayerId}", x => x);

            var logs = new List<FavorabilityGiftLogEntity>();

            foreach (var (a, b) in pairs)
            {
                await ProcessPairAsync(a, b, today, recordDict, logs);
                await ProcessPairAsync(b, a, today, recordDict, logs);
            }

            if (logs.Count > 0)
            {
                await _db.Insertable(logs).ExecuteCommandAsync();
            }
        }

        private async Task ProcessPairAsync(
            string playerId, string targetPlayerId, string today,
            Dictionary<string, PlayerFavorabilityEntity> recordDict,
            List<FavorabilityGiftLogEntity> logs)
        {
            var key = $"{playerId}|{targetPlayerId}";
            recordDict.TryGetValue(key, out var record);

            if (record == null)
            {
                // 创建新记录
                record = new PlayerFavorabilityEntity
                {
                    PlayerId = playerId,
                    TargetPlayerId = targetPlayerId,
                    Value = _partyBattleValue,
                    TodayPartyBattleJson = $"{{\"{today}\":true}}",
                    LastGiftTime = DateTime.UtcNow
                };
                await _db.Insertable(record).ExecuteCommandAsync();
                recordDict[key] = record;

                logs.Add(new FavorabilityGiftLogEntity
                {
                    PlayerId = playerId,
                    TargetPlayerId = targetPlayerId,
                    ItemName = "组队战斗",
                    FavorabilityChange = _partyBattleValue,
                    GiftTime = DateTime.UtcNow
                });
                return;
            }

            // 检查今日是否已通过组队战斗增长过
            var todayPartyBattle = ParseTodayPartyBattle(record.TodayPartyBattleJson);
            if (todayPartyBattle.ContainsKey(today)) return;

            // 更新好感度
            record.Value += _partyBattleValue;
            record.TodayPartyBattleJson = UpdateTodayPartyBattleJson(record.TodayPartyBattleJson, today);
            record.LastGiftTime = DateTime.UtcNow;

            await _db.Updateable(record)
                .UpdateColumns(x => new { x.Value, x.TodayPartyBattleJson, x.LastGiftTime })
                .ExecuteCommandAsync();

            logs.Add(new FavorabilityGiftLogEntity
            {
                PlayerId = playerId,
                TargetPlayerId = targetPlayerId,
                ItemName = "组队战斗",
                FavorabilityChange = _partyBattleValue,
                GiftTime = DateTime.UtcNow
            });
        }

        private static Dictionary<string, bool> ParseTodayPartyBattle(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, bool>();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(json)
                       ?? new Dictionary<string, bool>();
            }
            catch
            {
                return new Dictionary<string, bool>();
            }
        }

        private static string UpdateTodayPartyBattleJson(string? existing, string today)
        {
            var dict = ParseTodayPartyBattle(existing);
            dict[today] = true;
            return System.Text.Json.JsonSerializer.Serialize(dict);
        }

        // ==================== Admin ====================

        public async Task<List<AdminFavorabilityRelationDto>> AdminGetRelationsAsync(string? keyword, int pageIndex, int pageSize)
        {
            var query = _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var matchingPlayerIds = await _db.Queryable<UserEntity>()
                    .Where(x => x.Name.Contains(keyword))
                    .Select(x => x.GID)
                    .ToListAsync();

                query = query.Where(x =>
                    matchingPlayerIds.Contains(x.PlayerId) ||
                    matchingPlayerIds.Contains(x.TargetPlayerId) ||
                    x.PlayerId.Contains(keyword) ||
                    x.TargetPlayerId.Contains(keyword));
            }

            var relations = await query
                .OrderByDescending(x => x.Value)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (relations.Count == 0) return [];

            var playerIds = relations.SelectMany(x => new[] { x.PlayerId, x.TargetPlayerId }).Distinct().ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => playerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x.Name);

            var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return relations.Select(x =>
            {
                var levelInfo = FavorabilityManager.GetLevelInfo(x.Value, levels);
                return new AdminFavorabilityRelationDto
                {
                    Id = x.Id,
                    PlayerId = x.PlayerId,
                    PlayerName = playerDict.GetValueOrDefault(x.PlayerId, "未知"),
                    TargetPlayerId = x.TargetPlayerId,
                    TargetPlayerName = playerDict.GetValueOrDefault(x.TargetPlayerId, "未知"),
                    Value = x.Value,
                    LevelName = levelInfo.Name,
                    LastGiftTime = x.LastGiftTime,
                    CreatedTime = x.CreatedAt
                };
            }).ToList();
        }

        public async Task<int> AdminGetRelationsCountAsync(string? keyword)
        {
            var query = _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var matchingPlayerIds = await _db.Queryable<UserEntity>()
                    .Where(x => x.Name.Contains(keyword))
                    .Select(x => x.GID)
                    .ToListAsync();

                query = query.Where(x =>
                    matchingPlayerIds.Contains(x.PlayerId) ||
                    matchingPlayerIds.Contains(x.TargetPlayerId) ||
                    x.PlayerId.Contains(keyword) ||
                    x.TargetPlayerId.Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<bool> AdminUpdateValueAsync(long id, int newValue)
        {
            return await _db.Updateable<PlayerFavorabilityEntity>()
                .SetColumns(x => x.Value == newValue)
                .Where(x => x.Id == id && !x.IsDeleted)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<bool> AdminDeleteRelationAsync(long id)
        {
            return await _db.Updateable<PlayerFavorabilityEntity>()
                .SetColumns(x => x.IsDeleted == true)
                .Where(x => x.Id == id)
                .ExecuteCommandAsync() > 0;
        }

        public async Task<List<AdminFavorabilityGiftLogDto>> AdminGetGiftLogAsync(string? playerId, string? targetPlayerId, int pageIndex, int pageSize)
        {
            var query = _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(playerId))
                query = query.Where(x => x.PlayerId == playerId);
            if (!string.IsNullOrWhiteSpace(targetPlayerId))
                query = query.Where(x => x.TargetPlayerId == targetPlayerId);

            var logs = await query
                .OrderByDescending(x => x.GiftTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (logs.Count == 0) return [];

            var pids = logs.SelectMany(x => new[] { x.PlayerId, x.TargetPlayerId }).Distinct().ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => pids.Contains(x.GID))
                .Select(x => new { x.GID, x.Name })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x.Name);

            return logs.Select(x => new AdminFavorabilityGiftLogDto
            {
                Id = x.Id,
                PlayerId = x.PlayerId,
                PlayerName = playerDict.GetValueOrDefault(x.PlayerId, "未知"),
                TargetPlayerId = x.TargetPlayerId,
                TargetPlayerName = playerDict.GetValueOrDefault(x.TargetPlayerId, "未知"),
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                FavorabilityChange = x.FavorabilityChange,
                GiftTime = x.GiftTime
            }).ToList();
        }

        public async Task<int> AdminGetGiftLogCountAsync(string? playerId, string? targetPlayerId)
        {
            var query = _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(playerId))
                query = query.Where(x => x.PlayerId == playerId);
            if (!string.IsNullOrWhiteSpace(targetPlayerId))
                query = query.Where(x => x.TargetPlayerId == targetPlayerId);

            return await query.CountAsync();
        }

        public async Task<AdminFavorabilityStatsDto> AdminGetStatsAsync()
        {
            var totalRelations = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted)
                .CountAsync();
            var totalGifts = await _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => !x.IsDeleted)
                .CountAsync();

            var mostGifted = await _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.TargetPlayerId)
                .Select(x => new { PlayerId = x.TargetPlayerId, GiftCount = SqlFunc.AggregateCount(x.Id) })
                .OrderByDescending(x => x.GiftCount)
                .Take(10)
                .ToListAsync();

            var playerIds = mostGifted.Select(x => x.PlayerId).ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => playerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x.Name);

            return new AdminFavorabilityStatsDto
            {
                TotalRelations = totalRelations,
                TotalGifts = totalGifts,
                MostGiftedPlayers = mostGifted.Select(x => new AdminPopularPlayerDto
                {
                    PlayerId = x.PlayerId,
                    PlayerName = playerDict.GetValueOrDefault(x.PlayerId, "未知"),
                    GiftCount = x.GiftCount
                }).ToList()
            };
        }

        public async Task<List<FavorabilityLevelDto>> AdminGetLevelsAsync()
        {
            return await GetLevelsAsync();
        }

        public async Task<FavorabilityLevelDto?> AdminSaveLevelAsync(FavorabilityLevelDto dto)
        {
            if (dto.Id > 0)
            {
                var entity = new FavorabilityLevelConfigEntity
                {
                    Id = dto.Id,
                    Level = dto.Level,
                    Name = dto.Name,
                    MinValue = dto.MinValue,
                    MaxValue = dto.MaxValue,
                    Color = string.IsNullOrWhiteSpace(dto.Color) ? "#CCCCCC" : dto.Color,
                    SortOrder = dto.SortOrder,
                    RewardJson = dto.RewardJson,
                    UpdatedAt = DateTime.Now
                };
                await _db.Updateable(entity).ExecuteCommandAsync();
                return MapToLevelDto(entity);
            }
            else
            {
                var entity = new FavorabilityLevelConfigEntity
                {
                    Level = dto.Level,
                    Name = dto.Name,
                    MinValue = dto.MinValue,
                    MaxValue = dto.MaxValue,
                    Color = string.IsNullOrWhiteSpace(dto.Color) ? "#CCCCCC" : dto.Color,
                    SortOrder = dto.SortOrder,
                    RewardJson = dto.RewardJson
                };
                entity.Id = await _db.Insertable(entity).ExecuteReturnIdentityAsync();
                return MapToLevelDto(entity);
            }
        }

        public async Task<bool> AdminDeleteLevelAsync(int id)
        {
            return await _db.Deleteable<FavorabilityLevelConfigEntity>()
                .Where(x => x.Id == id)
                .ExecuteCommandAsync() > 0;
        }

        // ==================== Leaderboard ====================

        public async Task<List<FavorabilityLeaderboardDto>> GetLeaderboardAsync(int count = 20)
        {
            // 按玩家收到的总好感度排行
            var grouped = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.PlayerId)
                .Select(x => new { PlayerId = x.PlayerId, TotalFavorability = SqlFunc.AggregateSum(x.Value), RelationCount = SqlFunc.AggregateCount(x.Id) })
                .OrderByDescending(x => x.TotalFavorability)
                .Take(count)
                .ToListAsync();

            if (grouped.Count == 0) return [];

            var playerIds = grouped.Select(x => x.PlayerId).ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => playerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name, x.AvatarImagePath })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x);

            return grouped.Select((x, i) =>
            {
                playerDict.TryGetValue(x.PlayerId, out var p);
                return new FavorabilityLeaderboardDto
                {
                    Rank = i + 1,
                    PlayerId = x.PlayerId,
                    PlayerName = p?.Name ?? "未知",
                    AvatarPath = p?.AvatarImagePath,
                    TotalFavorability = x.TotalFavorability,
                    RelationCount = x.RelationCount
                };
            }).ToList();
        }

        public async Task<List<DeepFriendshipDto>> GetDeepFriendshipLeaderboardAsync(int count = 10)
        {
            // 查找双向好感度之和最高的玩家对
            var allRelations = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => !x.IsDeleted)
                .Select(x => new { x.PlayerId, x.TargetPlayerId, x.Value })
                .ToListAsync();

            // 合并双向：(A,B) 和 (B,A) 的值相加
            var pairMap = new Dictionary<string, (string A, string B, int Combined)>();
            foreach (var r in allRelations)
            {
                var key = string.Compare(r.PlayerId, r.TargetPlayerId) < 0
                    ? $"{r.PlayerId}|{r.TargetPlayerId}"
                    : $"{r.TargetPlayerId}|{r.PlayerId}";
                var a = string.Compare(r.PlayerId, r.TargetPlayerId) < 0 ? r.PlayerId : r.TargetPlayerId;
                var b = string.Compare(r.PlayerId, r.TargetPlayerId) < 0 ? r.TargetPlayerId : r.PlayerId;
                if (pairMap.ContainsKey(key))
                {
                    var existing = pairMap[key];
                    pairMap[key] = (existing.A, existing.B, existing.Combined + r.Value);
                }
                else
                {
                    pairMap[key] = (a, b, r.Value);
                }
            }

            var topPairs = pairMap.Values
                .OrderByDescending(x => x.Combined)
                .Take(count)
                .ToList();

            if (topPairs.Count == 0) return [];

            var playerIds = topPairs.SelectMany(x => new[] { x.A, x.B }).Distinct().ToList();
            var players = await _db.Queryable<UserEntity>()
                .Where(x => playerIds.Contains(x.GID))
                .Select(x => new { x.GID, x.Name })
                .ToListAsync();
            var playerDict = players.ToDictionary(x => x.GID, x => x.Name);

            var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return topPairs.Select((x, i) =>
            {
                // 用双向平均值查等级
                var avgValue = x.Combined / 2;
                var levelInfo = FavorabilityManager.GetLevelInfo(avgValue, levels);
                return new DeepFriendshipDto
                {
                    Rank = i + 1,
                    Player1Id = x.A,
                    Player1Name = playerDict.GetValueOrDefault(x.A, "未知"),
                    Player2Id = x.B,
                    Player2Name = playerDict.GetValueOrDefault(x.B, "未知"),
                    CombinedValue = x.Combined,
                    LevelName = levelInfo.Name,
                    LevelColor = levelInfo.Color
                };
            }).ToList();
        }

        // ==================== Achievements ====================

        private static readonly List<FavorabilityAchievementDef> AchievementDefs =
        [
            new("fav_gift_10", "初识赠礼", "累计赠送10次好感度道具", 10, "gift_sent", """[{"type":"gold","count":500}]"""),
            new("fav_gift_50", "赠礼达人", "累计赠送50次好感度道具", 50, "gift_sent", """[{"type":"gold","count":2000}]"""),
            new("fav_gift_100", "赠礼大师", "累计赠送100次好感度道具", 100, "gift_sent", """[{"type":"spirit_stone","count":50}]"""),
            new("fav_friend_5", "广结善缘", "与5个玩家建立好感度关系", 5, "unique_friends", """[{"type":"gold","count":1000}]"""),
            new("fav_friend_20", "八方来友", "与20个玩家建立好感度关系", 20, "unique_friends", """[{"type":"spirit_stone","count":100}]"""),
            new("fav_max_1", "知己一人", "与1个玩家达到最高等级好感度", 1, "max_level", """[{"type":"spirit_stone","count":200}]"""),
            new("fav_max_5", "众星捧月", "与5个玩家达到最高等级好感度", 5, "max_level", """[{"type":"title","count":1,"title":"众星捧月"}]"""),
            new("fav_receive_50", "受人爱戴", "累计被赠送50次", 50, "gift_received", """[{"type":"gold","count":2000}]"""),
            new("fav_receive_100", "万人迷", "累计被赠送100次", 100, "gift_received", """[{"type":"spirit_stone","count":100}]"""),
            new("fav_party_10", "战友之谊", "通过组队战斗获得10次好感度", 10, "party_battle", """[{"type":"gold","count":1000}]"""),
        ];

        public async Task<FavorabilityAchievementProgressDto> GetAchievementProgressAsync(string playerId)
        {
            // 统计赠送次数
            var totalGiftSent = await _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => x.PlayerId == playerId && !x.IsDeleted && x.ItemId != null)
                .CountAsync();

            // 统计被赠送次数
            var totalGiftReceived = await _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => x.TargetPlayerId == playerId && !x.IsDeleted && x.ItemId != null)
                .CountAsync();

            // 统计组队战斗好感度次数
            var partyBattleCount = await _db.Queryable<FavorabilityGiftLogEntity>()
                .Where(x => x.PlayerId == playerId && !x.IsDeleted && x.ItemName == "组队战斗")
                .CountAsync();

            // 统计关系数
            var uniqueFriends = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => (x.PlayerId == playerId || x.TargetPlayerId == playerId) && !x.IsDeleted)
                .Select(x => x.PlayerId == playerId ? x.TargetPlayerId : x.PlayerId)
                .Distinct()
                .CountAsync();

            // 获取最大好感度值
            var maxFavorability = await _db.Queryable<PlayerFavorabilityEntity>()
                .Where(x => (x.PlayerId == playerId || x.TargetPlayerId == playerId) && !x.IsDeleted)
                .MaxAsync(x => x.Value);

            // 获取等级配置，计算最高等级
            var levels = await _db.Queryable<FavorabilityLevelConfigEntity>()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
            var maxLevel = levels.OrderByDescending(x => x.MinValue).FirstOrDefault();
            int relationsAtMaxLevel = 0;
            if (maxLevel != null)
            {
                relationsAtMaxLevel = await _db.Queryable<PlayerFavorabilityEntity>()
                    .Where(x => (x.PlayerId == playerId || x.TargetPlayerId == playerId) && !x.IsDeleted && x.Value >= maxLevel.MinValue)
                    .CountAsync();
            }

            // 查询已领取的成就
            var claimedAchievements = await _db.Queryable<FavorabilityAchievementEntity>()
                .Where(x => x.PlayerId == playerId && !x.IsDeleted)
                .Select(x => x.AchievementId)
                .ToListAsync();
            var claimedSet = new HashSet<string>(claimedAchievements);

            var achievements = AchievementDefs.Select(def =>
            {
                int currentValue = def.Category switch
                {
                    "gift_sent" => totalGiftSent,
                    "gift_received" => totalGiftReceived,
                    "unique_friends" => uniqueFriends,
                    "max_level" => relationsAtMaxLevel,
                    "party_battle" => partyBattleCount,
                    _ => 0
                };
                return new FavorabilityAchievementDto
                {
                    AchievementId = def.Id,
                    Name = def.Name,
                    Description = def.Description,
                    TargetValue = def.Target,
                    CurrentValue = Math.Min(currentValue, def.Target),
                    IsCompleted = currentValue >= def.Target,
                    IsClaimed = claimedSet.Contains(def.Id),
                    RewardJson = def.RewardJson
                };
            }).ToList();

            return new FavorabilityAchievementProgressDto
            {
                TotalGiftSent = totalGiftSent,
                TotalGiftReceived = totalGiftReceived,
                UniqueFriends = uniqueFriends,
                MaxFavorability = maxFavorability,
                RelationsAtMaxLevel = relationsAtMaxLevel,
                Achievements = achievements
            };
        }

        public async Task<List<FavorabilityAchievementRewardDto>> ClaimAchievementRewardAsync(string playerId, string achievementId)
        {
            var def = AchievementDefs.FirstOrDefault(x => x.Id == achievementId);
            if (def == null)
                return [new FavorabilityAchievementRewardDto { AchievementId = achievementId, Success = false, Message = "成就不存在" }];

            // 检查是否已领取
            var existing = await _db.Queryable<FavorabilityAchievementEntity>()
                .Where(x => x.PlayerId == playerId && x.AchievementId == achievementId && !x.IsDeleted)
                .FirstAsync();
            if (existing != null)
                return [new FavorabilityAchievementRewardDto { AchievementId = achievementId, Success = false, Message = "奖励已领取" }];

            // 检查是否达成
            var progress = await GetAchievementProgressAsync(playerId);
            var achievement = progress.Achievements.FirstOrDefault(x => x.AchievementId == achievementId);
            if (achievement == null || !achievement.IsCompleted)
                return [new FavorabilityAchievementRewardDto { AchievementId = achievementId, Success = false, Message = "成就未达成" }];

            // 记录领取
            var entity = new FavorabilityAchievementEntity
            {
                PlayerId = playerId,
                AchievementId = achievementId,
                ClaimedAt = DateTime.UtcNow
            };
            await _db.Insertable(entity).ExecuteCommandAsync();

            return [new FavorabilityAchievementRewardDto
            {
                AchievementId = achievementId,
                Success = true,
                Message = "奖励领取成功",
                RewardJson = def.RewardJson
            }];
        }

        private record FavorabilityAchievementDef(string Id, string Name, string Description, int Target, string Category, string RewardJson);

        // ==================== Helpers ====================

        private static ItemTable MapToItemTable(ItemTemplateEntity entity)
        {
            return new ItemTable
            {
                ItemId = entity.ItemId,
                Name = entity.Name,
                UseLevel = entity.UseLevel,
                Type = (ItemType)entity.Type,
                Description = entity.Description,
                MaxStack = entity.MaxStack,
                Quality = entity.Quality,
                IconPath = entity.IconPath,
                FavorabilityGiftConfig = entity.FavorabilityGiftConfig
            };
        }

        private static FavorabilityLevelDto MapToLevelDto(FavorabilityLevelConfigEntity entity)
        {
            return new FavorabilityLevelDto
            {
                Id = entity.Id,
                Level = entity.Level,
                Name = entity.Name,
                MinValue = entity.MinValue,
                MaxValue = entity.MaxValue,
                Color = entity.Color,
                SortOrder = entity.SortOrder,
                Description = null,
                RewardJson = entity.RewardJson,
                IsBuiltIn = false,
                LastUpdateTime = entity.UpdatedAt
            };
        }
    }
}
