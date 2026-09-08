#pragma warning disable CS1591
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.DTOs.DungeonInstance;
using XXX.Application.Interfaces;
using XXX.Battle;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class DungeonInstanceService : IDungeonInstanceService
    {
        private readonly IRepository<DungeonInstanceTemplateEntity> _templateRepository;
        private readonly IRepository<DungeonEventConfigEntity> _eventRepository;
        private readonly IRepository<DungeonInstanceEntity> _instanceRepository;
        private readonly IRepository<DungeonInstanceDailyRecordEntity> _dailyRecordRepository;
        private readonly IRepository<DungeonRewardPoolEntity> _rewardPoolRepository;
        private readonly IRepository<DungeonPartyEntity> _partyRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IEquipmentService _equipmentService;
        private readonly IInventoryService _inventoryService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly ICollectionService _collectionService;
        private readonly ILogger<DungeonInstanceService>? _logger;

        private static readonly Dictionary<string, SemaphoreSlim> _playerLocks = new(StringComparer.OrdinalIgnoreCase);

        public DungeonInstanceService(
            IRepository<DungeonInstanceTemplateEntity> templateRepository,
            IRepository<DungeonEventConfigEntity> eventRepository,
            IRepository<DungeonInstanceEntity> instanceRepository,
            IRepository<DungeonInstanceDailyRecordEntity> dailyRecordRepository,
            IRepository<DungeonRewardPoolEntity> rewardPoolRepository,
            IRepository<DungeonPartyEntity> partyRepository,
            IRepository<UserEntity> userRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IEquipmentService equipmentService,
            IInventoryService inventoryService,
            IPlayerAttributeService playerAttributeService,
            ICollectionService collectionService,
            ILogger<DungeonInstanceService>? logger = null)
        {
            _templateRepository = templateRepository;
            _eventRepository = eventRepository;
            _instanceRepository = instanceRepository;
            _dailyRecordRepository = dailyRecordRepository;
            _rewardPoolRepository = rewardPoolRepository;
            _partyRepository = partyRepository;
            _userRepository = userRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentService = equipmentService;
            _inventoryService = inventoryService;
            _playerAttributeService = playerAttributeService;
            _collectionService = collectionService;
            _logger = logger;
        }

        private static SemaphoreSlim GetPlayerLock(string playerId)
        {
            lock (_playerLocks)
            {
                if (!_playerLocks.TryGetValue(playerId, out var sem))
                {
                    sem = new SemaphoreSlim(1, 1);
                    _playerLocks[playerId] = sem;
                }
                return sem;
            }
        }

        private async Task<bool> TryAddEquipmentWithCapacityAsync(EquipmentInstanceEntity equipment)
        {
            if (!await InventoryCapacityRules.HasEquipmentSlotsAsync(
                    _equipmentRepository.Db,
                    equipment.PlayerId))
            {
                _logger?.LogInformation(
                    "Skip dungeon equipment reward because the equipment inventory is full. PlayerId={PlayerId}, Equipment={EquipmentName}",
                    equipment.PlayerId,
                    equipment.Name);
                return false;
            }

            await _equipmentRepository.AddAsync(equipment);
            return true;
        }

        public async Task<DungeonInstanceEnterResponseDto> EnterAsync(string playerId, string dungeonId)
        {
            var normalizedDungeonId = (dungeonId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedDungeonId))
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "秘境 ID 不能为空。" };
            }

            var playerLock = GetPlayerLock(playerId);
            await playerLock.WaitAsync();
            try
            {
                return await EnterCoreAsync(playerId, normalizedDungeonId);
            }
            finally
            {
                playerLock.Release();
            }
        }

        private async Task<DungeonInstanceEnterResponseDto> EnterCoreAsync(string playerId, string dungeonId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null)
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "玩家不存在。" };
            }

            if (!string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "你已经在秘境探索中，请先退出当前秘境。" };
            }

            if (user.BattleMode != BattleMode.Normal)
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "当前战斗模式不允许进入秘境。" };
            }

            if (!GameData.DungeonInstanceTemplates.TryGetValue(dungeonId, out var template))
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "秘境不存在。" };
            }

            if (!template.Enabled)
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "该秘境暂未开放。" };
            }

            // 检查等级
            if (user.Level < template.RecommendedLevel)
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = $"等级不足，需要达到 {template.RecommendedLevel} 级。" };
            }

            // 检查开放时间
            if (!IsInOpenSchedule(template))
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = "该秘境当前不在开放时间段内。" };
            }

            // 检查每日次数
            var today = DateTime.UtcNow.Date;
            var dailyRecord = await _dailyRecordRepository.Db.Queryable<DungeonInstanceDailyRecordEntity>()
                .Where(r => r.PlayerId == playerId && r.DungeonId == dungeonId && r.EnterDate == today)
                .FirstAsync();
            if (dailyRecord != null && dailyRecord.EnterCount >= template.DailyEnterLimit)
            {
                return new DungeonInstanceEnterResponseDto { Success = false, Message = $"今日进入次数已满（{template.DailyEnterLimit}次）。" };
            }

            // 检查进入消耗
            var costs = ParseEntryCosts(template.EntryCostsJson);
            foreach (var cost in costs)
            {
                if (cost.Type.Equals("Gold", StringComparison.OrdinalIgnoreCase))
                {
                    if (user.Gold < cost.Amount)
                    {
                        return new DungeonInstanceEnterResponseDto { Success = false, Message = $"金币不足，需要 {cost.Amount}。" };
                    }
                }
                else if (cost.Type.Equals("SpiritStone", StringComparison.OrdinalIgnoreCase))
                {
                    if (user.SpiritStone < cost.Amount)
                    {
                        return new DungeonInstanceEnterResponseDto { Success = false, Message = $"灵石不足，需要 {cost.Amount}。" };
                    }
                }
                else if (cost.Type.Equals("Item", StringComparison.OrdinalIgnoreCase))
                {
                    var count = await _inventoryService.GetItemCountAsync(playerId, cost.ItemId!);
                    if (count < cost.Amount)
                    {
                        return new DungeonInstanceEnterResponseDto { Success = false, Message = $"道具 {cost.ItemId} 不足，需要 {cost.Amount}，当前 {count}。" };
                    }
                }
            }

            // 扣除消耗
            foreach (var cost in costs)
            {
                if (cost.Type.Equals("Gold", StringComparison.OrdinalIgnoreCase))
                {
                    await _playerAttributeService.AddGoldAsync(playerId, -cost.Amount, "秘境进入消耗");
                }
                else if (cost.Type.Equals("SpiritStone", StringComparison.OrdinalIgnoreCase))
                {
                    user.SpiritStone -= cost.Amount;
                    await _userRepository.UpdateAsync(user);
                }
                else if (cost.Type.Equals("Item", StringComparison.OrdinalIgnoreCase))
                {
                    await _inventoryService.DeductItemAsync(playerId, cost.ItemId!, cost.Amount, "秘境进入消耗");
                }
            }

            // 更新每日记录
            if (dailyRecord == null)
            {
                dailyRecord = new DungeonInstanceDailyRecordEntity
                {
                    PlayerId = playerId,
                    DungeonId = dungeonId,
                    EnterDate = today,
                    EnterCount = 1
                };
                await _dailyRecordRepository.AddAsync(dailyRecord);
            }
            else
            {
                dailyRecord.EnterCount++;
                await _dailyRecordRepository.UpdateAsync(dailyRecord);
            }

            // 创建快照
            var snapshot = new DungeonPlayerSnapshot
            {
                Level = user.Level,
                Exp = user.Exp,
                Gold = user.Gold,
                SpiritStone = user.SpiritStone,
                Profession = user.Profession,
                MaxHp = user.Type1,
                MaxMp = user.Type2,
                PhysicalAttack = user.Type3,
                MagicAttack = user.Type4,
                PhysicalDefense = user.Type5,
                MagicDefense = user.Type6,
                Speed = user.Type7,
                HitRate = user.Type8,
                DodgeRate = user.Type9,
                CritRate = user.Type10,
                CritDamage = user.Type11,
                ComboRate = user.Type12,
                CounterRate = user.Type13,
                ArmorBreakRate = user.Type14,
                ExtraDamage = user.Type15,
                SkillIds = user.SkillIds.ToList(),
                PassiveIds = user.PassiveIds.ToList()
            };

            // 创建实例
            var now = DateTime.UtcNow;
            var instance = new DungeonInstanceEntity
            {
                PlayerId = playerId,
                DungeonId = dungeonId,
                Status = (int)DungeonInstanceStatus.Running,
                EnterTime = now,
                LastTickTime = now,
                NextTickTime = now.AddSeconds(template.TickIntervalSeconds),
                CurrentHp = user.Type1 > 0 ? user.Type1 : 100,
                MaxHp = user.Type1 > 0 ? user.Type1 : 100,
                CurrentMp = user.Type2 > 0 ? user.Type2 : 50,
                MaxMp = user.Type2 > 0 ? user.Type2 : 50,
                SnapshotJson = JsonSerializer.Serialize(snapshot),
                ExploreLogJson = JsonSerializer.Serialize(new List<string> { $"[{DateTime.Now:HH:mm:ss}] 进入了 {template.Name}" })
            };
            // 使用 ExecuteReturnIdentity 确保自增ID正确回填
            var newId = await _instanceRepository.Db.Insertable(instance).ExecuteReturnIdentityAsync();
            _logger?.LogInformation("InsertReturnIdentity: {Id}", newId);
            instance.Id = newId;

            // 设置玩家活跃秘境
            user.ActiveDungeonInstanceId = instance.Id.ToString();
            await _userRepository.UpdateAsync(user);

            _logger?.LogInformation("Player {PlayerId} entered dungeon instance {InstanceId} ({DungeonId})", playerId, instance.Id, dungeonId);

            return new DungeonInstanceEnterResponseDto
            {
                Success = true,
                Message = $"成功进入 {template.Name}。",
                InstanceId = instance.Id.ToString(),
                DungeonName = template.Name
            };
        }

        public async Task<DungeonInstanceStatusDto> GetStatusAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
            {
                return new DungeonInstanceStatusDto { InDungeon = false };
            }

            if (!long.TryParse(user.ActiveDungeonInstanceId, out var instanceId))
            {
                return new DungeonInstanceStatusDto { InDungeon = false };
            }

            var instance = await _instanceRepository.GetByIdAsync(instanceId);
            if (instance == null || instance.Status != (int)DungeonInstanceStatus.Running)
            {
                return new DungeonInstanceStatusDto { InDungeon = false };
            }

            var dungeonName = GameData.DungeonInstanceTemplates.TryGetValue(instance.DungeonId, out var tpl)
                ? tpl.Name
                : instance.DungeonId;

            var exploreLog = new List<string>();
            if (!string.IsNullOrEmpty(instance.ExploreLogJson))
            {
                try { exploreLog = JsonSerializer.Deserialize<List<string>>(instance.ExploreLogJson) ?? []; }
                catch { /* ignore */ }
            }

            var allRewards = await _rewardPoolRepository.Db.Queryable<DungeonRewardPoolEntity>()
                .Where(r => r.PlayerId == playerId && r.InstanceId == instance.Id.ToString())
                .ToListAsync();
            var rewardSummary = allRewards
                .GroupBy(r => new { r.RewardType, r.RewardId })
                .Select(g =>
                {
                    var dto = new DungeonRewardSummaryDto
                    {
                        RewardType = g.Key.RewardType,
                        RewardId = g.Key.RewardId,
                        Quantity = g.Sum(x => x.Quantity)
                    };
                    dto.RewardName = dto.RewardType switch
                    {
                        "Item" when !string.IsNullOrEmpty(dto.RewardId)
                            && GameData.Items.TryGetValue(dto.RewardId, out var itemTpl) => itemTpl.Name,
                        "Equipment" when !string.IsNullOrEmpty(dto.RewardId)
                            && int.TryParse(dto.RewardId, out var eqId)
                            && GameData.EquipmentTemplates.TryGetValue(eqId, out var eqTpl) => eqTpl.Name,
                        _ => null
                    };
                    return dto;
                })
                .ToList();

            return new DungeonInstanceStatusDto
            {
                InDungeon = true,
                InstanceId = instance.Id.ToString(),
                DungeonId = instance.DungeonId,
                DungeonName = dungeonName,
                CurrentHp = instance.CurrentHp,
                MaxHp = instance.MaxHp,
                CurrentMp = instance.CurrentMp,
                MaxMp = instance.MaxMp,
                EnterTime = instance.EnterTime,
                LastTickTime = instance.LastTickTime,
                NextTickTime = instance.NextTickTime,
                ExploreLog = exploreLog,
                RewardSummary = rewardSummary,
                PartyId = instance.PartyId,
                IsPartyLeader = instance.IsPartyLeader
            };
        }

        public async Task<DungeonInstanceSettlementDto> QuitAsync(string playerId)
        {
            var playerLock = GetPlayerLock(playerId);
            await playerLock.WaitAsync();
            try
            {
                var user = await _userRepository.GetByIdAsync(playerId);
                if (user == null || string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
                {
                    return new DungeonInstanceSettlementDto { Success = false, Reason = "当前不在秘境中。" };
                }

                if (!long.TryParse(user.ActiveDungeonInstanceId, out var instanceId))
                {
                    return new DungeonInstanceSettlementDto { Success = false, Reason = "实例 ID 无效。" };
                }

                return await SettleCoreAsync(instanceId, (int)DungeonSettleReason.ActiveQuit);
            }
            finally
            {
                playerLock.Release();
            }
        }

        public async Task ProcessTickAsync(string instanceId)
        {
            if (!long.TryParse(instanceId, out var id))
            {
                return;
            }

            var instance = await _instanceRepository.GetByIdAsync(id);
            if (instance == null || instance.Status != (int)DungeonInstanceStatus.Running)
            {
                return;
            }


            // 检查是否需要队长触发（组队时只有队长触发事件）
            if (!string.IsNullOrEmpty(instance.PartyId) && !instance.IsPartyLeader)
            {
                // 检查队长是否还在运行
                var leaderInstance = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                    .Where(i => i.PartyId == instance.PartyId && i.IsPartyLeader)
                    .FirstAsync();

                if (leaderInstance != null && leaderInstance.Status == (int)DungeonInstanceStatus.Running)
                {
                    // 队长还在运行，非队长跳过事件
                    instance.LastTickTime = DateTime.UtcNow;
                    var tickInterval = GetTickInterval(instance.DungeonId);
                    instance.NextTickTime = DateTime.UtcNow.AddSeconds(tickInterval);
                    await _instanceRepository.UpdateAsync(instance);
                    return;
                }

                // 队长不存在或已结算，解除队伍关系
                instance.PartyId = null;
                instance.IsPartyLeader = false;
                await _instanceRepository.UpdateAsync(instance);
            }

            // 检查开放时间
            if (GameData.DungeonInstanceTemplates.TryGetValue(instance.DungeonId, out var template))
            {
                if (!IsInOpenSchedule(template))
                {
                    await SettleCoreAsync(instance.Id, (int)DungeonSettleReason.ForceClose);
                    return;
                }
            }

            // 检查是否有跳过标记
            var skipBuffs = LoadActiveBuffs(instance);
            if (skipBuffs.Any(b => b.BuffType == "Special_SkipTick"))
            {
                skipBuffs.RemoveAll(b => b.BuffType == "Special_SkipTick");
                SaveActiveBuffs(instance, skipBuffs);
                await AppendExploreLogAsync(instance, $"[{DateTime.Now:HH:mm:ss}] 探索被跳过了。");
                // 仍然处理 buff 递减
                if (instance.Status == (int)DungeonInstanceStatus.Running)
                {
                    await ProcessActiveBuffsOnTickAsync(instance, $"[{DateTime.Now:HH:mm:ss}] ");
                }
                instance.LastTickTime = DateTime.UtcNow;
                var tickInterval = GetTickInterval(instance.DungeonId);
                instance.NextTickTime = DateTime.UtcNow.AddSeconds(tickInterval);
                await _instanceRepository.UpdateAsync(instance);
                return;
            }

            // 选择事件
            var eventConfig = await RollEventAsync(instance, instance.DungeonId);
            if (eventConfig == null)
            {
                // 空事件
                await AppendExploreLogAsync(instance, "探索了一番，什么也没发生。");
            }
            else
            {
                await ExecuteEventAsync(instance, eventConfig);
            }

            // 处理 buff 持续效果（DOT/HOT）和 tick 递减
            if (instance.Status == (int)DungeonInstanceStatus.Running)
            {
                await ProcessActiveBuffsOnTickAsync(instance, $"[{DateTime.Now:HH:mm:ss}] ");
            }

            // 更新 tick 时间
            if (instance.Status == (int)DungeonInstanceStatus.Running)
            {
                instance.LastTickTime = DateTime.UtcNow;
                var tickInterval = GetTickInterval(instance.DungeonId);
                instance.NextTickTime = DateTime.UtcNow.AddSeconds(tickInterval);
                await _instanceRepository.UpdateAsync(instance);
            }
        }

        /// <summary>
        /// 强制触发指定事件（测试用，复用真实 tick 流程）。
        /// 跳过随机选事件，直接执行指定 eventId，其余逻辑（buff处理、tick更新）与真实 tick 完全一致。
        /// </summary>
        public async Task<string> ForceEventAsync(string playerId, string eventId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
                return "玩家不在秘境中。";

            if (!long.TryParse(user.ActiveDungeonInstanceId, out var instanceId))
                return "实例ID无效。";

            var instance = await _instanceRepository.GetByIdAsync(instanceId);
            if (instance == null || instance.Status != (int)DungeonInstanceStatus.Running)
                return "秘境实例未在运行。";

            // 查找事件配置
            if (!GameData.DungeonEventConfigsById.TryGetValue(eventId, out var eventConfig))
                return $"事件 {eventId} 不存在。";

            // 记录执行前快照
            var beforeHp = instance.CurrentHp;
            var beforeMp = instance.CurrentMp;
            var beforeBuffs = LoadActiveBuffs(instance);

            // 执行事件（与真实 tick 完全相同的流程）
            await ExecuteEventAsync(instance, eventConfig);

            // 处理 buff 持续效果和 tick 递减
            if (instance.Status == (int)DungeonInstanceStatus.Running)
            {
                await ProcessActiveBuffsOnTickAsync(instance, $"[{DateTime.Now:HH:mm:ss}] ");
            }

            // 更新 tick 时间
            if (instance.Status == (int)DungeonInstanceStatus.Running)
            {
                instance.LastTickTime = DateTime.UtcNow;
                var tickInterval = GetTickInterval(instance.DungeonId);
                instance.NextTickTime = DateTime.UtcNow.AddSeconds(tickInterval);
                await _instanceRepository.UpdateAsync(instance);
            }

            // 重新加载实例获取最新状态
            instance = await _instanceRepository.GetByIdAsync(instanceId);
            var afterBuffs = LoadActiveBuffs(instance);

            // 构建结果报告
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"事件: {eventConfig.Name} ({eventId})");
            sb.AppendLine($"类型: {(DungeonEventType)eventConfig.EventType}");
            sb.AppendLine($"HP: {beforeHp}/{instance.MaxHp} → {instance.CurrentHp}/{instance.MaxHp}");
            sb.AppendLine($"MP: {beforeMp}/{instance.MaxMp} → {instance.CurrentMp}/{instance.MaxMp}");

            // 读取最新日志
            var log = new List<string>();
            if (!string.IsNullOrEmpty(instance.ExploreLogJson))
            {
                try { log = JsonSerializer.Deserialize<List<string>>(instance.ExploreLogJson) ?? []; }
                catch { }
            }
            sb.AppendLine($"日志条数: {log.Count}");
            if (log.Count > 0)
            {
                sb.AppendLine("最新日志:");
                var showCount = Math.Min(5, log.Count);
                for (var i = log.Count - showCount; i < log.Count; i++)
                    sb.AppendLine($"  {log[i]}");
            }

            // buff 变化
            sb.AppendLine($"Buff数量: {beforeBuffs.Count} → {afterBuffs.Count}");
            if (afterBuffs.Count > 0)
            {
                sb.AppendLine("当前Buff:");
                foreach (var b in afterBuffs)
                    sb.AppendLine($"  {b.BuffType} val={b.Value} pct={b.IsPercent} dur={b.RemainingDuration} type={b.DurationType} src={b.Source}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取调试状态快照（测试用，包含buff详情、金币、经验等）。
        /// </summary>
        public async Task<string> GetDebugSnapshotAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
                return "玩家不在秘境中。";

            if (!long.TryParse(user.ActiveDungeonInstanceId, out var instanceId))
                return "实例ID无效。";

            var instance = await _instanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
                return "实例不存在。";

            var buffs = LoadActiveBuffs(instance);
            var log = new List<string>();
            if (!string.IsNullOrEmpty(instance.ExploreLogJson))
            {
                try { log = JsonSerializer.Deserialize<List<string>>(instance.ExploreLogJson) ?? []; }
                catch { }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Status: {(DungeonInstanceStatus)instance.Status}");
            sb.AppendLine($"HP: {instance.CurrentHp}/{instance.MaxHp}");
            sb.AppendLine($"MP: {instance.CurrentMp}/{instance.MaxMp}");
            sb.AppendLine($"Gold: {user.Gold}");
            sb.AppendLine($"Exp: {user.Exp}");
            sb.AppendLine($"SpiritStone: {user.SpiritStone}");
            sb.AppendLine($"PartyId: {instance.PartyId ?? "None"}");
            sb.AppendLine($"IsPartyLeader: {instance.IsPartyLeader}");
            // 战斗属性（基础值，不含buff加成）
            sb.AppendLine($"PAtk: {user.Type3}");
            sb.AppendLine($"MAtk: {user.Type4}");
            sb.AppendLine($"PDef: {user.Type5}");
            sb.AppendLine($"MDef: {user.Type6}");
            sb.AppendLine($"Speed: {user.Type7}");
            sb.AppendLine($"HitRate: {user.Type8}");
            sb.AppendLine($"DodgeRate: {user.Type9}");
            sb.AppendLine($"CritRate: {user.Type10}");
            sb.AppendLine($"ComboRate: {user.Type12}");
            sb.AppendLine($"CounterRate: {user.Type13}");
            sb.AppendLine($"BuffCount: {buffs.Count}");
            if (buffs.Count > 0)
            {
                sb.AppendLine("Buffs:");
                foreach (var b in buffs)
                    sb.AppendLine($"  {b.BuffType} val={b.Value} pct={b.IsPercent} dur={b.RemainingDuration} type={b.DurationType} src={b.Source}");
            }
            sb.AppendLine($"LogCount: {log.Count}");
            if (log.Count > 0)
            {
                sb.AppendLine("Logs:");
                foreach (var l in log)
                    sb.AppendLine($"  {l}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取战斗属性调试信息（测试用，显示buff加成后的实际战斗属性）。
        /// </summary>
        public async Task<string> GetDebugFighterStatsAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || string.IsNullOrEmpty(user.ActiveDungeonInstanceId))
                return "玩家不在秘境中。";

            if (!long.TryParse(user.ActiveDungeonInstanceId, out var instanceId))
                return "实例ID无效。";

            var instance = await _instanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
                return "实例不存在。";

            var snapshot = LoadSnapshot(instance);
            var buffs = LoadActiveBuffs(instance);

            // 基础属性（来自快照）
            var basePAtk = snapshot.PhysicalAttack;
            var baseMAtk = snapshot.MagicAttack;
            var basePDef = snapshot.PhysicalDefense;
            var baseMDef = snapshot.MagicDefense;
            var baseSpd = snapshot.Speed;
            var baseHit = snapshot.HitRate;
            var baseDodge = snapshot.DodgeRate;
            var baseCrit = snapshot.CritRate;
            var baseCombo = snapshot.ComboRate;
            var baseCounter = snapshot.CounterRate;

            // 计算 buff 加成（与 ApplySnapshotAndBuffs 逻辑一致）
            int pAtkBonus = 0, mAtkBonus = 0, pDefBonus = 0, mDefBonus = 0, spdBonus = 0;
            float hitBonus = 0, dodgeBonus = 0, critBonus = 0, comboBonus = 0, counterBonus = 0;

            foreach (var buff in buffs)
            {
                if (buff.RemainingDuration <= 0) continue;
                var v = buff.Value;
                var isPct = buff.IsPercent;

                switch (buff.BuffType)
                {
                    case "AttackUp":
                        pAtkBonus += isPct ? (int)(basePAtk * v / 100.0) : v;
                        mAtkBonus += isPct ? (int)(baseMAtk * v / 100.0) : v;
                        break;
                    case "DefenseUp":
                        pDefBonus += isPct ? (int)(basePDef * v / 100.0) : v;
                        mDefBonus += isPct ? (int)(baseMDef * v / 100.0) : v;
                        break;
                    case "SpeedUp": spdBonus += isPct ? (int)(baseSpd * v / 100.0) : v; break;
                    case "CritRateUp": critBonus += isPct ? v / 100f : v; break;
                    case "HitRateUp": hitBonus += isPct ? v / 100f : v; break;
                    case "DodgeRateUp": dodgeBonus += isPct ? v / 100f : v; break;
                    case "ComboRateUp": comboBonus += isPct ? v / 100f : v; break;
                    case "CounterRateUp": counterBonus += isPct ? v / 100f : v; break;
                    case "AllStatsUp":
                        if (isPct)
                        {
                            pAtkBonus += (int)(basePAtk * v / 100.0);
                            mAtkBonus += (int)(baseMAtk * v / 100.0);
                            pDefBonus += (int)(basePDef * v / 100.0);
                            mDefBonus += (int)(baseMDef * v / 100.0);
                            spdBonus += (int)(baseSpd * v / 100.0);
                        }
                        else
                        {
                            pAtkBonus += v; mAtkBonus += v;
                            pDefBonus += v; mDefBonus += v;
                            spdBonus += v;
                        }
                        break;
                    case "AttackDown":
                        pAtkBonus -= isPct ? (int)(basePAtk * v / 100.0) : v;
                        mAtkBonus -= isPct ? (int)(baseMAtk * v / 100.0) : v;
                        break;
                    case "DefenseDown":
                        pDefBonus -= isPct ? (int)(basePDef * v / 100.0) : v;
                        mDefBonus -= isPct ? (int)(baseMDef * v / 100.0) : v;
                        break;
                    case "SpeedDown": spdBonus -= isPct ? (int)(baseSpd * v / 100.0) : v; break;
                    case "CritRateDown": critBonus -= isPct ? v / 100f : v; break;
                    case "AllStatsDown":
                        if (isPct)
                        {
                            pAtkBonus -= (int)(basePAtk * v / 100.0);
                            mAtkBonus -= (int)(baseMAtk * v / 100.0);
                            pDefBonus -= (int)(basePDef * v / 100.0);
                            mDefBonus -= (int)(baseMDef * v / 100.0);
                            spdBonus -= (int)(baseSpd * v / 100.0);
                        }
                        else
                        {
                            pAtkBonus -= v; mAtkBonus -= v;
                            pDefBonus -= v; mDefBonus -= v;
                            spdBonus -= v;
                        }
                        break;
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 基础属性 (来自快照) ===");
            sb.AppendLine($"PAtk: {basePAtk}");
            sb.AppendLine($"MAtk: {baseMAtk}");
            sb.AppendLine($"PDef: {basePDef}");
            sb.AppendLine($"MDef: {baseMDef}");
            sb.AppendLine($"Speed: {baseSpd}");
            sb.AppendLine($"HitRate: {baseHit}");
            sb.AppendLine($"DodgeRate: {baseDodge}");
            sb.AppendLine($"CritRate: {baseCrit}");
            sb.AppendLine($"ComboRate: {baseCombo}");
            sb.AppendLine($"CounterRate: {baseCounter}");
            sb.AppendLine("=== Buff加成 ===");
            sb.AppendLine($"PAtkBonus: +{pAtkBonus}");
            sb.AppendLine($"MAtkBonus: +{mAtkBonus}");
            sb.AppendLine($"PDefBonus: +{pDefBonus}");
            sb.AppendLine($"MDefBonus: +{mDefBonus}");
            sb.AppendLine($"SpdBonus: +{spdBonus}");
            sb.AppendLine($"HitBonus: +{hitBonus}");
            sb.AppendLine($"DodgeBonus: +{dodgeBonus}");
            sb.AppendLine($"CritBonus: +{critBonus}");
            sb.AppendLine($"ComboBonus: +{comboBonus}");
            sb.AppendLine($"CounterBonus: +{counterBonus}");
            sb.AppendLine("=== 战斗中实际属性 ===");
            sb.AppendLine($"PAtk: {basePAtk} + {pAtkBonus} = {Math.Max(1, basePAtk + pAtkBonus)}");
            sb.AppendLine($"MAtk: {baseMAtk} + {mAtkBonus} = {Math.Max(1, baseMAtk + mAtkBonus)}");
            sb.AppendLine($"PDef: {basePDef} + {pDefBonus} = {Math.Max(0, basePDef + pDefBonus)}");
            sb.AppendLine($"MDef: {baseMDef} + {mDefBonus} = {Math.Max(0, baseMDef + mDefBonus)}");
            sb.AppendLine($"Speed: {baseSpd} + {spdBonus} = {Math.Max(1, baseSpd + spdBonus)}");
            return sb.ToString();
        }

        /// <summary>
        /// 强制将两个玩家组队（测试用）。调用方为队长，targetPlayerId 为队友。
        /// </summary>
        public async Task<string> ForcePartyAsync(string leaderPlayerId, string targetPlayerId)
        {
            var leaderUser = await _userRepository.GetByIdAsync(leaderPlayerId);
            if (leaderUser == null || string.IsNullOrEmpty(leaderUser.ActiveDungeonInstanceId))
                return "队长不在秘境中。";

            var targetUser = await _userRepository.GetByIdAsync(targetPlayerId);
            if (targetUser == null || string.IsNullOrEmpty(targetUser.ActiveDungeonInstanceId))
                return "目标玩家不在秘境中。";

            if (!long.TryParse(leaderUser.ActiveDungeonInstanceId, out var leaderInstanceId) ||
                !long.TryParse(targetUser.ActiveDungeonInstanceId, out var targetInstanceId))
                return "实例ID无效。";

            var leaderInstance = await _instanceRepository.GetByIdAsync(leaderInstanceId);
            var targetInstance = await _instanceRepository.GetByIdAsync(targetInstanceId);

            if (leaderInstance == null || leaderInstance.Status != (int)DungeonInstanceStatus.Running)
                return "队长实例未在运行。";
            if (targetInstance == null || targetInstance.Status != (int)DungeonInstanceStatus.Running)
                return "目标实例未在运行。";

            // 创建队伍
            var party = new DungeonPartyEntity
            {
                PartyId = Guid.NewGuid().ToString("N"),
                LeaderPlayerId = leaderPlayerId,
                MemberJson = JsonSerializer.Serialize(new List<string> { leaderPlayerId, targetPlayerId }),
                CreateTime = DateTime.UtcNow
            };
            await _partyRepository.AddAsync(party);

            // 更新两个实例
            leaderInstance.PartyId = party.PartyId;
            leaderInstance.IsPartyLeader = true;
            await _instanceRepository.UpdateAsync(leaderInstance);

            targetInstance.PartyId = party.PartyId;
            targetInstance.IsPartyLeader = false;
            await _instanceRepository.UpdateAsync(targetInstance);

            return $"组队成功。PartyId={party.PartyId}";
        }

        public async Task SettleAsync(string instanceId, int settleReason)
        {
            if (long.TryParse(instanceId, out var id))
            {
                await SettleCoreAsync(id, settleReason);
            }
        }

        private async Task<DungeonInstanceSettlementDto> SettleCoreAsync(long instanceId, int settleReason)
        {
            var instance = await _instanceRepository.GetByIdAsync(instanceId);
            if (instance == null)
            {
                return new DungeonInstanceSettlementDto { Success = false, Reason = "实例不存在。" };
            }

            if (instance.Status != (int)DungeonInstanceStatus.Running)
            {
                return new DungeonInstanceSettlementDto { Success = false, Reason = "秘境已结算。" };
            }

            // 先写结算日志（此时 instance 仍是 Running，GetStatusAsync 才能返回日志）
            var reasonText = settleReason switch
            {
                (int)DungeonSettleReason.ActiveQuit => "主动退出",
                (int)DungeonSettleReason.Death => "角色死亡",
                (int)DungeonSettleReason.ForceClose => "秘境关闭",
                _ => "未知原因"
            };
            await AppendExploreLogAsync(instance, $"[{DateTime.Now:HH:mm:ss}] 秘境结束：{reasonText}。");

            // 幂等：标记结算
            instance.Status = (int)DungeonInstanceStatus.Settled;
            instance.SettleTime = DateTime.UtcNow;
            instance.SettleReason = settleReason;
            await _instanceRepository.UpdateAsync(instance);

            // 解散队伍（如果有）
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var currentPartyId = instance.PartyId;
                if (instance.IsPartyLeader)
                {
                    // 队长离开 → 解散整个队伍
                    await DisbandPartyAsync(currentPartyId);
                }
                else
                {
                    // 队友离开 → 只移除自己，队伍继续
                    instance.PartyId = null;
                    instance.IsPartyLeader = false;
                    await _instanceRepository.UpdateAsync(instance);

                    // 更新 MemberJson
                    var remaining = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                        .Where(i => i.PartyId == currentPartyId && i.Status == (int)DungeonInstanceStatus.Running)
                        .ToListAsync();
                    if (remaining.Count <= 1)
                    {
                        await DisbandPartyAsync(currentPartyId);
                    }
                    else
                    {
                        var party = await _partyRepository.GetByIdAsync(currentPartyId);
                        if (party != null)
                        {
                            party.MemberJson = JsonSerializer.Serialize(remaining.Select(m => m.PlayerId).ToList());
                            await _partyRepository.UpdateAsync(party);
                        }
                    }
                }
            }

            // 根据结算原因筛选可发放收益
            var isDeath = settleReason == (int)DungeonSettleReason.Death;
            var rewards = await _rewardPoolRepository.Db.Queryable<DungeonRewardPoolEntity>()
                .Where(r => r.PlayerId == instance.PlayerId && r.InstanceId == instance.Id.ToString())
                .ToListAsync();

            var grantableRewards = isDeath
                ? rewards.Where(r => r.DeathKeep).ToList()
                : rewards;

            // 发放收益
            var grantedList = new List<DungeonRewardGrantDto>();
            long totalGold = 0, totalExp = 0, totalSpiritStone = 0;

            // 读取奖励倍率 buff
            var rewardMultiplier = 1;
            var settleBuffs = LoadActiveBuffs(instance);
            foreach (var buff in settleBuffs)
            {
                if (buff.BuffType == "Special_RewardMultiplier")
                    rewardMultiplier = Math.Max(rewardMultiplier, buff.Value);
            }

            foreach (var reward in grantableRewards)
            {
                var grant = new DungeonRewardGrantDto
                {
                    RewardType = reward.RewardType,
                    RewardId = reward.RewardId,
                    Quantity = reward.Quantity,
                    RewardName = reward.RewardType switch
                    {
                        "Item" when !string.IsNullOrEmpty(reward.RewardId)
                            && GameData.Items.TryGetValue(reward.RewardId, out var itemTpl) => itemTpl.Name,
                        "Equipment" when !string.IsNullOrEmpty(reward.RewardId)
                            && int.TryParse(reward.RewardId, out var eqId)
                            && GameData.EquipmentTemplates.TryGetValue(eqId, out var eqTpl) => eqTpl.Name,
                        _ => null
                    }
                };

                try
                {
                    switch (reward.RewardType)
                    {
                        case "Gold":
                            {
                                var goldQty = reward.Quantity * rewardMultiplier;
                                await _playerAttributeService.AddGoldAsync(instance.PlayerId, goldQty, "秘境结算");
                                totalGold += goldQty;
                                grant.Quantity = goldQty;
                                grant.Granted = true;
                            }
                            break;
                        case "Exp":
                            {
                                var expQty = reward.Quantity * rewardMultiplier;
                                await _playerAttributeService.AddExpAsync(instance.PlayerId, expQty);
                                totalExp += expQty;
                                grant.Quantity = expQty;
                                grant.Granted = true;
                            }
                            break;
                        case "SpiritStone":
                            {
                                var spiritQty = reward.Quantity * rewardMultiplier;
                                var spiritUser = await _userRepository.GetByIdAsync(instance.PlayerId);
                                if (spiritUser != null)
                                {
                                    spiritUser.SpiritStone += spiritQty;
                                    await _userRepository.UpdateAsync(spiritUser);
                                }
                                totalSpiritStone += spiritQty;
                                grant.Quantity = spiritQty;
                                grant.Granted = true;
                            }
                            break;
                        case "Item":
                            if (!string.IsNullOrEmpty(reward.RewardId))
                            {
                                await _inventoryService.AddItemAsync(instance.PlayerId, new AddItemRequestDto
                                {
                                    ItemId = reward.RewardId,
                                    Quantity = reward.Quantity
                                });
                                grant.Granted = true;
                            }
                            break;
                        case "Equipment":
                            // 装备掉落已在 tick 中直接入库
                            grant.Granted = true;
                            break;
                        default:
                            grant.Granted = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    grant.Granted = false;
                    grant.FailReason = ex.Message;
                    _logger?.LogWarning(ex, "Failed to grant reward {RewardType} to player {PlayerId}", reward.RewardType, instance.PlayerId);
                }

                grantedList.Add(grant);
            }

            // 秘境装备会在探索过程中直接入库，最终结算时按本次进入时间统一清理。
            var autoSell = await _equipmentService.AutoSellUnqualifiedEquipmentAsync(
                instance.PlayerId,
                acquiredAfter: instance.EnterTime,
                reason: "秘境最终结算自动出售");
            if (autoSell.SoldCount > 0)
            {
                totalGold += autoSell.GoldGained;
                await AppendExploreLogAsync(instance, $"自动出售 {autoSell.SoldCount} 件不符合保留条件的装备，获得 {autoSell.GoldGained} 金币。");
            }

            // 清除玩家活跃秘境
            var user = await _userRepository.GetByIdAsync(instance.PlayerId);
            if (user != null)
            {
                user.ActiveDungeonInstanceId = null;
                await _userRepository.UpdateAsync(user);
            }

            _logger?.LogInformation(
                "Dungeon instance {InstanceId} settled. Reason={Reason}, PlayerId={PlayerId}, Rewards={RewardCount}",
                instanceId, reasonText, instance.PlayerId, grantableRewards.Count);

            return new DungeonInstanceSettlementDto
            {
                Success = true,
                Reason = reasonText,
                GrantedRewards = grantedList,
                TotalGold = totalGold,
                TotalExp = totalExp,
                TotalSpiritStone = totalSpiritStone
            };
        }

        #region 事件执行

        private async Task<DungeonEventConfigEntity?> RollEventAsync(DungeonInstanceEntity instance, string dungeonId)
        {
            if (!GameData.DungeonInstanceTemplates.TryGetValue(dungeonId, out var template))
            {
                return null;
            }

            if (string.IsNullOrEmpty(template.EventGroupId))
            {
                return null;
            }

            // 从事件组中按权重抽取
            if (!GameData.DungeonEventGroups.TryGetValue(template.EventGroupId, out var group))
            {
                return null;
            }

            var items = ParseGroupItems(group.GroupItemsJson);
            if (items.Count == 0)
            {
                return null;
            }

            // 读取 Special_ 系列 buff 来修改权重
            var buffs = LoadActiveBuffs(instance);
            var weightMultiplier = 1.0;
            var boostEventTypes = new HashSet<int>();
            var rareBoost = 0;
            var trapBoost = 0;

            foreach (var buff in buffs)
            {
                switch (buff.BuffType)
                {
                    case "Special_WeightModifier":
                        weightMultiplier = buff.Value / 100.0;
                        // boostTypes 编码在 Source 中（逗号分隔）
                        if (!string.IsNullOrEmpty(buff.Source))
                        {
                            foreach (var part in buff.Source.Split(','))
                                if (int.TryParse(part.Trim(), out var bt)) boostEventTypes.Add(bt);
                        }
                        break;
                    case "Special_RareBoost":
                        rareBoost = buff.Value;
                        break;
                    case "Special_TrapBoost":
                        trapBoost = buff.Value;
                        break;
                }
            }

            // 应用权重修改
            var weightedItems = items.Select(item =>
            {
                var w = item.Weight;
                // 获取事件类型
                if (GameData.DungeonEventConfigsById.TryGetValue(item.EventId, out var evt))
                {
                    var evtType = evt.EventType;
                    // boostTypes 加成
                    if (boostEventTypes.Contains(evtType))
                        w = (int)(w * weightMultiplier);
                    // rareBoost 加成（事件类型 5=Resource, 6=Treasure, 7=Adventure）
                    if (rareBoost > 0 && (evtType == 5 || evtType == 6 || evtType == 7))
                        w = (int)(w * (1 + rareBoost / 100.0));
                    // trapBoost 加成（事件类型 4=Debuff）
                    if (trapBoost > 0 && evtType == 4)
                        w = (int)(w * (1 + trapBoost / 100.0));
                }
                return (item.EventId, Weight: Math.Max(1, w));
            })
            // 无效或已禁用的事件不占用万分比区间，剩余区间交给空事件兜底。
            .Where(item => GameData.DungeonEventConfigsById.TryGetValue(item.EventId, out var evt) && evt.Enabled)
            .ToList();

            const int probabilityScale = 10000;
            var totalWeight = weightedItems.Sum(i => i.Weight);
            if (totalWeight <= 0)
            {
                return PickConfiguredEmptyEvent(items);
            }

            // 事件权重统一按万分比解释：随机范围始终固定为 0~9999。
            // 当配置总和小于 10000 时，未覆盖的区间保留为“无事发生”。
            // 当临时 Buff 将总和推高到 10000 以上时，按比例压回 10000，避免超出区间被截断。
            var normalizedItems = totalWeight <= probabilityScale
                ? weightedItems
                : weightedItems
                    .Select(item => (item.EventId, Weight: Math.Max(1, (int)Math.Round(item.Weight * (double)probabilityScale / totalWeight))))
                    .ToList();

            var normalizedTotalWeight = normalizedItems.Sum(i => i.Weight);
            var roll = Random.Shared.Next(0, probabilityScale);
            var cumulative = 0;
            foreach (var item in normalizedItems)
            {
                cumulative += item.Weight;
                if (roll < cumulative)
                {
                    // 查找事件配置
                    if (GameData.DungeonEventConfigsById.TryGetValue(item.EventId, out var evt))
                    {
                        return evt;
                    }
                    return PickConfiguredEmptyEvent(items);
                }
            }

            // 未命中任何非空事件时，随机返回当前事件组中的一个空事件。
            return PickConfiguredEmptyEvent(items);
        }

        private static DungeonEventConfigEntity? PickConfiguredEmptyEvent(List<(string EventId, int Weight)> items)
        {
            var emptyEvents = items
                .Select(item => GameData.DungeonEventConfigsById.TryGetValue(item.EventId, out var evt) ? evt : null)
                .Where(evt => evt != null && evt.Enabled && evt.EventType == (int)DungeonEventType.Empty)
                .Cast<DungeonEventConfigEntity>()
                .ToList();

            if (emptyEvents.Count == 0)
            {
                return null;
            }

            return emptyEvents[Random.Shared.Next(emptyEvents.Count)];
        }

        private static List<(string EventId, int Weight)> ParseGroupItems(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];
            try
            {
                using var doc = JsonDocument.Parse(json);
                var result = new List<(string EventId, int Weight)>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    var eventId = item.GetProperty("eventId").GetString() ?? "";
                    var weight = item.GetProperty("weight").GetInt32();
                    if (!string.IsNullOrWhiteSpace(eventId) && weight > 0)
                    {
                        result.Add((eventId, weight));
                    }
                }
                return result;
            }
            catch
            {
                return [];
            }
        }

        private async Task ExecuteEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig)
        {
            var eventType = (DungeonEventType)eventConfig.EventType;
            var logPrefix = $"[{DateTime.Now:HH:mm:ss}] ";

            switch (eventType)
            {
                case DungeonEventType.Battle:
                    await ExecuteBattleEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Heal:
                    await ExecuteHealEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Buff:
                    await ExecuteBuffEventAsync(instance, eventConfig, logPrefix, isDebuff: false);
                    break;
                case DungeonEventType.Debuff:
                    await ExecuteBuffEventAsync(instance, eventConfig, logPrefix, isDebuff: true);
                    break;
                case DungeonEventType.Resource:
                    await ExecuteResourceEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Treasure:
                    await ExecuteTreasureEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Adventure:
                    await ExecuteAdventureEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Shop:
                    await ExecuteShopEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Event:
                    await ExecuteSpecialEventAsync(instance, eventConfig, logPrefix);
                    break;
                case DungeonEventType.Empty:
                    var emptyMsg = FormatEventLog(logPrefix, eventConfig, "");
                    await AppendExploreLogAsync(instance, emptyMsg);
                    await AppendPartyLogAsync(instance, emptyMsg);
                    break;
                case DungeonEventType.PlayerEncounter:
                    await ExecutePlayerEncounterAsync(instance, eventConfig, logPrefix);
                    break;
            }
        }

        private async Task ExecuteBattleEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var monsterIds = new List<string>();

            if (eventData.TryGetProperty("monsterId", out var singleMonster))
            {
                var mid = singleMonster.GetString() ?? "";
                var qty = 1;
                if (eventData.TryGetProperty("quantity", out var qtyProp) && qtyProp.TryGetInt32(out var q))
                    qty = Math.Clamp(q, 1, 20);
                for (var i = 0; i < qty; i++) monsterIds.Add(mid);
            }
            else if (eventData.TryGetProperty("monsterIds", out var monsterArray))
            {
                foreach (var id in monsterArray.EnumerateArray())
                {
                    monsterIds.Add(id.GetString() ?? "");
                }
            }

            if (monsterIds.Count == 0)
            {
                var noEnemyMsg = $"{logPrefix}{eventConfig.Name}：未发现敌人。";
                await AppendExploreLogAsync(instance, noEnemyMsg);
                await AppendPartyLogAsync(instance, noEnemyMsg);
                return;
            }

            // 创建怪物实体
            var monsters = new List<MonsterEntity>();
            foreach (var monsterId in monsterIds)
            {
                if (GameData.MonsterTemplates.TryGetValue(monsterId, out var template))
                {
                    monsters.Add(CreateMonsterFromTemplate(template));
                }
            }

            if (monsters.Count == 0)
            {
                var configErrMsg = $"{logPrefix}{eventConfig.Name}：怪物配置错误。";
                await AppendExploreLogAsync(instance, configErrMsg);
                await AppendPartyLogAsync(instance, configErrMsg);
                return;
            }

            // 获取玩家
            var user = await _userRepository.GetByIdAsync(instance.PlayerId);
            if (user == null)
            {
                return;
            }

            // 应用快照属性 + buff 修正（不影响真实数据）
            var snapshot = LoadSnapshot(instance);
            var activeBuffs = LoadActiveBuffs(instance);
            var originalStats = ApplySnapshotAndBuffs(user, snapshot, activeBuffs, instance);

            // 检查 Special_MonsterBuff：怪物属性加成
            var monsterBuffValue = 0;
            foreach (var buff in activeBuffs)
            {
                if (buff.BuffType == "Special_MonsterBuff") monsterBuffValue += buff.Value;
            }
            if (monsterBuffValue > 0)
            {
                foreach (var monster in monsters)
                {
                    // Type1=HP, Type2=MP, Type3=物攻, Type4=法攻, Type5=物防, Type6=法防, Type7=速度
                    monster.Type1 = monster.Type1 * (100 + monsterBuffValue) / 100;
                    monster.Type3 = monster.Type3 * (100 + monsterBuffValue) / 100;
                    monster.Type4 = monster.Type4 * (100 + monsterBuffValue) / 100;
                    monster.Type5 = monster.Type5 * (100 + monsterBuffValue) / 100;
                    monster.Type6 = monster.Type6 * (100 + monsterBuffValue) / 100;
                }
            }

            // 检查 Special_SummonClone：召唤分身
            var cloneAttackPercent = 0;
            var cloneHpPercent = 0;
            foreach (var buff in activeBuffs)
            {
                if (buff.BuffType == "Special_SummonClone")
                {
                    cloneAttackPercent = buff.Value;
                    cloneHpPercent = buff.IsPercent ? 50 : buff.Value;
                }
            }

            // 如果有队伍，获取队友一起战斗
            var fighters = new List<UserEntity> { user };
            var partyOriginalStats = new List<(string Gid, (int[] Stats, List<string> OriginalSkills, List<string> OriginalPassives) Original)>();
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var partyMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in partyMembers)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    {
                        var memberUser = await _userRepository.GetByIdAsync(member.PlayerId);
                        if (memberUser != null)
                        {
                            var memberSnapshot = LoadSnapshot(member);
                            var memberBuffs = LoadActiveBuffs(member);
                            var memberOriginal = ApplySnapshotAndBuffs(memberUser, memberSnapshot, memberBuffs, member);
                            partyOriginalStats.Add((memberUser.GID, memberOriginal));
                            fighters.Add(memberUser);
                        }
                    }
                }
            }

            // 添加召唤分身作为虚拟战斗者
            if (cloneAttackPercent > 0)
            {
                // Type1=HP, Type2=MP, Type3=物攻, Type4=法攻, Type5=物防, Type6=法防, Type7=速度
                var cloneHp = snapshot.MaxHp * cloneHpPercent / 100;
                var clone = new UserEntity
                {
                    GID = $"clone_{instance.Id}",
                    Name = $"{user.Name}的分身",
                    Type1 = cloneHp,                                    // HP
                    Type2 = snapshot.MaxMp * cloneHpPercent / 100,      // MP
                    Type3 = Math.Max(1, snapshot.PhysicalAttack * cloneAttackPercent / 100),  // 物攻
                    Type4 = Math.Max(1, snapshot.MagicAttack * cloneAttackPercent / 100),     // 法攻
                    Type5 = snapshot.PhysicalDefense * cloneAttackPercent / 100,               // 物防
                    Type6 = snapshot.MagicDefense * cloneAttackPercent / 100,                  // 法防
                    Type7 = snapshot.Speed,                             // 速度不变
                    Level = user.Level,
                    SkillIds = user.SkillIds.ToList(),
                    PassiveIds = user.PassiveIds.ToList()
                };
                fighters.Add(clone);
            }

            var result = BattleSystem.StartBattle(fighters, monsters, enableElementAdvantage: true);

            // 战斗后恢复真实属性
            RestoreOriginalStats(user, originalStats);
            foreach (var (gid, original) in partyOriginalStats)
            {
                var memberFighter = fighters.FirstOrDefault(f => f.GID == gid);
                if (memberFighter != null) RestoreOriginalStats(memberFighter, original);
            }

            // 战斗后递减 battle 类型 buff（队长 + 队友）
            await ProcessActiveBuffsOnBattleAsync(instance);
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var buffPartyMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in buffPartyMembers)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    {
                        await ProcessActiveBuffsOnBattleAsync(member);
                    }
                }
            }

            // 更新 HP/MP（主实例）
            if (result.FighterSkillStats.TryGetValue(user.GID, out var mainFighter))
            {
                instance.CurrentHp = Math.Clamp(instance.CurrentHp - mainFighter.TotalDamageTaken + mainFighter.TotalHealingReceived, 0, instance.MaxHp);
                instance.CurrentMp = Math.Max(0, instance.CurrentMp - mainFighter.TotalMpConsumed);
            }
            else if (!result.IsVictory)
            {
                instance.CurrentHp = Math.Max(1, instance.MaxHp / 10);
            }

            // 更新队友 HP/MP
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var partyMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in partyMembers)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    {
                        var memberUser = await _userRepository.GetByIdAsync(member.PlayerId);
                        if (memberUser != null && result.FighterSkillStats.TryGetValue(memberUser.GID, out var memberFighter))
                        {
                            member.CurrentHp = Math.Clamp(member.CurrentHp - memberFighter.TotalDamageTaken + memberFighter.TotalHealingReceived, 0, member.MaxHp);
                            member.CurrentMp = Math.Max(0, member.CurrentMp - memberFighter.TotalMpConsumed);
                        }
                        else if (!result.IsVictory)
                        {
                            member.CurrentHp = Math.Max(1, member.MaxHp / 10);
                        }
                        await _instanceRepository.UpdateAsync(member);
                    }
                }
            }

            var battleLogMsg = "";
            if (result.IsVictory)
            {
                // 收益入池（有队伍时平分，无队伍时全额）
                var goldForCaptain = result.GoldGained;
                var expForCaptain = result.ExpGained;
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    var rewardMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                    var partyCount = rewardMembers.Count(m => m.Status == (int)DungeonInstanceStatus.Running);
                    _logger?.LogInformation("[REWARD-SPLIT] PartyId={PartyId} partyCount={PartyCount} GoldGained={Gold} ExpGained={Exp}", instance.PartyId, partyCount, result.GoldGained, result.ExpGained);
                    if (partyCount > 1)
                    {
                        goldForCaptain = result.GoldGained / partyCount;
                        expForCaptain = result.ExpGained / partyCount;
                        foreach (var member in rewardMembers)
                        {
                            if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                            {
                                if (goldForCaptain > 0)
                                    await AddRewardAsync(member, "Gold", null, goldForCaptain, "Battle", eventConfig.DeathKeep ?? false);
                                if (expForCaptain > 0)
                                    await AddRewardAsync(member, "Exp", null, expForCaptain, "Battle", eventConfig.DeathKeep ?? false);
                            }
                        }
                    }
                }
                if (goldForCaptain > 0)
                    await AddRewardAsync(instance, "Gold", null, goldForCaptain, "Battle", eventConfig.DeathKeep ?? false);
                if (expForCaptain > 0)
                    await AddRewardAsync(instance, "Exp", null, expForCaptain, "Battle", eventConfig.DeathKeep ?? false);

                battleLogMsg = FormatEventLog(logPrefix, eventConfig, $"战斗胜利！击败了 {monsters.Count} 只怪物，获得 {expForCaptain} 经验、{goldForCaptain} 金币。");
                await AppendExploreLogAsync(instance, battleLogMsg);

                // 物品/装备掉落：随机分配给一个队友（如果有队伍），否则给队长
                foreach (var item in result.DroppedItems)
                {
                    var target = await PickRandomPartyMemberAsync(instance) ?? instance;
                    await AddRewardAsync(target, "Item", item.ItemId, 1, "Battle", eventConfig.DeathKeep ?? false);
                }
                foreach (var equip in result.DroppedEquipments)
                {
                    var target = await PickRandomPartyMemberAsync(instance) ?? instance;
                    await AddRewardAsync(target, "Equipment", equip.EquipmentId.ToString(), 1, "Battle", eventConfig.DeathKeep ?? false);
                }

                // 战斗后自动用药
                await TryAutoMedicineAsync(instance);

                // 队伍日志同步
                await AppendPartyLogAsync(instance, battleLogMsg);
            }
            else
            {
                battleLogMsg = FormatEventLog(logPrefix, eventConfig, "战斗失败，受到了重创。");
                await AppendExploreLogAsync(instance, battleLogMsg);
                instance.CurrentHp = Math.Max(1, instance.MaxHp / 10); // 战败保留 10% HP
                await AppendPartyLogAsync(instance, battleLogMsg);
            }

            await _instanceRepository.UpdateAsync(instance);

            // 检查队友死亡
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var deathCheckMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in deathCheckMembers)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running && member.CurrentHp <= 0)
                    {
                        if (await TryReviveOnce(member))
                        {
                            await AppendExploreLogAsync(member, $"{logPrefix}凤凰之羽生效，你从死亡中复活！");
                            await _instanceRepository.UpdateAsync(member);
                        }
                        else
                        {
                            await SettleCoreAsync(member.Id, (int)DungeonSettleReason.Death);
                            await RemoveFromPartyAsync(member);
                        }
                    }
                }
            }

            // 检查队长死亡
            if (instance.CurrentHp <= 0)
            {
                if (await TryReviveOnce(instance))
                {
                    await AppendExploreLogAsync(instance, $"{logPrefix}凤凰之羽生效，你从死亡中复活！");
                    await _instanceRepository.UpdateAsync(instance);
                }
                else
                {
                    // 队长死亡，解散整个队伍
                    if (!string.IsNullOrEmpty(instance.PartyId))
                    {
                        await DisbandPartyAsync(instance.PartyId);
                    }
                    await SettleCoreAsync(instance.Id, (int)DungeonSettleReason.Death);
                }
            }
        }

        private async Task ExecuteHealEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);

            long healHp = 0, healMp = 0;

            if (eventData.TryGetProperty("healPercent", out var percentProp))
            {
                var percent = percentProp.GetInt32();
                var target = "hp";
                if (eventData.TryGetProperty("target", out var targetProp))
                {
                    target = targetProp.GetString() ?? "hp";
                }

                if (target.Contains("hp", StringComparison.OrdinalIgnoreCase))
                {
                    healHp = instance.MaxHp * percent / 100;
                }
                if (target.Contains("mp", StringComparison.OrdinalIgnoreCase))
                {
                    healMp = instance.MaxMp * percent / 100;
                }
            }

            var oldHp = instance.CurrentHp;
            var oldMp = instance.CurrentMp;
            instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + healHp);
            instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + healMp);
            var actualHp = instance.CurrentHp - oldHp;
            var actualMp = instance.CurrentMp - oldMp;

            // 队伍共享回复
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in members)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    {
                        member.CurrentHp = Math.Min(member.MaxHp, member.CurrentHp + healHp);
                        member.CurrentMp = Math.Min(member.MaxMp, member.CurrentMp + healMp);
                        await _instanceRepository.UpdateAsync(member);
                    }
                }
            }

            if (actualHp == 0 && actualMp == 0)
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "生命和灵力充沛，无需恢复。"));
                // 即使队长满血，也给队友写日志（队友可能需要治疗）
                if (!string.IsNullOrEmpty(instance.PartyId))
                    await AppendPartyLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "生命和灵力充沛，无需恢复。"));
            }
            else
            {
                var healMsg = FormatEventLog(logPrefix, eventConfig, $"恢复了 {actualHp} 生命、{actualMp} 法力。");
                await AppendExploreLogAsync(instance, healMsg);
                await AppendPartyLogAsync(instance, healMsg);
            }
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task ExecuteBuffEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix, bool isDebuff)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var buffType = "";
            var value = 0;
            var isPercent = false;
            var duration = 0;
            var durationType = "battle";
            var target = "hp";

            if (eventData.TryGetProperty(isDebuff ? "debuffType" : "buffType", out var typeProp))
                buffType = typeProp.GetString() ?? "";
            if (eventData.TryGetProperty("value", out var valueProp))
                value = valueProp.GetInt32();
            if (eventData.TryGetProperty("isPercent", out var pctProp))
                isPercent = pctProp.GetBoolean();
            if (eventData.TryGetProperty("duration", out var durProp))
                duration = durProp.GetInt32();
            if (eventData.TryGetProperty("durationType", out var dtProp))
                durationType = dtProp.GetString() ?? "battle";
            if (eventData.TryGetProperty("target", out var targetProp))
                target = targetProp.GetString() ?? "hp";

            // 即时伤害（DamagePercent / duration=0 的伤害类）
            if (isDebuff && duration == 0 && value > 0)
            {
                var dmg = isPercent ? (int)(instance.MaxHp * value / 100.0) : value;
                if (target.Contains("hp", StringComparison.OrdinalIgnoreCase))
                    instance.CurrentHp = Math.Max(1, instance.CurrentHp - dmg);
                else
                    instance.CurrentMp = Math.Max(0, instance.CurrentMp - dmg);
                var dmgMsg = FormatEventLog(logPrefix, eventConfig, $"受到 {dmg} 点伤害。");
                await AppendExploreLogAsync(instance, dmgMsg);
                await _instanceRepository.UpdateAsync(instance);

                // 队伍共享伤害
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        if (target.Contains("hp", StringComparison.OrdinalIgnoreCase))
                            member.CurrentHp = Math.Max(1, member.CurrentHp - dmg);
                        else
                            member.CurrentMp = Math.Max(0, member.CurrentMp - dmg);
                    });
                    await AppendPartyLogAsync(instance, dmgMsg);

                    // 检查队友死亡
                    var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                    foreach (var member in members)
                    {
                        if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running && member.CurrentHp <= 0)
                        {
                            if (await TryReviveOnce(member))
                            {
                                await AppendExploreLogAsync(member, $"{logPrefix}凤凰之羽生效，你从死亡中复活！");
                                await _instanceRepository.UpdateAsync(member);
                            }
                            else
                            {
                                await SettleCoreAsync(member.Id, (int)DungeonSettleReason.Death);
                                await RemoveFromPartyAsync(member);
                            }
                        }
                    }
                }

                if (instance.CurrentHp <= 0)
                {
                    if (await TryReviveOnce(instance))
                    {
                        await AppendExploreLogAsync(instance, $"{logPrefix}凤凰之羽生效，你从死亡中复活！");
                        await _instanceRepository.UpdateAsync(instance);
                    }
                    else
                    {
                        await SettleCoreAsync(instance.Id, (int)DungeonSettleReason.Death);
                    }
                }
                return;
            }

            // 持续效果：存入 ActiveBuffs
            if (duration > 0 && !string.IsNullOrEmpty(buffType))
            {
                var buffs = LoadActiveBuffs(instance);
                var existingIdx = buffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
                var newBuff = new ActiveBuff(buffType, value, isPercent, duration, durationType, eventConfig.Id);

                if (existingIdx >= 0)
                {
                    // 刷新：取较大持续时间
                    var old = buffs[existingIdx];
                    buffs[existingIdx] = old with { RemainingDuration = Math.Max(old.RemainingDuration, duration), Value = value };
                }
                else
                {
                    buffs.Add(newBuff);
                }

                SaveActiveBuffs(instance, buffs);
                var durationLabel = durationType == "battle" ? $"{duration}场战斗" : durationType == "tick" ? $"{duration}步" : "整个秘境";
                var effectLabel = isDebuff ? "减益" : "增益";
                var valueStr = isPercent ? $"{value}%" : $"+{value}";
                var buffMsg = FormatEventLog(logPrefix, eventConfig, $"获得{effectLabel} {GetBuffDisplayName(buffType)}（{valueStr}），持续{durationLabel}。");
                await AppendExploreLogAsync(instance, buffMsg);

                // 队伍共享 buff
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        var memberBuffs = LoadActiveBuffs(member);
                        var memberIdx = memberBuffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
                        if (memberIdx >= 0)
                        {
                            var old = memberBuffs[memberIdx];
                            memberBuffs[memberIdx] = old with { RemainingDuration = Math.Max(old.RemainingDuration, duration), Value = value };
                        }
                        else
                        {
                            memberBuffs.Add(newBuff);
                        }
                        SaveActiveBuffs(member, memberBuffs);
                    });
                    await AppendPartyLogAsync(instance, buffMsg);
                }
            }
            else
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, $"获得 {GetBuffDisplayName(buffType)} 效果。"));
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task ExecuteResourceEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var rewardType = "Gold";
            var amount = 0;

            if (eventData.TryGetProperty("rewardType", out var typeProp))
                rewardType = typeProp.GetString() ?? "Gold";
            if (eventData.TryGetProperty("amount", out var amountProp))
                amount = ParseRangeValue(GetJsonStringValue(amountProp));

            switch (rewardType)
            {
                case "Equipment":
                    if (eventData.TryGetProperty("equipmentId", out var eqProp)
                        && int.TryParse(eqProp.GetString(), out int equipTemplateId)
                        && equipTemplateId > 0)
                    {
                        var equip = BattleRewards.CreateEquipmentFromTemplate(equipTemplateId);
                        if (equip != null)
                        {
                            // 随机分配给一个队友（如果有队伍），否则给队长
                            var equipTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                            var entity = EquipmentBalanceHelper.CreateEntity(equipTarget.PlayerId, equip, false);
                            if (await TryAddEquipmentWithCapacityAsync(entity))
                            {
                                await AddRewardAsync(equipTarget, "Equipment", equipTemplateId.ToString(), 1, "Resource", eventConfig.DeathKeep ?? false);
                                var equipMsg = FormatEventLog(logPrefix, eventConfig, $"获得了装备 [{equip.Name}]。");
                                await AppendExploreLogAsync(instance, equipMsg);
                                await AppendPartyLogAsync(instance, equipMsg);
                            }
                        }
                        else
                        {
                            await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, $"装备模板 {equipTemplateId} 不存在。"));
                        }
                    }
                    else if (eventData.TryGetProperty("quality", out var qProp))
                    {
                        // 按品质随机选择装备模板
                        var qualityRange = GetJsonStringValue(qProp, "1");
                        var qualityVal = (EquipmentQuality)ParseRangeValue(qualityRange);
                        var templates = GameData.EquipmentTemplates.Values.Where(t => t.Quality == qualityVal).ToList();
                        if (templates.Count > 0)
                        {
                            var picked = templates[Random.Shared.Next(templates.Count)];
                            var equip = BattleRewards.CreateEquipmentFromTemplate(picked.EquipmentId);
                            if (equip != null)
                            {
                                var equipTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                var entity = EquipmentBalanceHelper.CreateEntity(equipTarget.PlayerId, equip, false);
                                if (await TryAddEquipmentWithCapacityAsync(entity))
                                {
                                    await AddRewardAsync(equipTarget, "Equipment", picked.EquipmentId.ToString(), 1, "Resource", eventConfig.DeathKeep ?? false);
                                    var equipMsg = FormatEventLog(logPrefix, eventConfig, $"获得了装备 [{equip.Name}]。");
                                    await AppendExploreLogAsync(instance, equipMsg);
                                    await AppendPartyLogAsync(instance, equipMsg);
                                }
                            }
                        }
                        else
                        {
                            var noTplMsg = $"{logPrefix}{eventConfig.Name}：品质 {qualityVal} 的装备模板不存在。";
                            await AppendExploreLogAsync(instance, noTplMsg);
                            await AppendPartyLogAsync(instance, noTplMsg);
                        }
                    }
                    break;

                case "Collection":
                    if (eventData.TryGetProperty("collectionSeriesId", out var csProp))
                    {
                        var seriesIdRaw = csProp.GetString() ?? "";
                        // 格式: "text:seriesId" 或 "image:seriesId"
                        var parts = seriesIdRaw.Split(':', 2);
                        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            var collectionType = parts[0] == "image" ? 1 : 0; // 0=文字, 1=图片
                            var seriesId = parts[1];
                            try
                            {
                                // 随机分配给一个队友（如果有队伍），否则给队长
                                var collTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                await _collectionService.GrantRandomCollectionItemAsync(collTarget.PlayerId, seriesId, collectionType);
                                var typeName = collectionType == 0 ? "文字图鉴" : "图片图鉴";
                                var collMsg = FormatEventLog(logPrefix, eventConfig, $"获得了{typeName}（系列 {seriesId}）。");
                                await AppendExploreLogAsync(instance, collMsg);
                                await AppendPartyLogAsync(instance, collMsg);
                            }
                            catch (Exception ex)
                            {
                                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "图鉴获取失败。"));
                                _logger?.LogWarning(ex, "Failed to grant collection from series {SeriesId}", seriesId);
                            }
                        }
                    }
                    else if (eventData.TryGetProperty("collectionType", out var ctProp))
                    {
                        // 按类型随机选择图鉴系列
                        var collTypeName = ctProp.GetString() ?? "text";
                        var collTypeInt = collTypeName == "image" ? 1 : 0;
                        try
                        {
                            var config = await _collectionService.GetCollectionConfigAsync();
                            var matchingSeries = config.Where(s => s.CollectionType == collTypeInt).ToList();
                            if (matchingSeries.Count > 0)
                            {
                                var pickedSeries = matchingSeries[Random.Shared.Next(matchingSeries.Count)];
                                var collTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                await _collectionService.GrantRandomCollectionItemAsync(collTarget.PlayerId, pickedSeries.SeriesId, collTypeInt);
                                var displayName = collTypeInt == 0 ? "文字图鉴" : "图片图鉴";
                                var collMsg = FormatEventLog(logPrefix, eventConfig, $"获得了{displayName}（{pickedSeries.Name}）。");
                                await AppendExploreLogAsync(instance, collMsg);
                                await AppendPartyLogAsync(instance, collMsg);
                            }
                        }
                        catch (Exception ex)
                        {
                            await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "图鉴获取失败。"));
                            _logger?.LogWarning(ex, "Failed to grant random collection of type {CollectionType}", collTypeName);
                        }
                    }
                    break;

                case "Item":
                    var itemId = "";
                    if (eventData.TryGetProperty("itemId", out var iidProp)) itemId = iidProp.GetString() ?? "";
                    var itemQty = 1;
                    if (eventData.TryGetProperty("quantity", out var iqtyProp)) itemQty = ParseRangeValue(GetJsonStringValue(iqtyProp, "1"));
                    if (!string.IsNullOrEmpty(itemId) && itemQty > 0)
                    {
                        var itemTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                        await _inventoryService.AddItemAsync(itemTarget.PlayerId, new AddItemRequestDto { ItemId = itemId, Quantity = itemQty });
                        var itemMsg = FormatEventLog(logPrefix, eventConfig, $"获得了道具 {GetItemDisplayName(itemId)} x{itemQty}。");
                        await AppendExploreLogAsync(instance, itemMsg);
                        await AppendPartyLogAsync(instance, itemMsg);
                    }
                    break;

                default:
                    if (amount > 0)
                    {
                        await AddRewardAsync(instance, rewardType, null, amount, "Resource", eventConfig.DeathKeep ?? false);
                        // 队伍共享收益
                        if (!string.IsNullOrEmpty(instance.PartyId))
                        {
                            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                            foreach (var member in members)
                            {
                                if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                                    await AddRewardAsync(member, rewardType, null, amount, "Resource", eventConfig.DeathKeep ?? false);
                            }
                        }
                        var resMsg = FormatEventLog(logPrefix, eventConfig, $"获得了 {amount} {GetResourceDisplayName(rewardType)}。");
                        await AppendExploreLogAsync(instance, resMsg);
                        await AppendPartyLogAsync(instance, resMsg);
                    }
                    break;
            }
        }

        private async Task ExecuteTreasureEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);

            if (eventData.TryGetProperty("rewards", out var rewardsProp))
            {
                foreach (var reward in rewardsProp.EnumerateArray())
                {
                    var type = "Gold";
                    var amount = 0;
                    if (reward.TryGetProperty("type", out var t)) type = t.GetString() ?? "Gold";
                    if (reward.TryGetProperty("amount", out var a)) amount = ParseRangeValue(GetJsonStringValue(a));

                    switch (type)
                    {
                        case "Equipment":
                            if (reward.TryGetProperty("equipmentId", out var eqProp)
                                && int.TryParse(eqProp.GetString(), out int eqId) && eqId > 0)
                            {
                                var equip = BattleRewards.CreateEquipmentFromTemplate(eqId);
                                if (equip != null)
                                {
                                    // 随机分配给一个队友（如果有队伍），否则给队长
                                    var equipTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                    var entity = EquipmentBalanceHelper.CreateEntity(equipTarget.PlayerId, equip, false);
                                    if (await TryAddEquipmentWithCapacityAsync(entity))
                                    {
                                        await AddRewardAsync(equipTarget, "Equipment", eqId.ToString(), 1, "Treasure", eventConfig.DeathKeep ?? false);
                                    }
                                }
                            }
                            break;

                        case "Collection":
                            if (reward.TryGetProperty("collectionSeriesId", out var csProp))
                            {
                                var raw = csProp.GetString() ?? "";
                                var parts = raw.Split(':', 2);
                                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                                {
                                    var cType = parts[0] == "image" ? 1 : 0;
                                    try
                                    {
                                        // 随机分配给一个队友（如果有队伍），否则给队长
                                        var collTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                        await _collectionService.GrantRandomCollectionItemAsync(collTarget.PlayerId, parts[1], cType);
                                    }
                                    catch { }
                                }
                            }
                            break;

                        case "Item":
                            if (reward.TryGetProperty("itemId", out var iProp))
                            {
                                var itemId = iProp.GetString();
                                var qty = 1;
                                if (reward.TryGetProperty("quantity", out var qProp))
                                    qty = ParseRangeValue(GetJsonStringValue(qProp, "1"));
                                if (!string.IsNullOrEmpty(itemId) && qty > 0)
                                {
                                    // 随机分配给一个队友（如果有队伍），否则给队长
                                    var itemTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                    await AddRewardAsync(itemTarget, "Item", itemId, qty, "Treasure", eventConfig.DeathKeep ?? false);
                                }
                            }
                            break;

                        default:
                            if (amount > 0)
                            {
                                await AddRewardAsync(instance, type, null, amount, "Treasure", eventConfig.DeathKeep ?? false);
                                // 队伍共享收益
                                if (!string.IsNullOrEmpty(instance.PartyId))
                                {
                                    var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                                    foreach (var member in members)
                                    {
                                        if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                                            await AddRewardAsync(member, type, null, amount, "Treasure", eventConfig.DeathKeep ?? false);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            var treasureMsg = FormatEventLog(logPrefix, eventConfig, "打开了宝箱，获得了丰厚奖励！");
            await AppendExploreLogAsync(instance, treasureMsg);
            await AppendPartyLogAsync(instance, treasureMsg);
        }

        private async Task ExecuteAdventureEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var adventureType = "";
            if (eventData.TryGetProperty("adventureType", out var atProp))
                adventureType = atProp.GetString() ?? "";

            switch (adventureType)
            {
                case "randomBuff":
                    await AdventureRandomBuff(instance, eventConfig, eventData, logPrefix);
                    break;
                case "grantExp":
                    await AdventureGrantReward(instance, eventConfig, eventData, "Adventure", logPrefix);
                    break;
                case "grantItem":
                    await AdventureGrantItem(instance, eventConfig, eventData, logPrefix);
                    break;
                case "permanentBuff":
                    await AdventurePermanentBuff(instance, eventConfig, eventData, logPrefix);
                    break;
                case "riddle":
                    await AdventureRiddle(instance, eventConfig, eventData, logPrefix);
                    break;
                case "choice":
                    await AdventureChoice(instance, eventConfig, eventData, logPrefix);
                    break;
                case "sacrifice":
                    await AdventureSacrifice(instance, eventConfig, eventData, logPrefix);
                    break;
                case "purgeDebuff":
                    await AdventurePurgeDebuff(instance, eventConfig, logPrefix);
                    break;
                case "fullRestore":
                    await AdventureFullRestore(instance, eventConfig, eventData, logPrefix);
                    break;
                case "risk":
                    await AdventureRisk(instance, eventConfig, eventData, logPrefix);
                    break;
                case "gamble":
                    await AdventureGamble(instance, eventConfig, eventData, logPrefix);
                    break;
                case "wheel":
                    await AdventureWheel(instance, eventConfig, eventData, logPrefix);
                    break;
                case "rest":
                    await AdventureRest(instance, eventConfig, eventData, logPrefix);
                    break;
                case "fake":
                    await AdventureFake(instance, eventConfig, eventData, logPrefix);
                    break;
                case "lore":
                    var loreMsg = FormatEventLog(logPrefix, eventConfig, "");
                    await AppendExploreLogAsync(instance, loreMsg);
                    await AppendPartyLogAsync(instance, loreMsg);
                    break;
                case "treasureChest":
                    await AdventureTreasureChest(instance, eventConfig, eventData, logPrefix);
                    break;
                default:
                    var advDefaultMsg = FormatEventLog(logPrefix, eventConfig, "");
                    await AppendExploreLogAsync(instance, advDefaultMsg);
                    await AppendPartyLogAsync(instance, advDefaultMsg);
                    break;
            }
        }

        private async Task AdventureRandomBuff(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var buffPool = new List<string>();
            if (eventData.TryGetProperty("buffPool", out var poolProp))
                foreach (var p in poolProp.EnumerateArray()) buffPool.Add(p.GetString() ?? "");
            if (buffPool.Count == 0) buffPool.AddRange(["AttackUp", "DefenseUp", "CritRateUp", "SpeedUp"]);

            var duration = 3;
            var durationType = "battle";
            if (eventData.TryGetProperty("duration", out var durProp)) duration = durProp.GetInt32();
            if (eventData.TryGetProperty("durationType", out var dtProp)) durationType = dtProp.GetString() ?? "battle";

            var picked = buffPool[Random.Shared.Next(buffPool.Count)];
            var buffs = LoadActiveBuffs(instance);
            var existingIdx = buffs.FindIndex(b => b.BuffType == picked && b.DurationType == durationType);
            var newBuff = new ActiveBuff(picked, 10, true, duration, durationType, eventConfig.Id);
            if (existingIdx >= 0)
                buffs[existingIdx] = buffs[existingIdx] with { RemainingDuration = Math.Max(buffs[existingIdx].RemainingDuration, duration) };
            else
                buffs.Add(newBuff);
            SaveActiveBuffs(instance, buffs);

            var buffMsg = FormatEventLog(logPrefix, eventConfig, $"获得了随机增益 {GetBuffDisplayName(picked)}，持续 {duration} 场战斗。");
            await AppendExploreLogAsync(instance, buffMsg);

            // 队伍共享 buff
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    var memberIdx = memberBuffs.FindIndex(b => b.BuffType == picked && b.DurationType == durationType);
                    if (memberIdx >= 0)
                        memberBuffs[memberIdx] = memberBuffs[memberIdx] with { RemainingDuration = Math.Max(memberBuffs[memberIdx].RemainingDuration, duration) };
                    else
                        memberBuffs.Add(newBuff);
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, buffMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureGrantReward(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string source, string logPrefix)
        {
            var rewardType = "Gold";
            if (eventData.TryGetProperty("rewardType", out var rt)) rewardType = rt.GetString() ?? "Gold";
            var amount = 0;
            if (eventData.TryGetProperty("amount", out var amt)) amount = ParseRangeValue(GetJsonStringValue(amt));

            if (amount > 0)
            {
                await AddRewardAsync(instance, rewardType, null, amount, source, eventConfig.DeathKeep ?? false);
                // 队伍共享收益
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                    foreach (var member in members)
                    {
                        if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                            await AddRewardAsync(member, rewardType, null, amount, source, eventConfig.DeathKeep ?? false);
                    }
                }
                var rewardMsg = FormatEventLog(logPrefix, eventConfig, $"获得了 {amount} {GetResourceDisplayName(rewardType)}。");
                await AppendExploreLogAsync(instance, rewardMsg);
                await AppendPartyLogAsync(instance, rewardMsg);
            }
            else
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, ""));
            }
        }

        private async Task AdventureGrantItem(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var itemId = "";
            if (eventData.TryGetProperty("itemId", out var idProp)) itemId = idProp.GetString() ?? "";
            var quantity = 1;
            if (eventData.TryGetProperty("quantity", out var qProp)) quantity = ParseRangeValue(GetJsonStringValue(qProp, "1"));

            if (!string.IsNullOrEmpty(itemId) && quantity > 0)
            {
                // 随机分配给一个队友（如果有队伍），否则给队长
                var target = await PickRandomPartyMemberAsync(instance) ?? instance;
                await _inventoryService.AddItemAsync(target.PlayerId, new AddItemRequestDto { ItemId = itemId, Quantity = quantity });
                var itemMsg = FormatEventLog(logPrefix, eventConfig, $"获得了道具 {GetItemDisplayName(itemId)} x{quantity}。");
                await AppendExploreLogAsync(instance, itemMsg);
                await AppendPartyLogAsync(instance, itemMsg);
            }
        }

        private async Task AdventurePermanentBuff(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var buffType = "";
            if (eventData.TryGetProperty("buffType", out var btProp)) buffType = btProp.GetString() ?? "";
            var value = 0;
            if (eventData.TryGetProperty("value", out var vProp)) value = vProp.GetInt32();
            var isPercent = false;
            if (eventData.TryGetProperty("isPercent", out var pctProp)) isPercent = pctProp.GetBoolean();

            if (!string.IsNullOrEmpty(buffType))
            {
                var buffs = LoadActiveBuffs(instance);
                var existingIdx = buffs.FindIndex(b => b.BuffType == buffType && b.DurationType == "dungeon");
                var newBuff = new ActiveBuff(buffType, value, isPercent, 9999, "dungeon", eventConfig.Id);
                if (existingIdx >= 0)
                    buffs[existingIdx] = newBuff;
                else
                    buffs.Add(newBuff);
                SaveActiveBuffs(instance, buffs);
                var valueStr = isPercent ? $"{value}%" : $"+{value}";
                var permMsg = FormatEventLog(logPrefix, eventConfig, $"获得永久增益 {GetBuffDisplayName(buffType)}（{valueStr}），持续整个秘境。");
                await AppendExploreLogAsync(instance, permMsg);

                // 队伍共享永久 buff
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        var memberBuffs = LoadActiveBuffs(member);
                        var memberIdx = memberBuffs.FindIndex(b => b.BuffType == buffType && b.DurationType == "dungeon");
                        if (memberIdx >= 0)
                            memberBuffs[memberIdx] = newBuff;
                        else
                            memberBuffs.Add(newBuff);
                        SaveActiveBuffs(member, memberBuffs);
                    });
                    await AppendPartyLogAsync(instance, permMsg);
                }

                await _instanceRepository.UpdateAsync(instance);
            }
        }

        private async Task AdventureRiddle(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var successRate = 50;
            if (eventData.TryGetProperty("successRate", out var srProp)) successRate = srProp.GetInt32();

            var roll = Random.Shared.Next(100);
            if (roll < successRate)
            {
                // 成功：给奖励
                if (eventData.TryGetProperty("successReward", out var rewardProp))
                {
                    var type = "Gold";
                    var amount = 0;
                    if (rewardProp.TryGetProperty("type", out var t)) type = t.GetString() ?? "Gold";
                    if (rewardProp.TryGetProperty("amount", out var a)) amount = ParseRangeValue(GetJsonStringValue(a));
                    if (amount > 0)
                    {
                        await AddRewardAsync(instance, type, null, amount, "Adventure", eventConfig.DeathKeep ?? false);
                        // 队伍共享收益
                        if (!string.IsNullOrEmpty(instance.PartyId))
                        {
                            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                            foreach (var member in members)
                            {
                                if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                                    await AddRewardAsync(member, type, null, amount, "Adventure", eventConfig.DeathKeep ?? false);
                            }
                        }
                    }
                    var riddleMsg = FormatEventLog(logPrefix, eventConfig, $"谜题答对了！获得了 {amount} {GetResourceDisplayName(type)}。");
                    await AppendExploreLogAsync(instance, riddleMsg);
                    await AppendPartyLogAsync(instance, riddleMsg);
                }
                else
                {
                    await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "谜题答对了！"));
                }
            }
            else
            {
                // 失败：挂 debuff
                if (eventData.TryGetProperty("failDebuff", out var debuffProp))
                {
                    var debuffType = "AllStatsDown";
                    var value = 10;
                    var duration = 2;
                    if (debuffProp.TryGetProperty("debuffType", out var dt)) debuffType = dt.GetString() ?? "AllStatsDown";
                    if (debuffProp.TryGetProperty("value", out var v)) value = v.GetInt32();
                    if (debuffProp.TryGetProperty("duration", out var dur)) duration = dur.GetInt32();

                    var buffs = LoadActiveBuffs(instance);
                    var newDebuff = new ActiveBuff(debuffType, value, true, duration, "battle", eventConfig.Id);
                    buffs.Add(newDebuff);
                    SaveActiveBuffs(instance, buffs);
                    var riddleFailMsg = FormatEventLog(logPrefix, eventConfig, $"谜题答错了！受到 {GetBuffDisplayName(debuffType)} 减益，持续 {duration} 场战斗。");
                    await AppendExploreLogAsync(instance, riddleFailMsg);

                    // 队伍共享 debuff
                    if (!string.IsNullOrEmpty(instance.PartyId))
                    {
                        await ApplyToPartyMembersAsync(instance, member =>
                        {
                            var memberBuffs = LoadActiveBuffs(member);
                            memberBuffs.Add(newDebuff);
                            SaveActiveBuffs(member, memberBuffs);
                        });
                        await AppendPartyLogAsync(instance, riddleFailMsg);
                    }
                }
                else
                {
                    await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "谜题答错了！"));
                }
            }
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureChoice(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            // 秘境是自动tick，无法等玩家选，随机选一个执行
            var useA = Random.Shared.Next(2) == 0;
            var chosen = useA ? "optionA" : "optionB";
            var chosenName = useA ? "选项A" : "选项B";

            if (eventData.TryGetProperty(chosen, out var optProp))
            {
                var name = "";
                if (optProp.TryGetProperty("name", out var n)) name = n.GetString() ?? "";
                if (!string.IsNullOrEmpty(name)) chosenName = name;

                // 执行选项的 reward
                if (optProp.TryGetProperty("reward", out var rewardProp))
                {
                    await ApplyAdventureRewardWithParty(instance, rewardProp, eventConfig, logPrefix);
                }
            }

            var choiceMsg = FormatEventLog(logPrefix, eventConfig, $"随机选择了「{chosenName}」。");
            await AppendExploreLogAsync(instance, choiceMsg);
            await AppendPartyLogAsync(instance, choiceMsg);
        }

        private async Task AdventureSacrifice(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var hpCostStr = "20%";
            if (eventData.TryGetProperty("hpCost", out var hcProp)) hpCostStr = hcProp.GetString() ?? "20%";

            var hpCost = hpCostStr.EndsWith('%')
                ? (int)(instance.MaxHp * ParseRangeValue(hpCostStr.TrimEnd('%')) / 100.0)
                : ParseRangeValue(hpCostStr);

            instance.CurrentHp = Math.Max(1, instance.CurrentHp - hpCost);
            var sacrificeMsg = FormatEventLog(logPrefix, eventConfig, $"献祭了 {hpCost} 点生命。");
            await AppendExploreLogAsync(instance, sacrificeMsg);

            // 给buff奖励（共享给队伍）
            if (eventData.TryGetProperty("reward", out var rewardProp))
            {
                await ApplyAdventureRewardWithParty(instance, rewardProp, eventConfig, logPrefix);
            }
            await AppendPartyLogAsync(instance, sacrificeMsg);
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventurePurgeDebuff(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var debuffTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DamagePercent", "DamageOverTime", "AttackDown", "DefenseDown", "SpeedDown",
                "CritRateDown", "Silence", "Stun", "HealBlock", "AllStatsDown", "WeightModifier"
            };
            var buffs = LoadActiveBuffs(instance);
            var removed = buffs.RemoveAll(b => debuffTypes.Contains(b.BuffType));
            SaveActiveBuffs(instance, buffs);
            var purgeEffect = removed > 0 ? $"净化了 {removed} 个减益效果。" : "没有减益效果需要净化。";
            var purgeMsg = FormatEventLog(logPrefix, eventConfig, purgeEffect);
            await AppendExploreLogAsync(instance, purgeMsg);

            // 队伍共享净化
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    memberBuffs.RemoveAll(b => debuffTypes.Contains(b.BuffType));
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, purgeMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureFullRestore(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            instance.CurrentHp = instance.MaxHp;
            instance.CurrentMp = instance.MaxMp;
            var restoreMsg = FormatEventLog(logPrefix, eventConfig, "生命和灵力完全恢复！");
            await AppendExploreLogAsync(instance, restoreMsg);

            // 队伍共享完全恢复
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    member.CurrentHp = member.MaxHp;
                    member.CurrentMp = member.MaxMp;
                });
                await AppendPartyLogAsync(instance, restoreMsg);
            }

            if (eventData.TryGetProperty("extraBuff", out var buffProp))
            {
                var buffType = "AllStatsUp";
                var value = 15;
                var duration = 2;
                if (buffProp.TryGetProperty("buffType", out var bt)) buffType = bt.GetString() ?? "AllStatsUp";
                if (buffProp.TryGetProperty("value", out var v)) value = v.GetInt32();
                if (buffProp.TryGetProperty("duration", out var dur)) duration = dur.GetInt32();

                var newBuff = new ActiveBuff(buffType, value, true, duration, "battle", eventConfig.Id);
                var buffs = LoadActiveBuffs(instance);
                buffs.Add(newBuff);
                SaveActiveBuffs(instance, buffs);
                var buffMsg = $"{logPrefix}额外获得增益 {GetBuffDisplayName(buffType)}，持续 {duration} 场战斗。";
                await AppendExploreLogAsync(instance, buffMsg);

                // 队伍共享 buff
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        var memberBuffs = LoadActiveBuffs(member);
                        memberBuffs.Add(newBuff);
                        SaveActiveBuffs(member, memberBuffs);
                    });
                    await AppendPartyLogAsync(instance, buffMsg);
                }
            }
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureRisk(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var hpCostStr = "30%";
            if (eventData.TryGetProperty("hpCost", out var hcProp)) hpCostStr = hcProp.GetString() ?? "30%";
            var hpCost = hpCostStr.EndsWith('%')
                ? (int)(instance.MaxHp * ParseRangeValue(hpCostStr.TrimEnd('%')) / 100.0)
                : ParseRangeValue(hpCostStr);

            instance.CurrentHp = Math.Max(1, instance.CurrentHp - hpCost);
            var riskMsg = FormatEventLog(logPrefix, eventConfig, $"冒险消耗了 {hpCost} 点生命。");
            await AppendExploreLogAsync(instance, riskMsg);

            var winRate = 60;
            if (eventData.TryGetProperty("winRate", out var wrProp)) winRate = wrProp.GetInt32();

            if (Random.Shared.Next(100) < winRate)
            {
                if (eventData.TryGetProperty("bigReward", out var bigProp))
                    await ApplyAdventureRewardWithParty(instance, bigProp, eventConfig, $"{logPrefix}大奖：");
            }
            else
            {
                if (eventData.TryGetProperty("smallReward", out var smallProp))
                    await ApplyAdventureRewardWithParty(instance, smallProp, eventConfig, $"{logPrefix}安慰奖：");
            }
            await AppendPartyLogAsync(instance, riskMsg);
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureGamble(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            // 扣成本（仅队长）
            if (eventData.TryGetProperty("cost", out var costProp))
            {
                var costType = "Gold";
                var costAmount = 0;
                if (costProp.TryGetProperty("type", out var ct)) costType = ct.GetString() ?? "Gold";
                if (costProp.TryGetProperty("amount", out var ca)) costAmount = ca.GetInt32();
                if (costAmount > 0 && costType == "Gold")
                {
                    var user = await _userRepository.GetByIdAsync(instance.PlayerId);
                    if (user != null) { user.Gold = Math.Max(0, user.Gold - costAmount); await _userRepository.UpdateAsync(user); }
                }
            }

            var winRate = 50;
            if (eventData.TryGetProperty("winRate", out var wrProp)) winRate = wrProp.GetInt32();

            if (Random.Shared.Next(100) < winRate)
            {
                if (eventData.TryGetProperty("winReward", out var winProp))
                    await ApplyAdventureRewardWithParty(instance, winProp, eventConfig, $"{logPrefix}赌赢了！");
                var gambleMsg = FormatEventLog(logPrefix, eventConfig, "赌博获胜！");
                await AppendExploreLogAsync(instance, gambleMsg);
                await AppendPartyLogAsync(instance, gambleMsg);
            }
            else
            {
                if (eventData.TryGetProperty("losePenalty", out var loseProp))
                {
                    var penaltyType = "Gold";
                    var penaltyAmount = 0;
                    if (loseProp.TryGetProperty("type", out var pt)) penaltyType = pt.GetString() ?? "Gold";
                    if (loseProp.TryGetProperty("amount", out var pa)) penaltyAmount = ParseRangeValue(GetJsonStringValue(pa));
                    if (penaltyAmount > 0 && penaltyType == "Gold")
                    {
                        var user = await _userRepository.GetByIdAsync(instance.PlayerId);
                        if (user != null) { user.Gold = Math.Max(0, user.Gold - penaltyAmount); await _userRepository.UpdateAsync(user); }
                    }
                }
                var gambleFailMsg = FormatEventLog(logPrefix, eventConfig, "赌博失败了...");
                await AppendExploreLogAsync(instance, gambleFailMsg);
                await AppendPartyLogAsync(instance, gambleFailMsg);
            }
        }

        private async Task AdventureWheel(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            if (!eventData.TryGetProperty("rewards", out var rewardsProp) || rewardsProp.GetArrayLength() == 0)
            {
                var emptyMsg = $"{logPrefix}{eventConfig.Name}：转盘为空。";
                await AppendExploreLogAsync(instance, emptyMsg);
                await AppendPartyLogAsync(instance, emptyMsg);
                return;
            }

            // 计算总权重
            var totalWeight = 0;
            foreach (var r in rewardsProp.EnumerateArray())
                if (r.TryGetProperty("weight", out var w)) totalWeight += w.GetInt32();

            if (totalWeight <= 0)
            {
                var errMsg = $"{logPrefix}{eventConfig.Name}：转盘权重异常。";
                await AppendExploreLogAsync(instance, errMsg);
                await AppendPartyLogAsync(instance, errMsg);
                return;
            }

            // 权重随机
            var pick = Random.Shared.Next(totalWeight);
            var cumulative = 0;
            foreach (var r in rewardsProp.EnumerateArray())
            {
                var w = 0;
                if (r.TryGetProperty("weight", out var wProp)) w = wProp.GetInt32();
                cumulative += w;
                if (pick < cumulative)
                {
                    await ApplyAdventureRewardWithParty(instance, r, eventConfig, $"{logPrefix}转盘获得：");
                    return;
                }
            }
        }

        private async Task AdventureRest(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var healHpPercent = 50;
            var healMpPercent = 50;
            if (eventData.TryGetProperty("healHpPercent", out var hpProp)) healHpPercent = hpProp.GetInt32();
            if (eventData.TryGetProperty("healMpPercent", out var mpProp)) healMpPercent = mpProp.GetInt32();

            var hpHeal = instance.MaxHp * healHpPercent / 100;
            var mpHeal = instance.MaxMp * healMpPercent / 100;
            var oldHp = instance.CurrentHp;
            var oldMp = instance.CurrentMp;
            instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + hpHeal);
            instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + mpHeal);
            var actualHp = instance.CurrentHp - oldHp;
            var actualMp = instance.CurrentMp - oldMp;

            // 队伍共享回复
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    member.CurrentHp = Math.Min(member.MaxHp, member.CurrentHp + member.MaxHp * healHpPercent / 100);
                    member.CurrentMp = Math.Min(member.MaxMp, member.CurrentMp + member.MaxMp * healMpPercent / 100);
                });
            }

            if (actualHp == 0 && actualMp == 0)
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "生命和灵力充沛，无需恢复。"));
            else
            {
                var restMsg = FormatEventLog(logPrefix, eventConfig, $"休息恢复了 {actualHp} 生命和 {actualMp} 灵力。");
                await AppendExploreLogAsync(instance, restMsg);
                await AppendPartyLogAsync(instance, restMsg);
            }

            if (eventData.TryGetProperty("buff", out var buffProp))
            {
                var buffType = "DefenseUp";
                var value = 20;
                var duration = 1;
                if (buffProp.TryGetProperty("buffType", out var bt)) buffType = bt.GetString() ?? "DefenseUp";
                if (buffProp.TryGetProperty("value", out var v)) value = v.GetInt32();
                if (buffProp.TryGetProperty("duration", out var dur)) duration = dur.GetInt32();

                var newBuff = new ActiveBuff(buffType, value, true, duration, "battle", eventConfig.Id);
                var buffs = LoadActiveBuffs(instance);
                buffs.Add(newBuff);
                SaveActiveBuffs(instance, buffs);
                var restBuffMsg = $"{logPrefix}休息后获得增益 {GetBuffDisplayName(buffType)}，持续 {duration} 场战斗。";
                await AppendExploreLogAsync(instance, restBuffMsg);

                // 队伍共享 buff
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        var memberBuffs = LoadActiveBuffs(member);
                        memberBuffs.Add(newBuff);
                        SaveActiveBuffs(member, memberBuffs);
                    });
                    await AppendPartyLogAsync(instance, restBuffMsg);
                }
            }
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task AdventureFake(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var realEventId = "";
            if (eventData.TryGetProperty("realEvent", out var reProp)) realEventId = reProp.GetString() ?? "";

            if (!string.IsNullOrEmpty(realEventId) && GameData.DungeonEventConfigsById.TryGetValue(realEventId, out var realEvent))
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, ""));
                // 执行真实事件的逻辑
                await ExecuteEventAsync(instance, realEvent);
            }
            else
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "什么都没发生。"));
            }
        }

        private async Task AdventureTreasureChest(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            // 从道具箱类型中随机选一个
            var chestItems = GameData.Items.Values.Where(i => i.Type == ItemType.Chest).ToList();
            if (chestItems.Count == 0)
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "没有可用的宝箱道具。"));
                return;
            }

            var quantity = 1;
            if (eventData.TryGetProperty("quantity", out var qProp)) quantity = ParseRangeValue(GetJsonStringValue(qProp, "1"));

            var picked = chestItems[Random.Shared.Next(chestItems.Count)];
            // 随机分配给一个队友（如果有队伍），否则给队长
            var target = await PickRandomPartyMemberAsync(instance) ?? instance;
            await _inventoryService.AddItemAsync(target.PlayerId, new AddItemRequestDto { ItemId = picked.ItemId, Quantity = quantity });
            var chestMsg = FormatEventLog(logPrefix, eventConfig, $"获得了宝箱 [{picked.Name}] x{quantity}。");
            await AppendExploreLogAsync(instance, chestMsg);
            await AppendPartyLogAsync(instance, chestMsg);
        }

        private async Task ApplyAdventureReward(DungeonInstanceEntity instance, JsonElement rewardProp, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var type = "Gold";
            var amount = 0;
            if (rewardProp.TryGetProperty("type", out var t)) type = t.GetString() ?? "Gold";
            if (rewardProp.TryGetProperty("amount", out var a)) amount = ParseRangeValue(GetJsonStringValue(a));

            switch (type)
            {
                case "Item":
                    var itemId = "";
                    if (rewardProp.TryGetProperty("itemId", out var idProp)) itemId = idProp.GetString() ?? "";
                    var qty = 1;
                    if (rewardProp.TryGetProperty("quantity", out var qProp)) qty = ParseRangeValue(GetJsonStringValue(qProp, "1"));
                    if (!string.IsNullOrEmpty(itemId) && qty > 0)
                    {
                        await _inventoryService.AddItemAsync(instance.PlayerId, new AddItemRequestDto { ItemId = itemId, Quantity = qty });
                        await AppendExploreLogAsync(instance, $"{logPrefix}获得了道具 {GetItemDisplayName(itemId)} x{qty}。");
                    }
                    break;
                case "Equipment":
                    if (rewardProp.TryGetProperty("equipmentId", out var eqProp) && int.TryParse(eqProp.GetString(), out int eqId) && eqId > 0)
                    {
                        var equip = BattleRewards.CreateEquipmentFromTemplate(eqId);
                        if (equip != null)
                        {
                            var entity = EquipmentBalanceHelper.CreateEntity(instance.PlayerId, equip, false);
                            if (await TryAddEquipmentWithCapacityAsync(entity))
                            {
                                await AppendExploreLogAsync(instance, $"{logPrefix}获得了装备 [{equip.Name}]。");
                            }
                        }
                    }
                    break;
                case "Collection":
                    if (rewardProp.TryGetProperty("collectionSeriesId", out var csProp))
                    {
                        var raw = csProp.GetString() ?? "";
                        var parts = raw.Split(':', 2);
                        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            var cType = parts[0] == "image" ? 1 : 0;
                            try { await _collectionService.GrantRandomCollectionItemAsync(instance.PlayerId, parts[1], cType); }
                            catch { }
                        }
                    }
                    break;
                default:
                    if (amount > 0)
                    {
                        await AddRewardAsync(instance, type, null, amount, "Adventure", eventConfig.DeathKeep ?? false);
                        await AppendExploreLogAsync(instance, $"{logPrefix}获得了 {amount} {GetResourceDisplayName(type)}。");
                    }
                    break;
            }
        }

        /// <summary>
        /// 带队伍共享的奇遇奖励分配。Gold/Exp 等数值型奖励全队共享，物品/装备/图鉴随机分配给一个队友。
        /// </summary>
        private async Task ApplyAdventureRewardWithParty(DungeonInstanceEntity instance, JsonElement rewardProp, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var type = "Gold";
            var amount = 0;
            if (rewardProp.TryGetProperty("type", out var t)) type = t.GetString() ?? "Gold";
            if (rewardProp.TryGetProperty("amount", out var a)) amount = ParseRangeValue(GetJsonStringValue(a));

            switch (type)
            {
                case "Item":
                    var itemId = "";
                    if (rewardProp.TryGetProperty("itemId", out var idProp)) itemId = idProp.GetString() ?? "";
                    var qty = 1;
                    if (rewardProp.TryGetProperty("quantity", out var qProp)) qty = ParseRangeValue(GetJsonStringValue(qProp, "1"));
                    if (!string.IsNullOrEmpty(itemId) && qty > 0)
                    {
                        var itemTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                        await _inventoryService.AddItemAsync(itemTarget.PlayerId, new AddItemRequestDto { ItemId = itemId, Quantity = qty });
                        var itemMsg = $"{logPrefix}获得了道具 {GetItemDisplayName(itemId)} x{qty}。";
                        await AppendExploreLogAsync(instance, itemMsg);
                        await AppendPartyLogAsync(instance, itemMsg);
                    }
                    break;
                case "Equipment":
                    if (rewardProp.TryGetProperty("equipmentId", out var eqProp) && int.TryParse(GetJsonStringValue(eqProp, "0"), out int eqId) && eqId > 0)
                    {
                        var equip = BattleRewards.CreateEquipmentFromTemplate(eqId);
                        if (equip != null)
                        {
                            var equipTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                            var entity = EquipmentBalanceHelper.CreateEntity(equipTarget.PlayerId, equip, false);
                            if (await TryAddEquipmentWithCapacityAsync(entity))
                            {
                                var equipMsg = $"{logPrefix}获得了装备 [{equip.Name}]。";
                                await AppendExploreLogAsync(instance, equipMsg);
                                await AppendPartyLogAsync(instance, equipMsg);
                            }
                        }
                    }
                    else if (rewardProp.TryGetProperty("quality", out var qualProp))
                    {
                        var qualityRange = GetJsonStringValue(qualProp, "1");
                        var qualityVal = (EquipmentQuality)ParseRangeValue(qualityRange);
                        var templates = GameData.EquipmentTemplates.Values.Where(t => t.Quality == qualityVal).ToList();
                        if (templates.Count > 0)
                        {
                            var picked = templates[Random.Shared.Next(templates.Count)];
                            var genEquip = BattleRewards.CreateEquipmentFromTemplate(picked.EquipmentId);
                            if (genEquip != null)
                            {
                                var genTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                var genEntity = EquipmentBalanceHelper.CreateEntity(genTarget.PlayerId, genEquip, false);
                                if (await TryAddEquipmentWithCapacityAsync(genEntity))
                                {
                                    await AddRewardAsync(genTarget, "Equipment", picked.EquipmentId.ToString(), 1, "Adventure", eventConfig.DeathKeep ?? false);
                                    var genMsg = $"{logPrefix}获得了装备 [{genEquip.Name}]。";
                                    await AppendExploreLogAsync(instance, genMsg);
                                    await AppendPartyLogAsync(instance, genMsg);
                                }
                            }
                        }
                    }
                    break;
                case "Collection":
                    if (rewardProp.TryGetProperty("collectionSeriesId", out var csProp))
                    {
                        var raw = csProp.GetString() ?? "";
                        var parts = raw.Split(':', 2);
                        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            var cType = parts[0] == "image" ? 1 : 0;
                            try
                            {
                                var collTarget = await PickRandomPartyMemberAsync(instance) ?? instance;
                                await _collectionService.GrantRandomCollectionItemAsync(collTarget.PlayerId, parts[1], cType);
                                var collMsg = $"{logPrefix}获得了图鉴。";
                                await AppendExploreLogAsync(instance, collMsg);
                                await AppendPartyLogAsync(instance, collMsg);
                            }
                            catch { }
                        }
                    }
                    break;
                default:
                    if (amount > 0)
                    {
                        await AddRewardAsync(instance, type, null, amount, "Adventure", eventConfig.DeathKeep ?? false);
                        // 队伍共享收益
                        if (!string.IsNullOrEmpty(instance.PartyId))
                        {
                            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
                            foreach (var member in members)
                            {
                                if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                                    await AddRewardAsync(member, type, null, amount, "Adventure", eventConfig.DeathKeep ?? false);
                            }
                        }
                        var rewardMsg = $"{logPrefix}获得了 {amount} {GetResourceDisplayName(type)}。";
                        await AppendExploreLogAsync(instance, rewardMsg);
                        await AppendPartyLogAsync(instance, rewardMsg);
                    }
                    break;
            }
        }

        private async Task ExecuteShopEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var user = await _userRepository.GetByIdAsync(instance.PlayerId);
            if (user == null)
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "遇到了神秘商人，但你不存在。"));
                return;
            }

            await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "遇到了神秘商人！"));

            // 构建购买池
            var pool = new List<(string Name, int Weight, string Category, JsonElement Data)>();

            // 商品列表
            if (eventData.TryGetProperty("items", out var itemsProp))
            {
                foreach (var item in itemsProp.EnumerateArray())
                {
                    var weight = 10;
                    if (item.TryGetProperty("weight", out var w)) weight = w.GetInt32();
                    var name = "商品";
                    if (item.TryGetProperty("itemId", out var idProp)) name = $"道具 {idProp.GetString()}";
                    else if (item.TryGetProperty("equipId", out var eqProp)) name = $"装备 #{eqProp.GetInt32()}";
                    else if (item.TryGetProperty("seriesId", out var sProp)) name = $"图鉴 {sProp.GetString()}";
                    pool.Add((name, weight, "item", item.Clone()));
                }
            }

            // 治疗服务：只在HP/MP不满时加入池
            if (eventData.TryGetProperty("healing", out var healProp))
            {
                var hpMissing = instance.MaxHp - instance.CurrentHp;
                var mpMissing = instance.MaxMp - instance.CurrentMp;
                if (hpMissing > 0 || mpMissing > 0)
                {
                    var weight = 30;
                    if (healProp.TryGetProperty("weight", out var w)) weight = w.GetInt32();
                    pool.Add(("治疗服务", weight, "healing", healProp.Clone()));
                }
            }

            // 强化服务
            if (eventData.TryGetProperty("enhance", out var enhanceProp))
            {
                var weight = 15;
                if (enhanceProp.TryGetProperty("weight", out var w)) weight = w.GetInt32();
                pool.Add(("强化服务", weight, "enhance", enhanceProp.Clone()));
            }

            if (pool.Count == 0)
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}商人今天没有商品。");
                return;
            }

            // 权重随机抽取
            var totalWeight = pool.Sum(p => p.Weight);
            var pick = Random.Shared.Next(totalWeight);
            var cumulative = 0;
            var chosen = pool[0];
            foreach (var entry in pool)
            {
                cumulative += entry.Weight;
                if (pick < cumulative) { chosen = entry; break; }
            }

            // 执行购买逻辑
            switch (chosen.Category)
            {
                case "item":
                    await ShopBuyItem(instance, user, chosen.Data, logPrefix);
                    break;
                case "healing":
                    await ShopHealing(instance, user, chosen.Data, logPrefix);
                    break;
                case "enhance":
                    await ShopEnhance(instance, user, chosen.Data, logPrefix, eventConfig);
                    break;
            }

            // 队伍各自触发：每个队友独立执行商店逻辑
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                var partyMembers = await GetPartyMemberInstancesAsync(instance.PartyId);
                foreach (var member in partyMembers)
                {
                    if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    {
                        var memberUser = await _userRepository.GetByIdAsync(member.PlayerId);
                        if (memberUser == null) continue;

                        // 为每个队友重新构建购买池
                        var memberPool = new List<(string Name, int Weight, string Category, JsonElement Data)>();
                        if (eventData.TryGetProperty("items", out var mItemsProp))
                        {
                            foreach (var item in mItemsProp.EnumerateArray())
                            {
                                var w = 10;
                                if (item.TryGetProperty("weight", out var mw)) w = mw.GetInt32();
                                var n = "商品";
                                if (item.TryGetProperty("itemId", out var midProp)) n = $"道具 {midProp.GetString()}";
                                else if (item.TryGetProperty("equipId", out var meqProp)) n = $"装备 #{meqProp.GetInt32()}";
                                else if (item.TryGetProperty("seriesId", out var msProp)) n = $"图鉴 {msProp.GetString()}";
                                memberPool.Add((n, w, "item", item.Clone()));
                            }
                        }
                        if (eventData.TryGetProperty("healing", out var mHealProp))
                        {
                            var hpMissing = member.MaxHp - member.CurrentHp;
                            var mpMissing = member.MaxMp - member.CurrentMp;
                            if (hpMissing > 0 || mpMissing > 0)
                            {
                                var w = 30;
                                if (mHealProp.TryGetProperty("weight", out var mw)) w = mw.GetInt32();
                                memberPool.Add(("治疗服务", w, "healing", mHealProp.Clone()));
                            }
                        }
                        if (eventData.TryGetProperty("enhance", out var mEnhanceProp))
                        {
                            var w = 15;
                            if (mEnhanceProp.TryGetProperty("weight", out var mw)) w = mw.GetInt32();
                            memberPool.Add(("强化服务", w, "enhance", mEnhanceProp.Clone()));
                        }

                        if (memberPool.Count == 0) continue;

                        // 权重随机抽取
                        var mTotalWeight = memberPool.Sum(p => p.Weight);
                        var mPick = Random.Shared.Next(mTotalWeight);
                        var mCumulative = 0;
                        var mChosen = memberPool[0];
                        foreach (var entry in memberPool)
                        {
                            mCumulative += entry.Weight;
                            if (mPick < mCumulative) { mChosen = entry; break; }
                        }

                        // 执行购买逻辑
                        switch (mChosen.Category)
                        {
                            case "item":
                                await ShopBuyItem(member, memberUser, mChosen.Data, logPrefix);
                                break;
                            case "healing":
                                await ShopHealing(member, memberUser, mChosen.Data, logPrefix);
                                break;
                            case "enhance":
                                await ShopEnhance(member, memberUser, mChosen.Data, logPrefix, eventConfig);
                                break;
                        }
                    }
                }
            }
        }

        private async Task ShopBuyItem(DungeonInstanceEntity instance, UserEntity user, JsonElement itemData, string logPrefix)
        {
            var price = 0;
            if (itemData.TryGetProperty("price", out var p)) price = p.GetInt32();

            var availableGold = await GetDungeonAvailableGoldAsync(instance);
            if (availableGold < price)
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}金币不足（需要 {price}，拥有 {availableGold}），无法购买。");
                return;
            }

            // 记录金币消费到奖励池
            await AddRewardAsync(instance, "ShopSpend", null, price, "Shop", false);

            if (itemData.TryGetProperty("itemId", out var idProp))
            {
                var itemId = idProp.GetString() ?? "";
                var qty = 1;
                if (itemData.TryGetProperty("quantity", out var q)) qty = q.GetInt32();
                if (!string.IsNullOrEmpty(itemId))
                {
                    await _inventoryService.AddItemAsync(instance.PlayerId, new AddItemRequestDto { ItemId = itemId, Quantity = qty });
                    var itemName = GameData.Items.TryGetValue(itemId, out var item) ? item.Name : itemId;
                    await AppendExploreLogAsync(instance, $"{logPrefix}花费 {price} 金币购买了 [{itemName}] x{qty}。");
                }
            }
            else if (itemData.TryGetProperty("equipmentId", out var eqProp) && int.TryParse(eqProp.GetString(), out int equipId) && equipId > 0)
            {
                var equip = BattleRewards.CreateEquipmentFromTemplate(equipId);
                if (equip != null)
                {
                    var entity = EquipmentBalanceHelper.CreateEntity(instance.PlayerId, equip, false);
                    if (await TryAddEquipmentWithCapacityAsync(entity))
                    {
                        await AppendExploreLogAsync(instance, $"{logPrefix}花费 {price} 金币购买了装备 [{equip.Name}]。");
                    }
                }
            }
            else if (itemData.TryGetProperty("seriesId", out var sProp))
            {
                var raw = sProp.GetString() ?? "";
                var parts = raw.Split(':', 2);
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    var cType = parts[0] == "image" ? 1 : 0;
                    try
                    {
                        await _collectionService.GrantRandomCollectionItemAsync(instance.PlayerId, parts[1], cType);
                        var typeName = cType == 0 ? "文字图鉴" : "图片图鉴";
                        await AppendExploreLogAsync(instance, $"{logPrefix}花费 {price} 金币购买了{typeName}。");
                    }
                    catch { await AppendExploreLogAsync(instance, $"{logPrefix}图鉴购买失败。"); }
                }
            }
        }

        private async Task ShopHealing(DungeonInstanceEntity instance, UserEntity user, JsonElement healData, string logPrefix)
        {
            var hpPerGold = 10;
            var mpPerGold = 10;
            if (healData.TryGetProperty("hpPerGold", out var hp)) hpPerGold = hp.GetInt32();
            if (healData.TryGetProperty("mpPerGold", out var mp)) mpPerGold = mp.GetInt32();

            var hpMissing = instance.MaxHp - instance.CurrentHp;
            var mpMissing = instance.MaxMp - instance.CurrentMp;

            if (hpMissing <= 0 && mpMissing <= 0)
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}你不需要治疗。");
                return;
            }

            // 计算需要多少金币
            var hpGoldNeeded = hpPerGold > 0 ? (int)Math.Ceiling((double)hpMissing / hpPerGold) : 0;
            var mpGoldNeeded = mpPerGold > 0 ? (int)Math.Ceiling((double)mpMissing / mpPerGold) : 0;
            var totalGold = hpGoldNeeded + mpGoldNeeded;

            var availableGold = await GetDungeonAvailableGoldAsync(instance);
            if (availableGold < totalGold)
            {
                // 金币不够全恢复，按比例恢复
                var affordable = (int)availableGold;
                var hpSpend = Math.Min(affordable, hpGoldNeeded);
                var mpSpend = affordable - hpSpend;
                var hpHeal = hpSpend * hpPerGold;
                var mpHeal = mpSpend * mpPerGold;
                instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + hpHeal);
                instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + mpHeal);
                await AddRewardAsync(instance, "ShopSpend", null, affordable, "Shop", false);
                await _instanceRepository.UpdateAsync(instance);
                await AppendExploreLogAsync(instance, $"{logPrefix}花费 {affordable} 金币治疗，恢复了 {hpHeal} 生命和 {mpHeal} 灵力。");
            }
            else
            {
                // 金币够，全恢复
                instance.CurrentHp = instance.MaxHp;
                instance.CurrentMp = instance.MaxMp;
                await AddRewardAsync(instance, "ShopSpend", null, totalGold, "Shop", false);
                await _instanceRepository.UpdateAsync(instance);
                await AppendExploreLogAsync(instance, $"{logPrefix}花费 {totalGold} 金币完全恢复了生命和灵力。");
            }
        }

        private async Task ShopEnhance(DungeonInstanceEntity instance, UserEntity user, JsonElement enhanceData, string logPrefix, DungeonEventConfigEntity eventConfig)
        {
            var cost = 500;
            if (enhanceData.TryGetProperty("cost", out var c)) cost = c.GetInt32();

            var availableGold = await GetDungeonAvailableGoldAsync(instance);
            if (availableGold < cost)
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}金币不足（需要 {cost}），无法强化。");
                return;
            }

            var enhanceType = "attack";
            var enhanceValue = 10;
            if (enhanceData.TryGetProperty("type", out var t)) enhanceType = t.GetString() ?? "attack";
            if (enhanceData.TryGetProperty("value", out var v)) enhanceValue = v.GetInt32();

            await AddRewardAsync(instance, "ShopSpend", null, cost, "Shop", false);

            // 强化效果：临时buff存入ActiveBuffs
            var buffType = enhanceType switch
            {
                "attack" => "AttackUp",
                "defense" => "DefenseUp",
                "speed" => "SpeedUp",
                "crit" => "CritRateUp",
                _ => "AllStatsUp"
            };

            var buffs = LoadActiveBuffs(instance);
            var existingIdx = buffs.FindIndex(b => b.BuffType == buffType && b.DurationType == "battle");
            if (existingIdx >= 0)
            {
                var old = buffs[existingIdx];
                buffs[existingIdx] = old with { Value = old.Value + enhanceValue };
            }
            else
            {
                buffs.Add(new ActiveBuff(buffType, enhanceValue, false, 3, "battle", eventConfig.Id));
            }
            SaveActiveBuffs(instance, buffs);

            await AppendExploreLogAsync(instance, $"{logPrefix}花费 {cost} 金币强化了 {enhanceType}（+{enhanceValue}），持续 3 场战斗。");
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task ExecuteSpecialEventAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var eventData = ParseEventData(eventConfig.EventDataJson);
            var effectType = "";
            if (eventData.TryGetProperty("effectType", out var effectProp))
                effectType = effectProp.GetString() ?? "";

            switch (effectType)
            {
                // === 无参数即时事件 ===
                case "skipTick":
                    // 存标记，下次tick跳过事件
                    {
                        var buffs = LoadActiveBuffs(instance);
                        buffs.Add(new ActiveBuff("Special_SkipTick", 1, false, 1, "tick", eventConfig.Id));
                        SaveActiveBuffs(instance, buffs);
                        var skipMsg = FormatEventLog(logPrefix, eventConfig, "地震！下次探索将被跳过。");
                        await AppendExploreLogAsync(instance, skipMsg);
                        await AppendPartyLogAsync(instance, skipMsg);
                        await _instanceRepository.UpdateAsync(instance);
                    }
                    break;

                case "purgeDebuffs":
                    await SpecialPurgeDebuffs(instance, eventConfig, logPrefix);
                    break;

                case "hpMpSwap":
                    var tmpHp = instance.CurrentHp;
                    instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentMp);
                    instance.CurrentMp = Math.Min(instance.MaxMp, tmpHp);
                    var swapMsg = FormatEventLog(logPrefix, eventConfig, "生命和灵力互换了！");
                    await AppendExploreLogAsync(instance, swapMsg);
                    // 队伍共享 HP/MP 互换
                    if (!string.IsNullOrEmpty(instance.PartyId))
                    {
                        await ApplyToPartyMembersAsync(instance, member =>
                        {
                            var mTmpHp = member.CurrentHp;
                            member.CurrentHp = Math.Min(member.MaxHp, member.CurrentMp);
                            member.CurrentMp = Math.Min(member.MaxMp, mTmpHp);
                        });
                        await AppendPartyLogAsync(instance, swapMsg);
                    }
                    await _instanceRepository.UpdateAsync(instance);
                    break;

                // === 有持续时间的事件（存入ActiveBuffs）===
                case "rewardMultiplier":
                    await SpecialStoreBuff(instance, eventConfig, eventData, "Special_RewardMultiplier", logPrefix);
                    break;

                case "monsterBuff":
                    await SpecialStoreBuff(instance, eventConfig, eventData, "Special_MonsterBuff", logPrefix);
                    break;

                case "rareBoost":
                    await SpecialStoreBuff(instance, eventConfig, eventData, "Special_RareBoost", logPrefix);
                    break;

                case "trapBoost":
                    await SpecialStoreBuff(instance, eventConfig, eventData, "Special_TrapBoost", logPrefix);
                    break;

                case "regenTick":
                    await SpecialRegenTick(instance, eventConfig, eventData, logPrefix);
                    break;

                case "buffMultiplier":
                    await SpecialStoreBuff(instance, eventConfig, eventData, "Special_BuffMultiplier", logPrefix);
                    break;

                case "weightModifier":
                    await SpecialWeightModifier(instance, eventConfig, eventData, logPrefix);
                    break;

                case "instantBuff":
                    await SpecialInstantBuff(instance, eventConfig, eventData, logPrefix);
                    break;

                case "extendBuffs":
                    await SpecialExtendBuffs(instance, eventConfig, eventData, logPrefix);
                    break;

                case "summonClone":
                    await SpecialSummonClone(instance, eventConfig, eventData, logPrefix);
                    break;

                // === 即时效果事件 ===
                case "instantHeal":
                    await SpecialInstantHeal(instance, eventConfig, eventData, logPrefix);
                    break;

                case "reviveOnce":
                    await SpecialReviveOnce(instance, eventConfig, eventData, logPrefix);
                    break;

                case "mystery":
                    await SpecialMystery(instance, eventConfig, eventData, logPrefix);
                    break;

                case "randomEvent":
                    await SpecialRandomEvent(instance, eventConfig, eventData, logPrefix);
                    break;

                default:
                    var defaultMsg = FormatEventLog(logPrefix, eventConfig, "");
                    await AppendExploreLogAsync(instance, defaultMsg);
                    await AppendPartyLogAsync(instance, defaultMsg);
                    break;
            }
        }

        private async Task SpecialPurgeDebuffs(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            var debuffTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DamagePercent", "DamageOverTime", "AttackDown", "DefenseDown", "SpeedDown",
                "CritRateDown", "Silence", "Stun", "HealBlock", "AllStatsDown", "WeightModifier"
            };
            var buffs = LoadActiveBuffs(instance);
            var removed = buffs.RemoveAll(b => debuffTypes.Contains(b.BuffType));
            SaveActiveBuffs(instance, buffs);
            var purgeEffect = removed > 0 ? $"净化了 {removed} 个减益效果！" : "没有减益效果需要净化。";
            var purgeMsg = FormatEventLog(logPrefix, eventConfig, purgeEffect);
            await AppendExploreLogAsync(instance, purgeMsg);

            // 队伍共享净化
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    memberBuffs.RemoveAll(b => debuffTypes.Contains(b.BuffType));
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, purgeMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialStoreBuff(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string buffType, string logPrefix)
        {
            var value = 0;
            if (eventData.TryGetProperty("value", out var vProp)) value = vProp.GetInt32();
            var duration = 3;
            if (eventData.TryGetProperty("duration", out var dProp)) duration = dProp.GetInt32();
            var durationType = "tick";
            if (eventData.TryGetProperty("durationType", out var dtProp)) durationType = dtProp.GetString() ?? "tick";

            var newBuff = new ActiveBuff(buffType, value, false, duration, durationType, eventConfig.Id);
            var buffs = LoadActiveBuffs(instance);
            var existingIdx = buffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
            if (existingIdx >= 0)
                buffs[existingIdx] = newBuff;
            else
                buffs.Add(newBuff);
            SaveActiveBuffs(instance, buffs);

            var durationLabel = durationType == "battle" ? $"{duration}场战斗" : $"{duration}步";
            var storeMsg = FormatEventLog(logPrefix, eventConfig, $"效果激活（+{value}），持续{durationLabel}。");
            await AppendExploreLogAsync(instance, storeMsg);

            // 队伍共享 buff
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    var memberIdx = memberBuffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
                    if (memberIdx >= 0)
                        memberBuffs[memberIdx] = newBuff;
                    else
                        memberBuffs.Add(newBuff);
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, storeMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialRegenTick(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var hpPercent = 0;
            var mpPercent = 0;
            if (eventData.TryGetProperty("healHpPercent", out var hp)) hpPercent = hp.GetInt32();
            if (eventData.TryGetProperty("healMpPercent", out var mp)) mpPercent = mp.GetInt32();
            var duration = 3;
            if (eventData.TryGetProperty("duration", out var d)) duration = d.GetInt32();
            var durationType = "tick";
            if (eventData.TryGetProperty("durationType", out var dt)) durationType = dt.GetString() ?? "tick";

            // 用特殊buffType存储，值 = hpPercent * 1000 + mpPercent
            var encodedValue = hpPercent * 1000 + mpPercent;
            var newBuff = new ActiveBuff("Special_RegenTick", encodedValue, false, duration, durationType, eventConfig.Id);
            var buffs = LoadActiveBuffs(instance);
            buffs.Add(newBuff);
            SaveActiveBuffs(instance, buffs);

            var regenMsg = FormatEventLog(logPrefix, eventConfig, $"持续恢复效果激活（HP {hpPercent}%/MP {mpPercent}%），持续 {duration} 步。");
            await AppendExploreLogAsync(instance, regenMsg);

            // 队伍共享 regen buff
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    memberBuffs.Add(newBuff);
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, regenMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialWeightModifier(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var multiplier = 100;
            if (eventData.TryGetProperty("multiplier", out var m)) multiplier = m.GetInt32();
            var duration = 1;
            if (eventData.TryGetProperty("duration", out var d)) duration = d.GetInt32();
            var durationType = "tick";
            if (eventData.TryGetProperty("durationType", out var dt)) durationType = dt.GetString() ?? "tick";

            var boostTypes = new List<int>();
            if (eventData.TryGetProperty("boostTypes", out var btProp))
                foreach (var bt in btProp.EnumerateArray()) boostTypes.Add(bt.GetInt32());

            // 存入ActiveBuffs，boostTypes 编码到 Source 字段（逗号分隔）
            var boostTypesStr = string.Join(",", boostTypes);
            var buffs = LoadActiveBuffs(instance);
            buffs.Add(new ActiveBuff("Special_WeightModifier", multiplier, false, duration, durationType, boostTypesStr));
            SaveActiveBuffs(instance, buffs);

            var typeNames = string.Join(",", boostTypes);
            var weightMsg = FormatEventLog(logPrefix, eventConfig, $"事件权重修改（类型 [{typeNames}] x{multiplier}%），持续 {duration} 步。");
            await AppendExploreLogAsync(instance, weightMsg);
            await AppendPartyLogAsync(instance, weightMsg);
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialInstantBuff(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var buffType = "";
            if (eventData.TryGetProperty("buffType", out var bt)) buffType = bt.GetString() ?? "";
            var value = 0;
            if (eventData.TryGetProperty("value", out var v)) value = v.GetInt32();
            var duration = 3;
            if (eventData.TryGetProperty("duration", out var d)) duration = d.GetInt32();
            var durationType = "battle";
            if (eventData.TryGetProperty("durationType", out var dt)) durationType = dt.GetString() ?? "battle";

            if (!string.IsNullOrEmpty(buffType))
            {
                var newBuff = new ActiveBuff(buffType, value, true, duration, durationType, eventConfig.Id);
                var buffs = LoadActiveBuffs(instance);
                var existingIdx = buffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
                if (existingIdx >= 0)
                    buffs[existingIdx] = buffs[existingIdx] with { RemainingDuration = Math.Max(buffs[existingIdx].RemainingDuration, duration), Value = value };
                else
                    buffs.Add(newBuff);
                SaveActiveBuffs(instance, buffs);

                var durationLabel = durationType == "battle" ? $"{duration}场战斗" : $"{duration}步";
                var instantMsg = FormatEventLog(logPrefix, eventConfig, $"获得增益 {GetBuffDisplayName(buffType)}（+{value}），持续{durationLabel}。");
                await AppendExploreLogAsync(instance, instantMsg);

                // 队伍共享 buff
                if (!string.IsNullOrEmpty(instance.PartyId))
                {
                    await ApplyToPartyMembersAsync(instance, member =>
                    {
                        var memberBuffs = LoadActiveBuffs(member);
                        var memberIdx = memberBuffs.FindIndex(b => b.BuffType == buffType && b.DurationType == durationType);
                        if (memberIdx >= 0)
                            memberBuffs[memberIdx] = memberBuffs[memberIdx] with { RemainingDuration = Math.Max(memberBuffs[memberIdx].RemainingDuration, duration), Value = value };
                        else
                            memberBuffs.Add(newBuff);
                        SaveActiveBuffs(member, memberBuffs);
                    });
                    await AppendPartyLogAsync(instance, instantMsg);
                }

                await _instanceRepository.UpdateAsync(instance);
            }
        }

        private async Task SpecialExtendBuffs(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var extendTicks = 1;
            if (eventData.TryGetProperty("duration", out var d)) extendTicks = d.GetInt32();

            var buffs = LoadActiveBuffs(instance);
            var count = 0;
            for (var i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].DurationType != "dungeon")
                {
                    buffs[i] = buffs[i] with { RemainingDuration = buffs[i].RemainingDuration + extendTicks };
                    count++;
                }
            }
            SaveActiveBuffs(instance, buffs);

            var extendEffect = count > 0 ? $"延长了 {count} 个效果 {extendTicks} 步。" : "没有可延长的效果。";
            var extendMsg = FormatEventLog(logPrefix, eventConfig, extendEffect);
            await AppendExploreLogAsync(instance, extendMsg);

            // 队伍共享延长 buff
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    for (var i = 0; i < memberBuffs.Count; i++)
                    {
                        if (memberBuffs[i].DurationType != "dungeon")
                        {
                            memberBuffs[i] = memberBuffs[i] with { RemainingDuration = memberBuffs[i].RemainingDuration + extendTicks };
                        }
                    }
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, extendMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        /// <summary>
        /// 检查是否有复活 buff，如果有则消耗并复活玩家。返回 true 表示成功复活。
        /// </summary>
        private async Task<bool> TryReviveOnce(DungeonInstanceEntity instance)
        {
            var buffs = LoadActiveBuffs(instance);
            var reviveIdx = buffs.FindIndex(b => b.BuffType == "Special_ReviveOnce");
            if (reviveIdx < 0) return false;

            var healPercent = buffs[reviveIdx].Value;
            buffs.RemoveAt(reviveIdx);
            SaveActiveBuffs(instance, buffs);

            var healHp = instance.MaxHp * healPercent / 100;
            instance.CurrentHp = Math.Max(1, healHp);
            return true;
        }

        private async Task SpecialSummonClone(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var attackPercent = 40;
            var hpPercent = 50;
            if (eventData.TryGetProperty("attackPercent", out var ap)) attackPercent = ap.GetInt32();
            if (eventData.TryGetProperty("hpPercent", out var hp)) hpPercent = hp.GetInt32();
            var duration = 1;
            if (eventData.TryGetProperty("duration", out var d)) duration = d.GetInt32();
            var durationType = "battle";
            if (eventData.TryGetProperty("durationType", out var dt)) durationType = dt.GetString() ?? "battle";

            // 编码：attackPercent * 1000 + hpPercent
            var encodedValue = attackPercent * 1000 + hpPercent;
            var buffs = LoadActiveBuffs(instance);
            var existingIdx = buffs.FindIndex(b => b.BuffType == "Special_SummonClone" && b.DurationType == durationType);
            var newBuff = new ActiveBuff("Special_SummonClone", encodedValue, false, duration, durationType, eventConfig.Id);
            if (existingIdx >= 0)
                buffs[existingIdx] = newBuff;
            else
                buffs.Add(newBuff);
            SaveActiveBuffs(instance, buffs);

            var durationLabel = durationType == "battle" ? $"{duration}场战斗" : $"{duration}步";
            var cloneMsg = FormatEventLog(logPrefix, eventConfig, $"召唤分身（攻击{attackPercent}%，生命{hpPercent}%），持续{durationLabel}。");
            await AppendExploreLogAsync(instance, cloneMsg);
            await AppendPartyLogAsync(instance, cloneMsg);
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialInstantHeal(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var hpPercent = 0;
            var mpPercent = 0;
            if (eventData.TryGetProperty("healHpPercent", out var hp)) hpPercent = hp.GetInt32();
            if (eventData.TryGetProperty("healMpPercent", out var mp)) mpPercent = mp.GetInt32();

            var hpHeal = instance.MaxHp * hpPercent / 100;
            var mpHeal = instance.MaxMp * mpPercent / 100;
            var oldHp = instance.CurrentHp;
            var oldMp = instance.CurrentMp;
            instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + hpHeal);
            instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + mpHeal);
            var actualHp = instance.CurrentHp - oldHp;
            var actualMp = instance.CurrentMp - oldMp;

            // 队伍共享回复
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    member.CurrentHp = Math.Min(member.MaxHp, member.CurrentHp + member.MaxHp * hpPercent / 100);
                    member.CurrentMp = Math.Min(member.MaxMp, member.CurrentMp + member.MaxMp * mpPercent / 100);
                });
            }

            if (actualHp == 0 && actualMp == 0)
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "生命和灵力充沛，无需恢复。"));
                if (!string.IsNullOrEmpty(instance.PartyId))
                    await AppendPartyLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "生命和灵力充沛，无需恢复。"));
            }
            else
            {
                var healMsg = FormatEventLog(logPrefix, eventConfig, $"恢复了 {actualHp} 生命和 {actualMp} 灵力。");
                await AppendExploreLogAsync(instance, healMsg);
                await AppendPartyLogAsync(instance, healMsg);
            }
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialReviveOnce(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var healPercent = 50;
            if (eventData.TryGetProperty("healPercent", out var h)) healPercent = h.GetInt32();

            // 存标记：死亡时自动复活
            var newBuff = new ActiveBuff("Special_ReviveOnce", healPercent, false, 1, "dungeon", eventConfig.Id);
            var buffs = LoadActiveBuffs(instance);
            buffs.Add(newBuff);
            SaveActiveBuffs(instance, buffs);

            var reviveMsg = FormatEventLog(logPrefix, eventConfig, $"获得了复活祝福（死亡时恢复 {healPercent}% HP）。");
            await AppendExploreLogAsync(instance, reviveMsg);

            // 队伍共享复活祝福
            if (!string.IsNullOrEmpty(instance.PartyId))
            {
                await ApplyToPartyMembersAsync(instance, member =>
                {
                    var memberBuffs = LoadActiveBuffs(member);
                    memberBuffs.Add(newBuff);
                    SaveActiveBuffs(member, memberBuffs);
                });
                await AppendPartyLogAsync(instance, reviveMsg);
            }

            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task SpecialMystery(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var options = new List<string>();
            var weights = new List<int>();
            if (eventData.TryGetProperty("options", out var oProp))
                foreach (var o in oProp.EnumerateArray()) options.Add(o.GetString() ?? "");
            if (eventData.TryGetProperty("weights", out var wProp))
                foreach (var w in wProp.EnumerateArray()) weights.Add(w.GetInt32());

            if (options.Count == 0)
            {
                var noOptMsg = FormatEventLog(logPrefix, eventConfig, "神秘力量涌动，但什么都没发生。");
                await AppendExploreLogAsync(instance, noOptMsg);
                await AppendPartyLogAsync(instance, noOptMsg);
                return;
            }

            // 权重随机
            var totalWeight = weights.Take(options.Count).Sum();
            if (totalWeight <= 0) totalWeight = options.Count;
            var pick = Random.Shared.Next(totalWeight);
            var cumulative = 0;
            var chosen = options[0];
            for (var i = 0; i < options.Count; i++)
            {
                cumulative += i < weights.Count ? weights[i] : 1;
                if (pick < cumulative) { chosen = options[i]; break; }
            }

            // 执行效果
            switch (chosen)
            {
                case "fullRestore":
                    instance.CurrentHp = instance.MaxHp;
                    instance.CurrentMp = instance.MaxMp;
                    // 队伍共享完全恢复
                    if (!string.IsNullOrEmpty(instance.PartyId))
                    {
                        await ApplyToPartyMembersAsync(instance, member =>
                        {
                            member.CurrentHp = member.MaxHp;
                            member.CurrentMp = member.MaxMp;
                        });
                    }
                    var mysteryRestoreMsg = FormatEventLog(logPrefix, eventConfig, "神秘力量完全恢复了你的生命和灵力！");
                    await AppendExploreLogAsync(instance, mysteryRestoreMsg);
                    await AppendPartyLogAsync(instance, mysteryRestoreMsg);
                    await _instanceRepository.UpdateAsync(instance);
                    break;
                case "allStatsBuff":
                    var mysteryBuff = new ActiveBuff("AllStatsUp", 20, true, 3, "battle", eventConfig.Id);
                    var buffs = LoadActiveBuffs(instance);
                    buffs.Add(mysteryBuff);
                    SaveActiveBuffs(instance, buffs);
                    // 队伍共享全属性增益
                    if (!string.IsNullOrEmpty(instance.PartyId))
                    {
                        await ApplyToPartyMembersAsync(instance, member =>
                        {
                            var memberBuffs = LoadActiveBuffs(member);
                            memberBuffs.Add(mysteryBuff);
                            SaveActiveBuffs(member, memberBuffs);
                        });
                    }
                    var mysteryBuffMsg = FormatEventLog(logPrefix, eventConfig, "神秘力量提升了你的全属性！");
                    await AppendExploreLogAsync(instance, mysteryBuffMsg);
                    await AppendPartyLogAsync(instance, mysteryBuffMsg);
                    await _instanceRepository.UpdateAsync(instance);
                    break;
                case "forceBattle":
                    // 触发一个随机战斗事件
                    var battleEvents = GameData.DungeonEventConfigsByDungeonAndType
                        .Where(kv => kv.Key.EndsWith($":{(int)DungeonEventType.Battle}"))
                        .SelectMany(kv => kv.Value)
                        .Where(e => e.Enabled)
                        .ToList();
                    if (battleEvents.Count > 0)
                    {
                        var randomBattle = battleEvents[Random.Shared.Next(battleEvents.Count)];
                        var battleMsg = FormatEventLog(logPrefix, eventConfig, "神秘力量引来了怪物！");
                        await AppendExploreLogAsync(instance, battleMsg);
                        await AppendPartyLogAsync(instance, battleMsg);
                        await ExecuteBattleEventAsync(instance, randomBattle, logPrefix);
                    }
                    break;
                case "noEffect":
                    var noEffectMsg = FormatEventLog(logPrefix, eventConfig, "神秘力量涌动，但什么都没发生。");
                    await AppendExploreLogAsync(instance, noEffectMsg);
                    await AppendPartyLogAsync(instance, noEffectMsg);
                    break;
                default:
                    var chosenMsg = FormatEventLog(logPrefix, eventConfig, $"触发了「{chosen}」效果。");
                    await AppendExploreLogAsync(instance, chosenMsg);
                    await AppendPartyLogAsync(instance, chosenMsg);
                    break;
            }
        }

        private async Task SpecialRandomEvent(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, JsonElement eventData, string logPrefix)
        {
            var eventTypes = new List<int>();
            var weights = new List<int>();
            if (eventData.TryGetProperty("eventTypes", out var etProp))
                foreach (var et in etProp.EnumerateArray()) eventTypes.Add(et.GetInt32());
            if (eventData.TryGetProperty("weights", out var wProp))
                foreach (var w in wProp.EnumerateArray()) weights.Add(w.GetInt32());

            if (eventTypes.Count == 0)
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, "随机事件池为空。"));
                return;
            }

            // 权重随机选事件类型
            var totalWeight = weights.Take(eventTypes.Count).Sum();
            if (totalWeight <= 0) totalWeight = eventTypes.Count;
            var pick = Random.Shared.Next(totalWeight);
            var cumulative = 0;
            var chosenType = eventTypes[0];
            for (var i = 0; i < eventTypes.Count; i++)
            {
                cumulative += i < weights.Count ? weights[i] : 1;
                if (pick < cumulative) { chosenType = eventTypes[i]; break; }
            }

            // 从该类型中随机选一个事件
            var key = $"*:{chosenType}";
            List<DungeonEventConfigEntity> candidates = [];
            if (GameData.DungeonEventConfigsByDungeonAndType.TryGetValue(key, out var list))
                candidates = list.Where(e => e.Enabled).ToList();

            if (candidates.Count == 0)
            {
                // 尝试按 dungeonId 查找
                candidates = GameData.DungeonEventConfigsByDungeonAndType
                    .Where(kv => kv.Key.EndsWith($":{chosenType}"))
                    .SelectMany(kv => kv.Value)
                    .Where(e => e.Enabled)
                    .ToList();
            }

            if (candidates.Count > 0)
            {
                var chosenEvent = candidates[Random.Shared.Next(candidates.Count)];
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, $"随机触发了事件类型 {chosenType}。"));
                await ExecuteEventAsync(instance, chosenEvent);
            }
            else
            {
                await AppendExploreLogAsync(instance, FormatEventLog(logPrefix, eventConfig, $"事件类型 {chosenType} 没有可用事件。"));
            }
        }

        private async Task ExecutePlayerEncounterAsync(DungeonInstanceEntity instance, DungeonEventConfigEntity eventConfig, string logPrefix)
        {
            // 查询同秘境中其他运行中的实例
            var otherInstances = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.DungeonId == instance.DungeonId &&
                            i.Status == (int)DungeonInstanceStatus.Running &&
                            i.Id != instance.Id &&
                            i.PartyId == null) // 未组队的
                .ToListAsync();

            if (otherInstances.Count == 0)
            {
                var noTargetMsg = FormatEventLog(logPrefix, eventConfig, "秘境中没有遇到其他探索者。");
                await AppendExploreLogAsync(instance, noTargetMsg);
                await AppendPartyLogAsync(instance, noTargetMsg);
                return;
            }

            // 随机选择一个
            var target = otherInstances[Random.Shared.Next(otherInstances.Count)];

            // 二次确认目标仍未组队（防止竞态：两个玩家同时触发偶遇）
            var freshTarget = await _instanceRepository.GetByIdAsync(target.Id);
            if (freshTarget == null || freshTarget.Status != (int)DungeonInstanceStatus.Running || !string.IsNullOrEmpty(freshTarget.PartyId))
            {
                var passMsg = FormatEventLog(logPrefix, eventConfig, "擦肩而过，没有深入交流。");
                await AppendExploreLogAsync(instance, passMsg);
                await AppendPartyLogAsync(instance, passMsg);
                return;
            }

            // 读取偶遇配置
            var encounterConfig = ParseEncounterConfig(instance.DungeonId);
            var teamThreshold = encounterConfig.FavorabilityTeamThreshold;

            // 检查好感度
            var favorability = await GetFavorabilityAsync(instance.PlayerId, target.PlayerId);

            // 概率判定：好感度越高，组队概率越大
            var teamChance = Math.Min(0.8, teamThreshold + favorability * 0.01);
            var roll = Random.Shared.NextDouble();

            if (roll < teamChance)
            {
                // 组队
                await FormPartyAsync(instance, target);
                var teamMsg = $"{logPrefix}偶遇玩家，志趣相投，结伴同行！";
                await AppendExploreLogAsync(instance, teamMsg);
                await AppendExploreLogAsync(target, $"[{DateTime.Now:HH:mm:ss}] 偶遇玩家，志趣相投，结伴同行！");
                await AppendPartyLogAsync(instance, teamMsg);
            }
            else
            {
                // PvP
                await ExecutePvPEncounterAsync(instance, target, encounterConfig, logPrefix);
            }
        }

        private async Task ExecutePvPEncounterAsync(DungeonInstanceEntity instance, DungeonInstanceEntity target, EncounterConfig config, string logPrefix)
        {
            var user = await _userRepository.GetByIdAsync(instance.PlayerId);
            var targetUser = await _userRepository.GetByIdAsync(target.PlayerId);
            if (user == null || targetUser == null)
            {
                return;
            }

            user.Type1 = (int)instance.CurrentHp;
            user.Type2 = (int)instance.CurrentMp;
            targetUser.Type1 = (int)target.CurrentHp;
            targetUser.Type2 = (int)target.CurrentMp;

            var result = BattleSystem.StartBattle([user], [targetUser]);

            if (result.IsVictory)
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}遭遇其他玩家，PvP 战斗胜利！");
                await AppendExploreLogAsync(target, $"[{DateTime.Now:HH:mm:ss}] 遭遇其他玩家，PvP 战斗失败。");

                // 掠夺
                var plunder = config.PvpPlunderConfig;
                if (plunder != null)
                {
                    var goldPlunder = (long)(target.CurrentHp > 0 ? 100 : 50) * plunder.CurrencyPlunderRate / 100;
                    if (goldPlunder > 0)
                    {
                        await AddRewardAsync(instance, "Gold", null, (int)goldPlunder, "PlayerEncounter", false);
                    }
                }

                // 失败方结算退出
                await SettleCoreAsync(target.Id, (int)DungeonSettleReason.Death);
            }
            else
            {
                await AppendExploreLogAsync(instance, $"{logPrefix}遭遇其他玩家，PvP 战斗失败...");
                await AppendExploreLogAsync(target, $"[{DateTime.Now:HH:mm:ss}] 遭遇其他玩家，PvP 战斗胜利！");

                // 失败方结算退出
                await SettleCoreAsync(instance.Id, (int)DungeonSettleReason.Death);
            }
        }

        private async Task FormPartyAsync(DungeonInstanceEntity instance, DungeonInstanceEntity target)
        {
            var party = new DungeonPartyEntity
            {
                PartyId = Guid.NewGuid().ToString("N"),
                DungeonId = instance.DungeonId,
                LeaderPlayerId = instance.PlayerId,
                MemberJson = JsonSerializer.Serialize(new List<string> { instance.PlayerId, target.PlayerId })
            };
            await _partyRepository.AddAsync(party);

            instance.PartyId = party.PartyId;
            instance.IsPartyLeader = true;
            await _instanceRepository.UpdateAsync(instance);

            target.PartyId = party.PartyId;
            target.IsPartyLeader = false;
            await _instanceRepository.UpdateAsync(target);
        }

        private async Task DisbandPartyAsync(string partyId)
        {
            var party = await _partyRepository.GetByIdAsync(partyId);
            if (party == null)
            {
                return;
            }

            // 清除所有成员的队伍引用
            var members = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.PartyId == partyId)
                .ToListAsync();

            foreach (var member in members)
            {
                member.PartyId = null;
                member.IsPartyLeader = false;
                await _instanceRepository.UpdateAsync(member);
            }

            await _partyRepository.DeleteAsync(partyId);
        }

        private async Task<List<DungeonInstanceEntity>> GetPartyMemberInstancesAsync(string partyId)
        {
            return await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.PartyId == partyId)
                .ToListAsync();
        }

        private async Task ApplyToPartyMembersAsync(DungeonInstanceEntity instance, Action<DungeonInstanceEntity> action)
        {
            if (string.IsNullOrEmpty(instance.PartyId)) return;
            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
            foreach (var member in members)
            {
                if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                {
                    action(member);
                    await _instanceRepository.UpdateAsync(member);
                }
            }
        }

        private async Task AppendPartyLogAsync(DungeonInstanceEntity instance, string message)
        {
            if (string.IsNullOrEmpty(instance.PartyId)) return;
            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
            foreach (var member in members)
            {
                if (member.Id != instance.Id && member.Status == (int)DungeonInstanceStatus.Running)
                    await AppendExploreLogAsync(member, message);
            }
        }

        private async Task<DungeonInstanceEntity?> PickRandomPartyMemberAsync(DungeonInstanceEntity instance)
        {
            if (string.IsNullOrEmpty(instance.PartyId)) return null;
            var members = await GetPartyMemberInstancesAsync(instance.PartyId);
            var candidates = members.Where(m => m.Id != instance.Id && m.Status == (int)DungeonInstanceStatus.Running).ToList();
            if (candidates.Count == 0) return null;
            return candidates[Random.Shared.Next(candidates.Count)];
        }

        private async Task RemoveFromPartyAsync(DungeonInstanceEntity instance)
        {
            if (string.IsNullOrEmpty(instance.PartyId)) return;
            var partyId = instance.PartyId;

            // 移除该成员
            instance.PartyId = null;
            instance.IsPartyLeader = false;
            await _instanceRepository.UpdateAsync(instance);

            // 检查剩余成员
            var remaining = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.PartyId == partyId && i.Status == (int)DungeonInstanceStatus.Running)
                .ToListAsync();

            if (remaining.Count <= 1)
            {
                // 只剩一人或无人，解散队伍
                await DisbandPartyAsync(partyId);
            }
            else if (instance.IsPartyLeader)
            {
                // 队长离开，转移队长给第一个剩余成员
                var newLeader = remaining.First();
                newLeader.IsPartyLeader = true;
                await _instanceRepository.UpdateAsync(newLeader);

                // 更新 DungeonPartyEntity
                var party = await _partyRepository.GetByIdAsync(partyId);
                if (party != null)
                {
                    party.LeaderPlayerId = newLeader.PlayerId;
                    party.MemberJson = JsonSerializer.Serialize(remaining.Select(m => m.PlayerId).ToList());
                    await _partyRepository.UpdateAsync(party);
                }
            }
            else
            {
                // 普通成员离开，更新 MemberJson
                var party = await _partyRepository.GetByIdAsync(partyId);
                if (party != null)
                {
                    party.MemberJson = JsonSerializer.Serialize(remaining.Select(m => m.PlayerId).ToList());
                    await _partyRepository.UpdateAsync(party);
                }
            }
        }

        private async Task<int> GetFavorabilityAsync(string playerId, string targetPlayerId)
        {
            // 简化实现：查询好感度表
            try
            {
                var fav = await _userRepository.Db.Queryable<PlayerFavorabilityEntity>()
                    .Where(f => f.PlayerId == playerId && f.TargetPlayerId == targetPlayerId)
                    .FirstAsync();
                return fav?.Value ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<DungeonInstanceAvailableDto>> GetAvailableDungeonsAsync(string playerId)
        {
            var templates = await _templateRepository.GetAllAsync();
            var enabled = templates.Where(t => t.Enabled).ToList();

            var today = DateTime.UtcNow.Date;
            var dailyRecords = await _dailyRecordRepository.GetAllAsync();
            var todayRecords = dailyRecords
                .Where(r => r.PlayerId == playerId && r.EnterDate.Date == today)
                .ToDictionary(r => r.DungeonId, r => r.EnterCount);

            // 预加载道具名称映射
            var itemNames = new Dictionary<string, string>();
            foreach (var t in enabled)
            {
                if (string.IsNullOrEmpty(t.EntryCostsJson)) continue;
                try
                {
                    var costs = JsonSerializer.Deserialize<JsonElement[]>(t.EntryCostsJson);
                    if (costs == null) continue;
                    foreach (var cost in costs)
                    {
                        if (cost.TryGetProperty("type", out var typeProp) &&
                            typeProp.GetString() == "Item" &&
                            cost.TryGetProperty("id", out var idProp))
                        {
                            var itemId = idProp.GetString() ?? "";
                            if (!string.IsNullOrEmpty(itemId) && !itemNames.ContainsKey(itemId))
                            {
                                var itemTemplate = GameData.Items.GetValueOrDefault(itemId);
                                itemNames[itemId] = itemTemplate?.Name ?? itemId;
                            }
                        }
                    }
                }
                catch { /* ignore parse errors */ }
            }

            return enabled.Select(t => new DungeonInstanceAvailableDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description ?? "",
                RecommendedLevel = t.RecommendedLevel,
                DailyEnterLimit = t.DailyEnterLimit,
                TodayEnterCount = todayRecords.TryGetValue(t.Id, out var count) ? count : 0,
                EntryCostsJson = ResolveEntryCostNames(t.EntryCostsJson, itemNames)
            }).ToList();
        }

        public async Task<List<DungeonInstanceHistoryDto>> GetHistoryAsync(string playerId, int limit = 20)
        {
            var instances = await _instanceRepository.Db.Queryable<DungeonInstanceEntity>()
                .Where(i => i.PlayerId == playerId && i.Status == (int)DungeonInstanceStatus.Settled)
                .OrderByDescending(i => i.SettleTime)
                .Take(limit)
                .ToListAsync();

            if (instances.Count == 0)
                return [];

            var instanceIds = instances.Select(i => i.Id.ToString()).ToList();
            var allRewards = await _rewardPoolRepository.Db.Queryable<DungeonRewardPoolEntity>()
                .Where(r => instanceIds.Contains(r.InstanceId))
                .ToListAsync();

            var result = new List<DungeonInstanceHistoryDto>();
            foreach (var instance in instances)
            {
                var dungeonName = GameData.DungeonInstanceTemplates.TryGetValue(instance.DungeonId, out var tpl)
                    ? tpl.Name
                    : instance.DungeonId;

                var exploreLog = new List<string>();
                if (!string.IsNullOrEmpty(instance.ExploreLogJson))
                {
                    try { exploreLog = JsonSerializer.Deserialize<List<string>>(instance.ExploreLogJson) ?? []; }
                    catch { /* ignore */ }
                }

                var rewards = allRewards
                    .Where(r => r.InstanceId == instance.Id.ToString())
                    .GroupBy(r => new { r.RewardType, r.RewardId })
                    .Select(g =>
                    {
                        var dto = new DungeonRewardSummaryDto
                        {
                            RewardType = g.Key.RewardType,
                            RewardId = g.Key.RewardId,
                            Quantity = g.Sum(x => x.Quantity)
                        };
                        dto.RewardName = dto.RewardType switch
                        {
                            "Item" when !string.IsNullOrEmpty(dto.RewardId)
                                && GameData.Items.TryGetValue(dto.RewardId, out var itemTpl) => itemTpl.Name,
                            "Equipment" when !string.IsNullOrEmpty(dto.RewardId)
                                && int.TryParse(dto.RewardId, out var eqId)
                                && GameData.EquipmentTemplates.TryGetValue(eqId, out var eqTpl) => eqTpl.Name,
                            _ => null
                        };
                        return dto;
                    })
                    .ToList();

                var settleReason = instance.SettleReason switch
                {
                    (int)DungeonSettleReason.ActiveQuit => "主动退出",
                    (int)DungeonSettleReason.Death => "角色死亡",
                    (int)DungeonSettleReason.ForceClose => "秘境关闭",
                    _ => "未知"
                };

                result.Add(new DungeonInstanceHistoryDto
                {
                    InstanceId = instance.Id,
                    DungeonId = instance.DungeonId,
                    DungeonName = dungeonName,
                    EnterTime = instance.EnterTime,
                    SettleTime = instance.SettleTime,
                    SettleReason = settleReason,
                    ExploreLog = exploreLog,
                    RewardSummary = rewards
                });
            }

            return result;
        }

        private static string? ResolveEntryCostNames(string? json, Dictionary<string, string> itemNames)
        {
            if (string.IsNullOrEmpty(json)) return json;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var resolved = doc.RootElement.EnumerateArray().Select(cost =>
                {
                    var type = cost.TryGetProperty("type", out var t) ? t.GetString() : "";
                    var qty = cost.TryGetProperty("quantity", out var q) ? q.GetInt32() : 0;
                    var id = cost.TryGetProperty("id", out var i) ? i.GetString() : "";
                    var name = "";
                    if (type == "Item" && !string.IsNullOrEmpty(id))
                        name = itemNames.GetValueOrDefault(id, id);
                    return new { type, quantity = qty, id, name };
                }).ToArray();
                return JsonSerializer.Serialize(resolved);
            }
            catch { return json; }
        }

        #endregion

        #region 辅助方法

        private async Task<long> GetDungeonAvailableGoldAsync(DungeonInstanceEntity instance)
        {
            // 计算秘境内可用金币 = 奖励池中获得的金币 - 商店消费的金币
            var rewards = await _rewardPoolRepository.Db.Queryable<DungeonRewardPoolEntity>()
                .Where(r => r.InstanceId == instance.Id.ToString())
                .ToListAsync();

            long earned = 0;
            long spent = 0;
            foreach (var r in rewards)
            {
                if (r.RewardType == "Gold" && r.Quantity > 0) earned += r.Quantity;
                if (r.RewardType == "ShopSpend") spent += r.Quantity;
            }
            return earned - spent;
        }

        private async Task AddRewardAsync(DungeonInstanceEntity instance, string rewardType, string? rewardId, int quantity, string source, bool deathKeep)
        {
            var reward = new DungeonRewardPoolEntity
            {
                PlayerId = instance.PlayerId,
                InstanceId = instance.Id.ToString(),
                RewardType = rewardType,
                RewardId = rewardId,
                Quantity = quantity,
                Source = source,
                DeathKeep = deathKeep
            };
            await _rewardPoolRepository.AddAsync(reward);
        }

        private string FormatEventLog(string logPrefix, DungeonEventConfigEntity eventConfig, string effect)
        {
            var desc = eventConfig.Description;
            if (!string.IsNullOrEmpty(desc))
                return string.IsNullOrEmpty(effect)
                    ? $"{logPrefix}【{eventConfig.Name}】{desc}"
                    : $"{logPrefix}【{eventConfig.Name}】{desc} → {effect}";
            return string.IsNullOrEmpty(effect)
                ? $"{logPrefix}【{eventConfig.Name}】"
                : $"{logPrefix}【{eventConfig.Name}】{effect}";
        }

        private async Task AppendExploreLogAsync(DungeonInstanceEntity instance, string logEntry)
        {
            var log = new List<string>();
            if (!string.IsNullOrEmpty(instance.ExploreLogJson))
            {
                try { log = JsonSerializer.Deserialize<List<string>>(instance.ExploreLogJson) ?? []; }
                catch { log = []; }
            }

            // 防止重复日志：如果最后一条相同则跳过
            if (log.Count > 0 && log[^1] == logEntry) return;
            log.Add(logEntry);

            // 保留最近 100 条
            if (log.Count > 100)
            {
                log = log.Skip(log.Count - 100).ToList();
            }

            instance.ExploreLogJson = JsonSerializer.Serialize(log);
            await _instanceRepository.UpdateAsync(instance);
        }

        private async Task TryAutoMedicineAsync(DungeonInstanceEntity instance)
        {
            if (!GameData.DungeonInstanceTemplates.TryGetValue(instance.DungeonId, out var template))
            {
                return;
            }

            var config = ParseAutoMedicineConfig(template.AutoMedicineConfigJson);
            if (config == null || !config.Enabled)
            {
                return;
            }

            var hpPercent = (double)instance.CurrentHp / instance.MaxHp * 100;
            var mpPercent = (double)instance.CurrentMp / instance.MaxMp * 100;
            var needHp = hpPercent <= config.HpThresholdPercent;
            var needMp = mpPercent <= config.MpThresholdPercent;

            if (!needHp && !needMp)
            {
                return;
            }

            // 新格式：pills 列表
            if (config.Pills.Count > 0)
            {
                foreach (var pill in config.Pills)
                {
                    if (string.IsNullOrEmpty(pill.ItemId)) continue;

                    // 检查是否需要这种药
                    if (pill.HealAmount > 0 && !needHp && pill.HealMpAmount <= 0) continue;
                    if (pill.HealMpAmount > 0 && !needMp && pill.HealAmount <= 0) continue;

                    var itemCount = await _inventoryService.GetItemCountAsync(instance.PlayerId, pill.ItemId);
                    if (itemCount <= 0) continue;

                    if (await _inventoryService.DeductItemAsync(instance.PlayerId, pill.ItemId, 1, "秘境自动用药"))
                    {
                        var logParts = new List<string>();
                        if (pill.HealAmount > 0 && needHp)
                        {
                            var healHp = Math.Min(instance.MaxHp - instance.CurrentHp, pill.HealAmount);
                            instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + healHp);
                            logParts.Add($"恢复HP {healHp}");
                        }
                        if (pill.HealMpAmount > 0 && needMp)
                        {
                            var healMp = Math.Min(instance.MaxMp - instance.CurrentMp, pill.HealMpAmount);
                            instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + healMp);
                            logParts.Add($"恢复MP {healMp}");
                        }
                        if (logParts.Count > 0)
                        {
                            await AppendExploreLogAsync(instance, $"[{DateTime.Now:HH:mm:ss}] 使用了药品，{string.Join("，", logParts)}。");
                        }
                        break; // 每次 tick 只用一个药
                    }
                }
            }
            // 兼容旧格式
            else if (!string.IsNullOrEmpty(config.ItemId) && needHp)
            {
                var hpMissing = instance.MaxHp - instance.CurrentHp;
                var itemCount = await _inventoryService.GetItemCountAsync(instance.PlayerId, config.ItemId);
                if (itemCount <= 0) return;

                var healAmount = Math.Min(hpMissing, config.HealAmount);
                if (await _inventoryService.DeductItemAsync(instance.PlayerId, config.ItemId, 1, "秘境自动用药"))
                {
                    instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + healAmount);
                    await AppendExploreLogAsync(instance, $"[{DateTime.Now:HH:mm:ss}] 使用了药品，恢复 {healAmount} 生命。");
                }
            }
        }

        private static MonsterEntity CreateMonsterFromTemplate(MonsterTemplate template)
        {
            return new MonsterEntity
            {
                MonsterTempID = template.GID,
                GID = Guid.NewGuid().ToString("N"),
                Name = template.Name,
                Level = template.Level,
                SkillIds = template.SkillIds.ToList(),
                PassiveIds = template.PassiveIds.ToList(),
                ItemDrops = template.ItemDrops.ToList(),
                EquipmentDrops = template.EquipmentDrops.ToList(),
                ExpReward = template.ExpReward,
                GoldReward = template.GoldReward,
                Type1 = RandomRange(template.MinType1, template.MaxType1),
                Type2 = RandomRange(template.MinType2, template.MaxType2),
                Type3 = RandomRange(template.MinType3, template.MaxType3),
                Type4 = RandomRange(template.MinType4, template.MaxType4),
                Type5 = RandomRange(template.MinType5, template.MaxType5),
                Type6 = RandomRange(template.MinType6, template.MaxType6),
                Type7 = RandomRange(template.MinType7, template.MaxType7)
            };
        }

        private static int RandomRange(int? min, int? max)
        {
            var lo = min ?? 0;
            var hi = max ?? lo;
            return lo >= hi ? lo : Random.Shared.Next(lo, hi + 1);
        }

        private static bool IsInOpenSchedule(DungeonInstanceTemplateEntity template)
        {
            if (string.IsNullOrEmpty(template.OpenScheduleJson))
            {
                return true; // 无配置 = 全天开放
            }

            try
            {
                var schedule = JsonSerializer.Deserialize<OpenScheduleConfig>(template.OpenScheduleJson);
                if (schedule == null || schedule.Schedules == null || schedule.Schedules.Count == 0)
                {
                    return true;
                }

                var now = DateTime.Now;
                var currentDay = (int)now.DayOfWeek;
                var currentMinutes = now.Hour * 60 + now.Minute;

                foreach (var s in schedule.Schedules)
                {
                    if (s.Days != null && s.Days.Count > 0 && !s.Days.Contains(currentDay))
                    {
                        continue;
                    }

                    var startMinutes = s.StartHour * 60 + s.StartMinute;
                    var endMinutes = s.EndHour * 60 + s.EndMinute;

                    if (currentMinutes >= startMinutes && currentMinutes <= endMinutes)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return true; // 解析失败默认开放
            }
        }

        private int GetTickInterval(string dungeonId)
        {
            if (GameData.DungeonInstanceTemplates.TryGetValue(dungeonId, out var template))
            {
                return Math.Max(5, template.TickIntervalSeconds);
            }
            return 30;
        }

        private static List<EventTypeWeight> ParseEventTypeWeights(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<EventTypeWeight>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static List<EntryCost> ParseEntryCosts(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<EntryCost>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static JsonElement ParseEventData(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            catch
            {
                return default;
            }
        }

        private static int ParseRangeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (value.Contains('-'))
            {
                var parts = value.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var max))
                {
                    return min >= max ? min : Random.Shared.Next(min, max + 1);
                }
            }

            return int.TryParse(value, out var result) ? result : 0;
        }

        private static string GetJsonStringValue(JsonElement element, string defaultValue = "0")
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? defaultValue,
                JsonValueKind.Number => element.GetRawText(),
                _ => defaultValue
            };
        }

        private EncounterConfig ParseEncounterConfig(string dungeonId)
        {
            if (GameData.DungeonInstanceTemplates.TryGetValue(dungeonId, out var template) &&
                !string.IsNullOrEmpty(template.EncounterConfigJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<EncounterConfig>(template.EncounterConfigJson) ?? new EncounterConfig();
                }
                catch { /* ignore */ }
            }
            return new EncounterConfig();
        }

        private AutoMedicineConfig? ParseAutoMedicineConfig(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var config = new AutoMedicineConfig { Enabled = true };

                // 兼容新旧字段名：hpThresholdPercent / hpThreshold
                if (root.TryGetProperty("hpThresholdPercent", out var ht))
                    config.HpThresholdPercent = ht.GetInt32();
                else if (root.TryGetProperty("hpThreshold", out var htOld))
                    config.HpThresholdPercent = htOld.GetInt32();

                // 兼容：mpThresholdPercent / mpThreshold
                if (root.TryGetProperty("mpThresholdPercent", out var mt))
                    config.MpThresholdPercent = mt.GetInt32();
                else if (root.TryGetProperty("mpThreshold", out var mtOld))
                    config.MpThresholdPercent = mtOld.GetInt32();

                // 新格式：pills 列表
                if (root.TryGetProperty("pills", out var pillsEl) && pillsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var pillEl in pillsEl.EnumerateArray())
                    {
                        var pill = new AutoMedicinePill();
                        if (pillEl.TryGetProperty("itemId", out var iid)) pill.ItemId = iid.GetString() ?? "";
                        if (pillEl.TryGetProperty("healAmount", out var ha)) pill.HealAmount = ha.GetInt32();
                        if (pillEl.TryGetProperty("healMpAmount", out var hma)) pill.HealMpAmount = hma.GetInt32();
                        config.Pills.Add(pill);
                    }
                }

                // 兼容旧格式：pillTypes + itemId + healAmount
                if (config.Pills.Count == 0)
                {
                    if (root.TryGetProperty("itemId", out var itemIdEl))
                        config.ItemId = itemIdEl.GetString() ?? "";
                    if (root.TryGetProperty("healAmount", out var healEl))
                        config.HealAmount = healEl.GetInt32();
                }

                return config;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Buff 读写

        private static List<ActiveBuff> LoadActiveBuffs(DungeonInstanceEntity instance)
        {
            if (string.IsNullOrEmpty(instance.ActiveBuffsJson))
                return [];
            try
            {
                return JsonSerializer.Deserialize<List<ActiveBuff>>(instance.ActiveBuffsJson) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static void SaveActiveBuffs(DungeonInstanceEntity instance, List<ActiveBuff> buffs)
        {
            instance.ActiveBuffsJson = buffs.Count > 0 ? JsonSerializer.Serialize(buffs) : null;
        }

        /// <summary>
        /// 从快照中读取玩家战斗属性，叠加 active buffs 修正后返回。
        /// </summary>
        private static DungeonPlayerSnapshot LoadSnapshot(DungeonInstanceEntity instance)
        {
            if (string.IsNullOrEmpty(instance.SnapshotJson))
                return new DungeonPlayerSnapshot();
            try
            {
                return JsonSerializer.Deserialize<DungeonPlayerSnapshot>(instance.SnapshotJson) ?? new DungeonPlayerSnapshot();
            }
            catch
            {
                return new DungeonPlayerSnapshot();
            }
        }

        /// <summary>
        /// 将快照属性 + buff 修正写入 user 的 Type 字段（战斗前调用）。
        /// 返回原始值以便战斗后恢复。
        /// </summary>
        private static (int[] Stats, List<string> OriginalSkills, List<string> OriginalPassives) ApplySnapshotAndBuffs(UserEntity user, DungeonPlayerSnapshot snapshot, List<ActiveBuff> buffs, DungeonInstanceEntity instance)
        {
            var original = new[] { user.Type1, user.Type2, user.Type3, user.Type4, user.Type5, user.Type6, user.Type7 };
            var originalSkills = user.SkillIds.ToList();
            var originalPassives = user.PassiveIds.ToList();

            // 使用快照中的技能
            if (snapshot.SkillIds.Count > 0)
                user.SkillIds = snapshot.SkillIds.ToList();
            if (snapshot.PassiveIds.Count > 0)
                user.PassiveIds = snapshot.PassiveIds.ToList();

            // 计算 buff 修正（分别计算物攻/法攻/物防/法防）
            int physAtkBonus = 0, magAtkBonus = 0, physDefBonus = 0, magDefBonus = 0, spdBonus = 0;
            float hitBonus = 0, dodgeBonus = 0, critBonus = 0, comboBonus = 0, counterBonus = 0;

            foreach (var buff in buffs)
            {
                if (buff.RemainingDuration <= 0) continue;

                var v = buff.Value;
                var isPct = buff.IsPercent;

                switch (buff.BuffType)
                {
                    case "AttackUp":
                        physAtkBonus += isPct ? (int)(snapshot.PhysicalAttack * v / 100.0) : v;
                        magAtkBonus += isPct ? (int)(snapshot.MagicAttack * v / 100.0) : v;
                        break;
                    case "DefenseUp":
                        physDefBonus += isPct ? (int)(snapshot.PhysicalDefense * v / 100.0) : v;
                        magDefBonus += isPct ? (int)(snapshot.MagicDefense * v / 100.0) : v;
                        break;
                    case "SpeedUp": spdBonus += isPct ? (int)(snapshot.Speed * v / 100.0) : v; break;
                    case "CritRateUp": critBonus += isPct ? v / 100f : v; break;
                    case "HitRateUp": hitBonus += isPct ? v / 100f : v; break;
                    case "DodgeRateUp": dodgeBonus += isPct ? v / 100f : v; break;
                    case "ComboRateUp": comboBonus += isPct ? v / 100f : v; break;
                    case "CounterRateUp": counterBonus += isPct ? v / 100f : v; break;
                    case "AllStatsUp":
                        if (isPct)
                        {
                            physAtkBonus += (int)(snapshot.PhysicalAttack * v / 100.0);
                            magAtkBonus += (int)(snapshot.MagicAttack * v / 100.0);
                            physDefBonus += (int)(snapshot.PhysicalDefense * v / 100.0);
                            magDefBonus += (int)(snapshot.MagicDefense * v / 100.0);
                            spdBonus += (int)(snapshot.Speed * v / 100.0);
                        }
                        else
                        {
                            physAtkBonus += v; magAtkBonus += v;
                            physDefBonus += v; magDefBonus += v;
                            spdBonus += v;
                        }
                        break;
                    case "AttackDown":
                        physAtkBonus -= isPct ? (int)(snapshot.PhysicalAttack * v / 100.0) : v;
                        magAtkBonus -= isPct ? (int)(snapshot.MagicAttack * v / 100.0) : v;
                        break;
                    case "DefenseDown":
                        physDefBonus -= isPct ? (int)(snapshot.PhysicalDefense * v / 100.0) : v;
                        magDefBonus -= isPct ? (int)(snapshot.MagicDefense * v / 100.0) : v;
                        break;
                    case "SpeedDown": spdBonus -= isPct ? (int)(snapshot.Speed * v / 100.0) : v; break;
                    case "CritRateDown": critBonus -= isPct ? v / 100f : v; break;
                    case "AllStatsDown":
                        if (isPct)
                        {
                            physAtkBonus -= (int)(snapshot.PhysicalAttack * v / 100.0);
                            magAtkBonus -= (int)(snapshot.MagicAttack * v / 100.0);
                            physDefBonus -= (int)(snapshot.PhysicalDefense * v / 100.0);
                            magDefBonus -= (int)(snapshot.MagicDefense * v / 100.0);
                            spdBonus -= (int)(snapshot.Speed * v / 100.0);
                        }
                        else
                        {
                            physAtkBonus -= v; magAtkBonus -= v;
                            physDefBonus -= v; magDefBonus -= v;
                            spdBonus -= v;
                        }
                        break;
                }
            }

            user.Type1 = (int)instance.CurrentHp;  // 保持 HP 快照
            user.Type2 = (int)instance.CurrentMp;  // 保持 MP 快照
            user.Type3 = Math.Max(1, snapshot.PhysicalAttack + physAtkBonus);
            user.Type4 = Math.Max(1, snapshot.MagicAttack + magAtkBonus);
            user.Type5 = Math.Max(0, snapshot.PhysicalDefense + physDefBonus);
            user.Type6 = Math.Max(0, snapshot.MagicDefense + magDefBonus);
            user.Type7 = Math.Max(1, snapshot.Speed + spdBonus);
            user.Type8 = Math.Clamp(snapshot.HitRate + hitBonus, 0, 1);
            user.Type9 = Math.Clamp(snapshot.DodgeRate + dodgeBonus, 0, 1);
            user.Type10 = Math.Clamp(snapshot.CritRate + critBonus, 0, 1);
            user.Type12 = Math.Clamp(snapshot.ComboRate + comboBonus, 0, 1);
            user.Type13 = Math.Clamp(snapshot.CounterRate + counterBonus, 0, 1);

            return (original, originalSkills, originalPassives);
        }

        /// <summary>
        /// 战斗后恢复 user 的真实属性和技能。
        /// </summary>
        private static void RestoreOriginalStats(UserEntity user, (int[] Stats, List<string> OriginalSkills, List<string> OriginalPassives) original)
        {
            user.Type1 = original.Stats[0];
            user.Type2 = original.Stats[1];
            user.Type3 = original.Stats[2];
            user.Type4 = original.Stats[3];
            user.Type5 = original.Stats[4];
            user.Type6 = original.Stats[5];
            user.Type7 = original.Stats[6];
            user.SkillIds = original.OriginalSkills;
            user.PassiveIds = original.OriginalPassives;
        }

        /// <summary>
        /// 处理 tick 时的 buff 效果（DOT/HOT）和持续时间递减。
        /// </summary>
        private async Task ProcessActiveBuffsOnTickAsync(DungeonInstanceEntity instance, string logPrefix)
        {
            var buffs = LoadActiveBuffs(instance);
            if (buffs.Count == 0) return;

            var toRemove = new List<int>();
            var changed = false;

            for (var i = 0; i < buffs.Count; i++)
            {
                var buff = buffs[i];

                // DOT/HOT 每 tick 生效（仅限 tick 类型的 buff，battle 类型在战斗中生效）
                if (buff.DurationType == "tick")
                {
                    if (buff.BuffType == "DamageOverTime" || buff.BuffType == "DamagePercent")
                    {
                        var dmg = buff.IsPercent ? (int)(instance.MaxHp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentHp = Math.Max(1, instance.CurrentHp - dmg);
                        await AppendExploreLogAsync(instance, $"{logPrefix}{ResolveEventSourceName(buff.Source)} 效果生效：受到 {dmg} 点持续伤害。");
                        changed = true;
                    }
                    else if (buff.BuffType == "HealOverTime")
                    {
                        var heal = buff.IsPercent ? (int)(instance.MaxHp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + heal);
                        await AppendExploreLogAsync(instance, $"{logPrefix}{ResolveEventSourceName(buff.Source)} 效果生效：恢复 {heal} 点生命。");
                        changed = true;
                    }
                    else if (buff.BuffType == "ManaRegen")
                    {
                        var heal = buff.IsPercent ? (int)(instance.MaxMp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + heal);
                        changed = true;
                    }
                    else if (buff.BuffType == "Special_RegenTick")
                    {
                        // 值编码：hpPercent * 1000 + mpPercent
                        var hpPercent = buff.Value / 1000;
                        var mpPercent = buff.Value % 1000;
                        if (hpPercent > 0)
                        {
                            var hpHeal = (int)(instance.MaxHp * hpPercent / 100.0);
                            instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + hpHeal);
                        }
                        if (mpPercent > 0)
                        {
                            var mpHeal = (int)(instance.MaxMp * mpPercent / 100.0);
                            instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + mpHeal);
                        }
                        changed = true;
                    }
                }

                // 递减 tick 类型的持续时间
                if (buff.DurationType == "tick" && buff.RemainingDuration > 0)
                {
                    buffs[i] = buff with { RemainingDuration = buff.RemainingDuration - 1 };
                    changed = true;
                    if (buffs[i].RemainingDuration <= 0)
                    {
                        toRemove.Add(i);
                        // 过滤内部机制 buff，不显示给玩家
                        if (!buff.BuffType.StartsWith("Special_"))
                        {
                            await AppendExploreLogAsync(instance, $"{logPrefix}{GetBuffDisplayName(buff.BuffType)} 效果已过期。");
                        }
                    }
                }
            }

            // 移除过期 buff
            for (var i = toRemove.Count - 1; i >= 0; i--)
                buffs.RemoveAt(toRemove[i]);

            if (changed)
            {
                SaveActiveBuffs(instance, buffs);
                await _instanceRepository.UpdateAsync(instance);
            }
        }

        /// <summary>
        /// 战斗结束后递减 battle 类型 buff 的持续时间。
        /// </summary>
        private async Task ProcessActiveBuffsOnBattleAsync(DungeonInstanceEntity instance)
        {
            var buffs = LoadActiveBuffs(instance);
            if (buffs.Count == 0) return;

            var toRemove = new List<int>();
            var changed = false;

            for (var i = 0; i < buffs.Count; i++)
            {
                var buff = buffs[i];
                if (buff.DurationType == "battle" && buff.RemainingDuration > 0)
                {
                    // 战斗类型 buff 效果生效
                    if (buff.BuffType == "HealOverTime")
                    {
                        var heal = buff.IsPercent ? (int)(instance.MaxHp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentHp = Math.Min(instance.MaxHp, instance.CurrentHp + heal);
                        changed = true;
                    }
                    else if (buff.BuffType == "ManaRegen")
                    {
                        var heal = buff.IsPercent ? (int)(instance.MaxMp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentMp = Math.Min(instance.MaxMp, instance.CurrentMp + heal);
                        changed = true;
                    }
                    else if (buff.BuffType == "DamageOverTime" || buff.BuffType == "DamagePercent")
                    {
                        var dmg = buff.IsPercent ? (int)(instance.MaxHp * buff.Value / 100.0) : buff.Value;
                        instance.CurrentHp = Math.Max(1, instance.CurrentHp - dmg);
                        changed = true;
                    }

                    buffs[i] = buff with { RemainingDuration = buff.RemainingDuration - 1 };
                    changed = true;
                    if (buffs[i].RemainingDuration <= 0)
                    {
                        toRemove.Add(i);
                        if (!buff.BuffType.StartsWith("Special_"))
                        {
                            await AppendExploreLogAsync(instance, $"[战斗] {GetBuffDisplayName(buff.BuffType)} 效果已过期。");
                        }
                    }
                }
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
                buffs.RemoveAt(toRemove[i]);

            if (changed)
            {
                SaveActiveBuffs(instance, buffs);
                await _instanceRepository.UpdateAsync(instance);
            }
        }

        #endregion

        #region 内部模型

        private class DungeonPlayerSnapshot
        {
            public int Level { get; set; }
            public long Exp { get; set; }
            public long Gold { get; set; }
            public long SpiritStone { get; set; }
            public string Profession { get; set; } = "";
            // 战斗属性快照
            public int MaxHp { get; set; }
            public int MaxMp { get; set; }
            public int PhysicalAttack { get; set; }
            public int MagicAttack { get; set; }
            public int PhysicalDefense { get; set; }
            public int MagicDefense { get; set; }
            public int Speed { get; set; }
            public float HitRate { get; set; }
            public float DodgeRate { get; set; }
            public float CritRate { get; set; }
            public float CritDamage { get; set; } = 1.5f;
            public float ComboRate { get; set; }
            public float CounterRate { get; set; }
            public float ArmorBreakRate { get; set; }
            public float ExtraDamage { get; set; }
            // 技能快照
            public List<string> SkillIds { get; set; } = [];
            public List<string> PassiveIds { get; set; } = [];
        }

        private record ActiveBuff(string BuffType, int Value, bool IsPercent, int RemainingDuration, string DurationType, string Source);

        private static string GetBuffDisplayName(string buffType)
        {
            return buffType switch
            {
                "AttackUp" => "攻击提升",
                "DefenseUp" => "防御提升",
                "SpeedUp" => "速度提升",
                "CritRateUp" => "暴击率提升",
                "HitRateUp" => "命中率提升",
                "ComboRateUp" => "连击率提升",
                "CounterRateUp" => "反击率提升",
                "DodgeRateUp" => "闪避率提升",
                "AllStatsUp" => "全属性提升",
                "AttackDown" => "攻击降低",
                "DefenseDown" => "防御降低",
                "SpeedDown" => "速度降低",
                "CritRateDown" => "暴击率降低",
                "HitRateDown" => "命中率降低",
                "DodgeRateDown" => "闪避率降低",
                "AllStatsDown" => "全属性降低",
                "DamageOverTime" => "持续伤害",
                "DamagePercent" => "持续伤害",
                "HealOverTime" => "持续回复",
                "ManaRegen" => "灵力回复",
                "HealBlock" => "禁疗",
                "Shield" => "灵力护盾",
                "Silence" => "沉默",
                "Stun" => "眩晕",
                "Undying" => "不屈",
                "WeightModifier" => "负重改变",
                "ComboRateDown" => "连击率降低",
                "CounterRateDown" => "反击率降低",
                "Lifesteal" => "吸血光环",
                "ArmorBreak" => "破甲",
                "ExtraDamage" => "额外伤害",
                _ => buffType
            };
        }

        private static string GetResourceDisplayName(string resourceType)
        {
            return resourceType switch
            {
                "Gold" => "金币",
                "Exp" => "经验",
                "SpiritStone" => "灵石",
                "Item" => "道具",
                "Equipment" => "装备",
                _ => resourceType
            };
        }

        private static string ResolveEventSourceName(string source)
        {
            if (GameData.DungeonEventConfigsById.TryGetValue(source, out var config))
                return config.Name;
            return source;
        }

        private static string GetItemDisplayName(string itemId)
        {
            if (GameData.Items.TryGetValue(itemId, out var item))
                return item.Name;
            return itemId;
        }

        private class EventTypeWeight
        {
            public int EventType { get; set; }
            public int Weight { get; set; }
        }

        private class EntryCost
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "";

            [JsonPropertyName("id")]
            public string? ItemId { get; set; }

            [JsonPropertyName("quantity")]
            public int Amount { get; set; }
        }

        private class OpenScheduleConfig
        {
            public List<ScheduleEntry>? Schedules { get; set; }
        }

        private class ScheduleEntry
        {
            public List<int>? Days { get; set; }
            public int StartHour { get; set; }
            public int StartMinute { get; set; }
            public int EndHour { get; set; }
            public int EndMinute { get; set; }
        }

        private class EncounterConfig
        {
            public double FavorabilityTeamThreshold { get; set; } = 0.5;
            public PvpPlunderConfig? PvpPlunderConfig { get; set; }
        }

        private class PvpPlunderConfig
        {
            public int CurrencyPlunderRate { get; set; } = 30;
            public int EquipmentPlunderCount { get; set; } = 1;
            public int ItemPlunderCount { get; set; } = 1;
        }

        private class AutoMedicineConfig
        {
            public bool Enabled { get; set; }
            public string ItemId { get; set; } = "";
            public int HealAmount { get; set; }
            public int HpThresholdPercent { get; set; } = 50;
            public int MpThresholdPercent { get; set; } = 30;
            public List<AutoMedicinePill> Pills { get; set; } = [];
        }

        private class AutoMedicinePill
        {
            public string ItemId { get; set; } = "";
            public int HealAmount { get; set; }
            public int HealMpAmount { get; set; }
        }

        #endregion
    }
}
#pragma warning restore CS1591
