namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台聚灵阵列表项。
    /// </summary>
    public class AdminFiveElementListItemDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 聚灵阵主等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 金系分支等级。
        /// </summary>
        public int MetalLevel { get; set; }

        /// <summary>
        /// 木系分支等级。
        /// </summary>
        public int WoodLevel { get; set; }

        /// <summary>
        /// 水系分支等级。
        /// </summary>
        public int WaterLevel { get; set; }

        /// <summary>
        /// 火系分支等级。
        /// </summary>
        public int FireLevel { get; set; }

        /// <summary>
        /// 土系分支等级。
        /// </summary>
        public int EarthLevel { get; set; }

        /// <summary>
        /// 当前聚灵阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 五行等级上限，等于聚灵阵等级乘以 10，最大为 500。
        /// </summary>
        public int MaxElementLevel { get; set; }

        /// <summary>
        /// 五行当前实际属性加成。
        /// </summary>
        public List<FiveElementBonusDto> ElementBonuses { get; set; } = [];

        /// <summary>
        /// 灵田产量加成百分比。
        /// </summary>
        public int SpiritFieldYieldBonusPercent { get; set; }

        /// <summary>
        /// 战斗经验加成百分比。
        /// </summary>
        public int BattleExpBonusPercent { get; set; }
    }

    /// <summary>
    /// 后台聚灵阵详情。
    /// </summary>
    public class AdminFiveElementDetailDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 聚灵阵主等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 金系分支等级。
        /// </summary>
        public int MetalLevel { get; set; }

        /// <summary>
        /// 木系分支等级。
        /// </summary>
        public int WoodLevel { get; set; }

        /// <summary>
        /// 水系分支等级。
        /// </summary>
        public int WaterLevel { get; set; }

        /// <summary>
        /// 火系分支等级。
        /// </summary>
        public int FireLevel { get; set; }

        /// <summary>
        /// 土系分支等级。
        /// </summary>
        public int EarthLevel { get; set; }

        /// <summary>
        /// 金系经验。
        /// </summary>
        public long MetalExp { get; set; }

        /// <summary>
        /// 木系经验。
        /// </summary>
        public long WoodExp { get; set; }

        /// <summary>
        /// 水系经验。
        /// </summary>
        public long WaterExp { get; set; }

        /// <summary>
        /// 火系经验。
        /// </summary>
        public long FireExp { get; set; }

        /// <summary>
        /// 土系经验。
        /// </summary>
        public long EarthExp { get; set; }

        /// <summary>
        /// 当前聚灵阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 五行等级上限，等于聚灵阵等级乘以 10，最大为 500。
        /// </summary>
        public int MaxElementLevel { get; set; }

        /// <summary>
        /// 五行当前实际属性加成。
        /// </summary>
        public List<FiveElementBonusDto> ElementBonuses { get; set; } = [];

        /// <summary>
        /// 灵田产量加成百分比。
        /// </summary>
        public int SpiritFieldYieldBonusPercent { get; set; }

        /// <summary>
        /// 战斗经验加成百分比。
        /// </summary>
        public int BattleExpBonusPercent { get; set; }

        /// <summary>
        /// 当前激活的五行组合效果名称列表。
        /// </summary>
        public List<string> ActiveCombinations { get; set; } = [];
    }
}
