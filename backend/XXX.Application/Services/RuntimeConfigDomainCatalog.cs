using XXX.Balance;

namespace XXX.Application.Services
{
    /// <summary>
    /// 运行时配置域目录。
    /// 第 1 阶段先统一域定义和状态跟踪，后续逐步接入更多真实刷新器。
    /// </summary>
    internal static class RuntimeConfigDomainCatalog
    {
        internal sealed record RuntimeConfigDomainDefinition(
            string Domain,
            string Name,
            string Group,
            string Description,
            bool RefreshSupported,
            string? CurrentVersion);

        public const string Template = "template";
        public const string Quest = "quest";
        public const string Achievement = "achievement";
        public const string Shop = "shop";
        public const string Ranking = "ranking";
        public const string Content = "content";
        public const string Growth = "growth";
        public const string BattleRule = "battle-rule";
        public const string FiveElement = "five-element";
        public const string SpiritField = "spirit-field";
        public const string Alchemy = "alchemy";
        public const string Forge = "forge";
        public const string EquipmentReroll = "equipment-reroll";
        public const string Activity = "activity";
        public const string StarterPackage = "starter-package";

        private static readonly IReadOnlyList<RuntimeConfigDomainDefinition> Domains =
        [
            new(Template, "模板缓存", "基础设施", "运行时模板、地图、怪物、道具、技能等缓存。", true, "runtime-db"),
            new(Quest, "任务配置", "内容", "任务配置缓存与默认任务内存快照。", true, "runtime-db"),
            new(Achievement, "成就配置", "内容", "成就配置缓存与默认成就内存快照。", true, "runtime-db"),
            new(Shop, "商店配置", "内容", "商店入口、商品和价格缓存。", true, "runtime-db"),
            new(Ranking, "排行配置", "内容", "排行榜定义、奖励和刷新间隔缓存。", true, "runtime-db"),
            new(Content, "内容聚合", "内容", "一次刷新任务、成就、商店、排行四个内容域。", true, "runtime-db"),
            new(Activity, "活动奖励", "内容", "签到奖励、兑换码和后续活动配置。", true, $"{ActivityRewardCatalog.CheckInConfigVersion} / {ActivityRewardCatalog.RedeemCodeConfigVersion}"),
            new(Growth, "成长配置", "成长", "等级、境界、突破与属性点配置。", true, "runtime-db"),
            new(BattleRule, "战斗规则", "规则", "元素克制、基础战斗规则与战斗通用参数。", true, "runtime-db"),
            new(FiveElement, "聚灵阵规则", "规则", "聚灵阵等级成本、经验加成与职业上限。", true, "runtime-db"),
            new(SpiritField, "灵田规则", "规则", "灵田地块、催熟、产量加成与同步规则。", true, "runtime-db"),
            new(Alchemy, "炼丹规则", "规则", "炼丹职业成长、丹方耗时与成功率规则。", true, "runtime-db"),
            new(Forge, "锻造规则", "规则", "锻造职业成长、图纸耗时与成功率规则。", true, "runtime-db"),
            new(EquipmentReroll, "装备洗练规则", "规则", "洗练系统配置、槽位词条池与品阶配置。", true, "runtime-db"),
            new(StarterPackage, "新手礼包", "内容", "注册初始技能与起始道具包。", true, "runtime-db")
        ];

        public static IReadOnlyList<RuntimeConfigDomainDefinition> GetAll()
        {
            return Domains;
        }

        public static RuntimeConfigDomainDefinition? Find(string? domain)
        {
            return Domains.FirstOrDefault(item => string.Equals(item.Domain, domain?.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
