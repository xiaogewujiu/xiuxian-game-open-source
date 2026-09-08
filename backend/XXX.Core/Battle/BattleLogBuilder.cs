using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 战斗日志构建器
    /// 提供流畅API创建结构化日志
    /// </summary>
    public class BattleLogBuilder
    {
        private readonly BattleLogEntry _entry;
        private readonly RoundLog _log;
        private readonly BattleContext? _context;

        /// <summary>
        /// 初始化战斗日志构建器。
        /// </summary>
        public BattleLogBuilder(RoundLog log, BattleContext? context, BattleLogType type)
        {
            _log = log;
            _context = context;
            _entry = new BattleLogEntry
            {
                Type = type,
                Timestamp = GetNextTimestamp(context)
            };
        }

        /// <summary>
        /// 获取当前上下文的下一个日志时间戳
        /// 作用：保证同一场战斗日志按构建顺序递增
        /// 关键逻辑：不再使用全局静态计数，改为上下文内计数，避免并发战斗时间戳串扰
        /// </summary>
        private static int GetNextTimestamp(BattleContext? context)
        {
            if (context == null)
            {
                return 0;
            }

            var current = context.LogTimestampCounter;
            context.LogTimestampCounter = current + 1;
            return current;
        }

        // ===== 链式配置方法 =====

        /// <summary>
        /// 设置施法者
        /// </summary>
        public BattleLogBuilder Caster(BattleFighter? caster)
        {
            _entry.CasterId = caster?.Id ?? string.Empty;
            _entry.CasterName = caster?.Name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        public BattleLogBuilder Target(BattleFighter? target)
        {
            _entry.TargetId = target?.Id ?? string.Empty;
            _entry.TargetName = target?.Name ?? string.Empty;
            _entry.TargetCurrentHp = target?.CurrentHp;
            _entry.TargetMaxHp = target?.MaxHp;
            return this;
        }

        /// <summary>
        /// 设置技能
        /// </summary>
        public BattleLogBuilder Skill(Skill? skill)
        {
            _entry.SkillId = skill?.Id;
            _entry.SkillName = skill?.Name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// 设置Buff
        /// </summary>
        public BattleLogBuilder Buff(BuffTemplate? buff)
        {
            _entry.BuffId = buff?.Gid ?? string.Empty;
            _entry.BuffName = buff?.Name ?? string.Empty;
            return this;
        }

        /// <summary>
        /// 设置数值（伤害、治疗量等）
        /// </summary>
        public BattleLogBuilder Value(int value, int? value2 = null)
        {
            _entry.Value = value;
            _entry.Value2 = value2;
            return this;
        }

        /// <summary>
        /// 标记为暴击
        /// </summary>
        public BattleLogBuilder Crit()
        {
            _entry.IsCrit = true;
            return this;
        }

        /// <summary>
        /// 标记为闪避
        /// </summary>
        public BattleLogBuilder Dodge()
        {
            _entry.IsDodge = true;
            return this;
        }

        /// <summary>
        /// 设置多段伤害信息
        /// </summary>
        public BattleLogBuilder HitInfo(int hitIndex, int totalHits)
        {
            _entry.HitIndex = hitIndex;
            _entry.TotalHits = totalHits;
            return this;
        }

        /// <summary>
        /// 设置描述文本
        /// </summary>
        public BattleLogBuilder Description(string description)
        {
            _entry.Description = description;
            return this;
        }

        /// <summary>
        /// 设置额外数据
        /// </summary>
        public BattleLogBuilder ExtraData(string key, object value)
        {
            _entry.ExtraData = $"{key}:{value}";
            return this;
        }

        /// <summary>
        /// 设置是否暴击（可传入bool）
        /// </summary>
        public BattleLogBuilder Crit(bool isCrit)
        {
            _entry.IsCrit = isCrit;
            return this;
        }

        /// <summary>
        /// 设置是否闪避（可传入bool）
        /// </summary>
        public BattleLogBuilder Dodge(bool isDodge)
        {
            _entry.IsDodge = isDodge;
            return this;
        }

        // ===== 辅助方法 =====

        /// <summary>
        /// 根据ID查找战斗角色
        /// </summary>
        private BattleFighter? FindFighterById(string fighterId)
        {
            if (string.IsNullOrEmpty(fighterId)) return null;
            if (_context == null) return null;

            var allFighters = _context.PlayerSide.Concat(_context.EnemySide);
            return allFighters.FirstOrDefault(f => f.Id == fighterId);
        }

        /// <summary>
        /// 比较两个状态快照是否发生变化（只比较关键属性）
        /// </summary>
        private bool IsStateChanged(FighterStateSnapshot current, FighterStateSnapshot? previous)
        {
            if (previous == null) return true; // 没有缓存，视为变化

            // 比较关键属性
            if (current.CurrentHp != previous.CurrentHp) return true;
            if (current.CurrentMp != previous.CurrentMp) return true;
            if (current.MaxHp != previous.MaxHp) return true;
            if (current.MaxMp != previous.MaxMp) return true;

            // 比较Buff列表（数量或内容变化）
            if (current.Buffs.Count != previous.Buffs.Count) return true;

            // 简单比较：检查Buff ID和层数是否一致
            var previousBuffs = previous.Buffs.ToDictionary(b => b.BuffId, b => b.Stack);

            foreach (var buff in current.Buffs)
            {
                if (!previousBuffs.TryGetValue(buff.BuffId, out var prevStack) || prevStack != buff.Stack)
                    return true;
            }

            return false;
        }

        // ===== 构建方法 =====

        /// <summary>
        /// 构建并添加到日志
        /// </summary>
        public BattleLogEntry Build()
        {
            // 自动生成描述（如果没有提供）
            if (string.IsNullOrEmpty(_entry.Description))
            {
                if (_entry.Type == BattleLogType.Heal && _entry.Value == 0)
                {
                    return _entry;
                }
                _entry.Description = GenerateDefaultDescription();
            }

            // 只记录发生变化的角色的状态快照
            if (_context != null)
            {
                _entry.FighterStates = [];

                // 确保缓存字典存在
                if (_context.FighterStateCache == null)
                    _context.FighterStateCache = [];

                // 处理 Caster
                if (!string.IsNullOrEmpty(_entry.CasterId))
                {
                    var caster = FindFighterById(_entry.CasterId);
                    if (caster != null)
                    {
                        var currentState = BattleRecorder.CreateFighterStateSnapshot(caster);
                        _context.FighterStateCache.TryGetValue(_entry.CasterId, out var previousState);

                        // 只有状态发生变化时才记录
                        if (IsStateChanged(currentState, previousState))
                        {
                            _entry.FighterStates[_entry.CasterId] = currentState;
                            _context.FighterStateCache[_entry.CasterId] = currentState;
                        }
                    }
                }

                // 处理 Target
                if (!string.IsNullOrEmpty(_entry.TargetId) && _entry.TargetId != _entry.CasterId)
                {
                    var target = FindFighterById(_entry.TargetId);
                    if (target != null)
                    {
                        var currentState = BattleRecorder.CreateFighterStateSnapshot(target);
                        _context.FighterStateCache.TryGetValue(_entry.TargetId, out var previousState);

                        // 只有状态发生变化时才记录
                        if (IsStateChanged(currentState, previousState))
                        {
                            _entry.FighterStates[_entry.TargetId] = currentState;
                            _context.FighterStateCache[_entry.TargetId] = currentState;
                        }
                    }
                }
            }

            _log.Entries.Add(_entry);
            return _entry;
        }

        /// <summary>
        /// 生成默认描述文本
        /// </summary>
        private string GenerateDefaultDescription()
        {
            return _entry.Type switch
            {
                BattleLogType.DamageDealt => FormatDamageDescription(),
                BattleLogType.Heal => $"{_entry.TargetName} 恢复了 {_entry.Value} 点生命。",
                BattleLogType.SkillUse => $"{_entry.CasterName} 使用了技能[{_entry.SkillName}]。",
                BattleLogType.BuffApply => $"{_entry.TargetName} 获得了[{_entry.BuffName}]。",
                BattleLogType.BuffRemove => $"{_entry.TargetName} 的[{_entry.BuffName}]消失了。",
                BattleLogType.BuffExpired => $"{_entry.TargetName} 的[{_entry.BuffName}]已结束。",
                BattleLogType.Dodge => $"{_entry.TargetName} 闪避了 {_entry.CasterName} 的攻击。",
                BattleLogType.Miss => $"{_entry.CasterName} 的攻击未命中。",
                BattleLogType.Death => $"{_entry.TargetName} 倒下了。",
                BattleLogType.CounterAttack => $"{_entry.CasterName} 发起了反击。",
                BattleLogType.Stun => $"{_entry.TargetName} 受到[眩晕]影响。",
                BattleLogType.Silence => $"{_entry.TargetName} 受到[沉默]影响。",
                BattleLogType.Execute => $"{_entry.CasterName} 对 {_entry.TargetName} 触发了斩杀。",
                BattleLogType.PassiveTrigger => $"{_entry.CasterName} 的被动效果触发了。",
                BattleLogType.RoundStart => $"━━ 第 {_entry.Value ?? 1} 回合开始 ━━",
                BattleLogType.RoundEnd => $"━━ 第 {_entry.Value ?? 1} 回合结束 ━━",
                _ => string.IsNullOrEmpty(_entry.TargetName)
                    ? $"{_entry.CasterName} 执行了一次行动。"
                    : $"{_entry.CasterName} 对 {_entry.TargetName} 执行了一次行动。"
            };
        }

        /// <summary>
        /// 格式化伤害描述
        /// </summary>
        private string FormatDamageDescription()
        {
            var critText = _entry.IsCrit ? "（暴击）" : "";
            return $"{_entry.CasterName} 对 {_entry.TargetName} 造成 {_entry.Value}{critText} 点伤害。";
        }
    }
}
