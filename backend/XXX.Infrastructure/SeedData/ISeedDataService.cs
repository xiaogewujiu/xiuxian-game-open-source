namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 种子数据服务接口
    /// 用于初始化系统基础数据
    /// </summary>
    public interface ISeedDataService
    {
        /// <summary>
        /// 初始化所有种子数据
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 初始化物品数据
        /// </summary>
        Task SeedItemsAsync();

        /// <summary>
        /// 初始化装备模板数据
        /// </summary>
        Task SeedEquipmentTemplatesAsync();

        /// <summary>
        /// 初始化技能数据
        /// </summary>
        Task SeedSkillsAsync();

        /// <summary>
        /// 初始化怪物数据
        /// </summary>
        Task SeedMonstersAsync();

        /// <summary>
        /// 初始化地图模板数据
        /// </summary>
        Task SeedMapsAsync();

        /// <summary>
        /// 初始化任务数据
        /// </summary>
        Task SeedQuestsAsync();

        /// <summary>
        /// 初始化成就数据
        /// </summary>
        Task SeedAchievementsAsync();

        /// <summary>
        /// 初始化新手礼包数据。
        /// </summary>
        Task SeedStarterPackagesAsync();

        /// <summary>
        /// 初始化商店数据
        /// </summary>
        Task SeedShopsAsync();

        /// <summary>
        /// 初始化灵宠数据
        /// </summary>
        Task SeedPetsAsync();

        /// <summary>
        /// 初始化丹药配方数据
        /// </summary>
        Task SeedAlchemyRecipesAsync();

        /// <summary>
        /// 初始化锻造配方数据
        /// </summary>
        Task SeedForgeRecipesAsync();

        /// <summary>
        /// 初始化副本数据
        /// </summary>
        Task SeedDungeonsAsync();

        /// <summary>
        /// 初始化等级成长配置。
        /// </summary>
        Task SeedPlayerLevelConfigsAsync();

        /// <summary>
        /// 初始化属性点配置。
        /// </summary>
        Task SeedAttributePointConfigsAsync();

        /// <summary>
        /// 初始化道具扩展配置拆表数据。
        /// </summary>
        Task SeedItemExtensionConfigsAsync();

        /// <summary>
        /// 初始化聚灵阵规则配置。
        /// </summary>
        Task SeedFiveElementRulesAsync();

        /// <summary>
        /// 初始化灵田规则配置。
        /// </summary>
        Task SeedSpiritFieldRulesAsync();

        /// <summary>
        /// 初始化炼丹职业规则配置。
        /// </summary>
        Task SeedAlchemyProfessionRulesAsync();

        /// <summary>
        /// 初始化锻造职业规则配置。
        /// </summary>
        Task SeedForgeProfessionRulesAsync();

        /// <summary>
        /// 初始化元素克制矩阵配置。
        /// </summary>
        Task SeedElementRelationRulesAsync();
    }
}
