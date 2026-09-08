using Microsoft.Extensions.DependencyInjection;
using XXX.Application.Interfaces;
using XXX.Application.Mappings;
using XXX.Application.Services;

namespace XXX.Application
{
    /// <summary>
    /// 应用层依赖注入扩展。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册应用层服务。
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminAuthService, AdminAuthService>();
            services.AddScoped<IAdminAuditService, AdminAuditService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminRuntimeRefreshService, AdminRuntimeRefreshService>();
            services.AddScoped<IAdminMapService, AdminMapService>();
            services.AddScoped<IAdminMonsterService, AdminMonsterService>();
            services.AddScoped<IAdminWorldBossService, AdminWorldBossService>();
            services.AddScoped<IAdminItemService, AdminItemService>();
            services.AddScoped<IAdminDungeonService, AdminDungeonService>();
            services.AddScoped<IAdminDungeonInstanceService, AdminDungeonInstanceService>();
            services.AddScoped<IDungeonInstanceService, DungeonInstanceService>();
            services.AddScoped<IAdminEquipmentService, AdminEquipmentService>();
            services.AddScoped<IAdminAttributePointConfigService, AdminAttributePointConfigService>();
            services.AddScoped<IAdminPlayerLevelConfigService, AdminPlayerLevelConfigService>();
            services.AddScoped<IAdminRealmLevelConfigService, AdminRealmLevelConfigService>();
            services.AddScoped<IAdminPlayerService, AdminPlayerService>();
            services.AddScoped<IAdminFiveElementService, AdminFiveElementService>();
            services.AddScoped<IAdminFiveElementRuleService, AdminFiveElementRuleService>();
            services.AddScoped<IAdminPetService, AdminPetService>();
            services.AddScoped<IAdminCropService, AdminCropService>();
            services.AddScoped<IAdminSpiritFieldService, AdminSpiritFieldService>();
            services.AddScoped<IAdminSpiritFieldRuleService, AdminSpiritFieldRuleService>();
            services.AddScoped<IAdminRewardConfigService, AdminRewardConfigService>();
            services.AddScoped<IAdminStarterPackageService, AdminStarterPackageService>();
            services.AddScoped<IAdminElementRelationRuleService, AdminElementRelationRuleService>();
            services.AddScoped<IAdminAlchemyService, AdminAlchemyService>();
            services.AddScoped<IAdminAlchemyRuleService, AdminAlchemyRuleService>();
            services.AddScoped<IAdminAlchemyRuntimeService, AdminAlchemyRuntimeService>();
            services.AddScoped<IAdminForgeService, AdminForgeService>();
            services.AddScoped<IAdminForgeRuleService, AdminForgeRuleService>();
            services.AddScoped<IAdminEquipmentRerollRuleService, AdminEquipmentRerollRuleService>();
            services.AddScoped<IAdminEquipmentDecomposeRuleService, AdminEquipmentDecomposeRuleService>();
            services.AddScoped<IAdminForgeRuntimeService, AdminForgeRuntimeService>();
            services.AddScoped<IAdminQuestService, AdminQuestService>();
            services.AddScoped<IAdminAchievementService, AdminAchievementService>();
            services.AddScoped<IAdminShopService, AdminShopService>();
            services.AddScoped<IAdminRankingService, AdminRankingService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminSkillService, AdminSkillService>();
            services.AddScoped<IAdminBuffService, AdminBuffService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IPlayerAttributeService, PlayerAttributeService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<ISpiritFieldService, SpiritFieldService>();
            services.AddScoped<IAlchemyService, AlchemyService>();
            services.AddScoped<IFiveElementService, FiveElementService>();
            services.AddScoped<IQuestService, QuestService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<IStarterPackageService, StarterPackageService>();
            services.AddScoped<IBattleService, BattleService>();
            services.AddScoped<IRankingService, RankingService>();
            services.AddScoped<IAchievementService, AchievementService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IGuildService, GuildService>();
            services.AddScoped<ISectService, SectService>();
            services.AddScoped<IAdminSectService, AdminSectService>();
            services.AddScoped<IGameSyncService, GameSyncService>();

            services.AddSingleton<IBattleProgressQueue, BattleProgressQueue>();
            services.AddScoped<IPlayerRewardService, PlayerRewardService>();
            services.AddScoped<ICheckInService, CheckInService>();
            services.AddScoped<IRedeemCodeService, RedeemCodeService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IForgeService, ForgeService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IWorldBossService, WorldBossService>();
            services.AddScoped<IGrowthConfigRuntimeService, GrowthConfigRuntimeService>();
            services.AddScoped<IFiveElementRuleRuntimeService, FiveElementRuleRuntimeService>();
            services.AddScoped<ISpiritFieldRuleRuntimeService, SpiritFieldRuleRuntimeService>();
            services.AddScoped<IAdminSpiritFieldRuleService, AdminSpiritFieldRuleService>();
            services.AddScoped<IAlchemyProfessionRuleRuntimeService, AlchemyProfessionRuleRuntimeService>();
            services.AddScoped<IForgeProfessionRuleRuntimeService, ForgeProfessionRuleRuntimeService>();
            services.AddScoped<IEquipmentRerollRuleRuntimeService, EquipmentRerollRuleRuntimeService>();
            services.AddScoped<IElementRelationRuleRuntimeService, ElementRelationRuleRuntimeService>();

            services.AddScoped<IAdminCollectionService, AdminCollectionService>();
            services.AddScoped<IAdminLotteryService, AdminLotteryService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<ILotteryService, LotteryService>();

            services.AddScoped<IFavorabilityService, FavorabilityService>();
            services.AddScoped<IFeedbackService, FeedbackService>();

            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IAdminMailService, AdminMailService>();

            services.AddScoped<ITitleService, TitleService>();
            services.AddScoped<IAdminTitleService, AdminTitleService>();

            services.AddScoped<IArenaService, ArenaService>();
            services.AddScoped<IAdminArenaService, AdminArenaService>();

            services.AddScoped<ITowerService, TowerService>();
            services.AddScoped<IAdminTowerService, AdminTowerService>();

            services.AddScoped<IGemService, GemService>();
            services.AddScoped<IAdminGemService, AdminGemService>();

            services.AddScoped<IMarketService, MarketService>();
            services.AddScoped<IAdminMarketService, AdminMarketService>();

            return services;
        }
    }
}
