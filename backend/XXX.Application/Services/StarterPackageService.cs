using System.Collections.Concurrent;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 新手礼包运行时服务。
    /// 第 2 阶段开始承担注册礼包真源，替代 AuthService 内部硬编码发放。
    /// </summary>
    public class StarterPackageService : IStarterPackageService
    {
        /// <summary>
        /// 新手礼包运行时快照。
        /// 一条礼包配置会同时携带其物品奖励和技能奖励列表。
        /// </summary>
        private sealed record StarterPackageRuntimeSnapshot(
            StarterPackageConfigEntity Config,
            IReadOnlyList<StarterPackageGrantItemEntity> ItemGrants,
            IReadOnlyList<StarterPackageGrantSkillEntity> SkillGrants);

        /// <summary>
        /// 初始化锁。
        /// 防止并发首次加载时重复构建运行时缓存。
        /// </summary>
        private static readonly SemaphoreSlim InitializationLock = new(1, 1);

        /// <summary>
        /// 全局共享的新手礼包缓存。
        /// 键为礼包编号。
        /// </summary>
        private static readonly ConcurrentDictionary<string, StarterPackageRuntimeSnapshot> SharedPackages = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 当前进程内是否已完成初始化。
        /// </summary>
        private static volatile bool _initialized;

        /// <summary>
        /// 礼包配置仓储。
        /// </summary>
        private readonly IRepository<StarterPackageConfigEntity> _configRepository;

        /// <summary>
        /// 礼包物品奖励仓储。
        /// </summary>
        private readonly IRepository<StarterPackageGrantItemEntity> _itemGrantRepository;

        /// <summary>
        /// 礼包技能奖励仓储。
        /// </summary>
        private readonly IRepository<StarterPackageGrantSkillEntity> _skillGrantRepository;

        /// <summary>
        /// 玩家仓储。
        /// </summary>
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// 背包服务。
        /// 注册时礼包物品最终通过它发放给玩家。
        /// </summary>
        private readonly IInventoryService _inventoryService;

        /// <summary>
        /// 初始化新手礼包运行时服务。
        /// </summary>
        /// <param name="configRepository">礼包配置仓储。</param>
        /// <param name="itemGrantRepository">礼包物品奖励仓储。</param>
        /// <param name="skillGrantRepository">礼包技能奖励仓储。</param>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="inventoryService">背包服务。</param>
        public StarterPackageService(
            IRepository<StarterPackageConfigEntity> configRepository,
            IRepository<StarterPackageGrantItemEntity> itemGrantRepository,
            IRepository<StarterPackageGrantSkillEntity> skillGrantRepository,
            IRepository<UserEntity> userRepository,
            IInventoryService inventoryService)
        {
            _configRepository = configRepository;
            _itemGrantRepository = itemGrantRepository;
            _skillGrantRepository = skillGrantRepository;
            _userRepository = userRepository;
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// 强制重载新手礼包运行时缓存。
        /// </summary>
        public async Task ReloadCacheAsync()
        {
            await InitializeInternalAsync(forceReload: true);
        }

        /// <summary>
        /// 为新注册玩家发放当前启用的新手礼包。
        /// 技能会直接写入玩家角色，物品则通过背包服务发放。
        /// </summary>
        /// <param name="user">刚注册完成的玩家实体。</param>
        public async Task ApplyOnRegisterAsync(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user);

            await InitializeInternalAsync(forceReload: false);
            var snapshot = ResolveRegisterPackage();
            if (snapshot == null)
            {
                return;
            }

            ApplyGrantedSkills(user, snapshot.SkillGrants);
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            foreach (var grant in snapshot.ItemGrants.OrderBy(item => item.SortOrder).ThenBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(grant.ItemId) || grant.Quantity <= 0)
                {
                    continue;
                }

                await _inventoryService.AddItemAsync(user.GID, new AddItemRequestDto
                {
                    ItemId = grant.ItemId,
                    Quantity = grant.Quantity,
                    Source = $"StarterPackage:{snapshot.Config.PackageId}"
                });
            }
        }

        /// <summary>
        /// 初始化或重载新手礼包缓存。
        /// </summary>
        /// <param name="forceReload">是否强制忽略已初始化状态并重新加载。</param>
        private async Task InitializeInternalAsync(bool forceReload)
        {
            if (_initialized && !forceReload)
            {
                return;
            }

            await InitializationLock.WaitAsync();
            try
            {
                if (_initialized && !forceReload)
                {
                    return;
                }

                var configs = await _configRepository.Db.Queryable<StarterPackageConfigEntity>()
                    .OrderBy(item => item.SortOrder)
                    .OrderBy(item => item.PackageId)
                    .ToListAsync();
                var itemGrants = await _itemGrantRepository.Db.Queryable<StarterPackageGrantItemEntity>()
                    .OrderBy(item => item.SortOrder)
                    .OrderBy(item => item.GID)
                    .ToListAsync();
                var skillGrants = await _skillGrantRepository.Db.Queryable<StarterPackageGrantSkillEntity>()
                    .OrderBy(item => item.SortOrder)
                    .OrderBy(item => item.GID)
                    .ToListAsync();

                SharedPackages.Clear();
                foreach (var config in configs)
                {
                    SharedPackages[config.PackageId] = new StarterPackageRuntimeSnapshot(
                        config,
                        itemGrants.Where(item => item.PackageId == config.PackageId).ToList(),
                        skillGrants.Where(item => item.PackageId == config.PackageId).ToList());
                }

                _initialized = true;
            }
            finally
            {
                InitializationLock.Release();
            }
        }

        /// <summary>
        /// 解析当前应在注册时自动发放的礼包。
        /// 只会取启用且勾选“注册自动发放”的第一条礼包。
        /// </summary>
        /// <returns>匹配到的礼包快照；没有可发礼包时返回空。</returns>
        private static StarterPackageRuntimeSnapshot? ResolveRegisterPackage()
        {
            return SharedPackages.Values
                .Where(item => item.Config.IsEnabled && item.Config.AutoGrantOnRegister)
                .OrderBy(item => item.Config.SortOrder)
                .ThenBy(item => item.Config.PackageId, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        /// <summary>
        /// 把礼包中赠送的技能写入玩家已拥有和已装备技能列表。
        /// 会过滤掉职业不兼容或不存在的技能。
        /// </summary>
        /// <param name="user">玩家实体。</param>
        /// <param name="grants">技能奖励列表。</param>
        private static void ApplyGrantedSkills(UserEntity user, IReadOnlyList<StarterPackageGrantSkillEntity> grants)
        {
            var owned = NormalizeSkillList(user.OwnedSkillIds);
            var equipped = NormalizeSkillList(user.SkillIds);

            foreach (var grant in grants.OrderBy(item => item.SortOrder).ThenBy(item => item.SkillId))
            {
                if (grant.SkillId <= 0)
                {
                    continue;
                }

                var resolvedSkillId = grant.SkillId == 1001
                    ? user.Profession switch
                    {
                        PlayerProfessionCatalog.Mage => 1101,
                        PlayerProfessionCatalog.Body => 1201,
                        _ => 1001
                    }
                    : grant.SkillId;
                var skillId = resolvedSkillId.ToString();
                if (resolvedSkillId is not (1001 or 1101 or 1201))
                {
                    continue;
                }

                if (!owned.Contains(skillId, StringComparer.OrdinalIgnoreCase))
                {
                    owned.Add(skillId);
                }

                if (!equipped.Contains(skillId, StringComparer.OrdinalIgnoreCase))
                {
                    equipped.Add(skillId);
                }
            }

            user.OwnedSkillIds = owned;
            user.SkillIds = equipped;
        }

        /// <summary>
        /// 归一化技能编号列表。
        /// 会去掉空白项并按忽略大小写方式去重。
        /// </summary>
        /// <param name="skills">原始技能编号列表。</param>
        /// <returns>清洗后的技能编号列表。</returns>
        private static List<string> NormalizeSkillList(List<string>? skills)
        {
            return (skills ?? [])
                .Select(item => item?.Trim() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
