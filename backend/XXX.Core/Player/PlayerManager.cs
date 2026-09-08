using XXX.Entity;

namespace XXX.Player
{
    /// <summary>
    /// 玩家管理器
    /// 负责玩家数据的创建、经验管理、货币操作等核心功能
    ///
    /// 功能说明：
    /// 1. 玩家创建与初始化
    /// 2. 经验添加与升级处理
    /// 3. 货币获取与消耗
    /// 4. 属性计算与管理
    /// 5. 数据验证与边界检查
    /// </summary>
    public static class PlayerManager
    {
        /// <summary>
        /// 货币变更记录列表
        /// 用于记录所有货币变动历史（可选功能）
        /// </summary>
        private static List<CurrencyChangeRecord> currencyRecords = [];

        #region 玩家创建

        /// <summary>
        /// 创建新玩家
        /// 根据默认配置初始化一个新玩家
        /// </summary>
        /// <param name="playerId">玩家唯一ID</param>
        /// <param name="playerName">玩家名称</param>
        /// <returns>新创建的玩家实体</returns>
        public static UserEntity CreateNewPlayer(string playerId, string playerName, string? profession = null)
        {
            int level = PlayerInitialResourceConfig.StartLevel;

            var player = new UserEntity
            {
                GID = playerId,
                Name = playerName,
                Profession = PlayerProfessionCatalog.Normalize(profession),
                Level = level,
                // 中文注释：
                // 新角色创建时直接分配一个基础灵根，避免玩家登录后主界面灵根区域长期显示为空。
                // 这里使用玩家 ID 的哈希做稳定分配，同一个角色不会因为重启服务而反复变化。
                Element = GenerateInitialElement(playerId),
                Exp = PlayerInitialResourceConfig.StartExp,
                XExp = RealmLevelCatalog.GetRequiredExp(level),
                Gold = PlayerInitialResourceConfig.StartGold,
                SpiritStone = PlayerInitialResourceConfig.StartSpiritStone,
                Exp2 = 0,
                XExp2 = 0,
                CreateTime = DateTime.Now,
                LastLoginTime = DateTime.Now,
                TotalBattles = 0,
                WinBattles = 0,
                TotalKills = 0,
                MaxCombo = 0,
                TotalGoldEarned = PlayerInitialResourceConfig.StartGold,
                TotalGoldSpent = 0,
                TotalExpEarned = PlayerInitialResourceConfig.StartExp
            };

            // 初始化基础属性
            ApplyBaseAttributes(player, level);

            return player;
        }

        /// <summary>
        /// 中文注释：
        /// 根据玩家 ID 稳定生成初始灵根。
        /// 之所以不用纯随机，是为了避免测试环境下重复建号、删库重建后灵根来回波动，影响联调判断。
        /// </summary>
        private static Element GenerateInitialElement(string playerId)
        {
            var availableElements = new[]
            {
                Element.Metal,
                Element.Wood,
                Element.Water,
                Element.Fire,
                Element.Earth
            };

            var index = Math.Abs((playerId ?? string.Empty).GetHashCode()) % availableElements.Length;
            return availableElements[index];
        }

        /// <summary>
        /// 初始化玩家基础属性
        /// 根据等级设置玩家的基础属性值
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="level">等级</param>
        /// <summary>
        /// 中文注释：
        /// 按指定等级重建角色的基础属性底板。
        /// 这个方法除了新建角色外，还会被“属性重算”流程复用，
        /// 目的是保证人物在装备变化、洗练变化、属性加点变化后，
        /// 都能先回到当前等级应有的基础值，再叠加额外来源，避免越算越偏。
        /// </summary>
        public static void ApplyBaseAttributes(UserEntity player, int level)
        {
            // 设置基础属性
            player.Type1 = LevelConfig.GetBaseHp(level);           // 最大血量
            player.Type2 = LevelConfig.GetBaseMp(level);           // 最大蓝量
            player.Type3 = LevelConfig.GetBasePhysicalAttack(level); // 物理攻击
            player.Type4 = LevelConfig.GetBaseMagicAttack(level);    // 法术攻击
            player.Type5 = LevelConfig.GetBasePhysicalDefense(level); // 物理防御
            player.Type6 = LevelConfig.GetBaseMagicDefense(level);   // 法术防御
            player.Type7 = LevelConfig.GetBaseSpeed(level);          // 速度

            // 设置高级属性（默认值）
            player.Type8 = 0.96f;   // 命中率
            player.Type9 = 0.03f;   // 闪避率
            player.Type10 = 0.04f;  // 暴击率
            player.Type11 = 1.55f;  // 暴击伤害
            player.Type12 = 0.01f;  // 连击率
            player.Type13 = 0.01f;  // 反击率
            player.Type14 = 0.0f;   // 破甲率
            player.Type15 = 0.0f;   // 额外伤害
        }

