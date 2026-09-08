using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台锻造运行态服务。
    /// </summary>
    public class AdminForgeRuntimeService : IAdminForgeRuntimeService
    {
        /// <summary>
        /// 玩家仓储。
        /// </summary>
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// 锻造系统仓储。
        /// </summary>
        private readonly IRepository<ForgeSystemEntity> _forgeSystemRepository;

        /// <summary>
        /// 锻造配方仓储。
        /// </summary>
        private readonly IRepository<ForgeRecipeEntity> _forgeRecipeRepository;

        /// <summary>
        /// 五行阵仓储。
        /// 用于推导锻造职业等级上限。
        /// </summary>
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;

        /// <summary>
        /// 初始化锻造运行态服务。
        /// </summary>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="forgeSystemRepository">锻造系统仓储。</param>
        /// <param name="forgeRecipeRepository">锻造配方仓储。</param>
        /// <param name="fiveElementRepository">五行阵仓储。</param>
        public AdminForgeRuntimeService(
            IRepository<UserEntity> userRepository,
            IRepository<ForgeSystemEntity> forgeSystemRepository,
            IRepository<ForgeRecipeEntity> forgeRecipeRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository)
        {
            _userRepository = userRepository;
            _forgeSystemRepository = forgeSystemRepository;
            _forgeRecipeRepository = forgeRecipeRepository;
            _fiveElementRepository = fiveElementRepository;
        }

        /// <summary>
        /// 获取后台锻造系统列表。
        /// 支持按玩家编号、名称或账号做关键字筛选。
        /// </summary>
        /// <param name="keyword">筛选关键字。</param>
        /// <returns>锻造系统列表项集合。</returns>
        public async Task<List<AdminForgeSystemListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _userRepository.Db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted);

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(user =>
                    user.GID.Contains(normalizedKeyword) ||
                    user.Name.Contains(normalizedKeyword) ||
                    user.Account.Contains(normalizedKeyword));
            }

            var users = await query.OrderByDescending(user => user.Level).ToListAsync();
            if (users.Count == 0)
            {
                return [];
            }

            var playerIds = users.Select(user => user.GID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var systemMap = await LoadSystemMapAsync(playerIds);
            var arrayMap = await LoadArrayMapAsync(playerIds);
            var recipeMap = await LoadRecipeNameMapAsync(
                systemMap.Values
                    .Select(system => system.ActiveRecipeId)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

            return users.Select(user =>
            {
                var system = systemMap.TryGetValue(user.GID, out var value) ? value : null;
                var professionCap = GetProfessionLevelCap(arrayMap.TryGetValue(user.GID, out var array) ? array : null);
                var activeRecipeName = !string.IsNullOrWhiteSpace(system?.ActiveRecipeId) &&
                                       recipeMap.TryGetValue(system.ActiveRecipeId!, out var recipeName)
                    ? recipeName
                    : null;

                return new AdminForgeSystemListItemDto
                {
                    PlayerId = user.GID,
                    Name = user.Name,
                    Account = user.Account,
                    BlacksmithLevel = Math.Min(professionCap, Math.Max(1, system?.BlacksmithLevel ?? 1)),
                    ProfessionLevelCap = professionCap,
                    IsForging = !string.IsNullOrWhiteSpace(system?.ActiveRecipeId),
                    ActiveRecipeId = system?.ActiveRecipeId,
                    ActiveRecipeName = activeRecipeName,
                    ActiveForgeCompleteAt = system?.ActiveForgeCompleteAt,
                    CanCollect = system?.ActiveForgeCompleteAt.HasValue == true && system.ActiveForgeCompleteAt <= DateTime.Now,
                    TotalForgeCount = Math.Max(0, system?.TotalForgeCount ?? 0)
                };
            }).ToList();
        }

        /// <summary>
        /// 获取指定玩家的锻造系统详情。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>锻造系统详情；不存在时返回空。</returns>
        public async Task<AdminForgeSystemDetailDto?> GetDetailAsync(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            var normalizedPlayerId = playerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            var system = await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var recipeName = await ResolveRecipeNameAsync(system?.ActiveRecipeId);
            return MapDetail(user, system, GetProfessionLevelCap(array), recipeName);
        }

        /// <summary>
        /// 保存指定玩家的锻造运行态数据。
        /// 后台可直接调整职业等级、经验等调试字段。
        /// </summary>
        /// <param name="request">锻造系统详情请求。</param>
        /// <returns>保存后的锻造系统详情。</returns>
        public async Task<AdminForgeSystemDetailDto> SaveAsync(AdminForgeSystemDetailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var normalizedPlayerId = request.PlayerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            var array = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var professionCap = GetProfessionLevelCap(array);
            var now = DateTime.Now;
            var system = await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);

            if (system == null)
            {
                system = new ForgeSystemEntity
                {
                    PlayerId = normalizedPlayerId,
                    DailyResetTime = now.Date
                };
                ApplyValues(system, request, professionCap, now);
                await _forgeSystemRepository.AddAsync(system);
            }
            else
            {
                ApplyValues(system, request, professionCap, now);
                await _forgeSystemRepository.UpdateAsync(system);
            }

            return await GetDetailAsync(normalizedPlayerId) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 清空当前锻造中的任务状态。
        /// 会直接移除正在锻造的配方、开始时间、完成时间和待领取结果。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <returns>清空后的锻造系统详情。</returns>
        public async Task<AdminForgeSystemDetailDto> ClearActiveTaskAsync(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var normalizedPlayerId = playerId.Trim();
            var system = await _forgeSystemRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            if (system == null)
            {
                throw new InvalidOperationException("该玩家暂无锻造系统数据。");
            }

            system.ActiveRecipeId = null;
            system.ActiveForgeStartedAt = null;
            system.ActiveForgeCompleteAt = null;
            system.PendingResultJson = null;
            system.LastUpdateTime = DateTime.Now;
            await _forgeSystemRepository.UpdateAsync(system);

            return await GetDetailAsync(normalizedPlayerId) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 把后台提交的详情值写回锻造系统实体。
        /// 会按五行阵等级上限收口职业等级，避免后台写出非法值。
        /// </summary>
        /// <param name="system">待写入的锻造系统实体。</param>
        /// <param name="request">后台详情请求。</param>
        /// <param name="professionCap">当前职业等级上限。</param>
        /// <param name="now">当前本地时间。</param>
        private static void ApplyValues(ForgeSystemEntity system, AdminForgeSystemDetailDto request, int professionCap, DateTime now)
        {
            system.BlacksmithLevel = Math.Clamp(request.BlacksmithLevel, 1, professionCap);
            system.BlacksmithExp = Math.Max(0, request.BlacksmithExp);
            system.LastUpdateTime = now;
        }

        /// <summary>
        /// 批量加载玩家编号到锻造系统实体的映射表。
        /// </summary>
        /// <param name="playerIds">玩家编号集合。</param>
        /// <returns>玩家编号到锻造系统实体的映射。</returns>
        private async Task<Dictionary<string, ForgeSystemEntity>> LoadSystemMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _forgeSystemRepository.Db.Queryable<ForgeSystemEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 批量加载玩家编号到五行阵实体的映射表。
        /// </summary>
        /// <param name="playerIds">玩家编号集合。</param>
        /// <returns>玩家编号到五行阵实体的映射。</returns>
        private async Task<Dictionary<string, FiveElementArrayEntity>> LoadArrayMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _fiveElementRepository.Db.Queryable<FiveElementArrayEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 批量加载配方编号到配方名称的映射表。
        /// </summary>
        /// <param name="recipeIds">配方编号集合。</param>
        /// <returns>配方编号到名称的映射。</returns>
        private async Task<Dictionary<string, string>> LoadRecipeNameMapAsync(IReadOnlyCollection<string> recipeIds)
        {
            if (recipeIds.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var recipes = await _forgeRecipeRepository.Db.Queryable<ForgeRecipeEntity>()
                .Where(entity => recipeIds.Contains(entity.RecipeId))
                .ToListAsync();
            return recipes.ToDictionary(item => item.RecipeId, item => item.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析单个配方编号对应的名称。
        /// </summary>
        /// <param name="recipeId">配方编号。</param>
        /// <returns>配方名称；为空或不存在时返回空。</returns>
        private async Task<string?> ResolveRecipeNameAsync(string? recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return null;
            }

            var recipe = await _forgeRecipeRepository.GetByIdAsync(recipeId.Trim());
            return recipe?.Name;
        }

        /// <summary>
        /// 根据玩家五行阵等级推导锻造职业等级上限。
        /// </summary>
        /// <param name="array">玩家五行阵实体。</param>
        /// <returns>锻造职业等级上限。</returns>
        private static int GetProfessionLevelCap(FiveElementArrayEntity? array)
        {
            var arrayLevel = Math.Clamp(array?.ArrayLevel <= 0 ? 1 : array?.ArrayLevel ?? 1, 1, 50);
            return FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel);
        }

        /// <summary>
        /// 把玩家实体和锻造系统实体映射成后台详情 DTO。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <param name="system">锻造系统实体。</param>
        /// <param name="professionCap">当前职业等级上限。</param>
        /// <param name="recipeName">当前任务配方名称。</param>
        /// <returns>后台锻造系统详情 DTO。</returns>
        private static AdminForgeSystemDetailDto MapDetail(UserEntity user, ForgeSystemEntity? system, int professionCap, string? recipeName)
        {
            return new AdminForgeSystemDetailDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                BlacksmithLevel = Math.Min(professionCap, Math.Max(1, system?.BlacksmithLevel ?? 1)),
                BlacksmithExp = Math.Max(0, system?.BlacksmithExp ?? 0),
                ProfessionLevelCap = professionCap,
                TodayForgeCount = Math.Max(0, system?.TodayForgeCount ?? 0),
                TotalForgeCount = Math.Max(0, system?.TotalForgeCount ?? 0),
                SuccessForgeCount = Math.Max(0, system?.SuccessForgeCount ?? 0),
                IsForging = !string.IsNullOrWhiteSpace(system?.ActiveRecipeId),
                ActiveRecipeId = system?.ActiveRecipeId,
                ActiveRecipeName = recipeName,
                ActiveForgeStartedAt = system?.ActiveForgeStartedAt,
                ActiveForgeCompleteAt = system?.ActiveForgeCompleteAt,
                CanCollect = system?.ActiveForgeCompleteAt.HasValue == true && system.ActiveForgeCompleteAt <= DateTime.Now
            };
        }
    }
}
