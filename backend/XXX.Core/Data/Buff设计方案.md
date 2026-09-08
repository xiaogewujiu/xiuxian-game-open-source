Buff系统设计方案

## 背景与目标

Buff系统是战斗系统的核心组成部分，用于实现状态效果、增益减益、持续伤害/治疗等战斗机制。本方案基于现有代码结构，设计完整的Buff分类体系、叠加规则、触发机制和实现方案。

## 一、Buff系统概览

### 1.1 核心设计原则

| 原则 | 说明 |
|------|------|
| 单一职责 | 每个Buff只负责一种效果类型 |
| 可叠加性 | 支持同类型Buff的叠加、刷新或替换 |
| 可扩展性 | 通过BuffEffectType枚举方便添加新效果 |
| 数据驱动 | Buff效果由配置数据决定，无需修改代码 |

### 1.2 Buff数据结构

```csharp
public class BuffTemplate
{
    public string GID { get; set; }           // Buff唯一标识
    public string Name { get; set; }          // Buff名称
    public int Duration { get; set; }         // 持续回合数
    public int MaxStack { get; set; }         // 最大叠加层数
    public StackRule StackRule { get; set; }  // 叠加规则
    public List<BuffEffect> Effects { get; set; }  // 效果列表
}

public class BuffEffect
{
    public BuffEffectType EffectType { get; set; }  // 效果类型
    public bool IsPercentage { get; set; }          // 是否百分比
    public float Value { get; set; }                // 效果数值
    public int TriggerChance { get; set; }          // 触发概率
}
```

## 二、Buff分类体系

### 2.1 按效果分类（46种BuffEffectType）

#### 持续伤害/治疗类（4种）

| 编号 | 效果类型 | 说明 | 触发时机 |
|------|----------|------|----------|
| 1 | DamageOverTime | 持续伤害 | 每回合开始时 |
| 2 | HealOverTime | 持续治疗 | 每回合开始时 |
| 17 | ManaOverTime | 持续回蓝 | 每回合开始时 |
| 24 | ManaDrain | 持续耗蓝 | 每回合开始时 |

#### 控制类（6种）

| 编号 | 效果类型 | 说明 | 免疫方式 |
|------|----------|------|----------|
| 3 | Stun | 眩晕（无法行动） | Immunity |
| 8 | Silence | 沉默（无法使用技能） | Immunity |
| 42 | Disarm | 缴械（无法普攻） | Immunity |
| 43 | Root | 束缚（无法反击） | - |
| 16 | Taunt | 嘲讽（强制攻击目标） | Immunity |
| 37 | Undying | 不屈（致死保留1血） | - |

#### 属性增益类（12种）

| 编号 | 效果类型 | 目标属性 | 常见数值 |
|------|----------|----------|----------|
| 5 | DefenseUp | 防御提升 | 20-50% |
| 6 | AttackUp | 攻击提升 | 15-50% |
| 9 | CritRateUp | 暴击率提升 | 10-25% |
| 11 | CritDamageUp | 暴击伤害提升 | 10-30% |
| 14 | DamageAmplify | 伤害增幅 | 15-30% |
| 18 | AccuracyUp | 命中率提升 | 20-30% |
| 25 | ComboRateUp | 连击率提升 | 15-20% |
| 27 | ComboDamageUp | 连击伤害提升 | 20-30% |
| 29 | CounterRateUp | 反击率提升 | 15-25% |
| 31 | CounterDamageUp | 反击伤害提升 | 20-30% |
| 35 | Haste | 加速（速度提升） | 20-40% |
| 38 | CooldownReduction | 冷却缩减 | +1点/回合 |

#### 属性减益类（10种）

| 编号 | 效果类型 | 目标属性 | 常见数值 |
|------|----------|----------|----------|
| 4 | DefenseDown | 防御降低 | 20-30% |
| 7 | AttackDown | 攻击降低 | 15-25% |
| 10 | CritRateDown | 暴击率降低 | 10-20% |
| 12 | CritDamageDown | 暴击伤害降低 | 10-20% |
| 13 | CritResist | 暴击抵抗 | 15-20% |
| 19 | AccuracyDown | 命中率降低 | 20-50% |
| 26 | ComboRateDown | 连击率降低 | 10-15% |
| 28 | ComboDamageDown | 连击伤害降低 | 15-20% |
| 30 | CounterRateDown | 反击率降低 | 10-15% |
| 32 | CounterDamageDown | 反击伤害降低 | 15-20% |
| 34 | Slow | 减速（速度降低） | 20-30% |
| 39 | CooldownIncrease | 冷却增加 | -0.5点/回合 |

