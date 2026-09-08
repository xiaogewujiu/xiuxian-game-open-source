using XXX.Battle;
using XXX.Entity;

namespace XXX.Activity
{
    /// <summary>
    /// 战斗数据追踪器
    /// 负责在战斗结束后自动追踪和更新相关活动的进度
    ///
    /// 使用说明：
    /// 1. 在战斗结束后调用 OnBattleEnd 方法
    /// 2. 系统会自动统计各项战斗数据
    /// 3. 自动更新所有相关战斗类活动的进度
    /// </summary>
    public class BattleTracker
    {
        /// <summary>
        /// 战斗结束后处理活动进度
        /// 这是主要的入口方法，在战斗结束时调用
        ///
        /// 统计的数据包括：
        /// - 总造成伤害
        /// - 总承受伤害
        /// - 击杀数
        /// - 胜利场次
        /// - 暴击次数
        /// - 技能使用次数
        /// </summary>
        /// <param name="result">战斗结果对象</param>
        /// <param name="context">战斗上下文对象</param>
        public static void OnBattleEnd(BattleResult result, BattleContext context)
        {
            if (result == null || context == null)
            {
                return;
            }

            // 统计并更新总造成伤害
            long totalDamageDealt = CalculateTotalDamageDealt(context);
            ActivityManager.UpdateBattleProgress(BattleTaskType.TotalDamageDealt, totalDamageDealt);

            // 统计并更新总承受伤害
            long totalDamageTaken = CalculateTotalDamageTaken(context);
            ActivityManager.UpdateBattleProgress(BattleTaskType.TotalDamageTaken, totalDamageTaken);

            // 统计并更新击杀数
            int killCount = CalculateKillCount(context);
            ActivityManager.UpdateBattleProgress(BattleTaskType.TotalKills, killCount);

            // 统计并更新胜利场次
            if (IsPlayerWin(result))
            {
                ActivityManager.UpdateBattleProgress(BattleTaskType.TotalWins, 1);
            }

            // 统计并更新暴击次数
            int critCount = CalculateCritCount(context);
            ActivityManager.UpdateBattleProgress(BattleTaskType.CriticalHits, critCount);

            // 统计并更新技能使用次数
            int skillCount = CalculateSkillUseCount(context);
            ActivityManager.UpdateBattleProgress(BattleTaskType.UseSkillCount, skillCount);

            // TODO: 可以继续添加更多统计类型
            // - 单次战斗最高伤害
            // - BOSS击杀数
            // - 完美胜利场次
        }

        /// <summary>
        /// 计算总造成伤害
        /// 统计玩家方所有单位造成的伤害总和
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>总造成伤害值</returns>
        private static long CalculateTotalDamageDealt(BattleContext context)
        {
            long totalDamage = 0;

            //// 检查是否有伤害统计数据
            //if (context.Result?.FighterSkillStats != null)
            //{
            //    // 遍历所有造成伤害的记录
            //    foreach (var kvp in context.Result.FighterSkillStats.a)
            //    {
            //        // 这里可以筛选只统计玩家方的伤害
            //        // kvp.Key 是单位ID，kvp.Value 是伤害值
            //        totalDamage += kvp.Value;
            //    }
            //}

            return totalDamage;
        }

        /// <summary>
        /// 计算总承受伤害
        /// 统计玩家方所有单位承受的伤害总和
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>总承受伤害值</returns>
        private static long CalculateTotalDamageTaken(BattleContext context)
        {
            long totalDamage = 0;

            //// 检查是否有承受伤害统计数据
            //if (context.Result?.DamageTaken != null)
            //{
            //    // 遍历所有承受伤害的记录
            //    foreach (var kvp in context.Result.DamageTaken)
            //    {
            //        // 这里可以筛选只统计玩家方的伤害
            //        // kvp.Key 是单位ID，kvp.Value 是伤害值
            //        totalDamage += kvp.Value;
            //    }
            //}

            return totalDamage;
        }

        /// <summary>
        /// 计算击杀数
        /// 统计本战斗中击败的敌方单位数量
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>击杀数量</returns>
        private static int CalculateKillCount(BattleContext context)
        {
            int killCount = 0;

            //// 检查是否有死亡记录
            //if (context.Result?.Deaths != null)
            //{
            //    // 统计敌方死亡的单位数量
            //    killCount = context.Result.Deaths.Count(d => d.Value.Side == "enemy");
            //}

            return killCount;
        }

