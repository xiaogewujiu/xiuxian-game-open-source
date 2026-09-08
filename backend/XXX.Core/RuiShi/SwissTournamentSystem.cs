using XXX.Battle;
using XXX.Entity;

namespace XXX.RuiShi
{
    /// <summary>
    /// 瑞士轮比赛系统核心实现
    /// </summary>
    public class SwissTournamentSystem
    {
        // ==================== 配置参数 ====================
        private int _configRounds;
        private bool _useConfiguredRounds;
        private IReadOnlyDictionary<string, PetEntity> _activePets = new Dictionary<string, PetEntity>(StringComparer.OrdinalIgnoreCase);

        // ==================== 内部数据结构 ====================

        /// <summary>
        /// 玩家状态追踪（内部使用）
        /// </summary>
        private class PlayerState
        {
            public UserEntity Entity { get; set; }
            public int Score { get; set; }
            public int Wins { get; set; }
            public HashSet<string> Opponents { get; set; } = [];
            public int ByeCount { get; set; }
            public int MatchesPlayed { get; set; }

            /// <summary>
            /// 初始化选手状态。
            /// </summary>
            public PlayerState(UserEntity entity)
            {
                Entity = entity;
                Score = 0;
                Wins = 0;
                ByeCount = 0;
                MatchesPlayed = 0;
            }

            /// <summary>
            /// 检查是否与指定选手交手过
            /// </summary>
            public bool HasPlayedAgainst(string opponentGid)
            {
                return Opponents.Contains(opponentGid);
            }

            /// <summary>
            /// 记录对战
            /// </summary>
            public void RecordOpponent(string opponentGid)
            {
                Opponents.Add(opponentGid);
                MatchesPlayed++;
            }

            /// <summary>
            /// 记录轮空
            /// </summary>
            public void RecordBye()
            {
                ByeCount++;
                Score++; // 轮空视为胜利
                Wins++;
            }

            /// <summary>
            /// 记录胜利
            /// </summary>
            public void RecordWin(string opponentGid)
            {
                Score++;
                Wins++;
                RecordOpponent(opponentGid);
            }

            /// <summary>
            /// 记录失败
            /// </summary>
            public void RecordLoss(string opponentGid)
            {
                RecordOpponent(opponentGid);
            }
        }

        /// <summary>
        /// 配对结果
        /// </summary>
        private class Pairing
        {
            public PlayerState Player1 { get; set; }
            public PlayerState? Player2 { get; set; }
            public bool IsBye { get; set; }

            /// <summary>
            /// 初始化一组配对结果。
            /// </summary>
            public Pairing(PlayerState p1, PlayerState? p2 = null)
            {
                Player1 = p1;
                Player2 = p2;
                IsBye = p2 == null;
            }
        }

        // ==================== 对外接口 ====================

        /// <summary>
        /// 瑞士轮比赛系统对外唯一入口
        /// </summary>
        /// <param name="players">参赛选手列表</param>
        /// <param name="configuredRounds">配置的轮数（可选，-1表示自动计算）</param>
        /// <returns>完整的比赛结果</returns>
        public SwissTournamentResult RunSwissTournament(
            List<UserEntity> players,
            int configuredRounds = -1)
        {
            return RunSwissTournament(players, null, configuredRounds);
        }

        /// <summary>
        /// 瑞士轮比赛系统对外入口，可额外提供当前出战灵宠快照。
        /// </summary>
        public SwissTournamentResult RunSwissTournament(
            List<UserEntity> players,
            IReadOnlyDictionary<string, PetEntity>? activePets,
            int configuredRounds = -1)
        {
            // 参数校验
            if (players == null || players.Count == 0)
            {
                throw new ArgumentException("选手列表不能为空");
            }

            _activePets = activePets ?? new Dictionary<string, PetEntity>(StringComparer.OrdinalIgnoreCase);

            // 设置轮数配置
            _useConfiguredRounds = configuredRounds > 0;
            _configRounds = configuredRounds;

            // 计算轮数
            int totalRounds = CalculateRounds(players.Count);

            // 初始化选手状态
            var playerStates = players
                .Select(p => new PlayerState(p))
                .ToList();

            // 初始化结果对象
            var tournamentResult = new SwissTournamentResult();

            // 逐轮进行比赛
            for (int round = 1; round <= totalRounds; round++)
            {
                var roundResult = RunRound(playerStates, round);
                tournamentResult.Rounds.Add(roundResult);
            }

            // 生成最终排名
            tournamentResult.FinalStandings = GenerateFinalStandings(playerStates);

            return tournamentResult;
        }

