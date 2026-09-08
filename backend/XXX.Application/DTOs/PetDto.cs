using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 玩家灵宠展示 DTO。
    /// </summary>
    public class PetDto
    {
        /// <summary>
        /// 灵宠实例 ID。
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠模板 ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠头像图标。
        /// </summary>
        public string Avatar { get; set; } = "\U0001F43E";

        /// <summary>
        /// 当前等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 当前经验。
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 升级所需经验。
        /// </summary>
        public long XExp { get; set; }

        /// <summary>
        /// 当前品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 品质文本。
        /// </summary>
        public string QualityText { get; set; } = string.Empty;

        /// <summary>
        /// 可进化的最大品质。
        /// </summary>
        public int MaxQuality { get; set; }

        /// <summary>
        /// 攻击。
        /// </summary>
        public int Attack { get; set; }

        /// <summary>
        /// 防御。
        /// </summary>
        public int Defense { get; set; }

        /// <summary>
        /// 生命值。
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// 法力值。
        /// </summary>
        public int MP { get; set; }

        /// <summary>
        /// 速度。
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 忠诚度。
        /// </summary>
        public int Loyalty { get; set; }

        /// <summary>
        /// 灵宠类型枚举。
        /// </summary>
        public PetType Type { get; set; }

        /// <summary>
        /// 灵宠类型文本。
        /// </summary>
        public string TypeText { get; set; } = string.Empty;

        /// <summary>
        /// 元素。
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// 元素文本。
        /// </summary>
        public string ElementText { get; set; } = string.Empty;

        /// <summary>
        /// 成长率。
        /// </summary>
        public double GrowthRate { get; set; }

        /// <summary>
        /// 命中率。
        /// </summary>
        public double HitRate { get; set; }

        /// <summary>
        /// 闪避率。
        /// </summary>
        public double DodgeRate { get; set; }

        /// <summary>
        /// 暴击率。
        /// </summary>
        public double CritRate { get; set; }

        /// <summary>
        /// 暴击伤害倍率。
        /// </summary>
        public double CritDamage { get; set; }

        /// <summary>
        /// 连击率。
        /// </summary>
        public double ComboRate { get; set; }

        /// <summary>
        /// 反击率。
        /// </summary>
        public double CounterRate { get; set; }

        /// <summary>
        /// 破甲率。
        /// </summary>
        public double ArmorBreak { get; set; }

        /// <summary>
        /// 额外伤害。
        /// </summary>
        public double BonusDamage { get; set; }

        /// <summary>
        /// 成长率文本。
        /// </summary>
        public string GrowthText { get; set; } = string.Empty;

        /// <summary>
        /// 是否为当前出战灵宠。
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 技能 ID 列表。
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 技能详情列表。
        /// </summary>
        public List<PetSkillDto> Skills { get; set; } = [];

        /// <summary>
        /// 获得时间。
        /// </summary>
        public DateTime AcquiredTime { get; set; }
    }

    /// <summary>
    /// 灵宠技能展示 DTO。
    /// </summary>
    public class PetSkillDto
    {
        /// <summary>
        /// 技能 ID。
        /// </summary>
        public string SkillId { get; set; } = string.Empty;

        /// <summary>
        /// 技能名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 技能描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 技能图标。
        /// </summary>
        public string Icon { get; set; } = "\u2694\uFE0F";
    }

    /// <summary>
    /// 灵宠模板展示 DTO。
    /// </summary>
    public class PetTemplateDto
    {
        /// <summary>
        /// 模板 ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 模板名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模板描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 模板头像图标。
        /// </summary>
        public string Avatar { get; set; } = "\U0001F43E";

        /// <summary>
        /// 灵宠类型枚举。
        /// </summary>
        public PetType Type { get; set; }

        /// <summary>
        /// 灵宠类型文本。
        /// </summary>
        public string TypeText { get; set; } = string.Empty;

        /// <summary>
        /// 元素。
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// 元素文本。
        /// </summary>
        public string ElementText { get; set; } = string.Empty;

        /// <summary>
        /// 初始品质下限。
        /// </summary>
        public int InitialQualityMin { get; set; }

        /// <summary>
        /// 初始品质上限。
        /// </summary>
        public int InitialQualityMax { get; set; }

        /// <summary>
        /// 品质文本。
        /// </summary>
        public string QualityText { get; set; } = string.Empty;

        /// <summary>
        /// 成长率下限。
        /// </summary>
        public double GrowthRateMin { get; set; }

        /// <summary>
        /// 成长率上限。
        /// </summary>
        public double GrowthRateMax { get; set; }

        /// <summary>
        /// 初始技能数量。
        /// </summary>
        public int InitialSkillCount { get; set; }

        /// <summary>
        /// 属性范围。
        /// </summary>
        public BaseAttributesRangeDto Attributes { get; set; } = new();

        /// <summary>
        /// 成长率文本。
        /// </summary>
        public string GrowthText { get; set; } = string.Empty;

        /// <summary>
        /// 获取途径说明。
        /// </summary>
        public string ObtainMethod { get; set; } = string.Empty;

        /// <summary>
        /// 模板自带技能列表。
        /// </summary>
        public List<PetSkillDto> Skills { get; set; } = [];
    }

    /// <summary>
    /// 喂养灵宠请求 DTO。
    /// </summary>
    public class PetFeedRequestDto
    {
        /// <summary>
        /// 灵宠实例 ID。
        /// </summary>
        public string PetId { get; set; } = string.Empty;

        /// <summary>
        /// 食物道具 ID。
        /// </summary>
        public string FoodItemId { get; set; } = string.Empty;

        /// <summary>
        /// 喂养数量。
        /// </summary>
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 灵宠进化请求 DTO。
    /// </summary>
    public class PetEvolveRequestDto
    {
        /// <summary>
        /// 灵宠实例 ID。
        /// </summary>
        public string PetId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 灵宠召唤请求 DTO。
    /// </summary>
    public class PetSummonRequestDto
    {
        /// <summary>
        /// 模板 ID。
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;
    }
}