        #endregion

        #region 经验与升级

        /// <summary>
        /// 添加经验
        /// 自动处理升级逻辑，支持连续升级
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="exp">要添加的经验值</param>
        /// <returns>升级结果</returns>
        public static LevelUpResult AddExp(UserEntity player, long exp)
        {
            if (player == null)
            {
                return new LevelUpResult
                {
                    Success = false,
                    Message = "玩家对象为空"
                };
            }

            if (exp <= 0)
            {
                return new LevelUpResult
                {
                    Success = false,
                    Message = "经验值必须大于0"
                };
            }

            int currentLevel = GetLevel(player);
            long currentExp = player.Exp;

            // 已达最高等级
            if (currentLevel >= LevelConfig.MaxLevel)
            {
                return new LevelUpResult
                {
                    Success = true,
                    LeveledUp = false,
                    Message = "已达到最高等级",
                    OldLevel = currentLevel,
                    NewLevel = currentLevel,
                    ExpAdded = 0,
                    RemainingExp = currentExp
                };
            }

            // 记录原始状态
            int oldLevel = currentLevel;
            int levelsGained = 0;
            long actualAddedExp = 0;
            long remainingToAdd = exp;
            long tempExp = currentExp;
            var isBlockedByBreakthrough = false;

            while (remainingToAdd > 0 && currentLevel < LevelConfig.MaxLevel)
            {
                var currentRealm = RealmLevelCatalog.Get(currentLevel);
                var requiredExp = RealmLevelCatalog.GetRequiredExp(currentLevel);

                if (requiredExp <= 0)
                {
                    break;
                }

                if (currentRealm.IsBreakthroughPoint)
                {
                    var missingToCap = Math.Max(0, requiredExp - tempExp);
                    var gainedAtCap = Math.Min(remainingToAdd, missingToCap);
                    tempExp += gainedAtCap;
                    remainingToAdd -= gainedAtCap;
                    actualAddedExp += gainedAtCap;
                    isBlockedByBreakthrough = tempExp >= requiredExp;
                    break;
                }

                var totalExp = tempExp + remainingToAdd;
                if (totalExp < requiredExp)
                {
                    tempExp = totalExp;
                    actualAddedExp += remainingToAdd;
                    remainingToAdd = 0;
                    break;
                }

                var consumedExp = requiredExp - tempExp;
                remainingToAdd -= consumedExp;
                actualAddedExp += consumedExp;
                tempExp = 0;
                currentLevel++;
                levelsGained++;
            }

            player.Exp = tempExp;
            SetLevel(player, currentLevel);
            player.XExp = RealmLevelCatalog.GetRequiredExp(currentLevel);
            player.TotalExpEarned += actualAddedExp;

            if (levelsGained > 0)
            {
                // 应用等级加成到属性
                ApplyLevelBonus(player, currentLevel);
            }

            var nextAlias = RealmLevelCatalog.GetNext(currentLevel)?.Alias ?? "更高境界";
            var currentAlias = RealmLevelCatalog.Get(currentLevel).Alias;

            return new LevelUpResult
            {
                Success = true,
                LeveledUp = levelsGained > 0,
                OldLevel = oldLevel,
                NewLevel = currentLevel,
                LevelsGained = levelsGained,
                ExpAdded = actualAddedExp,
                RemainingExp = player.Exp,
                IsBreakthroughBlocked = isBlockedByBreakthrough,
                CurrentRealmAlias = currentAlias,
                NextRealmAlias = nextAlias,
                Message = isBlockedByBreakthrough
                    ? $"已达到 {currentAlias} 圆满，经验已满，请尝试突破至 {nextAlias}。"
                    : levelsGained > 0
                        ? $"恭喜！连升{levelsGained}级！当前等级：{currentLevel}"
                        : $"获得{actualAddedExp}点经验"
            };
        }

        /// <summary>
        /// 设置玩家等级
        /// 安全地设置玩家等级，包含验证逻辑
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="level">等级</param>
        /// <returns>是否设置成功</returns>
        public static bool SetLevel(UserEntity player, int level)
        {
            if (!LevelConfig.IsValidLevel(level))
            {
                return false;
            }

            player.Level = level;
            return true;
        }

