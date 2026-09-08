using XXX.Entity;

namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 一份完整的平衡表快照。
    /// </summary>
    internal sealed class BalanceCatalogSnapshot
    {
        public string Version { get; init; } = BalanceCatalog.SeedSnapshotVersion;

        public IReadOnlyList<ItemTemplateEntity> Items { get; init; } = Array.Empty<ItemTemplateEntity>();

        public IReadOnlyList<EquipmentTemplateEntity> EquipmentTemplates { get; init; } = Array.Empty<EquipmentTemplateEntity>();

        public IReadOnlyList<MonsterTemplateEntity> MonsterTemplates { get; init; } = Array.Empty<MonsterTemplateEntity>();

        public IReadOnlyList<MapTemplateEntity> MapTemplates { get; init; } = Array.Empty<MapTemplateEntity>();

        public IReadOnlyList<PetTemplateEntity> PetTemplates { get; init; } = Array.Empty<PetTemplateEntity>();

        public IReadOnlyList<DungeonTemplateEntity> DungeonTemplates { get; init; } = Array.Empty<DungeonTemplateEntity>();
    }

    /// <summary>
    /// 内置平衡表导入源，负责生成仅供补种 / 修复链使用的模板快照。
    /// </summary>
    internal static class BalanceCatalog
    {
        public const string SeedSnapshotVersion = "balance-playable-v4-20260331";

        private static readonly object SyncRoot = new();
        private static readonly int[] LevelBands = { 1, 5, 10, 15, 20, 25, 30 };
        private static readonly string[] BandNames = { "青竹", "玄铁", "流云", "赤霄", "寒星", "天罡", "幽月" };
        private static BalanceCatalogSnapshot? _snapshot;

        /// <summary>
        /// 获取导入用平衡表快照，首次访问时按需构建并缓存。
        /// </summary>
        public static BalanceCatalogSnapshot GetSeedSnapshot()
        {
            if (_snapshot != null)
            {
                return _snapshot;
            }

            lock (SyncRoot)
            {
                if (_snapshot != null)
                {
                    return _snapshot;
                }

                // 装备模板体量最大，先独立构建后再复用到其它目录初始化逻辑。
                var equipmentTemplates = BuildEquipmentTemplates();
                _snapshot = new BalanceCatalogSnapshot
                {
                    Items = BuildItemTemplates(),
                    EquipmentTemplates = equipmentTemplates,
                    MonsterTemplates = BuildMonsterTemplates(),
                    MapTemplates = BuildMapTemplates(),
                    PetTemplates = BuildPetTemplates(),
                    DungeonTemplates = BuildDungeonTemplates()
                };

                return _snapshot;
            }
        }

        /// <summary>
        /// 构造基础道具模板。
        /// </summary>
        private static IReadOnlyList<ItemTemplateEntity> BuildItemTemplates()
        {
            return
            [
                CreateItem("item_001", "小型生命药水", 1, ItemType.Consumable, "恢复100点生命。", 99, 1),
                CreateItem("item_002", "中型生命药水", 10, ItemType.Consumable, "恢复300点生命。", 99, 2),
                CreateItem("item_003", "狼牙", 1, ItemType.Material, "前中期锻造与任务共用材料。", 999, 1),
                CreateItem("item_004", "阅历书", 1, ItemType.Material, "签到与活动投放的成长资源模板。", 99, 2),
                CreateItem("item_005", "护锻石", 1, ItemType.Material, "签到与活动投放的强化保护材料模板。", 99, 2),
                CreateItem("item_006", "小型法力药水", 1, ItemType.Consumable, "恢复120点法力。", 99, 1),
                CreateItem("item_007", "中型法力药水", 12, ItemType.Consumable, "恢复280点法力。", 99, 2),
                CreateItem("bag_expansion_token", "纳物令", 1, ItemType.InventoryExpansion, "使用后装备背包和道具背包各增加5格，容量上限为200格。", 99, 2),
                CreateItem("pet_egg", "火狐宠物蛋", 1, ItemType.PetEgg, "使用后可孵化出一只小火狐。", 99, 2, petEggConfig: new ItemPetEggConfig
                {
                    PetTemplateId = "pet_fox_fire"
                }),
                CreateItem("pet_food_basic", "灵兽口粮", 1, ItemType.Material, "喂养灵宠时消耗的基础食物。", 999, 1),
                CreateItem("evolution_stone", "进化石", 1, ItemType.Material, "灵宠进化所需的核心材料。", 999, 3),
                CreateItem("seed_blood_grass", "凝血草种子", 1, ItemType.Seed, "用于在灵田播种凝血草。", 999, 1),
                CreateItem("seed_spirit_lotus", "灵泉莲种子", 1, ItemType.Seed, "用于在灵田播种灵泉莲。", 999, 2),
                CreateItem("seed_stone_moss", "灵石苔孢子", 2, ItemType.Seed, "用于在灵田播种灵石苔。", 999, 3),
                CreateItem("alchemy_herb", "凝血草", 1, ItemType.Material, "基础炼丹药材。", 999, 1),
                CreateItem("spirit_water", "灵泉水", 1, ItemType.Material, "炼丹与灵田都能使用的灵液。", 999, 1),
                CreateItem("spirit_dust", "灵石粉末", 1, ItemType.Material, "恢复类丹药常用辅材。", 999, 2),
                CreateItem("beast_core", "妖兽内丹", 1, ItemType.Material, "高阶丹药与宠物培养共用材料。", 999, 3),
                CreateItem("chest_gold_basic", "初级金币箱", 1, ItemType.Chest, "开启后可随机获得金币与基础材料。", 99, 2, chestConfig: new ItemChestConfig
                {
                    OpenMode = ChestOpenMode.SinglePick,
                    RollCount = 1,
                    Rewards =
                    [
                        new ItemChestRewardEntry { RewardType = ChestRewardType.Gold, MinCount = 300, MaxCount = 600, Weight = 70 },
                        new ItemChestRewardEntry { RewardType = ChestRewardType.Item, TargetId = "item_003", MinCount = 1, MaxCount = 3, Weight = 30 }
                    ]
                }),
                CreateItem("skill_book_wind_blade", "风刃术残卷", 1, ItemType.SkillBook, "使用后学会一门基础技能。", 1, 2, skillBookConfig: new ItemSkillBookConfig
                {
                    SkillId = 2
                }),
                CreateItem("pill_exp_small", "聚气丹", 1, ItemType.Pill, "服用后直接获得修为。", 99, 2, pillConfig: new ItemPillConfig
                {
                    EffectType = ItemPillEffectType.AddExp,
                    ExpGain = 180
                }),
                CreateItem("pill_breakthrough_small", "破障丹", 8, ItemType.Pill, "服用后提高下次突破成功率。", 99, 3, pillConfig: new ItemPillConfig
                {
                    EffectType = ItemPillEffectType.BreakthroughChance,
                    BreakthroughBonusPercent = 8
                }),
                CreateItem("pill_guard_small", "护脉丹", 12, ItemType.Pill, "服用后提升基础防御。", 99, 3, pillConfig: new ItemPillConfig
                {
                    EffectType = ItemPillEffectType.AddAttribute,
                    AttributeType = AttributeType.Type5,
                    AttributeValue = 12
                }),
                CreateItem("ore_t1", "赤砂矿", 1, ItemType.Material, "低阶锻造矿石。", 999, 1),
                CreateItem("ore_t2", "玄铁矿", 15, ItemType.Material, "中阶锻造矿石。", 999, 2),
                CreateItem("ore_t3", "陨星矿", 25, ItemType.Material, "高阶锻造矿石。", 999, 3),
                CreateItem("wood_t1", "青纹木", 1, ItemType.Material, "低阶武器柄材与符木。", 999, 1),
                CreateItem("wood_t2", "沉香木", 15, ItemType.Material, "中阶武器柄材与法具骨架。", 999, 2),
                CreateItem("wood_t3", "雷击木", 25, ItemType.Material, "高阶武器柄材与灵性木料。", 999, 3),
                CreateItem("hide_t1", "硬皮", 1, ItemType.Material, "低阶防具缝制材料。", 999, 1),
                CreateItem("hide_t2", "玄纹皮", 15, ItemType.Material, "中阶防具缝制材料。", 999, 2),
                CreateItem("hide_t3", "龙鳞革", 25, ItemType.Material, "高阶防具缝制材料。", 999, 3),
                CreateItem("crystal_t1", "聚灵晶", 1, ItemType.Material, "低阶法器与饰品用晶石。", 999, 1),
                CreateItem("crystal_t2", "曜灵晶", 15, ItemType.Material, "中阶法器与饰品用晶石。", 999, 2),
                CreateItem("crystal_t3", "天辉晶", 25, ItemType.Material, "高阶法器与饰品用晶石。", 999, 3),
                CreateItem("dungeon_token_1", "试炼令牌", 6, ItemType.Material, "新手试炼副本掉落令牌。", 999, 2),
                CreateItem("dungeon_token_2", "迷雾令牌", 12, ItemType.Material, "迷雾林地副本掉落令牌。", 999, 2),
                CreateItem("dungeon_token_3", "裂谷令牌", 20, ItemType.Material, "烈焰裂谷副本掉落令牌。", 999, 3),
                CreateItem("dungeon_token_4", "幽月令牌", 28, ItemType.Material, "幽月古道副本掉落令牌。", 999, 4),
                CreateItem("pet_essence", "灵宠精华", 12, ItemType.Material, "灵宠成长与后续扩展保留材料。", 999, 3),
                // 好感度道具
                CreateItem("fav_rose", "玫瑰", 1, ItemType.FavorabilityGift, "赠送后增加对方10点好感度。", 99, 1, favorabilityGiftConfig: new ItemFavorabilityGiftConfig { FavorabilityValue = 10, DailyLimit = 5, CanGift = true }),
                CreateItem("fav_jade_pendant", "玉佩", 1, ItemType.FavorabilityGift, "赠送后增加对方20点好感度。", 99, 2, favorabilityGiftConfig: new ItemFavorabilityGiftConfig { FavorabilityValue = 20, DailyLimit = 3, CanGift = true }),
                CreateItem("fav_spirit_fruit", "灵果", 1, ItemType.FavorabilityGift, "赠送后增加对方30点好感度。", 99, 3, favorabilityGiftConfig: new ItemFavorabilityGiftConfig { FavorabilityValue = 30, DailyLimit = 2, CanGift = true }),
                CreateItem("fav_ancient_scroll", "古卷", 1, ItemType.FavorabilityGift, "赠送后增加对方50点好感度。", 99, 4, favorabilityGiftConfig: new ItemFavorabilityGiftConfig { FavorabilityValue = 50, DailyLimit = 1, CanGift = true }),
                CreateItem("fav_thorn", "荆棘", 1, ItemType.FavorabilityGift, "赠送后减少对方10点好感度。", 99, 1, favorabilityGiftConfig: new ItemFavorabilityGiftConfig { FavorabilityValue = -10, DailyLimit = 5, CanGift = true })
            ];
        }

        /// <summary>
        /// 构造整套装备模板。
        /// </summary>
        private static IReadOnlyList<EquipmentTemplateEntity> BuildEquipmentTemplates()
        {
            var templates = new List<EquipmentTemplateEntity>
            {
                CreateWeaponTemplate(1, "铁剑", 1, EquipmentQuality.Common, CombatStyle.Physical, WeaponCategory.Sword, 0),
                CreateWeaponTemplate(2, "精钢刀", 5, EquipmentQuality.Common, CombatStyle.Physical, WeaponCategory.Blade, 1),
                CreateWeaponTemplate(3, "焰纹书", 5, EquipmentQuality.Common, CombatStyle.Magic, WeaponCategory.Book, 1)
            };

            // 每个等级段都会生成一套物理/法术武器与防具，保证成长曲线连续。
            for (var bandIndex = 0; bandIndex < LevelBands.Length; bandIndex++)
            {
                var level = LevelBands[bandIndex];
                var bandName = BandNames[bandIndex];
                var quality = GetBandQuality(bandIndex);
                var baseId = (bandIndex + 1) * 1000;

                templates.Add(CreateWeaponTemplate(baseId + 1, $"{bandName}剑", level, quality, CombatStyle.Physical, WeaponCategory.Sword, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 2, $"{bandName}刀", level, quality, CombatStyle.Physical, WeaponCategory.Blade, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 3, $"{bandName}斧", level, quality, CombatStyle.Physical, WeaponCategory.Axe, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 4, $"{bandName}枪", level, quality, CombatStyle.Physical, WeaponCategory.Spear, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 11, $"{bandName}琴", level, quality, CombatStyle.Magic, WeaponCategory.Qin, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 12, $"{bandName}棋", level, quality, CombatStyle.Magic, WeaponCategory.Chess, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 13, $"{bandName}书", level, quality, CombatStyle.Magic, WeaponCategory.Book, bandIndex));
                templates.Add(CreateWeaponTemplate(baseId + 14, $"{bandName}笔", level, quality, CombatStyle.Magic, WeaponCategory.Brush, bandIndex));

                templates.Add(CreatePhysicalGearTemplate(baseId + 21, $"{bandName}战盔", level, quality, EquipmentSlot.Helmet, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 22, $"{bandName}法冠", level, quality, EquipmentSlot.Helmet, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 31, $"{bandName}战甲", level, quality, EquipmentSlot.Armor, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 32, $"{bandName}法袍", level, quality, EquipmentSlot.Armor, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 41, $"{bandName}护腿", level, quality, EquipmentSlot.Pants, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 42, $"{bandName}法裤", level, quality, EquipmentSlot.Pants, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 51, $"{bandName}战靴", level, quality, EquipmentSlot.Boots, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 52, $"{bandName}云履", level, quality, EquipmentSlot.Boots, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 61, $"{bandName}战链", level, quality, EquipmentSlot.Necklace, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 62, $"{bandName}灵坠", level, quality, EquipmentSlot.Necklace, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 71, $"{bandName}战戒", level, quality, EquipmentSlot.Ring, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 72, $"{bandName}法戒", level, quality, EquipmentSlot.Ring, bandIndex));
                templates.Add(CreatePhysicalGearTemplate(baseId + 81, $"{bandName}战法宝", level, quality, EquipmentSlot.Treasure, bandIndex));
                templates.Add(CreateMagicGearTemplate(baseId + 82, $"{bandName}灵法宝", level, quality, EquipmentSlot.Treasure, bandIndex));
            }

            return templates;
        }

        /// <summary>
        /// 按武器流派和等级段生成一把武器模板。
        /// </summary>
        private static EquipmentTemplateEntity CreateWeaponTemplate(
            int equipmentId,
            string name,
            int level,
            EquipmentQuality quality,
            CombatStyle combatStyle,
            WeaponCategory weaponCategory,
            int bandIndex)
        {
            var baseAttack = 16 + bandIndex * 10 + bandIndex * bandIndex * 2;
            var attackSpread = 5 + bandIndex * 2;
            var baseHp = combatStyle == CombatStyle.Magic
                ? 22 + bandIndex * 32 + bandIndex * bandIndex * 2
                : 26 + bandIndex * 36 + bandIndex * bandIndex * 2;
            var baseMp = combatStyle == CombatStyle.Magic
                ? 28 + bandIndex * 38 + bandIndex * bandIndex * 2
                : 8 + bandIndex * 10 + bandIndex;
            var hpSpread = 10 + bandIndex * 4;
            var mpSpread = combatStyle == CombatStyle.Magic
                ? 12 + bandIndex * 4
                : 6 + bandIndex * 2;
            var speedBonus = weaponCategory switch
            {
                WeaponCategory.Blade => 2,
                WeaponCategory.Spear => 3,
                WeaponCategory.Qin => 2,
                WeaponCategory.Brush => 3,
                WeaponCategory.Axe => 0,
                WeaponCategory.Chess => 0,
                _ => 1
            };
            var attackBias = weaponCategory switch
            {
                WeaponCategory.Axe => 4 + bandIndex,
                WeaponCategory.Spear => 2 + bandIndex / 2,
                WeaponCategory.Book => 4 + bandIndex,
                WeaponCategory.Chess => 2 + bandIndex / 2,
                _ => 0
            };

            return CreateEquipmentTemplate(
                equipmentId,
                name,
                level,
                quality,
                EquipmentSlot.Weapon,
                combatStyle,
                weaponCategory,
                baseHp,
                baseHp + hpSpread,
                baseMp,
                baseMp + mpSpread,
                combatStyle == CombatStyle.Physical ? baseAttack + attackBias : 0,
                combatStyle == CombatStyle.Physical ? baseAttack + attackBias + attackSpread : 0,
                combatStyle == CombatStyle.Magic ? baseAttack + attackBias : 0,
                combatStyle == CombatStyle.Magic ? baseAttack + attackBias + attackSpread : 0,
                0,
                0,
                0,
                0,
                speedBonus,
                speedBonus + (speedBonus > 0 ? 1 : 0),
                $"{name}适配Lv.{level}阶段的{(combatStyle == CombatStyle.Magic ? "法术" : "物理")}流派武器。");
        }

        /// <summary>
        /// 生成物理系防具模板。
        /// </summary>
        private static EquipmentTemplateEntity CreatePhysicalGearTemplate(
            int equipmentId,
            string name,
            int level,
            EquipmentQuality quality,
            EquipmentSlot slot,
            int bandIndex)
        {
            return slot switch
            {
                EquipmentSlot.Necklace => CreateEquipmentTemplate(
                    equipmentId,
                    name,
                    level,
                    quality,
                    slot,
                    CombatStyle.Physical,
                    WeaponCategory.None,
                    42 + bandIndex * 34 + bandIndex * bandIndex * 2,
                    54 + bandIndex * 40 + bandIndex * bandIndex * 2,
                    10 + bandIndex * 8 + bandIndex,
                    18 + bandIndex * 11 + bandIndex * 2,
                    6 + bandIndex * 5 + bandIndex * bandIndex / 2,
                    10 + bandIndex * 6 + bandIndex * bandIndex / 2,
                    0,
                    0,
                    4 + bandIndex * 3,
                    6 + bandIndex * 3 + bandIndex / 2,
                    3 + bandIndex * 2,
                    5 + bandIndex * 2 + bandIndex / 2,
                    0,
                    0,
                    $"{name}提升物理输出与双防生存。"),
                EquipmentSlot.Ring => CreateEquipmentTemplate(
                    equipmentId,
                    name,
                    level,
                    quality,
                    slot,
                    CombatStyle.Physical,
                    WeaponCategory.None,
                    28 + bandIndex * 22 + bandIndex * bandIndex * 2,
                    38 + bandIndex * 28 + bandIndex * bandIndex * 2,
                    8 + bandIndex * 6 + bandIndex,
                    14 + bandIndex * 9 + bandIndex * 2,
                    8 + bandIndex * 6 + bandIndex * bandIndex / 2,
                    12 + bandIndex * 7 + bandIndex * bandIndex / 2,
                    0,
                    0,
                    3 + bandIndex * 2,
                    5 + bandIndex * 2 + bandIndex / 2,
                    2 + bandIndex * 2,
                    4 + bandIndex * 2 + bandIndex / 2,
                    0,
                    0,
                    $"{name}补充物理输出与护体。"),
                _ => CreateArmorTemplate(equipmentId, name, level, quality, slot, CombatStyle.Physical, bandIndex)
            };
        }

        /// <summary>
        /// 生成法术系防具模板。
        /// </summary>
        private static EquipmentTemplateEntity CreateMagicGearTemplate(
            int equipmentId,
            string name,
            int level,
            EquipmentQuality quality,
            EquipmentSlot slot,
            int bandIndex)
        {
            return slot switch
            {
                EquipmentSlot.Necklace => CreateEquipmentTemplate(
                    equipmentId,
                    name,
                    level,
                    quality,
                    slot,
                    CombatStyle.Magic,
                    WeaponCategory.None,
                    38 + bandIndex * 32 + bandIndex * bandIndex * 2,
                    50 + bandIndex * 38 + bandIndex * bandIndex * 2,
                    28 + bandIndex * 24 + bandIndex * bandIndex * 2,
                    40 + bandIndex * 30 + bandIndex * bandIndex * 2,
                    0,
                    0,
                    6 + bandIndex * 5 + bandIndex * bandIndex / 2,
                    10 + bandIndex * 6 + bandIndex * bandIndex / 2,
                    3 + bandIndex * 2,
                    5 + bandIndex * 2 + bandIndex / 2,
                    4 + bandIndex * 3,
                    6 + bandIndex * 3 + bandIndex / 2,
                    0,
                    0,
                    $"{name}提升法术输出、续航与双防生存。"),
                EquipmentSlot.Ring => CreateEquipmentTemplate(
                    equipmentId,
                    name,
                    level,
                    quality,
                    slot,
                    CombatStyle.Magic,
                    WeaponCategory.None,
                    24 + bandIndex * 20 + bandIndex * bandIndex * 2,
                    34 + bandIndex * 26 + bandIndex * bandIndex * 2,
                    24 + bandIndex * 18 + bandIndex * bandIndex * 2,
                    34 + bandIndex * 24 + bandIndex * bandIndex * 2,
                    0,
                    0,
                    8 + bandIndex * 6 + bandIndex * bandIndex / 2,
                    12 + bandIndex * 7 + bandIndex * bandIndex / 2,
                    2 + bandIndex * 2,
                    4 + bandIndex * 2 + bandIndex / 2,
                    3 + bandIndex * 2,
                    5 + bandIndex * 2 + bandIndex / 2,
                    0,
                    0,
                    $"{name}补充法术输出与法力护持。"),
                _ => CreateArmorTemplate(equipmentId, name, level, quality, slot, CombatStyle.Magic, bandIndex)
            };
        }

        /// <summary>
        /// 生成通用护甲模板，并按部位计算主副属性权重。
        /// </summary>
        private static EquipmentTemplateEntity CreateArmorTemplate(
            int equipmentId,
            string name,
            int level,
            EquipmentQuality quality,
            EquipmentSlot slot,
            CombatStyle combatStyle,
            int bandIndex)
        {
            var slotHp = slot switch
            {
                EquipmentSlot.Helmet => 54 + bandIndex * 38 + bandIndex * bandIndex * 3,
                EquipmentSlot.Armor => 110 + bandIndex * 72 + bandIndex * bandIndex * 6,
                EquipmentSlot.Pants => 84 + bandIndex * 56 + bandIndex * bandIndex * 4,
                EquipmentSlot.Boots => 60 + bandIndex * 42 + bandIndex * bandIndex * 3,
                _ => 48 + bandIndex * 34 + bandIndex * bandIndex * 2
            };
            var slotMp = slot switch
            {
                EquipmentSlot.Helmet => 32 + bandIndex * 26 + bandIndex * bandIndex * 2,
                EquipmentSlot.Armor => 64 + bandIndex * 52 + bandIndex * bandIndex * 4,
                EquipmentSlot.Pants => 50 + bandIndex * 40 + bandIndex * bandIndex * 3,
                EquipmentSlot.Boots => 36 + bandIndex * 28 + bandIndex * bandIndex * 2,
                _ => 28 + bandIndex * 20 + bandIndex * bandIndex * 2
            };
            var primaryDefense = slot switch
            {
                EquipmentSlot.Helmet => 6 + bandIndex * 4 + bandIndex / 2,
                EquipmentSlot.Armor => 10 + bandIndex * 6 + bandIndex,
                EquipmentSlot.Pants => 8 + bandIndex * 5 + bandIndex / 2,
                EquipmentSlot.Boots => 5 + bandIndex * 4 + bandIndex / 2,
                _ => 4 + bandIndex * 3
            };
            var secondaryDefense = Math.Max(2, ScaleStat(primaryDefense, 0.72d));
            var hpBase = combatStyle == CombatStyle.Physical
                ? slotHp
                : ScaleStat(slotHp, 0.92d);
            var mpBase = combatStyle == CombatStyle.Magic
                ? slotMp
                : Math.Max(6, ScaleStat(slotMp, 0.35d));
            var hpSpread = 14 + bandIndex * 6;
            var mpSpread = 10 + bandIndex * 5;
            var speed = slot == EquipmentSlot.Boots
                ? (combatStyle == CombatStyle.Physical ? 2 : 1) + bandIndex / 4
                : 0;

            return CreateEquipmentTemplate(
                equipmentId,
                name,
                level,
                quality,
                slot,
                combatStyle,
                WeaponCategory.None,
                hpBase,
                hpBase + hpSpread,
                mpBase,
                mpBase + mpSpread,
                0,
                0,
                0,
                0,
                combatStyle == CombatStyle.Physical ? primaryDefense : secondaryDefense,
                (combatStyle == CombatStyle.Physical ? primaryDefense : secondaryDefense) + 2 + bandIndex / 2,
                combatStyle == CombatStyle.Magic ? primaryDefense : secondaryDefense,
                (combatStyle == CombatStyle.Magic ? primaryDefense : secondaryDefense) + 2 + bandIndex / 2,
                speed,
                speed > 0 ? speed + 1 : 0,
                $"{name}适配Lv.{level}阶段的{(combatStyle == CombatStyle.Magic ? "法术" : "物理")}防具体系。");
        }

        /// <summary>
        /// 按完整属性区间组装装备模板实体。
        /// </summary>
        private static EquipmentTemplateEntity CreateEquipmentTemplate(
            int equipmentId,
            string name,
            int level,
            EquipmentQuality quality,
            EquipmentSlot slot,
            CombatStyle combatStyle,
            WeaponCategory weaponCategory,
            int minType1,
            int maxType1,
            int minType2,
            int maxType2,
            int minType3,
            int maxType3,
            int minType4,
            int maxType4,
            int minType5,
            int maxType5,
            int minType6,
            int maxType6,
            int minType7,
            int maxType7,
            string description)
        {
            return new EquipmentTemplateEntity
            {
                EquipmentId = equipmentId,
                Name = name,
                Level = level,
                Quality = (int)quality,
                Slot = (int)slot,
                CombatStyle = (int)combatStyle,
                WeaponCategory = (int)weaponCategory,
                Description = description,
                MinType1 = minType1,
                MaxType1 = maxType1,
                MinType2 = minType2,
                MaxType2 = maxType2,
                MinType3 = minType3,
                MaxType3 = maxType3,
                MinType4 = minType4,
                MaxType4 = maxType4,
                MinType5 = minType5,
                MaxType5 = maxType5,
                MinType6 = minType6,
                MaxType6 = maxType6,
                MinType7 = minType7,
                MaxType7 = maxType7,
                MinType8 = 0,
                MaxType8 = 0,
                MinType9 = 0,
                MaxType9 = 0,
                MinType10 = 0,
                MaxType10 = 0,
                MinType11 = 0,
                MaxType11 = 0,
                MinType12 = 0,
                MaxType12 = 0,
                MinType13 = 0,
                MaxType13 = 0,
                MinType14 = 0,
                MaxType14 = 0,
                MinType15 = 0,
                MaxType15 = 0,
                Element = Element.None
            };
        }

        /// <summary>
        /// 根据等级段确定装备品质。
        /// </summary>
        private static EquipmentQuality GetBandQuality(int bandIndex)
        {
            if (bandIndex >= 6)
            {
                return EquipmentQuality.Epic;
            }

            if (bandIndex >= 4)
            {
                return EquipmentQuality.Rare;
            }

            if (bandIndex >= 2)
            {
                return EquipmentQuality.Uncommon;
            }

            return EquipmentQuality.Common;
        }

        /// <summary>
        /// 组装单个道具模板。
        /// </summary>
        private static ItemTemplateEntity CreateItem(
            string itemId,
            string name,
            int useLevel,
            ItemType type,
            string description,
            int maxStack,
            int quality,
            ItemChestConfig? chestConfig = null,
            ItemSkillBookConfig? skillBookConfig = null,
            ItemPetEggConfig? petEggConfig = null,
            ItemPillConfig? pillConfig = null,
            ItemFavorabilityGiftConfig? favorabilityGiftConfig = null)
        {
            return new ItemTemplateEntity
            {
                ItemId = itemId,
                Name = name,
                UseLevel = useLevel,
                Type = (int)type,
                Description = description,
                MaxStack = maxStack,
                Quality = quality,
                ChestConfig = chestConfig,
                SkillBookConfig = skillBookConfig,
                PetEggConfig = petEggConfig,
                PillConfig = pillConfig,
                FavorabilityGiftConfig = favorabilityGiftConfig
            };
        }

        /// <summary>
        /// 构造怪物模板。
        /// </summary>
        private static IReadOnlyList<MonsterTemplateEntity> BuildMonsterTemplates()
        {
            var seeds = new (string Id, string Name, int Level, CombatStyle Style, double HpScale, double OffenseScale, double DefenseScale, double SpeedScale, Element[] ElementPool, string[] ItemDrops, string[] EquipmentDrops)[]
            {
                ("monster_001", "山兔妖", 1, CombatStyle.Physical, 0.95, 0.92, 0.88, 1.08, new[] { Element.Earth, Element.Wood }, new[] { "item_001", "item_003", "ore_t1" }, new[] { "1", "1001" }),
                ("monster_002", "竹皮狸", 1, CombatStyle.Neutral, 1.00, 0.95, 0.95, 1.00, new[] { Element.Wood }, new[] { "item_001", "item_003", "wood_t1" }, new[] { "1002", "1021" }),
                ("monster_003", "雾芽灵", 2, CombatStyle.Magic, 0.92, 0.98, 0.88, 1.06, new[] { Element.Water, Element.Wood }, new[] { "item_006", "item_003", "crystal_t1" }, new[] { "3", "1013" }),
                ("monster_004", "青爪狼", 4, CombatStyle.Physical, 1.02, 1.02, 0.96, 1.08, new[] { Element.Wind, Element.Wood }, new[] { "item_001", "item_003", "hide_t1" }, new[] { "2", "2001" }),
                ("monster_005", "竹甲兵", 4, CombatStyle.Neutral, 1.12, 1.00, 1.06, 0.96, new[] { Element.Wood, Element.Earth }, new[] { "item_001", "item_003", "ore_t1" }, new[] { "2021", "2031" }),
                ("monster_006", "风咒鸦", 5, CombatStyle.Magic, 0.98, 1.05, 0.92, 1.12, new[] { Element.Wind }, new[] { "item_006", "item_003", "spirit_dust" }, new[] { "2011", "2014" }),
                ("monster_007", "黑风盗", 8, CombatStyle.Physical, 1.08, 1.10, 1.00, 1.02, new[] { Element.Wind }, new[] { "item_002", "item_003", "ore_t1" }, new[] { "3001", "3041" }),
                ("monster_008", "裂石猿", 8, CombatStyle.Neutral, 1.18, 1.04, 1.10, 0.92, new[] { Element.Earth }, new[] { "item_001", "hide_t1", "beast_core" }, new[] { "3021", "3031" }),
                ("monster_009", "噬火蝶", 9, CombatStyle.Magic, 1.00, 1.12, 0.94, 1.12, new[] { Element.Fire }, new[] { "item_006", "crystal_t1", "alchemy_herb" }, new[] { "3011", "3014" }),
                ("monster_010", "落霞刀客", 12, CombatStyle.Physical, 1.10, 1.14, 1.02, 1.04, new[] { Element.Fire, Element.Metal }, new[] { "item_002", "ore_t1", "hide_t1" }, new[] { "4002", "4051" }),
                ("monster_011", "寒潭鳞妖", 12, CombatStyle.Neutral, 1.22, 1.06, 1.12, 0.94, new[] { Element.Water }, new[] { "item_001", "wood_t1", "spirit_water" }, new[] { "4022", "4031" }),
                ("monster_012", "迷雾咒师", 13, CombatStyle.Magic, 1.02, 1.16, 0.96, 1.10, new[] { Element.Water, Element.Wood }, new[] { "item_006", "crystal_t1", "spirit_dust" }, new[] { "4012", "4013" }),
                ("monster_013", "泽地鳄妖", 16, CombatStyle.Physical, 1.16, 1.18, 1.08, 0.96, new[] { Element.Water, Element.Earth }, new[] { "item_002", "ore_t2", "hide_t2" }, new[] { "5003", "5031" }),
                ("monster_014", "寒水祭司", 16, CombatStyle.Magic, 1.06, 1.18, 1.02, 1.06, new[] { Element.Water, Element.Ice }, new[] { "item_007", "crystal_t2", "spirit_water" }, new[] { "5011", "5012" }),
                ("monster_015", "沼泽蛮牛", 17, CombatStyle.Neutral, 1.28, 1.10, 1.16, 0.90, new[] { Element.Earth }, new[] { "item_002", "wood_t2", "beast_core" }, new[] { "5021", "5041" }),
                ("monster_016", "赤岩战偶", 20, CombatStyle.Physical, 1.22, 1.22, 1.16, 0.94, new[] { Element.Fire, Element.Earth }, new[] { "item_002", "ore_t2", "hide_t2" }, new[] { "6001", "6031" }),
                ("monster_017", "炎砂术士", 20, CombatStyle.Magic, 1.08, 1.22, 1.04, 1.06, new[] { Element.Fire }, new[] { "item_007", "crystal_t2", "spirit_dust" }, new[] { "6011", "6014" }),
                ("monster_018", "火鳞蝎王", 21, CombatStyle.Neutral, 1.32, 1.12, 1.18, 0.92, new[] { Element.Fire }, new[] { "item_002", "wood_t2", "evolution_stone" }, new[] { "6021", "6051" }),
                ("monster_019", "星落游骑", 24, CombatStyle.Physical, 1.16, 1.24, 1.10, 1.10, new[] { Element.Thunder, Element.Wind }, new[] { "item_002", "ore_t3", "wood_t3" }, new[] { "7004", "7041" }),
                ("monster_020", "陨辉乐师", 24, CombatStyle.Magic, 1.08, 1.24, 1.08, 1.08, new[] { Element.Thunder, Element.Metal }, new[] { "item_007", "crystal_t3", "pet_essence" }, new[] { "7011", "7012" }),
                ("monster_021", "古原守卫", 25, CombatStyle.Neutral, 1.34, 1.14, 1.20, 0.94, new[] { Element.Earth, Element.Metal }, new[] { "item_002", "hide_t3", "beast_core" }, new[] { "7021", "7061" }),
                ("monster_022", "幽月刀魂", 28, CombatStyle.Physical, 1.18, 1.28, 1.14, 1.08, new[] { Element.Ice, Element.Metal }, new[] { "item_002", "ore_t3", "dungeon_token_4" }, new[] { "7002", "7051" }),
                ("monster_023", "残卷术灵", 28, CombatStyle.Magic, 1.10, 1.28, 1.10, 1.10, new[] { Element.Ice, Element.Thunder }, new[] { "item_007", "crystal_t3", "pet_essence" }, new[] { "7013", "7062" }),
                ("monster_024", "夜巡魇兽", 29, CombatStyle.Neutral, 1.40, 1.18, 1.24, 0.98, new[] { Element.Wind, Element.Ice }, new[] { "item_002", "hide_t3", "evolution_stone" }, new[] { "7022", "7071" }),
                ("monster_025", "试炼监工", 6, CombatStyle.Physical, 1.28, 1.16, 1.12, 1.00, new[] { Element.Earth }, new[] { "dungeon_token_1", "item_003", "ore_t1" }, new[] { "2001", "2031" }),
                ("monster_026", "试炼守卫首", 8, CombatStyle.Neutral, 1.75, 1.26, 1.22, 0.96, new[] { Element.Metal, Element.Earth }, new[] { "dungeon_token_1", "pet_egg", "pet_food_basic" }, new[] { "2004", "2051" }),
                ("monster_027", "迷林斥候长", 14, CombatStyle.Physical, 1.32, 1.20, 1.14, 1.02, new[] { Element.Wood, Element.Wind }, new[] { "dungeon_token_2", "ore_t2", "hide_t2" }, new[] { "4001", "4031" }),
                ("monster_028", "迷雾林主", 16, CombatStyle.Magic, 1.88, 1.30, 1.20, 1.00, new[] { Element.Wood, Element.Water }, new[] { "dungeon_token_2", "evolution_stone", "pet_essence" }, new[] { "4011", "4062" }),
                ("monster_029", "裂谷炎卫", 22, CombatStyle.Physical, 1.36, 1.24, 1.18, 1.00, new[] { Element.Fire, Element.Earth }, new[] { "dungeon_token_3", "ore_t3", "wood_t3" }, new[] { "6001", "6051" }),
                ("monster_030", "裂谷焚心兽", 24, CombatStyle.Magic, 1.92, 1.32, 1.24, 1.00, new[] { Element.Fire, Element.Thunder }, new[] { "dungeon_token_3", "beast_core", "pet_essence" }, new[] { "6014", "6062" }),
                ("monster_031", "幽月暗卫", 30, CombatStyle.Physical, 1.42, 1.28, 1.20, 1.04, new[] { Element.Ice, Element.Metal }, new[] { "dungeon_token_4", "ore_t3", "hide_t3" }, new[] { "7001", "7071" }),
                ("monster_032", "幽月古道之主", 32, CombatStyle.Magic, 2.05, 1.38, 1.28, 1.02, new[] { Element.Ice, Element.Thunder, Element.Wind }, new[] { "dungeon_token_4", "evolution_stone", "pet_essence" }, new[] { "7013", "7062" }),
                // 精英怪物（秘境专用）
                ("monster_elite_001", "石魔", 10, CombatStyle.Physical, 1.55, 1.22, 1.20, 0.90, new[] { Element.Earth }, new[] { "item_002", "ore_t2", "crystal_t1" }, new[] { "3021", "3031" }),
                ("monster_elite_002", "影刺客", 15, CombatStyle.Physical, 1.40, 1.30, 1.10, 1.18, new[] { Element.Wind, Element.Thunder }, new[] { "item_002", "hide_t2", "spirit_dust" }, new[] { "5001", "5041" }),
                ("monster_elite_003", "火焰灵", 20, CombatStyle.Magic, 1.50, 1.28, 1.15, 1.08, new[] { Element.Fire }, new[] { "item_007", "crystal_t2", "alchemy_herb" }, new[] { "6011", "6051" }),
                ("monster_elite_004", "寒冰蛟", 25, CombatStyle.Neutral, 1.65, 1.26, 1.22, 0.96, new[] { Element.Water, Element.Ice }, new[] { "item_002", "crystal_t2", "spirit_water" }, new[] { "7021", "7061" }),
                ("monster_elite_005", "雷兽", 28, CombatStyle.Neutral, 1.70, 1.32, 1.24, 1.00, new[] { Element.Thunder }, new[] { "item_002", "beast_core", "evolution_stone" }, new[] { "7022", "7071" })
            };

            return seeds
                .Select(seed => CreateMonsterTemplate(seed.Id, seed.Name, seed.Level, seed.Style, seed.HpScale, seed.OffenseScale, seed.DefenseScale, seed.SpeedScale, seed.ElementPool, seed.ItemDrops, seed.EquipmentDrops))
                .ToList();
        }

        /// <summary>
        /// 构造地图模板。
        /// </summary>
        private static IReadOnlyList<MapTemplateEntity> BuildMapTemplates()
        {
            var seeds = new (string Id, string Name, int Level, string Description, string? NextMapId, int MonsterCountMin, int MonsterCountMax, MapMonsterSpawnRule[] SpawnRules)[]
            {
                ("map_001", "新手村外", 1, "低风险的新手练级区域，用于建立初始资源循环。", null, 1, 1, new[] { SpawnRule("monster_001", 60, 1), SpawnRule("monster_002", 30, 1), SpawnRule("monster_003", 10, 1) }),
                ("map_002", "青竹林", 4, "木灵气浓郁的林地，开始出现成体系的物理与法术怪物。", null, 1, 2, new[] { SpawnRule("monster_004", 50, 2), SpawnRule("monster_005", 30, 1), SpawnRule("monster_006", 20, 1) }),
                ("map_003", "黑风坡", 8, "地势起伏较大的中前期练级图，掉落开始偏向锻造材料。", null, 1, 2, new[] { SpawnRule("monster_007", 45, 2), SpawnRule("monster_008", 35, 1), SpawnRule("monster_009", 20, 1) }),
                ("map_004", "落霞涧", 12, "法术怪与精英怪混编的过渡地图。", null, 2, 2, new[] { SpawnRule("monster_010", 40, 1), SpawnRule("monster_011", 35, 1), SpawnRule("monster_012", 25, 1) }),
                ("map_005", "寒水泽", 16, "中期资源图，开始稳定产出二阶材料与宠物培养材料。", null, 2, 3, new[] { SpawnRule("monster_013", 40, 2), SpawnRule("monster_014", 35, 1), SpawnRule("monster_015", 25, 1) }),
                ("map_006", "赤岩谷", 20, "二阶毕业前的主力练级图，物法怪压迫感明显上升。", null, 2, 3, new[] { SpawnRule("monster_016", 40, 2), SpawnRule("monster_017", 35, 1), SpawnRule("monster_018", 25, 1) }),
                ("map_007", "星落原", 24, "高收益资源图，掉落三阶材料与高阶宠物资源。", null, 2, 3, new[] { SpawnRule("monster_019", 38, 1), SpawnRule("monster_020", 34, 1), SpawnRule("monster_021", 28, 1) }),
                ("map_008", "幽月古道", 28, "Lv.30 前最后一张普通地图，用于进入高阶副本前的整备。", null, 2, 3, new[] { SpawnRule("monster_022", 38, 1), SpawnRule("monster_023", 34, 1), SpawnRule("monster_024", 28, 1) }),
                ("fuben_001_1", "试炼洞窟·一层", 5, "新手试炼的第一层，先由监工与杂兵试探玩家。", "fuben_001_2", 2, 2, new[] { SpawnRule("monster_025", 100, 2) }),
                ("fuben_001_2", "试炼洞窟·二层", 5, "第二层开始出现更完整的守卫配置。", "fuben_001_3", 2, 3, new[] { SpawnRule("monster_025", 75, 2), SpawnRule("monster_026", 25, 1) }),
                ("fuben_001_3", "试炼洞窟·三层", 6, "第三层由试炼守卫首压轴，作为最终结算层。", null, 1, 1, new[] { SpawnRule("monster_026", 100, 1) }),
                ("fuben_002_1", "迷雾林地·一层", 12, "迷雾林地外围巡逻层，适合队伍热身。", "fuben_002_2", 2, 2, new[] { SpawnRule("monster_027", 100, 2) }),
                ("fuben_002_2", "迷雾林地·二层", 13, "中层迷障更深，开始混入林地主怪。", "fuben_002_3", 2, 3, new[] { SpawnRule("monster_027", 70, 2), SpawnRule("monster_028", 30, 1) }),
                ("fuben_002_3", "迷雾林地·三层", 14, "最终层由迷雾林主坐镇。", null, 1, 1, new[] { SpawnRule("monster_028", 100, 1) }),
                ("fuben_003_1", "烈焰裂谷·一层", 20, "裂谷外围灼热异常，先由炎卫组成第一道封锁。", "fuben_003_2", 2, 2, new[] { SpawnRule("monster_029", 100, 2) }),
                ("fuben_003_2", "烈焰裂谷·二层", 21, "中层火势更盛，炎卫与焚心兽开始联动。", "fuben_003_3", 2, 3, new[] { SpawnRule("monster_029", 70, 2), SpawnRule("monster_030", 30, 1) }),
                ("fuben_003_3", "烈焰裂谷·三层", 22, "最终层由裂谷焚心兽镇守。", null, 1, 1, new[] { SpawnRule("monster_030", 100, 1) }),
                ("fuben_004_1", "幽月禁路·一层", 28, "幽月禁路前段由暗卫巡守。", "fuben_004_2", 2, 3, new[] { SpawnRule("monster_031", 100, 3) }),
                ("fuben_004_2", "幽月禁路·二层", 29, "中层开始出现古道主怪与暗卫混编。", "fuben_004_3", 2, 3, new[] { SpawnRule("monster_031", 65, 2), SpawnRule("monster_032", 35, 1) }),
                ("fuben_004_3", "幽月禁路·三层", 30, "最终层由幽月古道之主镇守。", null, 1, 1, new[] { SpawnRule("monster_032", 100, 1) })
            };

            return seeds
                .Select(seed => new MapTemplateEntity
                {
                    MapId = seed.Id,
                    Name = seed.Name,
                    Level = seed.Level,
                    Description = seed.Description,
                    NextMapId = seed.NextMapId,
                    Carrying = false,
                    MonsterCountMin = seed.MonsterCountMin,
                    MonsterCountMax = seed.MonsterCountMax,
                    SpawnRules = seed.SpawnRules.ToList()
                })
                .ToList();
        }

        private static MapMonsterSpawnRule SpawnRule(string monsterTemplateId, int weight, int maxCount)
        {
            return new MapMonsterSpawnRule
            {
                MonsterTemplateId = monsterTemplateId,
                Weight = Math.Max(1, weight),
                MaxCount = Math.Max(1, maxCount)
            };
        }

        /// <summary>
        /// 构造灵宠模板。
        /// </summary>
        private static IReadOnlyList<PetTemplateEntity> BuildPetTemplates()
        {
            return
            [
                CreatePetTemplate("pet_fox_fire", "小火狐", "擅长单体爆发的攻击型灵宠。", PetType.Attack, 2, 5, 58, 24, 260, 110, 28, 1.18, new[] { "1", "3" }),
                CreatePetTemplate("pet_wolf_wind", "青风狼", "强调先手节奏的均衡型灵宠。", PetType.Balanced, 2, 5, 52, 30, 290, 90, 34, 1.16, new[] { "2", "13" }),
                CreatePetTemplate("pet_turtle_guard", "玄甲龟", "高生存高防守的护卫型灵宠。", PetType.Defense, 3, 5, 34, 60, 420, 120, 14, 1.22, new[] { "7", "9" }),
                CreatePetTemplate("pet_crane_spirit", "灵羽鹤", "偏法术与辅助的后排灵宠。", PetType.Support, 3, 5, 46, 32, 300, 190, 30, 1.20, new[] { "3", "6" }),
                CreatePetTemplate("pet_shadow_cat", "夜影猫", "兼顾速度与骚扰能力的轻型灵宠。", PetType.Balanced, 2, 5, 54, 26, 250, 120, 38, 1.19, new[] { "1", "11" })
            ];
        }

        /// <summary>
        /// 构造副本模板。
        /// </summary>
        private static IReadOnlyList<DungeonTemplateEntity> BuildDungeonTemplates()
        {
            return
            [
                CreateDungeonTemplate("dungeon_001", "新手试炼", "单人入门副本，按三层推进，稳定产出第一批副本令牌与宠物材料。", 5, 3, "map_002", "fuben_001_1", 1),
                CreateDungeonTemplate("dungeon_002", "迷雾林地", "三人组队副本，按三层推进，产出二阶装备素材与宠物资源。", 12, 3, "map_004", "fuben_002_1", 3),
                CreateDungeonTemplate("dungeon_003", "烈焰裂谷", "双人进阶副本，按三层推进，奖励显著高于同级普通图。", 20, 2, "map_006", "fuben_003_1", 2),
                CreateDungeonTemplate("dungeon_004", "幽月禁路", "三人高阶副本，按三层推进，作为Lv.30前段的阶段毕业挑战。", 28, 2, "map_008", "fuben_004_1", 3)
            ];
        }

        /// <summary>
        /// 根据种子参数生成单个怪物模板。
        /// </summary>
        private static MonsterTemplateEntity CreateMonsterTemplate(
            string monsterId,
            string name,
            int level,
            CombatStyle style,
            double hpScale,
            double offenseScale,
            double defenseScale,
            double speedScale,
            IReadOnlyList<Element> elementPool,
            IReadOnlyList<string> itemDropIds,
            IReadOnlyList<string> equipmentDropIds)
        {
            var hp = ScaleStat(320 + level * 88 + level * level * 3, hpScale);
            var mp = ScaleStat(80 + level * 24 + level * level, style == CombatStyle.Magic ? hpScale * 0.92 : 0.78);
            var baseAttack = 20 + level * 5 + level * level / 10;
            var baseDefense = 10 + level * 3 + level * level / 20;
            var baseSpeed = 16 + level * 2;
            var physicalAttack = style == CombatStyle.Magic
                ? ScaleStat(baseAttack, offenseScale * 0.58)
                : ScaleStat(baseAttack, offenseScale * (style == CombatStyle.Physical ? 1.05 : 0.92));
            var magicAttack = style == CombatStyle.Physical
                ? ScaleStat(baseAttack, offenseScale * 0.52)
                : ScaleStat(baseAttack, offenseScale * (style == CombatStyle.Magic ? 1.05 : 0.92));
            var physicalDefense = style == CombatStyle.Magic
                ? ScaleStat(baseDefense, defenseScale * 0.78)
                : ScaleStat(baseDefense, defenseScale);
            var magicDefense = style == CombatStyle.Physical
                ? ScaleStat(baseDefense, defenseScale * 0.78)
                : ScaleStat(baseDefense, defenseScale);
            var speed = ScaleStat(baseSpeed, speedScale);
            var isBoss = hpScale >= 1.7;

            return new MonsterTemplateEntity
            {
                MonsterId = monsterId,
                Name = name,
                Level = level,
                ExpRewardMin = 24 + level * 8 + level * level / 4 + (isBoss ? 55 : 0),
                ExpRewardMax = 34 + level * 9 + level * level / 3 + (isBoss ? 78 : 0),
                GoldRewardMin = 18 + level * 4 + level * level / 8 + (isBoss ? 18 : 0),
                GoldRewardMax = 28 + level * 5 + level * level / 7 + (isBoss ? 28 : 0),
                SkillIds = BuildMonsterSkills(style, isBoss),
                PassiveIds = [],
                ItemDrops = BuildItemDrops(itemDropIds, isBoss),
                EquipmentDrops = BuildEquipmentDrops(equipmentDropIds, isBoss),
                MinType1 = Math.Max(1, hp - Math.Max(18, hp / 12)),
                MaxType1 = hp + Math.Max(24, hp / 10),
                MinType2 = Math.Max(1, mp - Math.Max(10, mp / 12)),
                MaxType2 = mp + Math.Max(12, mp / 10),
                MinType3 = Math.Max(1, physicalAttack - Math.Max(2, physicalAttack / 10)),
                MaxType3 = physicalAttack + Math.Max(3, physicalAttack / 8),
                MinType4 = Math.Max(1, magicAttack - Math.Max(2, magicAttack / 10)),
                MaxType4 = magicAttack + Math.Max(3, magicAttack / 8),
                MinType5 = Math.Max(1, physicalDefense - Math.Max(2, physicalDefense / 10)),
                MaxType5 = physicalDefense + Math.Max(2, physicalDefense / 8),
                MinType6 = Math.Max(1, magicDefense - Math.Max(2, magicDefense / 10)),
                MaxType6 = magicDefense + Math.Max(2, magicDefense / 8),
                MinType7 = Math.Max(1, speed - 1),
                MaxType7 = speed + 1,
                MinType8 = 90,
                MaxType8 = 94,
                MinType9 = isBoss ? 4 : 2,
                MaxType9 = isBoss ? 8 : 5,
                MinType10 = isBoss ? 5 : 3,
                MaxType10 = isBoss ? 10 : 6,
                MinType11 = 150,
                MaxType11 = isBoss ? 170 : 160,
                MinType12 = 0,
                MaxType12 = isBoss ? 3 : 1,
                MinType13 = 0,
                MaxType13 = isBoss ? 3 : 1,
                MinType14 = 0,
                MaxType14 = 0,
                MinType15 = 0,
                MaxType15 = 0,
                Element = elementPool.Count == 1 ? elementPool[0] : Element.None,
                ElementPool = elementPool.ToList()
            };
        }

        /// <summary>
        /// 根据流派和首领标记生成怪物技能池。
        /// </summary>
        private static List<string> BuildMonsterSkills(CombatStyle style, bool isBoss)
        {
            var skills = style switch
            {
                CombatStyle.Physical => new List<string> { "1", "2" },
                CombatStyle.Magic => new List<string> { "3", "6" },
                _ => new List<string> { "1", "13" }
            };

            if (isBoss)
            {
                skills.Add(style == CombatStyle.Magic ? "11" : "7");
            }

            return skills;
        }

        /// <summary>
        /// 构造怪物的道具掉落表。
        /// </summary>
        private static List<DropItem> BuildItemDrops(IReadOnlyList<string> itemIds, bool isBoss)
        {
            var rates = isBoss
                ? new[] { 7800, 4200, 1800 }
                : new[] { 4200, 2400, 1000 };

            return itemIds
                .Select((itemId, index) => new DropItem
                {
                    ItemId = itemId,
                    Rate = rates[Math.Min(index, rates.Length - 1)]
                })
                .ToList();
        }

        /// <summary>
        /// 构造怪物的装备掉落表。
        /// </summary>
        private static List<DropEquipment> BuildEquipmentDrops(IReadOnlyList<string> equipmentIds, bool isBoss)
        {
            var rates = isBoss
                ? new[] { 1800, 800 }
                : new[] { 600, 200 };

            return equipmentIds
                .Select((equipmentId, index) => new DropEquipment
                {
                    EquipmentId = equipmentId,
                    Rate = rates[Math.Min(index, rates.Length - 1)]
                })
                .ToList();
        }

        /// <summary>
        /// 生成单个灵宠模板。
        /// </summary>
        private static PetTemplateEntity CreatePetTemplate(
            string templateId,
            string name,
            string description,
            PetType type,
            int initialQuality,
            int maxQuality,
            int baseAttack,
            int baseDefense,
            int baseHp,
            int baseMp,
            int baseSpeed,
            double growthRate,
            IReadOnlyList<string> skillIds)
        {
            var magicAttack = type == PetType.Support
                ? Math.Max(1, (int)Math.Round(baseAttack * 0.85, MidpointRounding.AwayFromZero))
                : Math.Max(1, (int)Math.Round(baseAttack * 0.58, MidpointRounding.AwayFromZero));
            var magicDefense = Math.Max(1, (int)Math.Round(baseDefense * 0.68, MidpointRounding.AwayFromZero));
            var dodgeMin = type == PetType.Balanced ? 3 : type == PetType.Attack ? 2 : 1;
            var dodgeMax = type == PetType.Balanced ? 6 : type == PetType.Attack ? 5 : 4;
            var critMin = type == PetType.Attack ? 4 : type == PetType.Support ? 3 : 2;
            var critMax = type == PetType.Attack ? 8 : type == PetType.Support ? 6 : 4;
            var comboMin = type == PetType.Attack ? 2 : 0;
            var comboMax = type == PetType.Attack ? 5 : 2;
            var counterMin = type == PetType.Defense ? 2 : 0;
            var counterMax = type == PetType.Defense ? 6 : 2;
            var armorBreakMin = type == PetType.Attack ? 2 : 0;
            var armorBreakMax = type == PetType.Attack ? 5 : 2;
            var bonusDamageMin = type == PetType.Attack ? 3 : 1;
            var bonusDamageMax = type == PetType.Attack ? 8 : 3;

            return new PetTemplateEntity
            {
                TemplateId = templateId,
                Name = name,
                Description = description,
                Type = type,
                InitialQualityMin = initialQuality,
                InitialQualityMax = initialQuality,
                MaxQuality = maxQuality,
                GrowthRateMin = growthRate,
                GrowthRateMax = growthRate,
                InitialSkillCount = Math.Min(2, skillIds.Count),
                MinType1 = Math.Max(1, baseHp - Math.Max(12, baseHp / 10)),
                MaxType1 = baseHp + Math.Max(18, baseHp / 10),
                MinType2 = Math.Max(1, baseMp - Math.Max(8, baseMp / 12)),
                MaxType2 = baseMp + Math.Max(10, baseMp / 10),
                MinType3 = Math.Max(1, baseAttack - Math.Max(2, baseAttack / 10)),
                MaxType3 = baseAttack + Math.Max(3, baseAttack / 8),
                MinType4 = Math.Max(1, magicAttack - Math.Max(2, magicAttack / 10)),
                MaxType4 = magicAttack + Math.Max(3, magicAttack / 8),
                MinType5 = Math.Max(1, baseDefense - Math.Max(2, baseDefense / 10)),
                MaxType5 = baseDefense + Math.Max(3, baseDefense / 8),
                MinType6 = Math.Max(1, magicDefense - Math.Max(2, magicDefense / 10)),
                MaxType6 = magicDefense + Math.Max(3, magicDefense / 8),
                MinType7 = Math.Max(1, baseSpeed - 2),
                MaxType7 = baseSpeed + 2,
                MinType8 = 90,
                MaxType8 = 94,
                MinType9 = dodgeMin,
                MaxType9 = dodgeMax,
                MinType10 = critMin,
                MaxType10 = critMax,
                MinType11 = 150,
                MaxType11 = 165,
                MinType12 = comboMin,
                MaxType12 = comboMax,
                MinType13 = counterMin,
                MaxType13 = counterMax,
                MinType14 = armorBreakMin,
                MaxType14 = armorBreakMax,
                MinType15 = bonusDamageMin,
                MaxType15 = bonusDamageMax,
                SkillIds = skillIds.ToList(),
                ObtainMethod = "使用宠物蛋孵化",
                IsTradable = false
            };
        }

        /// <summary>
        /// 生成单个副本模板。
        /// </summary>
        private static DungeonTemplateEntity CreateDungeonTemplate(
            string dungeonId,
            string name,
            string description,
            int recommendedLevel,
            int dailyLimit,
            string normalMapId,
            string fubenMapId,
            int requiredTeamSize)
        {
            return new DungeonTemplateEntity
            {
                DungeonId = dungeonId,
                Name = name,
                Description = description,
                RecommendedLevel = recommendedLevel,
                DailyLimit = dailyLimit,
                NormalMapId = normalMapId,
                FubenMapId = fubenMapId,
                RequiredTeamSize = requiredTeamSize
            };
        }

        /// <summary>
        /// 按倍率缩放数值并保证结果至少为 1。
        /// </summary>
        private static int ScaleStat(int value, double scale)
        {
            return Math.Max(1, (int)Math.Round(value * scale, MidpointRounding.AwayFromZero));
        }
    }
}
