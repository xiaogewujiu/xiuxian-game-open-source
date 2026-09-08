using XXX.Entity;

namespace XXX.RuiShi
{
    /// <summary>
    /// 瑞士轮比赛系统使用示例
    /// </summary>
    public class SwissTournamentExample
    {
        /// <summary>
        /// 基础使用示例
        /// </summary>
        public static void BasicExample()
        {
            // 1. 创建参赛选手（100人）
            var players = new List<UserEntity>();
            for (int i = 1; i <= 100; i++)
            {
                players.Add(CreateTestPlayer("玩家" + i, "10", false));
            }

            // 2. 创建瑞士轮系统实例
            var tournamentSystem = new SwissTournamentSystem();

            // 3. 运行比赛（使用默认轮数）
            // 默认轮数 = ceil(log2(人数))，最少3轮
            // 100人 -> ceil(log2(100)) = ceil(6.64) = 7轮
            SwissTournamentResult result = tournamentSystem.RunSwissTournament(players);

            // 4. 输出结果
            PrintTournamentResults(result);
        }

        /// <summary>
        /// 指定轮数示例
        /// </summary>
        public static void CustomRoundsExample()
        {
            // 1. 创建参赛选手（101人，奇数）
            var players = new List<UserEntity>();
            for (int i = 1; i <= 101; i++)
            {
                players.Add(CreateTestPlayer("玩家" + i, "10", false));
            }

            // 2. 创建瑞士轮系统实例
            var tournamentSystem = new SwissTournamentSystem();

            // 3. 运行比赛（指定5轮）
            SwissTournamentResult result = tournamentSystem.RunSwissTournament(players, 5);

            // 4. 输出结果
            PrintTournamentResults(result);
        }

        /// <summary>
        /// 打印比赛结果
        /// </summary>
        public static void PrintTournamentResults(SwissTournamentResult result)
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"瑞士轮比赛结束 - 共 {result.TotalRounds} 轮");
            Console.WriteLine("========================================");

            // 打印每轮详细配对
            foreach (var round in result.Rounds)
            {
                Console.WriteLine($"\n========== 第 {round.RoundNumber} 轮 ==========");

                foreach (var match in round.Matches)
                {
                    if (match.IsBye)
                    {
                        Console.WriteLine($"  {match.PlayerAName} 轮空（直接胜利）");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"  {match.PlayerAName} vs {match.PlayerBName} " +
                            $"→ {match.WinnerName} 获胜");
                    }
                }
            }

            // 打印最终排名
            Console.WriteLine("\n========================================");
            Console.WriteLine("最终排名（积分 → 胜场 → 对手强度 → 轮空次数）:");
            Console.WriteLine("========================================");

            foreach (var standing in result.FinalStandings)
            {
                Console.WriteLine(
                    $"#{standing.Rank,-3} | {standing.PlayerName,-10} | " +
                    $"积分: {standing.TotalScore,-2} | " +
                    $"胜场: {standing.TotalWins,-2} | " +
                    $"对手强度(SOS): {standing.StrengthOfSchedule,-2} | " +
                    $"参赛: {standing.MatchesPlayed,-2} | " +
                    $"轮空: {standing.ByeCount,-1}");
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("前10名详情:");
            Console.WriteLine("========================================");

            foreach (var standing in result.FinalStandings.Take(10))
            {
                Console.WriteLine(
                    $"#{standing.Rank} {standing.PlayerName}: " +
                    $"{standing.TotalWins}胜 {standing.MatchesPlayed - standing.TotalWins}负 " +
                    $"(积分{standing.TotalScore}, SOS:{standing.StrengthOfSchedule})");
            }
        }

        /// <summary>
        /// 打印详细比赛日志（包含每场比赛）
        /// </summary>
        public static void PrintDetailedMatchLog(SwissTournamentResult result)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("详细比赛日志");
            Console.WriteLine("========================================");

            foreach (var round in result.Rounds)
            {
                Console.WriteLine($"\n========== 第 {round.RoundNumber} 轮 ==========");

                foreach (var match in round.Matches)
                {
                    if (match.IsBye)
                    {
                        Console.WriteLine($"  {match.PlayerAName} 轮空（直接胜利）");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"  {match.PlayerAName} vs {match.PlayerBName} " +
                            $"→ {match.WinnerName} 获胜 " +
                            $"({match.PlayerAScore}:{match.PlayerBScore})");
                    }
                }
            }
        }

        /// <summary>
        /// 查询选手对战历史
        /// </summary>
        public static void PrintPlayerMatchHistory(
            SwissTournamentResult result,
            string playerName)
        {
            Console.WriteLine($"\n========================================");
            Console.WriteLine($"{playerName} 的对战历史");
            Console.WriteLine("========================================");

            foreach (var round in result.Rounds)
            {
                var match = round.Matches
                    .FirstOrDefault(m => m.PlayerAName == playerName || m.PlayerBName == playerName);

                if (match != null)
                {
                    if (match.IsBye)
                    {
                        Console.WriteLine($"第{round.RoundNumber}轮: 轮空（直接胜利）");
                    }
                    else
                    {
                        var opponent = match.PlayerAName == playerName
                            ? match.PlayerBName
                            : match.PlayerAName;
                        var resultText = match.WinnerName == playerName ? "胜利" : "失败";
                        Console.WriteLine($"第{round.RoundNumber}轮: vs {opponent} - {resultText}");
                    }
                }
            }
        }

        /// <summary>
        /// 创建测试选手
        /// </summary>
        public static UserEntity CreateTestPlayer(string name, string level, bool isBot)
        {
            return new UserEntity
            {
                GID = Guid.NewGuid().ToString(),
                Name = name,
                Level = int.TryParse(level, out int lv) ? lv : 1
            };
        }
    }
}