        /// <summary>
        /// 获取玩家等级
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>等级</returns>
        public static int GetLevel(UserEntity player)
        {
            return player.Level;
        }

        /// <summary>
        /// 应用等级加成到属性
        /// 升级时自动更新玩家属性
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="newLevel">新等级</param>
        private static void ApplyLevelBonus(UserEntity player, int newLevel)
        {
            // 中文注释：
            // 等级成长底板已经统一封装在 LevelConfig 中。
            // 升级时直接按新等级重建一遍基础底板，避免历史累计加法造成双重叠加。
            ApplyBaseAttributes(player, newLevel);
        }

        /// <summary>
        /// 检查是否可以升级
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>是否可以升级</returns>
        public static bool CanLevelUp(UserEntity player)
        {
            int level = GetLevel(player);
            return LevelConfig.CanLevelUp(level);
        }

        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>升级所需经验</returns>
        public static long GetRequiredExp(UserEntity player)
        {
            int level = GetLevel(player);
            return RealmLevelCatalog.GetRequiredExp(level);
        }

        /// <summary>
        /// 获取经验百分比
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>经验百分比（0-100）</returns>
        public static float GetExpPercent(UserEntity player)
        {
            long currentExp = player.Exp;
            long requiredExp = GetRequiredExp(player);

            if (requiredExp == 0)
            {
                return 100f;
            }

            return (float)currentExp / requiredExp * 100f;
        }

        #endregion

        #region 金币管理

        /// <summary>
        /// 添加金币
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">金币数量</param>
        /// <param name="reason">原因说明</param>
        /// <returns>是否成功</returns>
        public static bool AddGold(UserEntity player, long amount, string reason = "系统赠送")
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            // 检查是否会溢出
            if (player.Gold + amount < 0)
            {
                return false;
            }

            long beforeAmount = player.Gold;
            player.Gold += amount;
            player.TotalGoldEarned += amount;

            // 记录货币变更
            RecordCurrencyChange(player, CurrencyType.Gold, amount, beforeAmount, player.Gold,
                CurrencyOperationType.Other, reason);

            return true;
        }

        /// <summary>
        /// 消耗金币
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">金币数量</param>
        /// <param name="reason">原因说明</param>
        /// <returns>是否成功</returns>
        public static bool ConsumeGold(UserEntity player, long amount, string reason = "购买")
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            // 检查金币是否足够
            if (player.Gold < amount)
            {
                return false;
            }

            long beforeAmount = player.Gold;
            player.Gold -= amount;
            player.TotalGoldSpent += amount;

            // 记录货币变更（消耗用负数表示）
            RecordCurrencyChange(player, CurrencyType.Gold, -amount, beforeAmount, player.Gold,
                CurrencyOperationType.Other, reason);