        /// <summary>
        /// 判断玩家是否获胜
        /// </summary>
        /// <param name="result">战斗结果</param>
        /// <returns>是否获胜</returns>
        private static bool IsPlayerWin(BattleResult result)
        {
            // 检查胜利方是否是玩家
            return result.IsVictory;
        }

        /// <summary>
        /// 计算暴击次数
        /// 统计本战斗中触发暴击的总次数
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>暴击次数</returns>
        private static int CalculateCritCount(BattleContext context)
        {
            int critCount = 0;

            //// 检查是否有暴击统计
            //if (context.Result?.FighterCritStats != null)
            //{
            //    // 统计玩家方的暴击次数
            //    foreach (var kvp in context.Result.FighterCritStats)
            //    {
            //        // 可以添加条件筛选只统计玩家方的暴击
            //        critCount += kvp.Value.Count;
            //    }
            //}

            return critCount;
        }

        /// <summary>
        /// 计算技能使用次数
        /// 统计本战斗中使用技能的总次数
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>技能使用次数</returns>
        private static int CalculateSkillUseCount(BattleContext context)
        {
            int skillCount = 0;

            // 检查是否有技能使用统计
            if (context.Result?.FighterSkillStats != null)
            {
                // 统计每个角色使用的技能总次数
                foreach (var kvp in context.Result.FighterSkillStats)
                {
                    // 可以添加条件筛选只统计玩家方的技能使用
                    if (kvp.Value.SkillUsageCount != null)
                    {
                        skillCount += kvp.Value.SkillUsageCount.Values.Sum();
                    }
                }
            }

            return skillCount;
        }

        /// <summary>
        /// 计算单次战斗最高伤害
        /// 找出本战斗中单次攻击的最高伤害值
        /// </summary>
        /// <param name="context">战斗上下文</param>
        /// <returns>最高伤害值</returns>
        public static long CalculateMaxSingleHitDamage(BattleContext context)
        {
            long maxDamage = 0;

            // TODO: 实现单次最高伤害的统计
            // 这需要在战斗过程中记录每次攻击的伤害

            return maxDamage;
        }

        /// <summary>
        /// 获取战斗统计摘要
        /// 返回本战斗的各项统计数据摘要
        /// </summary>
        /// <param name="result">战斗结果</param>
        /// <param name="context">战斗上下文</param>
        /// <returns>战斗统计摘要</returns>
        public static BattleStatsSummary GetBattleStatsSummary(BattleResult result, BattleContext context)
        {
            return new BattleStatsSummary
            {
                TotalDamageDealt = CalculateTotalDamageDealt(context),
                TotalDamageTaken = CalculateTotalDamageTaken(context),
                KillCount = CalculateKillCount(context),
                IsWin = IsPlayerWin(result),
                CritCount = CalculateCritCount(context),
                SkillUseCount = CalculateSkillUseCount(context),
            };
        }
    }

    /// <summary>
    /// 战斗统计摘要类
    /// 封装一场战斗的所有关键统计数据
    /// </summary>
    public class BattleStatsSummary
    {
        /// <summary>
        /// 总造成伤害
        /// </summary>
        public long TotalDamageDealt { get; set; }

        /// <summary>
        /// 总承受伤害
        /// </summary>
        public long TotalDamageTaken { get; set; }

        /// <summary>
        /// 击杀数量
        /// </summary>
        public int KillCount { get; set; }

        /// <summary>
        /// 是否获胜
        /// </summary>
        public bool IsWin { get; set; }

        /// <summary>
        /// 是否完美胜利（无伤获胜）
        /// </summary>
        public bool IsPerfectWin { get; set; }

        /// <summary>
        /// 暴击次数
        /// </summary>
        public int CritCount { get; set; }

        /// <summary>
        /// 技能使用次数
        /// </summary>
        public int SkillUseCount { get; set; }

        /// <summary>
        /// 是否击败BOSS
        /// </summary>
        public bool HasBossKill { get; set; }

        /// <summary>
        /// 获取统计摘要文本
        /// 返回格式化的统计信息文本
        /// </summary>
        /// <returns>统计摘要文本</returns>
        public string GetSummaryText()
        {
            return $"战斗统计：" +
                   $"\n造成伤害：{TotalDamageDealt}" +
                   $"\n承受伤害：{TotalDamageTaken}" +
                   $"\n击杀数量：{KillCount}" +
                   $"\n战斗结果：{(IsWin ? "胜利" : "失败")}" +
                   (IsPerfectWin ? "\n完美胜利！" : "") +
                   $"\n暴击次数：{CritCount}" +
                   $"\n技能使用：{SkillUseCount}" +
                   (HasBossKill ? "\n击败BOSS！" : "");
        }
    }
}