#### 特殊效果类（10种）

| 编号 | 效果类型 | 说明 | 特殊机制 |
|------|----------|------|----------|
| 20 | Lifesteal | 吸血 | 造成伤害的20-30%转化为生命 |
| 21 | ReflectDamage | 反伤 | 反弹受到伤害的20-30% |
| 22 | Shield | 护盾 | 吸收固定数值伤害 |
| 23 | Immunity | 免疫 | 免疫所有控制效果 |
| 33 | HealBlock | 禁疗 | 无法恢复生命值 |
| 36 | Vulnerability | 易伤 | 受到伤害增加20-30% |
| 40 | Execute | 斩杀 | 对低血量目标额外伤害 |
| 41 | Mirror | 镜像/分身 | 创建分身参与战斗 |
| 44 | ArmorBreak | 破防 | 直接减少防御值（固定值） |
| 45 | Sanctuary | 庇护 | 为友方分担30%伤害 |
| 46 | AutoRevive | 自动复活 | 死亡时复活并恢复生命 |

### 2.2 按持续时间分类

| 类型 | 说明 | 示例 |
|------|------|------|
| 即时效果 | 立即生效，无持续回合 | 治疗、伤害 |
| 短持续 | 1-2回合 | 眩晕、冰冻、无敌 |
| 中持续 | 3-4回合 | 大部分增益/减益 |
| 长持续 | 5回合以上 | 某些特殊Buff |
| 永久持续 | 直到战斗结束或被驱散 | 不屈、自动复活 |

### 2.3 按目标分类

| 目标类型 | 说明 | 标识 |
|----------|------|------|
| 自己 | 仅对自己生效 | Self(0) |
| 敌方单体 | 对单个敌人生效 | Enemy(1) |
| 敌方全体 | 对所有敌人生效 | AllEnemies |
| 友方单体 | 对单个友方生效 | Ally(2) |
| 友方全体 | 对所有友方生效 | AllAllies |
| 随机敌方 | 随机选择敌人 | RandomEnemy(4) |

## 三、Buff叠加规则设计

### 3.1 叠加规则类型

```csharp
public enum StackRule
{
    None = 0,       // 不可叠加，新Buff无法施加
    Refresh = 1,    // 刷新持续时间，不叠加数值
    Stack = 2,      // 叠加层数，数值叠加
    Replace = 3,    // 替换旧Buff，保留新Buff
    Independent = 4 // 独立存在，互不影响
}
```

### 3.2 各效果类型叠加规则

| 效果类型 | 叠加规则 | 最大层数 | 说明 |
|----------|----------|----------|------|
| 持续伤害(DOT) | Stack | 3-5层 | 多层DOT可同时生效 |
| 持续治疗(HOT) | Refresh | 1层 | 新治疗刷新持续时间 |
| 眩晕 | Replace | 1层 | 新眩晕替换旧眩晕 |
| 防御提升 | Refresh | 1层 | 刷新持续时间 |
| 攻击提升 | Stack | 2-3层 | 多层攻击加成可叠加 |
| 护盾 | Replace | 1层 | 新护盾替换旧护盾 |
| 中毒 | Stack | 3层 | 每层独立计算伤害 |
| 流血 | Stack | 5层 | 可叠加多层流血 |
| 减速 | Stack | 2层 | 最多减速60% |
| 加速 | Stack | 2层 | 最多加速60% |

### 3.3 叠加数值计算公式

**叠加型（Stack）**
```
总效果 = 基础值 × 层数
或
总效果 = 基础值 × (1 + (层数-1) × 衰减系数)

衰减系数建议：0.5-0.8（防止过度叠加）
```

**刷新型（Refresh）**
```
持续时间 = 新Buff的完整持续时间
效果值 = 取最大值（旧效果 vs 新效果）
```

