using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 后台世界 Boss 管理服务。
    /// </summary>
    public class AdminWorldBossService : IAdminWorldBossService
    {
        /// <summary>
        /// 世界 Boss 模板仓储。
        /// </summary>
        private readonly IRepository<WorldBossTemplateEntity> _templateRepository;

        /// <summary>
        /// 世界 Boss 排期仓储。
        /// </summary>
        private readonly IRepository<WorldBossScheduleEntity> _scheduleRepository;

        /// <summary>
        /// 世界 Boss 玩家侧运行时服务。
        /// 后台运行时查询、手动生成和关闭都会复用它。
        /// </summary>
        private readonly IWorldBossService _worldBossService;

        /// <summary>
        /// 初始化后台世界 Boss 管理服务。
        /// </summary>
        /// <param name="templateRepository">世界 Boss 模板仓储。</param>
        /// <param name="scheduleRepository">世界 Boss 排期仓储。</param>
        /// <param name="worldBossService">世界 Boss 玩家侧运行时服务。</param>
        public AdminWorldBossService(
            IRepository<WorldBossTemplateEntity> templateRepository,
            IRepository<WorldBossScheduleEntity> scheduleRepository,
            IWorldBossService worldBossService)
        {
            _templateRepository = templateRepository;
            _scheduleRepository = scheduleRepository;
            _worldBossService = worldBossService;
        }

        /// <summary>
        /// 获取后台世界 Boss 模板列表。
        /// </summary>
        /// <returns>按排序字段和编号排序后的模板列表。</returns>
        public async Task<List<AdminWorldBossTemplateListItemDto>> GetTemplatesAsync()
        {
            var templates = await _templateRepository.Db.Queryable<WorldBossTemplateEntity>()
                .OrderBy(item => item.SortOrder)
                .OrderBy(item => item.BossId)
                .ToListAsync();

            return templates.Select(item => new AdminWorldBossTemplateListItemDto
            {
                BossId = item.BossId,
                Name = item.Name,
                MonsterTemplateId = item.MonsterTemplateId,
                IsEnabled = item.IsEnabled,
                Weight = item.Weight,
                DurationMinutes = item.DurationMinutes,
                SortOrder = item.SortOrder,
                LastUpdateTime = item.LastUpdateTime
            }).ToList();
        }

        /// <summary>
        /// 获取指定世界 Boss 模板详情。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>模板详情；编号为空或不存在时返回空。</returns>
        public async Task<AdminWorldBossTemplateDetailDto?> GetTemplateAsync(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId))
            {
                return null;
            }

            var entity = await _templateRepository.GetByIdAsync(bossId.Trim());
            return entity == null ? null : MapTemplate(entity);
        }

        /// <summary>
        /// 新增或更新世界 Boss 模板。
        /// </summary>
        /// <param name="request">模板保存请求。</param>
        /// <returns>保存后的模板详情。</returns>
        public async Task<AdminWorldBossTemplateDetailDto> SaveTemplateAsync(AdminWorldBossTemplateDetailDto request)
        {
            var bossId = (request.BossId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(bossId))
            {
                throw new InvalidOperationException("Boss编号不能为空。");
            }

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Boss名称不能为空。");
            }

            var monsterTemplateId = (request.MonsterTemplateId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(monsterTemplateId))
            {
                throw new InvalidOperationException("关联怪物模板不能为空。");
            }

            var entity = await _templateRepository.GetByIdAsync(bossId) ?? new WorldBossTemplateEntity
            {
                BossId = bossId
            };

            entity.Name = name;
            entity.MonsterTemplateId = monsterTemplateId;
            entity.PortraitPath = string.IsNullOrWhiteSpace(request.PortraitPath) ? null : request.PortraitPath.Trim();
            entity.IsEnabled = request.IsEnabled;
            entity.Weight = Math.Max(1, request.Weight);
            entity.DurationMinutes = Math.Max(1, request.DurationMinutes);
            entity.NoticeText = string.IsNullOrWhiteSpace(request.NoticeText) ? null : request.NoticeText.Trim();
            entity.ParticipationMinDamage = Math.Max(0, request.ParticipationMinDamage);
            entity.ParticipationRewardExp = Math.Max(0, request.ParticipationRewardExp);
            entity.ParticipationRewardGold = Math.Max(0, request.ParticipationRewardGold);
            entity.ParticipationRewardSpiritStone = Math.Max(0, request.ParticipationRewardSpiritStone);
            entity.Rank1RewardExp = Math.Max(0, request.Rank1RewardExp);
            entity.Rank1RewardGold = Math.Max(0, request.Rank1RewardGold);
            entity.Rank1RewardSpiritStone = Math.Max(0, request.Rank1RewardSpiritStone);
            entity.Rank2RewardExp = Math.Max(0, request.Rank2RewardExp);
            entity.Rank2RewardGold = Math.Max(0, request.Rank2RewardGold);
            entity.Rank2RewardSpiritStone = Math.Max(0, request.Rank2RewardSpiritStone);
            entity.Rank3RewardExp = Math.Max(0, request.Rank3RewardExp);
            entity.Rank3RewardGold = Math.Max(0, request.Rank3RewardGold);
            entity.Rank3RewardSpiritStone = Math.Max(0, request.Rank3RewardSpiritStone);
            entity.SortOrder = Math.Max(0, request.SortOrder);
            entity.LastUpdateTime = DateTime.Now;

            if (await _templateRepository.GetByIdAsync(bossId) == null)
            {
                await _templateRepository.AddAsync(entity);
            }
            else
            {
                await _templateRepository.UpdateAsync(entity);
            }

            return MapTemplate(entity);
        }

        /// <summary>
        /// 删除指定世界 Boss 模板。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>删除成功返回真。</returns>
        public Task<bool> DeleteTemplateAsync(string bossId)
        {
            if (string.IsNullOrWhiteSpace(bossId))
            {
                return Task.FromResult(false);
            }

            return DeleteInternalAsync(bossId.Trim());
        }

        /// <summary>
        /// 获取世界 Boss 默认排期。
        /// 如果数据库中尚未创建，则补一条默认记录，保证后台页面始终可编辑。
        /// </summary>
        /// <returns>世界 Boss 排期配置。</returns>
        public async Task<AdminWorldBossScheduleDto> GetScheduleAsync()
        {
            var entity = await _scheduleRepository.GetByIdAsync("default");
            if (entity == null)
            {
                entity = new WorldBossScheduleEntity();
                await _scheduleRepository.AddAsync(entity);
            }

            return MapSchedule(entity);
        }

        /// <summary>
        /// 保存世界 Boss 每日排期。
        /// </summary>
        /// <param name="request">排期保存请求。</param>
        /// <returns>保存后的排期配置。</returns>
        public async Task<AdminWorldBossScheduleDto> SaveScheduleAsync(AdminWorldBossScheduleDto request)
        {
            var entity = await _scheduleRepository.GetByIdAsync("default") ?? new WorldBossScheduleEntity();
            entity.SpawnTimeText = string.IsNullOrWhiteSpace(request.SpawnTimeText) ? "11:00" : request.SpawnTimeText.Trim();
            entity.TimeZoneId = string.IsNullOrWhiteSpace(request.TimeZoneId) ? "China Standard Time" : request.TimeZoneId.Trim();
            entity.SelectionMode = request.SelectionMode;
            entity.IsEnabled = request.IsEnabled;
            entity.LastUpdateTime = DateTime.Now;

            if (await _scheduleRepository.GetByIdAsync(entity.ScheduleId) == null)
            {
                await _scheduleRepository.AddAsync(entity);
            }
            else
            {
                await _scheduleRepository.UpdateAsync(entity);
            }

            return MapSchedule(entity);
        }

        /// <summary>
        /// 获取后台世界 Boss 运行时总览。
        /// 这里会聚合玩家侧运行时接口，方便后台页面一次拿到实例、排行和日志。
        /// </summary>
        /// <returns>世界 Boss 当前运行时信息。</returns>
        public async Task<AdminWorldBossRuntimeDto> GetRuntimeAsync()
        {
            var current = await _worldBossService.GetCurrentAsync(string.Empty);
            return new AdminWorldBossRuntimeDto
            {
                Current = current,
                RankingTop10 = await _worldBossService.GetRankingAsync(string.Empty, 10),
                RecentLogs = await _worldBossService.GetLogsAsync(string.Empty, 50),
                HasActiveInstance = current.HasActiveBoss
            };
        }

        /// <summary>
        /// 立即生成一个世界 Boss 实例。
        /// </summary>
        /// <param name="bossId">指定的 Boss 模板编号；为空时按服务内部规则选择。</param>
        /// <returns>生成后的世界 Boss 当前状态。</returns>
        public Task<WorldBossCurrentDto> SpawnNowAsync(string? bossId = null)
        {
            return _worldBossService.SpawnNowAsync(bossId);
        }

        /// <summary>
        /// 关闭当前运行中的世界 Boss 实例。
        /// </summary>
        /// <param name="reason">关闭原因。</param>
        /// <returns>存在可关闭实例时返回真。</returns>
        public Task<bool> CloseCurrentAsync(string? reason = null)
        {
            return _worldBossService.CloseCurrentAsync(reason);
        }

        /// <summary>
        /// 执行模板删除。
        /// 单独拆出来是为了让公共入口先做参数清洗，再统一处理仓储删除结果。
        /// </summary>
        /// <param name="bossId">已清洗完成的 Boss 模板编号。</param>
        /// <returns>删除成功返回真。</returns>
        private async Task<bool> DeleteInternalAsync(string bossId)
        {
            var deleted = await _templateRepository.DeleteAsync(bossId);
            return deleted > 0;
        }

        /// <summary>
        /// 将世界 Boss 模板实体映射为后台编辑 DTO。
        /// </summary>
        /// <param name="entity">世界 Boss 模板实体。</param>
        /// <returns>后台模板详情 DTO。</returns>
        private static AdminWorldBossTemplateDetailDto MapTemplate(WorldBossTemplateEntity entity)
        {
            return new AdminWorldBossTemplateDetailDto
            {
                BossId = entity.BossId,
                Name = entity.Name,
                MonsterTemplateId = entity.MonsterTemplateId,
                PortraitPath = entity.PortraitPath,
                IsEnabled = entity.IsEnabled,
                Weight = entity.Weight,
                DurationMinutes = entity.DurationMinutes,
                NoticeText = entity.NoticeText,
                ParticipationMinDamage = entity.ParticipationMinDamage,
                ParticipationRewardExp = entity.ParticipationRewardExp,
                ParticipationRewardGold = entity.ParticipationRewardGold,
                ParticipationRewardSpiritStone = entity.ParticipationRewardSpiritStone,
                Rank1RewardExp = entity.Rank1RewardExp,
                Rank1RewardGold = entity.Rank1RewardGold,
                Rank1RewardSpiritStone = entity.Rank1RewardSpiritStone,
                Rank2RewardExp = entity.Rank2RewardExp,
                Rank2RewardGold = entity.Rank2RewardGold,
                Rank2RewardSpiritStone = entity.Rank2RewardSpiritStone,
                Rank3RewardExp = entity.Rank3RewardExp,
                Rank3RewardGold = entity.Rank3RewardGold,
                Rank3RewardSpiritStone = entity.Rank3RewardSpiritStone,
                SortOrder = entity.SortOrder,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        /// <summary>
        /// 将世界 Boss 排期实体映射为后台编辑 DTO。
        /// </summary>
        /// <param name="entity">世界 Boss 排期实体。</param>
        /// <returns>后台排期 DTO。</returns>
        private static AdminWorldBossScheduleDto MapSchedule(WorldBossScheduleEntity entity)
        {
            return new AdminWorldBossScheduleDto
            {
                ScheduleId = entity.ScheduleId,
                SpawnTimeText = entity.SpawnTimeText,
                TimeZoneId = entity.TimeZoneId,
                SelectionMode = entity.SelectionMode,
                IsEnabled = entity.IsEnabled,
                LastUpdateTime = entity.LastUpdateTime
            };
        }
    }
}
