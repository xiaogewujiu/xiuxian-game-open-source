using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// UserEntity 扩展方法类
    /// 为现有的 UserEntity 类添加便利方法
    /// </summary>
    public static class PlayerExtensions
    {
        #region 货币属性扩展

        /// <summary>
        /// 获取或设置金币
        /// 扩展属性，存储在实体中
        /// </summary>
        public static long Gold { get; set; }

        /// <summary>
        /// 获取或设置荣誉点
        /// </summary>
        public static int Honor { get; set; }

        /// <summary>
        /// 获取或设置公会贡献
        /// </summary>
        public static int GuildContribution { get; set; }

        /// <summary>
        /// 获取或设置创建时间
        /// </summary>
        public static DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 获取或设置最后登录时间
        /// </summary>
        public static DateTime LastLoginTime { get; set; } = DateTime.Now;

        #endregion

        #region 统计属性扩展

        /// <summary>
        /// 总战斗场次
        /// </summary>
        public static int TotalBattles { get; set; }

        /// <summary>
        /// 胜利场次
        /// </summary>
        public static int WinBattles { get; set; }

        /// <summary>
        /// 总击杀数
        /// </summary>
        public static int TotalKills { get; set; }

        /// <summary>
        /// 最大连击数
        /// </summary>
        public static int MaxCombo { get; set; }

        /// <summary>
        /// 累计获得金币
        /// </summary>
        public static long TotalGoldEarned { get; set; }

        /// <summary>
        /// 累计消耗金币
        /// </summary>
        public static long TotalGoldSpent { get; set; }

        /// <summary>
        /// 累计获得经验
        /// </summary>
        public static long TotalExpEarned { get; set; }

        #endregion

        #region 便利属性

        /// <summary>
        /// 获取当前等级（整型）
        /// </summary>
        public static int GetIntLevel(this UserEntity player)
        {
            return player.Level;
        }

        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        public static long GetMaxExp(this UserEntity player)
        {
            return PlayerManager.GetRequiredExp(player);
        }

        /// <summary>
        /// 获取经验百分比（0-100）
        /// </summary>
        public static float GetExpPercent(this UserEntity player)
        {
            long maxExp = player.GetMaxExp();
            if (maxExp == 0) return 100f;
            return (float)player.Exp / maxExp * 100f;
        }

        /// <summary>
        /// 获取当前等级对应的境界别名。
        /// </summary>
        public static string GetLevelTitle(this UserEntity player)
        {
            int level = player.GetIntLevel();
            return RealmLevelCatalog.Get(level).Alias;
        }

        /// <summary>
        /// 获取胜率（0-100）
        /// </summary>
        public static float GetWinRate(this UserEntity player)
        {
            if (player.TotalBattles == 0) return 0f;
            return (float)player.WinBattles / player.TotalBattles * 100f;
        }

        /// <summary>
        /// 检查是否可以升级
        /// </summary>
        public static bool CanLevelUp(this UserEntity player)
        {
            int level = player.GetIntLevel();
            return LevelConfig.CanLevelUp(level);
        }

        /// <summary>
        /// 获取玩家信息摘要
        /// </summary>
        public static string GetPlayerSummary(this UserEntity player)
        {
            return $"[{player.GetLevelTitle()}] {player.Name} Lv.{player.Level}";
        }

        /// <summary>
        /// 格式化金币显示
        /// </summary>
        public static string FormatGold(this UserEntity player)
        {
            return FormatCurrency(player.Gold);
        }

        /// <summary>
        /// 格式化灵石显示
        /// </summary>
        public static string FormatSpiritStone(this UserEntity player)
        {
            return FormatCurrency(player.SpiritStone);
        }

        /// <summary>
        /// 格式化货币数值
        /// 大于10000显示为"万"
        /// </summary>
        /// <param name="amount">货币数量</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatCurrency(long amount)
        {
            if (amount >= 10000)
            {
                return $"{amount / 10000f:F1}万";
            }
            return amount.ToString();
        }

        #endregion

        #region 数据验证

        /// <summary>
        /// 检查玩家数据是否有效
        /// </summary>
        public static bool IsValid(this UserEntity player)
        {
            if (player == null) return false;
            if (string.IsNullOrEmpty(player.GID)) return false;
            if (string.IsNullOrEmpty(player.Name)) return false;
            if (player.GetIntLevel() < LevelConfig.StartLevel) return false;
            if (player.Exp < 0) return false;
            if (player.Gold < 0) return false;
            return true;
        }

        /// <summary>
        /// 获取玩家数据详细报告
        /// </summary>
        public static string GetDataReport(this UserEntity player)
        {
            if (!player.IsValid())
            {
                return "玩家数据无效";
            }

            int level = player.GetIntLevel();
            float expPercent = player.GetExpPercent();
            float winRate = player.GetWinRate();

            return $"""
            ==================== 玩家数据报告 ====================
            基本信息：
              名称：{player.Name}
              等级：{level} ({player.GetLevelTitle()})
              经验：{player.Exp}/{player.GetMaxExp()} ({expPercent:F1}%)
              创建时间：{player.CreateTime:yyyy-MM-dd HH:mm:ss}
              最后登录：{player.LastLoginTime:yyyy-MM-dd HH:mm:ss}

            货币：
              金币：{player.FormatGold()} (累计获得：{FormatCurrency(player.TotalGoldEarned)})
              灵石：{player.FormatSpiritStone()}

            战斗统计：
              总场次：{player.TotalBattles}
              胜利：{player.WinBattles} (胜率：{winRate:F1}%)
              击杀：{player.TotalKills}
              最大连击：{player.MaxCombo}

            基础属性：
              血量：{player.Type1}
              蓝量：{player.Type2}
              物攻：{player.Type3}
              法攻：{player.Type4}
              物防：{player.Type5}
              法防：{player.Type6}
              速度：{player.Type7}

            高级属性：
              命中：{player.Type8 * 100:F1}%
              闪避：{player.Type9 * 100:F1}%
              暴击：{player.Type10 * 100:F1}%
              暴击伤害：{player.Type11 * 100:F1}%
              连击：{player.Type12 * 100:F1}%
              反击：{player.Type13 * 100:F1}%
              破甲：{player.Type14 * 100:F1}%
              额外伤害：{player.Type15 * 100:F1}%
            ==================== 报告结束 ====================
            """;
        }

        #endregion

        #region 快捷操作

        /// <summary>
        /// 快捷添加金币
        /// </summary>
        public static bool AddGoldQuick(this UserEntity player, long amount)
        {
            return PlayerManager.AddGold(player, amount, "快捷操作");
        }

        /// <summary>
        /// 快捷添加经验
        /// </summary>
        public static LevelUpResult AddExpQuick(this UserEntity player, long exp)
        {
            return PlayerManager.AddExp(player, exp);
        }

        /// <summary>
        /// 快捷检查金币
        /// </summary>
        public static bool HasEnoughGoldQuick(this UserEntity player, long amount)
        {
            return PlayerManager.HasEnoughGold(player, amount);
        }

        /// <summary>
        /// 快捷消耗金币
        /// </summary>
        public static bool ConsumeGoldQuick(this UserEntity player, long amount)
        {
            return PlayerManager.ConsumeGold(player, amount, "快捷操作");
        }

        #endregion
    }

    // 扩展 UserEntity 类，添加新字段
    // 注意：由于C#的扩展方法不能添加字段，这里使用扩展属性
    // 实际使用时需要修改 UserEntity 类添加这些字段

    /*
    // 在 Entity\Entities.cs 的 UserEntity 类中添加以下字段：

    // ==================== 货币字段 ====================
    public long Gold { get; set; } = 1000;
    public long SpiritStone { get; set; } = 0;
    public int Honor { get; set; } = 0;
    public int GuildContribution { get; set; } = 0;

    // ==================== 时间字段 ====================
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime LastLoginTime { get; set; } = DateTime.Now;

    // ==================== 统计字段 ====================
    public int TotalBattles { get; set; } = 0;
    public int WinBattles { get; set; } = 0;
    public int TotalKills { get; set; } = 0;
    public int MaxCombo { get; set; } = 0;
    public long TotalGoldEarned { get; set; } = 0;
    public long TotalGoldSpent { get; set; } = 0;
    public long TotalExpEarned { get; set; } = 0;
    */
}