            return true;
        }

        /// <summary>
        /// 检查金币是否足够
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">所需金币数量</param>
        /// <returns>是否足够</returns>
        public static bool HasEnoughGold(UserEntity player, long amount)
        {
            if (player == null || amount < 0)
            {
                return false;
            }

            return player.Gold >= amount;
        }

        #endregion

        #region 灵石管理

        /// <summary>
        /// 添加灵石
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">灵石数量</param>
        /// <param name="reason">原因说明</param>
        /// <returns>是否成功</returns>
        public static bool AddSpiritStone(UserEntity player, long amount, string reason = "系统赠送")
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            if (player.SpiritStone + amount < 0)
            {
                return false;
            }

            long beforeAmount = player.SpiritStone;
            player.SpiritStone += amount;

            RecordCurrencyChange(player, CurrencyType.SpiritStone, amount, beforeAmount, player.SpiritStone,
                CurrencyOperationType.Other, reason);

            return true;
        }

        /// <summary>
        /// 消耗灵石
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">灵石数量</param>
        /// <param name="reason">原因说明</param>
        /// <returns>是否成功</returns>
        public static bool ConsumeSpiritStone(UserEntity player, long amount, string reason = "购买")
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            if (player.SpiritStone < amount)
            {
                return false;
            }

            long beforeAmount = player.SpiritStone;
            player.SpiritStone -= amount;

            RecordCurrencyChange(player, CurrencyType.SpiritStone, -amount, beforeAmount, player.SpiritStone,
                CurrencyOperationType.Other, reason);

            return true;
        }

        /// <summary>
        /// 检查灵石是否足够
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">所需灵石数量</param>
        /// <returns>是否足够</returns>
        public static bool HasEnoughSpiritStone(UserEntity player, long amount)
        {
            if (player == null || amount < 0)
            {
                return false;
            }

            return player.SpiritStone >= amount;
        }

        #endregion

        #region 其他货币管理

        /// <summary>
        /// 添加荣誉点
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">荣誉点数量</param>
        /// <returns>是否成功</returns>
        public static bool AddHonor(UserEntity player, int amount)
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            player.Honor += amount;
            return true;
        }

        /// <summary>
        /// 添加公会贡献
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="amount">公会贡献数量</param>
        /// <returns>是否成功</returns>
        public static bool AddGuildContribution(UserEntity player, int amount)
        {
            if (player == null || amount <= 0)
            {
                return false;
            }

            player.GuildContribution += amount;
            return true;
        }

        #endregion

        #region 统计数据

        /// <summary>
        /// 更新登录时间
        /// </summary>
        /// <param name="player">玩家实体</param>
        public static void UpdateLoginTime(UserEntity player)
        {
            if (player != null)
            {
                player.LastLoginTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 增加战斗场次
        /// </summary>
        /// <param name="player">玩家实体</param>
        public static void IncrementBattleCount(UserEntity player)
        {
            if (player != null)
            {
                player.TotalBattles++;
            }
        }

        /// <summary>
        /// 增加胜利场次
        /// </summary>
        /// <param name="player">玩家实体</param>
        public static void IncrementWinCount(UserEntity player)
        {
            if (player != null)
            {
                player.WinBattles++;
            }
        }

        /// <summary>
        /// 增加击杀数
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="count">击杀数量</param>
        public static void AddKillCount(UserEntity player, int count)
        {
            if (player != null && count > 0)
            {
                player.TotalKills += count;
            }
        }

        /// <summary>
        /// 更新最大连击数
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="combo">连击数</param>
        public static void UpdateMaxCombo(UserEntity player, int combo)
        {
            if (player != null && combo > player.MaxCombo)
            {
                player.MaxCombo = combo;
            }
        }

        /// <summary>
        /// 计算胜率
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>胜率（0-100）</returns>
        public static float GetWinRate(UserEntity player)
        {
            if (player == null || player.TotalBattles == 0)
            {
                return 0f;
            }

            return (float)player.WinBattles / player.TotalBattles * 100f;
        }

        #endregion

        #region 货币记录

        /// <summary>
        /// 记录货币变更
        /// </summary>
        private static void RecordCurrencyChange(UserEntity player, CurrencyType currencyType,
            long amount, long beforeAmount, long afterAmount, CurrencyOperationType operationType, string reason)
        {
            var record = new CurrencyChangeRecord
            {
                RecordId = Guid.NewGuid().ToString(),
                PlayerId = player.GID,
                CurrencyType = currencyType,
                Amount = amount,
                BeforeAmount = beforeAmount,
                AfterAmount = afterAmount,
                OperationType = operationType,
                Reason = reason,
                RecordTime = DateTime.Now
            };

            currencyRecords.Add(record);

            // 可选：限制记录数量，避免内存占用过大
            if (currencyRecords.Count > 1000)
            {
                currencyRecords.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取货币变更记录
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="count">获取数量</param>
        /// <returns>记录列表</returns>
        public static List<CurrencyChangeRecord> GetCurrencyRecords(string playerId, int count = 10)
        {
            return currencyRecords
                .Where(r => r.PlayerId == playerId)
                .OrderByDescending(r => r.RecordTime)
                .Take(count)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// 升级结果类
    /// 封装升级操作的返回结果
    /// </summary>
    public class LevelUpResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否升级了
        /// </summary>
        public bool LeveledUp { get; set; }

        /// <summary>
        /// 原等级
        /// </summary>
        public int OldLevel { get; set; }

        /// <summary>
        /// 新等级
        /// </summary>
        public int NewLevel { get; set; }

        /// <summary>
        /// 升级等级数（可能连升多级）
        /// </summary>
        public int LevelsGained { get; set; }

        /// <summary>
        /// 添加的经验值
        /// </summary>
        public long ExpAdded { get; set; }

        /// <summary>
        /// 剩余经验值
        /// </summary>
        public long RemainingExp { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 是否因突破点封顶而无法继续升级。
        /// </summary>
        public bool IsBreakthroughBlocked { get; set; }

        /// <summary>
        /// 当前境界别名。
        /// </summary>
        public string CurrentRealmAlias { get; set; } = string.Empty;

        /// <summary>
        /// 下一个境界别名。
        /// </summary>
        public string NextRealmAlias { get; set; } = string.Empty;
    }
}