**破防效果（ArmorBreak）- 固定值叠加**
```
总破防值 = 100点 × 层数（最多3层 = 300点）
```

## 四、现有Buff模板数据整理

### 4.1 基础状态效果（12种）

| BuffID | 名称 | 效果类型 | 数值 | 持续 | 叠加规则 | 目标 |
|--------|------|----------|------|------|----------|------|
| buff_001 | 中毒 | DamageOverTime | 50点/回合 | 3 | Stack(3) | 敌方 |
| buff_002 | 灼烧 | DamageOverTime | 5%最大生命/回合 | 2 | Refresh | 敌方 |
| buff_003 | 回春 | HealOverTime | 80点/回合 | 3 | Refresh | 友方 |
| buff_004 | 生命涌动 | HealOverTime | 8%最大生命/回合 | 3 | Refresh | 友方 |
| buff_005 | 眩晕 | Stun | - | 1 | Replace | 敌方 |
| buff_015 | 冰冻 | Stun | - | 1 | Replace | 敌方 |
| buff_006 | 破甲 | DefenseDown | 30% | 3 | Stack(2) | 敌方 |
| buff_016 | 流血 | DamageOverTime | 40点/回合 | 4 | Stack(5) | 敌方 |
| buff_010 | 沉默 | Silence | - | 2 | Replace | 敌方 |
| buff_034 | 减速 | Slow | 30% | 3 | Stack(2) | 敌方 |
| buff_033 | 嘲讽 | Taunt | - | 2 | Replace | 敌方 |
| buff_023 | 致盲 | AccuracyDown | 50% | 2 | Refresh | 敌方 |

### 4.2 增益效果（15种）

| BuffID | 名称 | 效果类型 | 数值 | 持续 | 叠加规则 | 目标 |
|--------|------|----------|------|------|----------|------|
| buff_007 | 铁壁 | DefenseUp | 50点 | 3 | Refresh | 友方 |
| buff_017 | 魔法护盾 | DefenseUp | 35% | 3 | Refresh | 友方 |
| buff_008 | 狂暴 | AttackUp | 25% | 3 | Refresh | 友方 |
| buff_022 | 狂怒 | AttackUp | 50% | 2 | Refresh | 自己 |
| buff_012 | 神圣护盾 | DefenseUp+HealOverTime | 30点+60点/回合 | 3 | Refresh | 友方 |
| buff_019 | 嗜血 | AttackUp+Lifesteal | 20%+25% | 3 | Refresh | 友方 |
| buff_024 | 专注 | AccuracyUp | 30% | 3 | Refresh | 友方 |
| buff_025 | 暴击强化 | CritRateUp | 20% | 3 | Refresh | 友方 |
| buff_035 | 连击强化 | ComboRateUp | 20% | 3 | Refresh | 友方 |
| buff_037 | 反击强化 | CounterRateUp | 20% | 3 | Refresh | 友方 |
| buff_035_haste | 加速 | Haste | 30% | 3 | Stack(2) | 友方 |
| buff_043_wind_step | 疾风步 | Haste+CooldownReduction | 40%+1点/回合 | 3 | Refresh | 友方 |
| buff_027 | 全能强化 | AttackUp+DefenseUp | 15%+15% | 3 | Refresh | 友方 |
| buff_031 | 伤害增幅 | DamageAmplify | 25% | 3 | Refresh | 友方 |
| buff_032 | 伤害减免 | DamageReduction | 20% | 3 | Refresh | 友方 |
| buff_026 | 免疫 | Immunity | - | 2 | Refresh | 友方 |
| buff_021 | 护盾 | Shield | 300点 | 3 | Replace | 友方 |
| buff_020 | 荆棘之甲 | ReflectDamage | 30% | 3 | Refresh | 友方 |
| buff_029 | 魔力涌动 | ManaOverTime | 50点/回合 | 3 | Refresh | 友方 |

### 4.3 减益效果（8种）

