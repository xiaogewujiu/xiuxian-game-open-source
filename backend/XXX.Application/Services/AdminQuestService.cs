#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminQuestService : IAdminQuestService
    {
        private readonly IRepository<QuestConfigEntity> _questRepository;
        private readonly IRepository<QuestProgressEntity> _questProgressRepository;
        private readonly IRepository<QuestCompletedRecordEntity> _questCompletedRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminQuestService(
            IRepository<QuestConfigEntity> questRepository,
            IRepository<QuestProgressEntity> questProgressRepository,
            IRepository<QuestCompletedRecordEntity> questCompletedRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _questRepository = questRepository;
            _questProgressRepository = questProgressRepository;
            _questCompletedRepository = questCompletedRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminQuestListItemDto>> GetListAsync(string? keyword = null, int? questType = null, int? resetCycle = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _questRepository.Db.Queryable<QuestConfigEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(quest => quest.QuestId.Contains(normalizedKeyword) || quest.QuestName.Contains(normalizedKeyword));
            }

            query = query.WhereIF(questType.HasValue, quest => quest.QuestType == questType!.Value);
            query = query.WhereIF(resetCycle.HasValue, quest => quest.ResetCycle == resetCycle!.Value);

            var quests = await query.OrderBy(quest => quest.SortOrder).ToListAsync();
            return quests.Select(quest => new AdminQuestListItemDto
            {
                QuestId = quest.QuestId,
                QuestName = quest.QuestName,
                QuestType = quest.QuestType,
                ResetCycle = quest.ResetCycle,
                RequiredLevel = quest.RequiredLevel,
                IsEnabled = quest.IsEnabled,
                IsBuiltIn = quest.IsBuiltIn,
                BuiltInVersion = quest.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminQuestDetailDto?> GetDetailAsync(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId)) return null;
            var quest = await _questRepository.GetByIdAsync(questId.Trim());
            if (quest == null) return null;

            var guildContribution = 0;
            if (!string.IsNullOrEmpty(quest.RewardItemsJson))
            {
                try
                {
                    var items = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(quest.RewardItemsJson);
                    if (items != null && items.TryGetValue("contribution", out var c))
                        guildContribution = c;
                }
                catch { }
            }

            return new AdminQuestDetailDto
            {
                QuestId = quest.QuestId,
                QuestName = quest.QuestName,
                QuestType = quest.QuestType,
                ResetCycle = quest.ResetCycle,
                Description = quest.Description,
                RequiredLevel = quest.RequiredLevel,
                PreQuestIds = quest.PreQuestIds,
                AutoAccept = quest.AutoAccept,
                AutoSubmit = quest.AutoSubmit,
                TimeLimit = quest.TimeLimit,
                RewardExp = quest.RewardExp,
                RewardGold = quest.RewardGold,
                RewardSpiritStone = quest.RewardSpiritStone,
                RewardGuildContribution = guildContribution,
                RewardItemsJson = quest.RewardItemsJson,
                RewardEquipmentIds = quest.RewardEquipmentIds,
                ObjectivesJson = quest.ObjectivesJson,
                SortOrder = quest.SortOrder,
                IsEnabled = quest.IsEnabled,
                IsBuiltIn = quest.IsBuiltIn,
                SeedKey = quest.SeedKey,
                BuiltInVersion = quest.BuiltInVersion,
                LastUpdateTime = quest.LastUpdateTime
            };
        }

        public async Task<AdminQuestDetailDto> SaveAsync(AdminQuestDetailDto request)
        {
            var questId = (request.QuestId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(questId)) throw new InvalidOperationException("任务编号不能为空。");
            if (string.IsNullOrWhiteSpace(request.QuestName)) throw new InvalidOperationException("任务名称不能为空。");

            var existing = await _questRepository.GetByIdAsync(questId);
            if (existing == null)
            {
                existing = new QuestConfigEntity { QuestId = questId };
                await _questRepository.AddAsync(ApplyQuest(existing, request));
                await _runtimeRefreshService.ReloadQuestCacheAsync();
                return request;
            }

            ApplyQuest(existing, request);
            await _questRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadQuestCacheAsync();
            return request;
        }

        public async Task<bool> DeleteAsync(string questId)
        {
            var normalizedQuestId = (questId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedQuestId)) return false;

            var allQuests = await _questRepository.Db.Queryable<QuestConfigEntity>().ToListAsync();
            var referencedQuest = allQuests.FirstOrDefault(quest => !string.Equals(quest.QuestId, normalizedQuestId, StringComparison.OrdinalIgnoreCase) && (quest.PreQuestIds ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).Any(item => string.Equals(item, normalizedQuestId, StringComparison.OrdinalIgnoreCase)));
            if (referencedQuest != null) throw new InvalidOperationException($"当前任务仍被任务 {referencedQuest.QuestId} 作为前置任务引用，不能直接删除。");

            if (await _questProgressRepository.Db.Queryable<QuestProgressEntity>().Where(progress => progress.QuestId == normalizedQuestId).AnyAsync())
            {
                throw new InvalidOperationException("当前任务仍被玩家任务进度引用，不能直接删除。");
            }

            if (await _questCompletedRepository.Db.Queryable<QuestCompletedRecordEntity>().Where(record => record.QuestId == normalizedQuestId).AnyAsync())
            {
                throw new InvalidOperationException("当前任务仍被任务完成记录引用，不能直接删除。");
            }

            var deleteRows = await _questRepository.DeleteAsync(normalizedQuestId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadQuestCacheAsync();
            }

            return deleteRows > 0;
        }

        private static QuestConfigEntity ApplyQuest(QuestConfigEntity entity, AdminQuestDetailDto request)
        {
            entity.QuestName = request.QuestName.Trim();
            entity.QuestType = request.QuestType;
            entity.ResetCycle = Math.Max(0, request.ResetCycle);
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.RequiredLevel = Math.Max(1, request.RequiredLevel);
            entity.PreQuestIds = string.IsNullOrWhiteSpace(request.PreQuestIds) ? null : request.PreQuestIds.Trim();
            entity.AutoAccept = request.AutoAccept;
            entity.AutoSubmit = request.AutoSubmit;
            entity.TimeLimit = Math.Max(0, request.TimeLimit);
            entity.RewardExp = Math.Max(0, request.RewardExp);
            entity.RewardGold = Math.Max(0, request.RewardGold);
            entity.RewardSpiritStone = Math.Max(0L, request.RewardSpiritStone);

            // 合并贡献奖励到 RewardItemsJson
            var rewardItems = new Dictionary<string, int>();
            if (!string.IsNullOrWhiteSpace(request.RewardItemsJson))
            {
                try { rewardItems = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(request.RewardItemsJson) ?? new(); } catch { }
            }
            if (request.RewardGuildContribution > 0)
                rewardItems["contribution"] = request.RewardGuildContribution;
            else
                rewardItems.Remove("contribution");
            entity.RewardItemsJson = rewardItems.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(rewardItems) : null;

            entity.RewardEquipmentIds = string.IsNullOrWhiteSpace(request.RewardEquipmentIds) ? null : request.RewardEquipmentIds.Trim();
            entity.ObjectivesJson = string.IsNullOrWhiteSpace(request.ObjectivesJson) ? null : request.ObjectivesJson.Trim();
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