        // ==================== 核心逻辑 ====================

        /// <summary>
        /// 计算比赛轮数
        /// 设计决策：根据参赛人数计算，确保能决出明确排名
        /// 公式：轮数 = ceil(log2(人数))，最少3轮
        /// </summary>
        private int CalculateRounds(int playerCount)
        {
            if (_useConfiguredRounds)
            {
                return _configRounds;
            }

            // 瑞士轮标准轮数计算：能决出冠军所需轮数
            int rounds = (int)Math.Ceiling(Math.Log(playerCount, 2));
            return Math.Max(rounds, 3); // 最少3轮
        }

        /// <summary>
        /// 执行单轮比赛
        /// </summary>
        private SwissRoundResult RunRound(List<PlayerState> players, int roundNumber)
        {
            // 按积分排序（降序）
            var sortedPlayers = players
                .OrderByDescending(p => p.Score)
                .ToList();

            // 生成配对
            var pairings = GeneratePairings(sortedPlayers);

            // 执行比赛
            var roundResult = new SwissRoundResult
            {
                RoundNumber = roundNumber
            };

            foreach (var pairing in pairings)
            {
                var matchResult = pairing.IsBye
                    ? ProcessBye(pairing.Player1, roundNumber)
                    : ProcessMatch(pairing.Player1, pairing.Player2!, roundNumber);

                roundResult.Matches.Add(matchResult);

                if (pairing.IsBye)
                {
                    roundResult.ByePlayerName = matchResult.PlayerAName;
                }
            }

            return roundResult;
        }

        /// <summary>
        /// 生成配对
        /// 核心算法：按优先级配对
        /// 1. 同积分、未交手
        /// 2. 相邻积分、未交手
        /// 3. 兜底：允许重复对战
        /// </summary>
        private List<Pairing> GeneratePairings(List<PlayerState> sortedPlayers)
        {
            var pairings = new List<Pairing>();
            var availablePlayers = new HashSet<PlayerState>(sortedPlayers);

            // 奇数人时需要轮空
            bool needsBye = availablePlayers.Count % 2 != 0;

            while (availablePlayers.Count > 0)
            {
                // 处理轮空（最后剩余一人时）
                if (needsBye && availablePlayers.Count == 1)
                {
                    var byePlayer = availablePlayers.First();
                    pairings.Add(new Pairing(byePlayer));
                    byePlayer.RecordBye();
                    availablePlayers.Remove(byePlayer);
                    break;
                }

                // 获取当前待配对选手（积分最高的）
                var currentPlayer = GetNextPlayer(availablePlayers);
                var opponent = FindOpponent(currentPlayer, availablePlayers);

                if (opponent != null)
                {
                    // 成功配对
                    pairings.Add(new Pairing(currentPlayer, opponent));
                    availablePlayers.Remove(currentPlayer);
                    availablePlayers.Remove(opponent);
                }
                else
                {
                    // 理论上不应到达此处，作为兜底处理
                    throw new InvalidOperationException($"无法为选手 {currentPlayer.Entity.Name} 找到对手");
                }
            }

            return pairings;
        }

        /// <summary>
        /// 获取下一个待配对选手
        /// 策略：优先选择积分最高且轮空次数少的选手
        /// </summary>
        private PlayerState GetNextPlayer(HashSet<PlayerState> availablePlayers)
        {
            // 先按积分降序，再按轮空次数升序
            return availablePlayers
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.ByeCount)
                .First();
        }

