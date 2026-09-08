namespace XXX.Player
{
    /// <summary>
    /// 突破阶段定义。
    /// </summary>
    public sealed class BreakthroughStage
    {
        /// <summary>
        /// 初始化单个突破阶段定义。
        /// </summary>
        public BreakthroughStage(int index, string realmName, int requiredLevel)
        {
            Index = index;
            RealmName = realmName;
            RequiredLevel = requiredLevel;
        }

        public int Index { get; }

        public string RealmName { get; }

        public int RequiredLevel { get; }
    }

    /// <summary>
    /// 突破配置。
    /// </summary>
    public static class BreakthroughConfig
    {
        private const int AttributeBonusPerBreakthroughPercent = 5;

        public static IReadOnlyList<BreakthroughStage> Stages { get; } = new[]
        {
            new BreakthroughStage(0, "练气期", 1),
            new BreakthroughStage(1, "筑基期", 10),
            new BreakthroughStage(2, "金丹期", 20),
            new BreakthroughStage(3, "元婴期", 30),
            new BreakthroughStage(4, "化神期", 40),
            new BreakthroughStage(5, "炼虚期", 50),
            new BreakthroughStage(6, "合体期", 60),
            new BreakthroughStage(7, "大乘期", 70)
        };

        /// <summary>
        /// 规范化已完成突破次数。
        /// </summary>
        public static int NormalizeCompletedCount(int completedCount)
        {
            return Math.Clamp(completedCount, 0, Stages.Count - 1);
        }

        /// <summary>
        /// 获取当前所处突破阶段。
        /// </summary>
        public static BreakthroughStage GetCurrentStage(int completedCount)
        {
            return Stages[NormalizeCompletedCount(completedCount)];
        }

        /// <summary>
        /// 获取下一阶段突破定义。
        /// </summary>
        public static BreakthroughStage? GetNextStage(int completedCount)
        {
            var nextIndex = NormalizeCompletedCount(completedCount) + 1;
            return nextIndex >= Stages.Count ? null : Stages[nextIndex];
        }

        /// <summary>
        /// 获取下一次突破所需等级。
        /// </summary>
        public static int GetNextRequiredLevel(int completedCount)
        {
            return GetNextStage(completedCount)?.RequiredLevel ?? 0;
        }

        /// <summary>
        /// 计算突破带来的属性百分比加成。
        /// </summary>
        public static int GetAttributeBonusPercent(int completedCount)
        {
            return NormalizeCompletedCount(completedCount) * AttributeBonusPerBreakthroughPercent;
        }

        /// <summary>
        /// 获取当前等级所在的小境界层数。
        /// </summary>
        public static int GetRealmLayer(int level)
        {
            var safeLevel = Math.Max(1, level);
            return ((safeLevel - 1) % 10) + 1;
        }
    }
}