| BuffID | 名称 | 效果类型 | 数值 | 持续 | 叠加规则 | 目标 |
|--------|------|----------|------|------|----------|------|
| buff_009 | 虚弱 | AttackDown | 20% | 2 | Refresh | 敌方 |
| buff_011 | 剧毒 | AttackDown+DamageOverTime | 15%+30点/回合 | 3 | Refresh | 敌方 |
| buff_018 | 魔法易伤 | DefenseDown | 30% | 3 | Stack(2) | 敌方 |
| buff_028 | 诅咒 | AttackDown+DefenseDown | 20%+20% | 3 | Refresh | 敌方 |
| buff_030 | 魔力枯竭 | ManaDrain | 30点/回合 | 3 | Refresh | 敌方 |
| buff_033_heal_block | 禁疗 | HealBlock | - | 2 | Refresh | 敌方 |
| buff_036_vulnerability | 易伤 | Vulnerability | 25% | 2 | Stack(2) | 敌方 |
| buff_042_tactical_weakness | 战术弱点 | Vulnerability+HealBlock | 30%+禁疗 | 2 | Refresh | 敌方 |

### 4.4 特殊效果（8种）

| BuffID | 名称 | 效果类型 | 数值 | 持续 | 叠加规则 | 目标 |
|--------|------|----------|------|------|----------|------|
| buff_037_undying | 不屈 | Undying | - | 永久 | Replace | 自己 |
| buff_040_execute | 斩杀意图 | Execute | 50%额外伤害 | 3 | Refresh | 自己 |
| buff_038_cooldown_reduction | 急速冷却 | CooldownReduction | +1点/回合 | 4 | Refresh | 自己 |
| buff_039_cooldown_increase | 冷却延迟 | CooldownIncrease | -0.5点/回合 | 2 | Refresh | 敌方 |
| buff_044_disarm | 缴械 | Disarm | - | 2 | Replace | 敌方 |
| buff_045_root | 束缚 | Root | - | 2 | Refresh | 敌方 |
| buff_046_armor_break | 破防 | ArmorBreak | 100点 | 3 | Stack(3) | 敌方 |
| buff_047_sanctuary | 庇护 | Sanctuary | 30%分担 | 3 | Refresh | 自己 |
| buff_048_auto_revive | 自动复活 | AutoRevive | 50%生命复活 | 永久 | Replace | 自己 |

## 五、Buff触发机制

### 5.1 触发时机

| 时机 | 说明 | 示例效果 |
|------|------|----------|
| 回合开始 | 每回合开始时触发 | DOT伤害、HOT治疗 |
| 回合结束 | 每回合结束时触发 | 某些特殊效果结算 |
| 受到伤害 | 受到攻击时触发 | 反伤、护盾减伤 |
| 造成伤害 | 造成攻击时触发 | 吸血、斩杀判定 |
| 技能释放 | 释放技能时触发 | 某些技能联动效果 |
| 生命值变化 | HP变化时触发 | 不屈、自动复活 |
| Buff施加 | 被施加Buff时触发 | 免疫效果 |

### 5.2 触发概率机制

```
实际触发概率 = 基础触发概率 + 效果来源的等级差加成

等级差加成 = (施法者等级 - 目标等级) × 2%
最低触发概率 = 5%
最高触发概率 = 100%
```

## 六、Buff与技能关联

### 6.1 技能Buff配置示例

| 技能ID | 技能名称 | Buff效果 | 触发概率 |
|--------|----------|----------|----------|
| 2 | 毒刃 | buff_001中毒 | 100% |
| 3 | 火球术 | buff_002灼烧 | 100% |
| 7 | 震慑 | buff_005眩晕 | 100% |
| 8 | 破甲斩 | buff_006破甲 | 100% |
| 9 | 铁壁守护 | buff_007铁壁 | 100% |
| 13 | 剧毒之刺 | buff_011剧毒 | 100% |
| 19 | 冰封 | buff_015冰冻 | 100% |
| 20 | 撕裂 | buff_016流血 | 100% |
| 61 | 寒冰箭 | buff_041减速 | 100% |
| 65 | 斩杀一击 | buff_040斩杀 | 100% |

### 6.2 Buff技能设计原则

1. **控制类技能**：高效果但持续时间短（1-2回合）
2. **DOT类技能**：中等持续伤害，持续3-4回合
3. **增益类技能**：持续时间较长（3-5回合），可配合战术使用
4. **Debuff类技能**：根据强度决定持续时间，强效果短持续