        /// <summary>
        /// 寻找对手
        /// 核心配对算法（按优先级）
        /// </summary>
        private PlayerState? FindOpponent(
            PlayerState currentPlayer,
            HashSet<PlayerState> availablePlayers)
        {
            // 获取所有可能的对手（排除自己）
            var candidates = availablePlayers
                .Where(p => p != currentPlayer)
                .OrderByDescending(p => p.Score)
                .ToList();

            // ========== 第一优先级：同积分、未交手 ==========
            var sameScoreOpponents = candidates
                .Where(p => p.Score == currentPlayer.Score &&
                           !currentPlayer.HasPlayedAgainst(p.Entity.GID))
                .ToList();

            if (sameScoreOpponents.Count > 0)
            {
                // 优先选择轮空次数少的
                return sameScoreOpponents
                    .OrderBy(p => p.ByeCount)
                    .First();
            }

            // ========== 第二优先级：相邻积分（±1分）、未交手 ==========
            var adjacentScoreOpponents = candidates
                .Where(p => Math.Abs(p.Score - currentPlayer.Score) <= 1 &&
                           !currentPlayer.HasPlayedAgainst(p.Entity.GID))
                .ToList();

            if (adjacentScoreOpponents.Count > 0)
            {
                // 优先选择积分接近且轮空次数少的
                return adjacentScoreOpponents
                    .OrderByDescending(p => p.Score)
                    .ThenBy(p => p.ByeCount)
                    .First();
            }

            // ========== 第三优先级：任意积分、未交手 ==========
            var anyUnplayedOpponents = candidates
                .Where(p => !currentPlayer.HasPlayedAgainst(p.Entity.GID))
                .ToList();

            if (anyUnplayedOpponents.Count > 0)
            {
                return anyUnplayedOpponents
                    .OrderByDescending(p => p.Score)
                    .ThenBy(p => p.ByeCount)
                    .First();
            }

            // ========== 兜底规则：所有人都交手过，选择最接近积分的对手 ==========
            // 这种情况在正常瑞士轮中极少出现
            // 标注：这是兜底行为，允许重复对战
            return candidates
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.ByeCount)
                .FirstOrDefault();
        }

        /// <summary>
        /// 处理正常比赛
        /// </summary>
        private MatchResult ProcessMatch(
            PlayerState player1,
            PlayerState player2,
            int roundNumber)
        {
            // 调用战斗方法（1v1）
            var battleResult = BattleSystem.StartBattle(
                [player1.Entity],
                [player2.Entity],
                activePets: _activePets);

            var matchResult = new MatchResult
            {
                PlayerAName = player1.Entity.Name,
                PlayerAGid = player1.Entity.GID,
                PlayerBName = player2.Entity.Name,
                PlayerBGid = player2.Entity.GID,
                IsBye = false,
                RoundNumber = roundNumber
            };

            // 根据战斗结果更新状态
            if (battleResult.IsVictory)
            {
                matchResult.WinnerName = player1.Entity.Name;
                matchResult.PlayerAScore = 1;
                matchResult.PlayerBScore = 0;

                player1.RecordWin(player2.Entity.GID);
                player2.RecordLoss(player1.Entity.GID);
            }
            else
            {
                matchResult.WinnerName = player2.Entity.Name;
                matchResult.PlayerAScore = 0;
                matchResult.PlayerBScore = 1;

                player2.RecordWin(player1.Entity.GID);
                player1.RecordLoss(player2.Entity.GID);
            }

            return matchResult;
        }

        /// <summary>
        /// 处理轮空
        /// </summary>
        private MatchResult ProcessBye(PlayerState player, int roundNumber)
        {
            return new MatchResult
            {
                PlayerAName = player.Entity.Name,
                PlayerAGid = player.Entity.GID,
                PlayerBName = string.Empty,
                PlayerBGid = string.Empty,
                IsBye = true,
                WinnerName = player.Entity.Name,
                RoundNumber = roundNumber,
                PlayerAScore = 1,
                PlayerBScore = 0
            };
        }

        /// <summary>
        /// 生成最终排名
        /// 排序规则：积分 → 胜场 → 对手强度(SOS) → 轮空次数
        /// </summary>
        private List<PlayerStanding> GenerateFinalStandings(List<PlayerState> playerStates)
        {
            // 创建 GID -> 积分 的映射，用于计算 SOS
            var scoreMap = playerStates.ToDictionary(p => p.Entity.GID, p => p.Score);

            var standings = playerStates
                .Select(p => new PlayerStanding
                {
                    PlayerGid = p.Entity.GID,
                    PlayerName = p.Entity.Name,
                    TotalWins = p.Wins,
                    TotalScore = p.Score,
                    // 计算对手强度(SOS)：所有对手的最终积分总和
                    StrengthOfSchedule = p.Opponents.Sum(opponentGid =>
                        scoreMap.ContainsKey(opponentGid) ? scoreMap[opponentGid] : 0),
                    MatchesPlayed = p.MatchesPlayed,
                    ByeCount = p.ByeCount
                })
                // 排序规则：积分 → 胜场 → 对手强度(SOS) → 轮空次数
                .OrderByDescending(s => s.TotalScore)
                .ThenByDescending(s => s.TotalWins)
                .ThenByDescending(s => s.StrengthOfSchedule)
                .ThenBy(s => s.ByeCount)
                .Select((s, index) =>
                {
                    s.Rank = index + 1;
                    return s;
                })
                .ToList();

            return standings;
        }
    }
}
