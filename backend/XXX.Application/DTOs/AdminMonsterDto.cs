using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台怪物列表项。
    /// </summary>
    public class AdminMonsterListItemDto
    {
        /// <summary>
        /// 怪物编号。
        /// </summary>
        public string MonsterId { get; set; } = string.Empty;

        /// <summary>
        /// 怪物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 技能数量。
        /// </summary>
        public int SkillCount { get; set; }

        /// <summary>
        /// 被动数量。
        /// </summary>
        public int PassiveCount { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台怪物详情。
    /// </summary>
    public class AdminMonsterDetailDto
    {
        /// <summary>
        /// 怪物编号。
        /// </summary>
        public string MonsterId { get; set; } = string.Empty;

        /// <summary>
        /// 怪物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 奖励经验最小值。
        /// </summary>
        public int ExpRewardMin { get; set; }

        /// <summary>
        /// 奖励经验最大值。
        /// </summary>
        public int ExpRewardMax { get; set; }

        /// <summary>
        /// 奖励金币最小值。
        /// </summary>
        public int GoldRewardMin { get; set; }

        /// <summary>
        /// 奖励金币最大值。
        /// </summary>
        public int GoldRewardMax { get; set; }

        /// <summary>
        /// 技能编号列表。
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 被动编号列表。
        /// </summary>
        public List<string> PassiveIds { get; set; } = [];

        /// <summary>
        /// 元素池。
        /// </summary>
        public List<Element> ElementPool { get; set; } = [];

        /// <summary>
        /// 道具掉落。
        /// </summary>
        public List<DropItem> ItemDrops { get; set; } = [];

        /// <summary>
        /// 装备掉落。
        /// </summary>
        public List<DropEquipment> EquipmentDrops { get; set; } = [];

        /// <summary>
        /// 图鉴掉落。
        /// </summary>
        public List<DropCollection> CollectionDrops { get; set; } = [];

        /// <summary>
        /// 基础属性模板。
        /// </summary>
        public BaseAttributesRangeDto Attributes { get; set; } = new();

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 保存怪物请求。
    /// </summary>
    public class AdminSaveMonsterRequestDto
    {
        /// <summary>
        /// 怪物编号。
        /// </summary>
        public string MonsterId { get; set; } = string.Empty;

        /// <summary>
        /// 怪物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 奖励经验最小值。
        /// </summary>
        public int ExpRewardMin { get; set; }

        /// <summary>
        /// 奖励经验最大值。
        /// </summary>
        public int ExpRewardMax { get; set; }

        /// <summary>
        /// 奖励金币最小值。
        /// </summary>
        public int GoldRewardMin { get; set; }

        /// <summary>
        /// 奖励金币最大值。
        /// </summary>
        public int GoldRewardMax { get; set; }

        /// <summary>
        /// 技能编号列表。
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 被动编号列表。
        /// </summary>
        public List<string> PassiveIds { get; set; } = [];

        /// <summary>
        /// 元素池。
        /// </summary>
        public List<Element> ElementPool { get; set; } = [];

        /// <summary>
        /// 道具掉落。
        /// </summary>
        public List<DropItem> ItemDrops { get; set; } = [];

        /// <summary>
        /// 装备掉落。
        /// </summary>
        public List<DropEquipment> EquipmentDrops { get; set; } = [];

        /// <summary>
        /// 图鉴掉落。
        /// </summary>
        public List<DropCollection> CollectionDrops { get; set; } = [];

        /// <summary>
        /// 属性区间。
        /// </summary>
        public BaseAttributesRangeDto Attributes { get; set; } = new();
    }

    /// <summary>
    /// 基础属性区间 DTO。
    /// </summary>
    public class BaseAttributesRangeDto
    {
        /// <summary>
        /// 生命最小值。
        /// </summary>
        public int? MinType1 { get; set; }

        /// <summary>
        /// 生命最大值。
        /// </summary>
        public int? MaxType1 { get; set; }

        /// <summary>
        /// 法力最小值。
        /// </summary>
        public int? MinType2 { get; set; }

        /// <summary>
        /// 法力最大值。
        /// </summary>
        public int? MaxType2 { get; set; }

        /// <summary>
        /// 物攻最小值。
        /// </summary>
        public int? MinType3 { get; set; }

        /// <summary>
        /// 物攻最大值。
        /// </summary>
        public int? MaxType3 { get; set; }

        /// <summary>
        /// 法攻最小值。
        /// </summary>
        public int? MinType4 { get; set; }

        /// <summary>
        /// 法攻最大值。
        /// </summary>
        public int? MaxType4 { get; set; }

        /// <summary>
        /// 物防最小值。
        /// </summary>
        public int? MinType5 { get; set; }

        /// <summary>
        /// 物防最大值。
        /// </summary>
        public int? MaxType5 { get; set; }

        /// <summary>
        /// 法防最小值。
        /// </summary>
        public int? MinType6 { get; set; }

        /// <summary>
        /// 法防最大值。
        /// </summary>
        public int? MaxType6 { get; set; }

        /// <summary>
        /// 速度最小值。
        /// </summary>
        public int? MinType7 { get; set; }

        /// <summary>
        /// 速度最大值。
        /// </summary>
        public int? MaxType7 { get; set; }

        /// <summary>
        /// 命中最小值。
        /// </summary>
        public int? MinType8 { get; set; }

        /// <summary>
        /// 命中最大值。
        /// </summary>
        public int? MaxType8 { get; set; }

        /// <summary>
        /// 闪避最小值。
        /// </summary>
        public int? MinType9 { get; set; }

        /// <summary>
        /// 闪避最大值。
        /// </summary>
        public int? MaxType9 { get; set; }

        /// <summary>
        /// 暴击最小值。
        /// </summary>
        public int? MinType10 { get; set; }

        /// <summary>
        /// 暴击最大值。
        /// </summary>
        public int? MaxType10 { get; set; }

        /// <summary>
        /// 暴伤最小值。
        /// </summary>
        public int? MinType11 { get; set; }

        /// <summary>
        /// 暴伤最大值。
        /// </summary>
        public int? MaxType11 { get; set; }

        /// <summary>
        /// 连击最小值。
        /// </summary>
        public int? MinType12 { get; set; }

        /// <summary>
        /// 连击最大值。
        /// </summary>
        public int? MaxType12 { get; set; }

        /// <summary>
        /// 反击最小值。
        /// </summary>
        public int? MinType13 { get; set; }

        /// <summary>
        /// 反击最大值。
        /// </summary>
        public int? MaxType13 { get; set; }

        /// <summary>
        /// 破甲最小值。
        /// </summary>
        public int? MinType14 { get; set; }

        /// <summary>
        /// 破甲最大值。
        /// </summary>
        public int? MaxType14 { get; set; }

        /// <summary>
        /// 附伤最小值。
        /// </summary>
        public int? MinType15 { get; set; }

        /// <summary>
        /// 附伤最大值。
        /// </summary>
        public int? MaxType15 { get; set; }

        /// <summary>
        /// 默认元素。
        /// </summary>
        public Element? Element { get; set; }
    }
}