## 七、实现方案

### 7.1 需要修改的文件

1. **Entity/Buffs.cs**
   - 完善BuffTemplate数据结构
   - 添加Buff效果配置

2. **Battle/BuffProcessor.cs**
   - 实现Buff叠加规则处理
   - 实现Buff触发时机处理

3. **Battle/BattleFighter.cs**
   - 添加ActiveBuffs列表
   - 实现Buff施加/移除接口

4. **Data/BuffsData.cs**
   - 完善Buff模板数据

### 7.2 核心算法伪代码

```csharp
// Buff施加
public bool ApplyBuff(BuffTemplate buff, BattleFighter caster)
{
    var existingBuff = FindSameTypeBuff(buff);

    if (existingBuff == null)
    {
        // 无相同Buff，直接添加
        ActiveBuffs.Add(new ActiveBuff(buff, caster));
        return true;
    }

    switch (buff.StackRule)
    {
        case StackRule.None:
            return false;

        case StackRule.Refresh:
            existingBuff.Duration = buff.Duration;
            existingBuff.Value = Math.Max(existingBuff.Value, buff.Value);
            return true;

        case StackRule.Stack:
            if (existingBuff.StackCount < buff.MaxStack)
            {
                existingBuff.StackCount++;
                existingBuff.Value += buff.Value * 0.8f; // 80%衰减
            }
            existingBuff.Duration = buff.Duration;
            return true;

        case StackRule.Replace:
            RemoveBuff(existingBuff);
            ActiveBuffs.Add(new ActiveBuff(buff, caster));
            return true;
    }
}

// 回合开始处理
public void OnTurnStart()
{
    foreach (var buff in ActiveBuffs)
    {
        // 触发持续效果
        if (buff.EffectType == BuffEffectType.DamageOverTime)
        {
            TakeDamage(buff.Value);
        }
        else if (buff.EffectType == BuffEffectType.HealOverTime)
        {
            Heal(buff.Value);
        }

        // 减少持续时间
        buff.Duration--;
        if (buff.Duration <= 0)
        {
            RemoveBuff(buff);
        }
    }
}
```

## 八、平衡性设计

### 8.1 控制类Buff限制

| 控制类型 | 持续时间 | 免疫机制 | 特殊说明 |
|----------|----------|----------|----------|
| 眩晕 | 1回合 | Immunity可免疫 | 最强控制 |
| 冰冻 | 1回合 | Immunity可免疫 | 同眩晕 |
| 沉默 | 2回合 | Immunity可免疫 | 限制技能 |
| 缴械 | 2回合 | Immunity可免疫 | 限制普攻 |
| 束缚 | 2回合 | - | 仅限制反击 |
| 嘲讽 | 2回合 | Immunity可免疫 | 强制目标 |

### 8.2 DOT类Buff限制

```
最大DOT伤害 = 目标最大生命值 × 15% / 回合

例如：
- 中毒3层：每回合15%生命，3回合共45%
- 灼烧：每回合8%生命，可无限叠加但会刷新
- 流血5层：每层2%生命，5层共10%/回合
```

### 8.3 增益Buff上限

| 属性 | 最大增益 | 说明 |
|------|----------|------|
| 攻击提升 | 100% | 超过则效果递减 |
| 防御提升 | 100% | 超过则效果递减 |
| 暴击率 | 100% | 物理上限 |
| 暴击伤害 | 300% | 防止过度爆发 |
| 速度提升 | 100% | 超过则效果递减 |
| 伤害减免 | 80% | 保留最低伤害 |

## 九、待确认问题

1. **驱散机制**
   - 是否添加驱散技能？
   - 驱散优先级（后施加的先驱散？）

2. **Buff抵抗**
   - 是否需要Buff抵抗属性？
   - 抵抗概率如何计算？

3. **Buff显示**
   - 战斗界面如何展示Buff图标？
   - 是否需要Buff剩余回合数提示？

4. **特殊Buff**
   - 某些强力Buff是否应该有使用限制（每战斗一次）？
   - 自动复活是否需要CD？
