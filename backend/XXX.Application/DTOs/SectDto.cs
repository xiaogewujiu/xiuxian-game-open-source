namespace XXX.Application.DTOs
{
    #region Player-facing DTOs

    /// <summary>
    /// 宗门摘要信息。
    /// </summary>
    public class SectTemplateDto
    {
        /// <summary>
        /// 宗门ID。
        /// </summary>
        public string SectId { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 宗门描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 宗门图标路径。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 当前成员总数。
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// 当前玩家是否已加入该宗门。
        /// </summary>
        public bool IsJoined { get; set; }
    }

    /// <summary>
    /// 宗门详情DTO。
    /// </summary>
    public class SectDetailDto : SectTemplateDto
    {
        /// <summary>
        /// 心法列表。
        /// </summary>
        public List<HeartSutraDto> HeartSutras { get; set; } = [];

        /// <summary>
        /// 公会等级。
        /// </summary>
        public int GuildLevel { get; set; }

        /// <summary>
        /// 公会公告。
        /// </summary>
        public string? Announcement { get; set; }
    }

    /// <summary>
    /// 心法DTO。
    /// </summary>
    public class HeartSutraDto
    {
        /// <summary>
        /// 心法ID。
        /// </summary>
        public string SutraId { get; set; } = string.Empty;

        /// <summary>
        /// 心法名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 心法描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxLayer { get; set; }

        /// <summary>
        /// 层级列表。
        /// </summary>
        public List<SutraLayerDto> Layers { get; set; } = [];
    }

    /// <summary>
    /// 心法层级DTO。
    /// </summary>
    public class SutraLayerDto
    {
        /// <summary>
        /// 层级编号。
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// 层级名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 贡献消耗。
        /// </summary>
        public int ContributionCost { get; set; }

        /// <summary>
        /// 金币消耗。
        /// </summary>
        public long GoldCost { get; set; }

        /// <summary>
        /// 属性加成列表。
        /// </summary>
        public List<SutraAttributeBonusDto> Bonuses { get; set; } = [];

        /// <summary>
        /// 解锁技能ID。
        /// </summary>
        public string? UnlockSkillId { get; set; }

        /// <summary>
        /// 解锁技能名称。
        /// </summary>
        public string? UnlockSkillName { get; set; }

        /// <summary>
        /// 解锁BuffID。
        /// </summary>
        public string? UnlockBuffId { get; set; }

        /// <summary>
        /// 解锁Buff名称。
        /// </summary>
        public string? UnlockBuffName { get; set; }

        /// <summary>
        /// 当前玩家是否已解锁该层。
        /// </summary>
        public bool IsUnlocked { get; set; }
    }

    /// <summary>
    /// 心法属性加成DTO。
    /// </summary>
    public class SutraAttributeBonusDto
    {
        /// <summary>
        /// 属性名称。
        /// </summary>
        public string AttributeName { get; set; } = string.Empty;

        /// <summary>
        /// 属性值。
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 是否为百分比。
        /// </summary>
        public bool IsPercentage { get; set; }
    }

    /// <summary>
    /// 玩家心法进度DTO。
    /// </summary>
    public class PlayerSutraProgressDto
    {
        /// <summary>
        /// 心法ID。
        /// </summary>
        public string SutraId { get; set; } = string.Empty;

        /// <summary>
        /// 心法名称。
        /// </summary>
        public string SutraName { get; set; } = string.Empty;

        /// <summary>
        /// 当前层数。
        /// </summary>
        public int CurrentLayer { get; set; }

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxLayer { get; set; }
    }

    /// <summary>
    /// 心法升级结果DTO。
    /// </summary>
    public class HeartSutraUpgradeResultDto
    {
        /// <summary>
        /// 是否升级成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 新层数。
        /// </summary>
        public int NewLayer { get; set; }

        /// <summary>
        /// 提示消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 解锁的技能ID（如有）。
        /// </summary>
        public int? UnlockedSkillId { get; set; }

        /// <summary>
        /// 解锁的技能名称（如有）。
        /// </summary>
        public string? UnlockedSkillName { get; set; }
    }

    /// <summary>
    /// 捐献请求DTO。
    /// </summary>
    public class SectDonateRequestDto
    {
        /// <summary>
        /// 捐献金币数量（1000/5000/10000/50000）。
        /// </summary>
        public long GoldAmount { get; set; }
    }

    /// <summary>
    /// 捐献结果DTO。
    /// </summary>
    public class DonationResultDto
    {
        /// <summary>
        /// 是否捐献成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消耗金币。
        /// </summary>
        public long GoldSpent { get; set; }

        /// <summary>
        /// 获得贡献值。
        /// </summary>
        public int ContributionEarned { get; set; }

        /// <summary>
        /// 提示消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 捐献状态DTO。
    /// </summary>
    public class DonationStatusDto
    {
        /// <summary>
        /// 是否可以捐献。
        /// </summary>
        public bool CanDonate { get; set; }

        /// <summary>
        /// 捐献需要的金币数量。
        /// </summary>
        public long GoldAmount { get; set; }

        /// <summary>
        /// 捐献获得的贡献值。
        /// </summary>
        public int ContributionReward { get; set; }

        /// <summary>
        /// 今日是否已捐献。
        /// </summary>
        public bool AlreadyDonatedToday { get; set; }
    }

    /// <summary>
    /// 宗门任务DTO。
    /// </summary>
    public class SectTaskDto
    {
        /// <summary>
        /// 任务ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 任务名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 任务图标。
        /// </summary>
        public string Icon { get; set; } = "📜";

        /// <summary>
        /// 任务描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型文本。
        /// </summary>
        public string TypeText { get; set; } = "宗门日常";

        /// <summary>
        /// 当前进度。
        /// </summary>
        public int CurrentProgress { get; set; }

        /// <summary>
        /// 目标进度。
        /// </summary>
        public int TargetProgress { get; set; } = 1;

        /// <summary>
        /// 是否已完成（进度已满，可领取奖励）。
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 奖励是否已领取。
        /// </summary>
        public bool RewardClaimed { get; set; }

        /// <summary>
        /// 奖励贡献值。
        /// </summary>
        public int ContributionReward { get; set; }

        /// <summary>
        /// 奖励金币。
        /// </summary>
        public long GoldReward { get; set; }
    }

    /// <summary>
    /// 宗门弟子DTO。
    /// </summary>
    public class SectDiscipleDto
    {
        /// <summary>
        /// 玩家ID。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int PlayerLevel { get; set; }

        /// <summary>
        /// 职位。
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// 贡献值。
        /// </summary>
        public int Contribution { get; set; }

        /// <summary>
        /// 五行属性（英文枚举名，如 metal/wood/water）。
        /// </summary>
        public string Element { get; set; } = string.Empty;

        /// <summary>
        /// 五行属性中文名。
        /// </summary>
        public string ElementName { get; set; } = string.Empty;

        /// <summary>
        /// 职业ID（如 warrior/mage/body）。
        /// </summary>
        public string Profession { get; set; } = string.Empty;

        /// <summary>
        /// 职业中文名。
        /// </summary>
        public string ProfessionName { get; set; } = string.Empty;

        /// <summary>
        /// 总战斗次数。
        /// </summary>
        public int TotalBattles { get; set; }

        /// <summary>
        /// 胜利次数。
        /// </summary>
        public int WinBattles { get; set; }

        /// <summary>
        /// 是否是当前玩家自己。
        /// </summary>
        public bool IsSelf { get; set; }
    }

    /// <summary>
    /// 切磋结果DTO。
    /// </summary>
    public class SparResultDto
    {
        /// <summary>
        /// 获胜者ID。
        /// </summary>
        public string WinnerId { get; set; } = string.Empty;

        /// <summary>
        /// 获胜者名称。
        /// </summary>
        public string WinnerName { get; set; } = string.Empty;

        /// <summary>
        /// 战斗日志JSON。
        /// </summary>
        public string BattleLogJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// 宗门大比状态DTO。
    /// </summary>
    public class SectTournamentStatusDto
    {
        /// <summary>
        /// 大比ID。
        /// </summary>
        public string TournamentId { get; set; } = string.Empty;

        /// <summary>
        /// 大比状态。
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 开始时间。
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间。
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 我的排名。
        /// </summary>
        public int MyRank { get; set; }

        /// <summary>
        /// 参赛选手列表。
        /// </summary>
        public List<string> Participants { get; set; } = [];
    }

    /// <summary>
    /// 宗门大比对战记录DTO。
    /// </summary>
    public class SectTournamentMatchDto
    {
        /// <summary>
        /// 对战ID。
        /// </summary>
        public string MatchId { get; set; } = string.Empty;

        /// <summary>
        /// 选手1名称。
        /// </summary>
        public string Player1Name { get; set; } = string.Empty;

        /// <summary>
        /// 选手2名称。
        /// </summary>
        public string Player2Name { get; set; } = string.Empty;

        /// <summary>
        /// 获胜者名称。
        /// </summary>
        public string? WinnerName { get; set; }

        /// <summary>
        /// 轮次。
        /// </summary>
        public int Round { get; set; }

        /// <summary>
        /// 战斗日志JSON。
        /// </summary>
        public string? BattleLogJson { get; set; }
    }

    /// <summary>
    /// 宗门大比奖励DTO。
    /// </summary>
    public class SectTournamentRewardDto
    {
        /// <summary>
        /// 排名。
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 奖励称号。
        /// </summary>
        public string? RewardTitle { get; set; }

        /// <summary>
        /// 贡献值奖励。
        /// </summary>
        public long Contribution { get; set; }

        /// <summary>
        /// 金币奖励。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 灵石奖励。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 是否已领取。
        /// </summary>
        public bool Claimed { get; set; }
    }

    /// <summary>
    /// 天骄赛状态DTO。
    /// </summary>
    public class GeniusTournamentStatusDto
    {
        /// <summary>
        /// 大比ID。
        /// </summary>
        public string TournamentId { get; set; } = string.Empty;

        /// <summary>
        /// 赛季。
        /// </summary>
        public int Season { get; set; }

        /// <summary>
        /// 状态。
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 开始时间。
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间。
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 参赛选手列表。
        /// </summary>
        public List<string> Participants { get; set; } = [];
    }

    /// <summary>
    /// 天骄赛对战记录DTO。
    /// </summary>
    public class GeniusTournamentMatchDto
    {
        /// <summary>
        /// 对战ID。
        /// </summary>
        public string MatchId { get; set; } = string.Empty;

        /// <summary>
        /// 选手1名称。
        /// </summary>
        public string Player1Name { get; set; } = string.Empty;

        /// <summary>
        /// 选手1宗门名称。
        /// </summary>
        public string Player1SectName { get; set; } = string.Empty;

        /// <summary>
        /// 选手2名称。
        /// </summary>
        public string Player2Name { get; set; } = string.Empty;

        /// <summary>
        /// 选手2宗门名称。
        /// </summary>
        public string Player2SectName { get; set; } = string.Empty;

        /// <summary>
        /// 获胜者名称。
        /// </summary>
        public string? WinnerName { get; set; }

        /// <summary>
        /// 轮次。
        /// </summary>
        public int Round { get; set; }
    }

    /// <summary>
    /// 天骄赛奖励DTO。
    /// </summary>
    public class GeniusTournamentRewardDto
    {
        /// <summary>
        /// 排名。
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 奖励称号。
        /// </summary>
        public string? RewardTitle { get; set; }

        /// <summary>
        /// 贡献值奖励。
        /// </summary>
        public long Contribution { get; set; }

        /// <summary>
        /// 金币奖励。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 灵石奖励。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 是否已领取。
        /// </summary>
        public bool Claimed { get; set; }
    }

    /// <summary>
    /// 宗门Boss状态DTO。
    /// </summary>
    public class SectBossStatusDto
    {
        /// <summary>
        /// Boss名称。
        /// </summary>
        public string BossName { get; set; } = string.Empty;

        /// <summary>
        /// 当前血量。
        /// </summary>
        public int CurrentHp { get; set; }

        /// <summary>
        /// 最大血量。
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// 结束时间。
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 我的总伤害。
        /// </summary>
        public long MyDamage { get; set; }

        /// <summary>
        /// 我的排名。
        /// </summary>
        public int MyRank { get; set; }

        /// <summary>
        /// 是否可以领取奖励。
        /// </summary>
        public bool CanClaim { get; set; }
    }

    /// <summary>
    /// 宗门Boss攻击结果DTO。
    /// </summary>
    public class SectBossActionResultDto
    {
        /// <summary>
        /// 造成的伤害。
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// 是否暴击。
        /// </summary>
        public bool IsCrit { get; set; }

        /// <summary>
        /// Boss当前血量。
        /// </summary>
        public int BossCurrentHp { get; set; }

        /// <summary>
        /// Boss是否已被击败。
        /// </summary>
        public bool BossDefeated { get; set; }
    }

    /// <summary>
    /// 宗门Boss奖励DTO。
    /// </summary>
    public class SectBossRewardDto
    {
        /// <summary>
        /// 贡献值奖励。
        /// </summary>
        public long Contribution { get; set; }

        /// <summary>
        /// 金币奖励。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 排名。
        /// </summary>
        public int Rank { get; set; }
    }

    /// <summary>
    /// 宗门商店商品DTO。
    /// </summary>
    public class SectShopItemDto
    {
        /// <summary>
        /// 商品ID。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 物品ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品名称。
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// 物品类型。
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// 贡献值消耗。
        /// </summary>
        public int ContributionCost { get; set; }

        /// <summary>
        /// 库存数量（-1表示无限）。
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 每日限购数量（-1表示无限）。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 今日已购买数量。
        /// </summary>
        public int PurchasedToday { get; set; }
    }

    /// <summary>
    /// 宗门商店购买结果DTO。
    /// </summary>
    public class SectShopPurchaseResultDto
    {
        /// <summary>
        /// 是否购买成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 提示消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 宗门福利DTO。
    /// </summary>
    public class SectBlessingDto
    {
        /// <summary>
        /// 福利ID。
        /// </summary>
        public string BlessingId { get; set; } = string.Empty;

        /// <summary>
        /// 福利名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 福利描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 需要的公会等级。
        /// </summary>
        public int RequiredGuildLevel { get; set; }

        /// <summary>
        /// 是否已激活。
        /// </summary>
        public bool IsActive { get; set; }
    }

    #endregion

    #region Admin DTOs

    /// <summary>
    /// 后台宗门模板列表项DTO。
    /// </summary>
    public class AdminSectTemplateListItemDto
    {
        /// <summary>
        /// 宗门ID。
        /// </summary>
        public string SectId { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 成员总数。
        /// </summary>
        public int MemberCount { get; set; }
    }

    /// <summary>
    /// 后台宗门模板详情DTO。
    /// </summary>
    public class AdminSectTemplateDetailDto
    {
        /// <summary>
        /// 宗门ID。
        /// </summary>
        public string SectId { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 宗门描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 宗门图标。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 宗门立绘路径。
        /// </summary>
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 心法ID列表JSON。
        /// </summary>
        public string? HeartSutraIdsJson { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 后台心法模板列表项DTO。
    /// </summary>
    public class AdminHeartSutraListItemDto
    {
        /// <summary>
        /// 心法ID。
        /// </summary>
        public string SutraId { get; set; } = string.Empty;

        /// <summary>
        /// 心法名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所属宗门ID。
        /// </summary>
        public string SectId { get; set; } = string.Empty;

        /// <summary>
        /// 所属宗门名称。
        /// </summary>
        public string SectName { get; set; } = string.Empty;

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxLayer { get; set; }
    }

    /// <summary>
    /// 后台心法模板详情DTO。
    /// </summary>
    public class AdminHeartSutraDetailDto
    {
        /// <summary>
        /// 心法ID。
        /// </summary>
        public string SutraId { get; set; } = string.Empty;

        /// <summary>
        /// 心法名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 心法描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 所属宗门ID。
        /// </summary>
        public string SectId { get; set; } = string.Empty;

        /// <summary>
        /// 最大层数。
        /// </summary>
        public int MaxLayer { get; set; }

        /// <summary>
        /// 层级配置JSON。
        /// </summary>
        public string LayersJson { get; set; } = "[]";

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 后台宗门Boss模板列表项DTO。
    /// </summary>
    public class AdminSectBossTemplateListItemDto
    {
        /// <summary>
        /// Boss ID。
        /// </summary>
        public string BossId { get; set; } = string.Empty;

        /// <summary>
        /// Boss名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 怪物模板ID。
        /// </summary>
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 后台宗门Boss模板详情DTO。
    /// </summary>
    public class AdminSectBossTemplateDetailDto
    {
        /// <summary>
        /// Boss ID。
        /// </summary>
        public string BossId { get; set; } = string.Empty;

        /// <summary>
        /// Boss名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 怪物模板ID。
        /// </summary>
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 立绘路径。
        /// </summary>
        public string? PortraitPath { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 持续时间（分钟）。
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// 参与奖励贡献值。
        /// </summary>
        public long ParticipationRewardContribution { get; set; }

        /// <summary>
        /// 第1名奖励贡献值。
        /// </summary>
        public long Rank1RewardContribution { get; set; }

        /// <summary>
        /// 第2名奖励贡献值。
        /// </summary>
        public long Rank2RewardContribution { get; set; }

        /// <summary>
        /// 第3名奖励贡献值。
        /// </summary>
        public long Rank3RewardContribution { get; set; }

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 后台宗门大比排期DTO。
    /// </summary>
    public class AdminSectTournamentScheduleDto
    {
        /// <summary>
        /// 排期ID。
        /// </summary>
        public string ScheduleId { get; set; } = "default";

        /// <summary>
        /// 生成时间文本。
        /// </summary>
        public string SpawnTimeText { get; set; } = "20:00";

        /// <summary>
        /// 时区ID。
        /// </summary>
        public string TimeZoneId { get; set; } = "China Standard Time";

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 持续时间（分钟）。
        /// </summary>
        public int DurationMinutes { get; set; }
    }

    /// <summary>
    /// 后台宗门商店商品列表项DTO。
    /// </summary>
    public class AdminSectShopItemListItemDto
    {
        /// <summary>
        /// 商品ID。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 商店ID。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 物品ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品类型。
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// 贡献值消耗。
        /// </summary>
        public int ContributionCost { get; set; }
    }

    /// <summary>
    /// 后台宗门商店商品详情DTO。
    /// </summary>
    public class AdminSectShopItemDetailDto
    {
        /// <summary>
        /// 商品ID。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 商店ID。
        /// </summary>
        public string ShopId { get; set; } = string.Empty;

        /// <summary>
        /// 物品ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品类型。
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// 贡献值消耗。
        /// </summary>
        public int ContributionCost { get; set; }

        /// <summary>
        /// 库存数量（-1表示无限）。
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 每日限购数量（-1表示无限）。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 后台宗门福利列表项DTO。
    /// </summary>
    public class AdminSectBlessingListItemDto
    {
        /// <summary>
        /// 福利ID。
        /// </summary>
        public string BlessingId { get; set; } = string.Empty;

        /// <summary>
        /// 福利名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 需要的公会等级。
        /// </summary>
        public int RequiredGuildLevel { get; set; }

        /// <summary>
        /// Buff ID。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 后台宗门福利详情DTO。
    /// </summary>
    public class AdminSectBlessingDetailDto
    {
        /// <summary>
        /// 福利ID。
        /// </summary>
        public string BlessingId { get; set; } = string.Empty;

        /// <summary>
        /// 福利名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 福利描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 需要的公会等级。
        /// </summary>
        public int RequiredGuildLevel { get; set; }

        /// <summary>
        /// Buff ID。
        /// </summary>
        public string BuffId { get; set; } = string.Empty;

        /// <summary>
        /// 排序。
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 后台公会列表项DTO。
    /// </summary>
    public class AdminGuildListItemDto
    {
        /// <summary>
        /// 公会ID。
        /// </summary>
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// 公会名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string SectName { get; set; } = string.Empty;

        /// <summary>
        /// 公会等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 成员数量。
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// 会长名称。
        /// </summary>
        public string LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// 累计捐献。
        /// </summary>
        public int TotalDonation { get; set; }
    }

    /// <summary>
    /// 后台公会详情DTO。
    /// </summary>
    public class AdminGuildDetailDto
    {
        /// <summary>
        /// 公会ID。
        /// </summary>
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// 公会名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 宗门模板ID。
        /// </summary>
        public string? SectTemplateId { get; set; }

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string SectName { get; set; } = string.Empty;

        /// <summary>
        /// 公会等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 公会经验。
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 成员数量。
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// 最大成员数。
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// 公会资金。
        /// </summary>
        public long Funds { get; set; }

        /// <summary>
        /// 会长ID。
        /// </summary>
        public string LeaderId { get; set; } = string.Empty;

        /// <summary>
        /// 会长名称。
        /// </summary>
        public string LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// 累计捐献。
        /// </summary>
        public int TotalDonation { get; set; }

        /// <summary>
        /// 公会公告。
        /// </summary>
        public string? Announcement { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    #endregion
}
