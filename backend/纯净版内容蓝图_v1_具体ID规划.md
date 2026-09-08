# 纯净版内容蓝图 v1：具体 ID 与关联规划

## 0. 使用说明

本文件是《纯净版内容基线 v1 规划》的具体化版本，目的不是直接替换数据库，而是先固定以下内容：

- 新版本 ID 命名规则
- 地图数量、等级和功能
- 道具族及其模块用途
- 装备编号、部位和等级带
- 怪物类型与专属地图
- 掉落组和概率模板
- 炼丹、锻造、灵田、灵宠、五行、洗练、副本等模块的引用关系

确定本文件后，再制作一次性导入脚本，写入空数据库并导出基准库。

## 1. 统一 ID 规则

新版本不复用当前旧 ID，例如 `map_001`、`item_001`、`monster_001` 和旧装备数字 ID 全部废弃。

### 1.1 地图

```text
map_s01_main_01       普通主线地图
map_s01_ore_02        矿石材料地图
map_s01_wood_03       木材材料地图
map_s01_hide_05       皮革材料地图
map_s01_crystal_06    灵晶材料地图
map_s01_elite_08      精英地图
map_s01_boss_09       区域首领地图
fuben_s01_01          副本第一层
```

其中 `s01` 至 `s10` 表示等级阶段，地图类型不能只依赖 ID，数据库还应保存 `MapKind`、`Stage` 和 `IsMainRoute`。

### 1.2 道具

```text
itm_mat_ore_s01       阶段一矿石
itm_core_s01_common   阶段一普通专属核心
itm_boss_s01          阶段一首领材料
itm_pill_s01_break    阶段一突破丹
seed_s01_herb         阶段一灵草种子
itm_crop_s01_herb     阶段一灵草产物
itm_token_s01         阶段一副本凭证
```

### 1.3 怪物

```text
mon_s01_main_01       主线普通怪
mon_s01_ore_guardian   矿石图专属怪
mon_s01_elite_01       精英怪
mon_s01_boss           区域首领
mon_s01_dungeon_01    副本专属怪
```

### 1.4 装备

装备仍使用整数主键，但使用新编号区间：

```text
普通等级带：10001-12099
精英/副本专属：20001-20999
区域首领专属：30001-30999
终局特殊装备：40001-40999
```

普通装备编号公式：

```text
装备 ID = 10000 + (等级带序号 - 1) * 100 + 模板序号
```

例如 B01 的 22 个模板为 `10001-10022`，B02 为 `10101-10122`，B20 为 `11901-11922`，B21 的普通模板使用 `12001-12012`。

## 2. 等级阶段与地图总表

每个阶段 9 张普通地图：3 张主线图、4 张材料图、1 张精英图、1 张首领图。普通地图不使用 `NextMapId`，副本层使用 `NextMapId`。

| 阶段 | 等级范围 | 区域 | 主线地图 | 材料地图 | 精英图 | 首领图 | 副本层 |
|---|---:|---|---|---|---|---|---|
| S01 | 1-9 | 青岚边境 | `main_01/04/07` | `ore_02`、`wood_03`、`hide_05`、`crystal_06` | `elite_08` | `boss_09` | 3 |
| S02 | 10-19 | 赤砂岭 | `main_10/13/16` | `ore_11`、`wood_12`、`hide_14`、`crystal_15` | `elite_18` | `boss_19` | 3 |
| S03 | 20-29 | 雾隐泽 | `main_20/23/26` | `ore_21`、`wood_22`、`hide_24`、`crystal_25` | `elite_28` | `boss_29` | 3 |
| S04 | 30-39 | 炎脉谷 | `main_30/33/36` | `ore_31`、`wood_32`、`hide_34`、`crystal_35` | `elite_38` | `boss_39` | 3 |
| S05 | 40-49 | 雷鸣原 | `main_40/43/46` | `ore_41`、`wood_42`、`hide_44`、`crystal_45` | `elite_48` | `boss_49` | 3 |
| S06 | 50-59 | 寒魄海 | `main_50/53/56` | `ore_51`、`wood_52`、`hide_54`、`crystal_55` | `elite_58` | `boss_59` | 3 |
| S07 | 60-69 | 万木天境 | `main_60/63/66` | `ore_61`、`wood_62`、`hide_64`、`crystal_65` | `elite_68` | `boss_69` | 3 |
| S08 | 70-79 | 金阙荒原 | `main_70/73/76` | `ore_71`、`wood_72`、`hide_74`、`crystal_75` | `elite_78` | `boss_79` | 3 |
| S09 | 80-89 | 幽冥古域 | `main_80/83/86` | `ore_81`、`wood_82`、`hide_84`、`crystal_85` | `elite_88` | `boss_89` | 3 |
| S10 | 90-100 | 天门遗境 | `main_90/94/98` | `ore_91`、`wood_93`、`hide_95`、`crystal_97` | `elite_99` | `boss_100` | 3 |

完整地图 ID 生成规则为 `map_sXX_<kind>_<level>`。例如：

| 地图 ID | 推荐等级 | 类型 | 作用 |
|---|---:|---|---|
| `map_s01_main_01` | 1 | Main | 初始练级、经验和基础装备 |
| `map_s01_ore_02` | 2 | MaterialOre | 只产出 S01 矿石链 |
| `map_s01_wood_03` | 3 | MaterialWood | 只产出 S01 木材链 |
| `map_s01_hide_05` | 5 | MaterialHide | 只产出 S01 皮革链 |
| `map_s01_crystal_06` | 6 | MaterialCrystal | 只产出 S01 灵晶链 |
| `map_s01_elite_08` | 8 | Elite | 精英材料、洗练材料和中档装备 |
| `map_s01_boss_09` | 9 | Boss | 首领材料、图纸和高档装备 |

## 3. 副本规划

每个阶段 1 个副本、3 层副本地图，共 10 个副本和 30 层副本地图。

| 副本 | 推荐等级 | 队伍人数 | 入口普通图 | 副本层链 | 主要产出 |
|---|---:|---:|---|---|---|
| `dungeon_s01_trial` | 5 | 1 | `map_s01_main_04` | `fuben_s01_01 → 02 → 03` | `itm_token_s01`、技能书、B01/B02 装备 |
| `dungeon_s02_red_sand` | 12 | 2 | `map_s02_main_13` | `fuben_s02_01 → 02 → 03` | S02 材料、B03/B04 装备 |
| `dungeon_s03_mist` | 22 | 2 | `map_s03_main_23` | `fuben_s03_01 → 02 → 03` | S03 突破材料、套装部件 |
| `dungeon_s04_flame` | 32 | 2 | `map_s04_main_33` | `fuben_s04_01 → 02 → 03` | S04 火系材料、B07/B08 装备 |
| `dungeon_s05_thunder` | 42 | 3 | `map_s05_main_43` | `fuben_s05_01 → 02 → 03` | S05 雷系材料、洗练材料 |
| `dungeon_s06_frost` | 52 | 3 | `map_s06_main_53` | `fuben_s06_01 → 02 → 03` | S06 宠物材料、B11/B12 装备 |
| `dungeon_s07_wood` | 62 | 3 | `map_s07_main_63` | `fuben_s07_01 → 02 → 03` | S07 职业图纸、套装部件 |
| `dungeon_s08_gold` | 72 | 3 | `map_s08_main_73` | `fuben_s08_01 → 02 → 03` | S08 高级强化、传说装备 |
| `dungeon_s09_ghost` | 82 | 3 | `map_s09_main_83` | `fuben_s09_01 → 02 → 03` | S09 首领材料、终局前装备 |
| `dungeon_s10_heaven` | 92 | 3 | `map_s10_main_94` | `fuben_s10_01 → 02 → 03` | S10 终局图纸、Lv.100 装备 |

副本层必须只引用本副本的怪物，不能引用其他副本层的 `NextMapId`。副本凭证由本阶段材料图、精英图和副本前置层产出。

## 4. 道具总量与具体 ID 族

道具目标总量为 224 个：24 个共用功能道具 + 每阶段 20 个阶段道具。

### 4.1 共用功能道具：24 个

| ID | 类型 | 用途 |
|---|---|---|
| `itm_cons_hp_small` | Consumable | 回复固定生命 |
| `itm_cons_hp_medium` | Consumable | 回复中量生命 |
| `itm_cons_hp_large` | Consumable | 回复大量生命 |
| `itm_cons_mp_small` | Consumable | 回复固定法力 |
| `itm_cons_mp_medium` | Consumable | 回复中量法力 |
| `itm_cons_mp_large` | Consumable | 回复大量法力 |
| `itm_cons_revive` | Consumable | 战斗复活 |
| `itm_cons_attack` | Pill/Consumable | 临时攻击增益 |
| `itm_cons_defense` | Pill/Consumable | 临时防御增益 |
| `itm_cons_speed` | Pill/Consumable | 临时速度增益 |
| `itm_enhance_basic` | Material | 基础装备强化 |
| `itm_enhance_protect` | Material | 强化失败保护 |
| `itm_reroll_stone` | Material | 装备洗练 |
| `itm_reroll_lock` | Material | 洗练锁定词条 |
| `itm_quality_crystal` | Material | 品质重铸或终局强化 |
| `itm_pet_food_basic` | Material | 灵宠喂养 |
| `itm_pet_food_advanced` | Material | 高级灵宠喂养 |
| `itm_pet_evolution_stone` | Material | 灵宠进化 |
| `itm_pet_skill_core` | Material | 灵宠技能培养 |
| `itm_chest_gold_basic` | Chest | 基础金币箱 |
| `itm_chest_stage` | Chest | 阶段奖励箱 |
| `itm_key_common` | Material | 普通宝箱钥匙 |
| `itm_key_boss` | Material | 首领宝箱钥匙 |
| `itm_skillbook_basic` | SkillBook | 基础技能书 |

### 4.2 每阶段固定 20 个道具

每个阶段将生成以下 20 个 ID，`s01` 替换为对应阶段编号：

| 数量 | ID 模板 | 类型 | 主要用途 | 主要来源 |
|---:|---|---|---|---|
| 1 | `itm_mat_ore_s01` | Material | 物理武器、法宝、五行升级 | `map_s01_ore_02` 专属怪 |
| 1 | `itm_mat_wood_s01` | Material | 武器、法器、部分配方 | `map_s01_wood_03` 专属怪 |
| 1 | `itm_mat_hide_s01` | Material | 防具、宠物培养 | `map_s01_hide_05` 专属怪 |
| 1 | `itm_mat_crystal_s01` | Material | 法器、饰品、宝石 | `map_s01_crystal_06` 专属怪 |
| 1 | `itm_core_s01_common` | Material | 阶段炼丹、锻造催化剂 | 指定材料怪唯一掉落 |
| 1 | `itm_core_s01_boss` | Material | 首领装备和终局配方 | 区域首领唯一掉落 |
| 1 | `itm_essence_s01` | Material | 突破、五行、装备进阶 | 精英和首领 |
| 1 | `itm_boss_s01` | Material | 区域专属装备、终极丹方 | 区域首领 |
| 1 | `itm_crop_s01_herb` | Material | 回复/修为丹药 | `seed_s01_herb` 收获 |
| 1 | `itm_crop_s01_flower` | Material | 突破/增益丹药 | `seed_s01_flower` 收获 |
| 1 | `seed_s01_herb` | Seed | 灵田种植 | 材料图、任务、商店 |
| 1 | `seed_s01_flower` | Seed | 灵田种植 | 精英图、任务、副本 |
| 1 | `itm_pill_s01_break` | Pill | 阶段突破辅助 | 炼丹 |
| 1 | `itm_enhance_s01` | Material | 当前阶段装备强化 | 精英、材料图、锻造 |
| 1 | `itm_reroll_s01` | Material | 当前阶段洗练消耗 | 精英、首领、兑换 |
| 1 | `itm_token_s01` | Material | 当前副本进入凭证 | 副本前置怪、首领 |
| 1 | `itm_skillbook_s01` | SkillBook | 阶段技能解锁 | 副本首领、宝箱 |
| 1 | `itm_blueprint_s01` | Material/Quest | 阶段装备图纸 | 首领、精英、成就 |
| 1 | `itm_pet_s01_fragment` | Material | 灵宠召唤或进化 | 精英、首领、副本 |

因此阶段 S01-S10 分别使用 `s01` 至 `s10`，合计 200 个阶段道具。

### 4.3 道具的模块归属

| 模块 | 直接消耗或产出 | 备注 |
|---|---|---|
| 战斗 | 共用 HP/MP/复活道具、阶段药剂 | 消耗效果必须读取配置，不再按旧 ID 判断 |
| 装备强化 | `itm_enhance_basic`、`itm_enhance_sXX`、`itm_enhance_protect` | 按装备等级选择阶段材料 |
| 装备洗练 | `itm_reroll_stone`、`itm_reroll_lock`、`itm_reroll_sXX` | 洗练材料与强化材料分离 |
| 锻造 | 矿石、木材、皮革、灵晶、核心、图纸 | 每个等级带生成对应配方 |
| 炼丹 | `itm_crop_sXX_*`、`itm_core_sXX_*`、`itm_boss_sXX` | 每阶段至少 4 个丹方 |
| 灵田 | `seed_sXX_*` → `itm_crop_sXX_*` | 每阶段至少 2 个种子/作物 |
| 灵宠 | 宠物食物、进化石、技能核心、阶段碎片 | 宠物蛋和碎片分布在精英/首领/副本 |
| 五行/聚灵阵 | 四类基础材料、阶段精魄 | 不能一直使用同一个低阶材料 |
| 副本 | `itm_token_sXX`、技能书、图纸、宝箱 | 每个副本使用自己的凭证 |
| 商店 | 消耗品、基础材料、部分种子 | 不出售首领专属材料和毕业装备 |
| 任务/成就/签到 | 药品、材料、图纸、宝箱 | 奖励 ID 必须通过引用校验 |
| 宗门/世界 Boss/塔 | 阶段精魄、特殊材料、装备 | 允许绑定奖励，不能引用旧 ID |

## 5. 装备模板矩阵

### 5.1 每个普通等级带的 22 个模板

| 模板序号 | 部位 | 流派 | 武器类别 |
|---:|---|---|---|
| 01 | Weapon | Physical | Sword |
| 02 | Weapon | Physical | Blade |
| 03 | Weapon | Physical | Axe |
| 04 | Weapon | Physical | Spear |
| 05 | Weapon | Magic | Qin |
| 06 | Weapon | Magic | Chess |
| 07 | Weapon | Magic | Book |
| 08 | Weapon | Magic | Brush |
| 09 | Helmet | Physical | None |
| 10 | Helmet | Magic | None |
| 11 | Armor | Physical | None |
| 12 | Armor | Magic | None |
| 13 | Pants | Physical | None |
| 14 | Pants | Magic | None |
| 15 | Boots | Physical | None |
| 16 | Boots | Magic | None |
| 17 | Necklace | Physical | None |
| 18 | Necklace | Magic | None |
| 19 | Ring | Physical | None |
| 20 | Ring | Magic | None |
| 21 | Treasure | Physical | None |
| 22 | Treasure | Magic | None |

### 5.2 普通等级带

| 带次 | 装备等级 | 普通模板 ID 区间 | 基础品质 | 主来源 |
|---|---:|---|---|---|
| B01 | 1 | 10001-10022 | Common | 主线 S01 |
| B02 | 5 | 10101-10122 | Common | 主线/试炼副本 |
| B03 | 10 | 10201-10222 | Uncommon | 主线 S02 |
| B04 | 15 | 10301-10322 | Uncommon | 主线/副本 S02 |
| B05 | 20 | 10401-10422 | Rare | 主线 S03 |
| B06 | 25 | 10501-10522 | Rare | 主线/副本 S03 |
| B07 | 30 | 10601-10622 | Rare | 主线 S04 |
| B08 | 35 | 10701-10722 | Epic | 主线/副本 S04 |
| B09 | 40 | 10801-10822 | Epic | 主线 S05 |
| B10 | 45 | 10901-10922 | Epic | 主线/副本 S05 |
| B11 | 50 | 11001-11022 | Epic | 主线 S06 |
| B12 | 55 | 11101-11122 | Epic | 主线/副本 S06 |
| B13 | 60 | 11201-11222 | Legendary | 主线 S07 |
| B14 | 65 | 11301-11322 | Legendary | 主线/副本 S07 |
| B15 | 70 | 11401-11422 | Legendary | 主线 S08 |
| B16 | 75 | 11501-11522 | Legendary | 主线/副本 S08 |
| B17 | 80 | 11601-11622 | Legendary | 主线 S09 |
| B18 | 85 | 11701-11722 | Legendary | 主线/副本 S09 |
| B19 | 90 | 11801-11822 | Legendary | 主线 S10 |
| B20 | 95 | 11901-11922 | Legendary | 主线/副本 S10 |
| B21 | 100 | 12001-12012 | Legendary | 终局首领/终局副本 |

B21 只制作 12 个终局普通模板，减少 Lv.100 重复装备；另外使用首领专属装备补足终局选择。

### 5.3 专属装备

| ID 区间 | 数量目标 | 来源 | 设计方式 |
|---|---:|---|---|
| 20001-20080 | 80 | 精英和普通副本 | 每阶段 8 个，突出流派或特殊部位 |
| 30001-30060 | 60 | 区域首领 | 每阶段 6 个，首领专属，不进入普通地图掉落 |
| 40001-40024 | 24 | S10 终局副本/世界 Boss | 终局套装和特殊法宝 |

装备总量目标：

```text
普通装备 452 个
精英/副本装备 80 个
区域首领装备 60 个
终局特殊装备 24 个
合计 616 个
```

装备掉落只允许掉落装备模板 ID；装备实例属性仍由现有 `MinType/MaxType` 随机生成。

## 6. 怪物模板规划

每阶段 16 个怪物模板，共约 160 个。

| 怪物组 | 每阶段数量 | ID 模板 | 绑定地图 |
|---|---:|---|---|
| 主线普通怪 | 6 | `mon_s01_main_01` 至 `06` | 3 张主线图 |
| 材料专属怪 | 4 | `mon_s01_ore_guardian`、`wood_guardian`、`hide_guardian`、`crystal_guardian` | 4 张材料图，各自唯一 |
| 精英怪 | 2 | `mon_s01_elite_01`、`mon_s01_elite_02` | 精英图、副本 |
| 区域首领 | 1 | `mon_s01_boss` | 首领图、对应副本终层 |
| 副本专属怪 | 3 | `mon_s01_dungeon_01/02/boss` | 本阶段副本层 |

### 6.1 十个阶段的怪物主题

| 阶段 | 主线怪主题 | 矿石专属怪 | 木材专属怪 | 皮革专属怪 | 灵晶专属怪 | 区域首领主题 |
|---|---|---|---|---|---|---|
| S01 | 兔妖、狸妖、雾灵、青狼、竹甲、风鸦 | 赤砂矿兽 | 青竹木灵 | 斑纹猎兽 | 萤晶蝶 | 边境岩蛛王 |
| S02 | 沙盗、赤蝎、砂灵、铁傀、炎鸦、岭狼 | 赤砂甲虫 | 枯木妖 | 赤鬃蛮兽 | 砂海晶灵 | 赤砂炎君 |
| S03 | 泽鳄、雾蛙、沼牛、泽巫、藤妖、黑鳞兽 | 雾铁鳞兽 | 雾藤树妖 | 泽地鳄王 | 雾晶咒灵 | 雾隐泽主 |
| S04 | 炎狼、火偶、焰蝶、熔兽、火鸦、赤甲兵 | 熔岩矿魔 | 焚木妖 | 炎鳞兽 | 火晶灵 | 炎脉焚天兽 |
| S05 | 雷骑、雷猿、电鸦、金甲、风猎、鸣蛇 | 雷纹矿兽 | 雷击古木灵 | 雷原猎王 | 雷晶术灵 | 九霄雷君 |
| S06 | 冰鱼、寒蛟、雪狐、霜甲、海巫、冰魄灵 | 寒铁冰兽 | 寒潮枯木灵 | 冰原雪兽 | 寒晶魅灵 | 寒魄海后 |
| S07 | 木灵、花妖、藤卫、古鹿、青鸾、森魈 | 天木矿卫 | 万年木灵 | 森罗兽王 | 碧晶花灵 | 万木天尊 |
| S08 | 金甲、荒狼、砂龙、古兵、金翅、荒灵 | 金阙矿皇 | 荒原灵木 | 金皮荒兽 | 曜金晶灵 | 金阙荒皇 |
| S09 | 魇兽、鬼将、幽蝶、魂鸦、冥骑、骨灵 | 幽冥魂矿兽 | 鬼木妖 | 冥甲兽王 | 幽晶魂灵 | 幽冥古主 |
| S10 | 天门卫、仙傀、星兽、天鸾、虚灵、劫兵 | 天门神矿兽 | 建木神灵 | 天门圣兽 | 太虚晶灵 | 天门劫主 |

同一阶段的材料专属怪只能出现在对应材料图。其核心物品必须只由该怪物掉落；区域首领核心只由区域首领掉落。

## 7. 地图—怪物分配

每阶段使用以下固定分配模板，`s01` 替换为阶段编号。

| 地图 | 刷怪规则 |
|---|---|
| `map_s01_main_01` | `main_01` 权重 55、`main_02` 权重 45，数量 1-2 |
| `map_s01_main_04` | `main_03` 权重 45、`main_04` 权重 35、`main_05` 权重 20，数量 1-2 |
| `map_s01_main_07` | `main_04` 权重 25、`main_05` 权重 35、`main_06` 权重 40，数量 2-3 |
| `map_s01_ore_02` | `ore_guardian` 权重 100，数量 1-2 |
| `map_s01_wood_03` | `wood_guardian` 权重 100，数量 1-2 |
| `map_s01_hide_05` | `hide_guardian` 权重 100，数量 1-2 |
| `map_s01_crystal_06` | `crystal_guardian` 权重 100，数量 1-2 |
| `map_s01_elite_08` | `elite_01` 权重 60、`elite_02` 权重 40，数量 1-2 |
| `map_s01_boss_09` | `elite_01` 权重 35、`elite_02` 权重 35、`boss` 权重 30，数量 1-2 |

主线地图只放主线普通怪；材料地图只放对应材料专属怪；首领地图可以放本阶段精英护卫，但区域首领必须是独立刷怪规则。

## 8. 掉落组与概率模板

当前代码的“所有道具和装备共用一个池、一次最多抽一个内容”不足以支持本规划。新版本数据采用独立掉落组。

### 8.1 普通主线怪

| 掉落组 | 判定 | 内容 | 数量/概率 |
|---|---|---|---|
| Gold | 必给 | 金币 | 按怪物等级区间 |
| StageMaterial | 必给 | 本地图阶段材料 | 1-3，100% |
| Consumable | 独立 | 当前阶段药品 | 1，20% |
| CommonCore | 独立 | 阶段普通核心 | 1，2% |
| Equipment | 独立 | 当前或上一装备带普通品质 | 1，3% |

### 8.2 材料图专属怪

| 掉落组 | 判定 | 内容 | 数量/概率 |
|---|---|---|---|
| Gold | 必给 | 金币 | 按怪物等级区间 |
| ExclusiveMaterial | 必给 | 本图唯一基础材料 | 1-4，100% |
| ExclusiveCore | 独立 | 本怪唯一核心 | 1，8% |
| Enhancement | 独立 | `itm_enhance_sXX` | 1，12% |
| Seed | 独立 | 对应阶段种子 | 1，5% |
| Equipment | 独立 | 当前装备带普通品质 | 1，2% |

### 8.3 精英怪

| 掉落组 | 判定 | 内容 | 数量/概率 |
|---|---|---|---|
| Gold | 必给 | 金币 | 普通怪的 1.5-2 倍 |
| StageEssence | 必给 | `itm_essence_sXX` | 1-2，100% |
| Enhancement | 独立 | `itm_enhance_sXX` | 1-3，45% |
| Reroll | 独立 | `itm_reroll_sXX` | 1，15% |
| Blueprint | 独立 | `itm_blueprint_sXX` | 1，3% |
| Equipment | 独立 | 当前装备带优秀/精良品质 | 1，12% |
| PetFragment | 独立 | `itm_pet_sXX_fragment` | 1，8% |

### 8.4 区域首领

| 掉落组 | 判定 | 内容 | 数量/概率 |
|---|---|---|---|
| Gold | 必给 | 金币 | 精英怪的 2-3 倍 |
| BossMaterial | 必给 | `itm_boss_sXX` | 1-2，100% |
| BossCore | 必给 | `itm_core_sXX_boss` | 1，100% |
| StageEssence | 必给 | `itm_essence_sXX` | 2-4，100% |
| Blueprint | 独立 | `itm_blueprint_sXX` | 1，20% |
| BossEquipment | 独立 | `300XX` 专属装备 | 1，35% |
| StageEquipment | 独立 | 当前装备带精良/史诗/传说品质 | 1，25% |
| PetFragment | 独立 | `itm_pet_sXX_fragment` | 1-3，15% |
| DungeonToken | 独立 | `itm_token_sXX` | 1，50% |

### 8.5 副本怪和副本首领

副本不复用普通地图掉落组：

- 副本普通怪：副本凭证 20%、阶段材料 30%、技能书碎片 5%、当前装备 5%。
- 副本精英：阶段精魄 100%、强化材料 50%、副本装备 15%、图纸 8%。
- 副本首领：副本凭证 100%、阶段首领材料 100%、副本专属装备 30%、技能书 20%、宠物碎片 15%。

掉落概率是单个掉落组独立判定概率，不再将所有内容相加后只抽一件。

## 9. 阶段材料用途闭环

每个阶段使用同一套用途结构，避免材料堆积：

```text
矿石 ─┐
木材 ─┼→ 锻造武器/法宝
灵晶 ─┘

皮革 ─┐
核心 ─┼→ 锻造防具/宠物培养
精魄 ─┘

灵田产物 ─┐
普通核心 ─┼→ 炼丹
首领材料 ─┘

四类基础材料 + 阶段精魄 → 五行/聚灵阵升级
强化材料 → 装备强化
洗练材料 → 装备洗练
图纸 + 首领材料 → 专属装备
```

### 9.1 每阶段锻造配方最低要求

- 8 个武器配方
- 8 个防具配方
- 6 个饰品/法宝配方
- 1 个阶段专属装备配方组

普通装备每个模板生成 1 个 `forge_<equipmentId>` 配方，共约 452 个；首领专属装备只通过图纸和首领材料制作。

### 9.2 每阶段炼丹配方最低要求

| 丹方 | 主要材料 | 作用 |
|---|---|---|
| SXX 聚气丹 | 阶段草药 + 灵泉类材料 | 修为经验 |
| SXX 破障丹 | 阶段花材 + 阶段精魄 | 突破成功率 |
| SXX 护脉丹 | 阶段核心 + 阶段草药 | 基础防御/生命 |
| SXX 回元丹 | 阶段草药 + 阶段灵晶 | HP/MP 恢复 |

因此炼丹配方目标至少 40 个，作物模板至少 20 个。

## 10. 数据库字段前置调整

在录入新数据前，建议先完成以下结构调整，否则很多规划只能靠 ID 猜测：

### MapTemplates

增加或等价实现：

- `MapKind`：Main、MaterialOre、MaterialWood、MaterialHide、MaterialCrystal、Elite、Boss、DungeonLayer、Event
- `Stage`
- `UnlockLevel`
- `IsMainRoute`
- `DropProfileId`（如果地图需要额外掉落）

### MonsterTemplates

增加或等价实现：

- `MonsterKind`：Normal、MaterialGuardian、Elite、Boss、Dungeon
- `Stage`
- `ExclusiveMapId`
- `DropProfileId`

### 掉落配置

建议新增独立掉落表，或至少把 JSON 改成支持：

- 掉落组 ID
- 组类型
- 目标物品/装备 ID
- 最小数量
- 最大数量
- 概率
- 是否必掉
- 是否绑定
- 保底计数（后续可选）

### ItemTemplates

增加通用使用效果配置，替代 `InventoryService` 对具体旧 ID 的判断：

- `UseEffectType`
- `EffectValue`
- `EffectValue2`
- `DurationMinutes`
- `MaxUsageCount`

## 11. 其他模块的基线引用规则

| 模块 | 基线要求 |
|---|---|
| 等级成长 | 保留 Lv.1-100 和当前运行时加载方式，但数值可单独调整 |
| 技能/Buff | 至少保留基础攻击、治疗、增益、减益和怪物技能所需的最小集合 |
| 商店 | 只引用共用消耗品、阶段基础材料、低品质装备和种子 |
| 新手礼包 | 只提供 S01 基础装备/药品/基础材料，不直接给高阶宠物、首领材料或终局资源 |
| 任务 | 主线任务按区域解锁，目标只引用新版地图和怪物 ID |
| 成就 | 奖励只引用新版道具/装备，删除旧 `item_003` 等引用 |
| 签到/兑换码 | 可以奖励共用道具和低阶段材料，不能成为高阶材料的主要来源 |
| 灵田 | 20 个阶段作物模板和 20 个阶段种子，种子与产物一一对应 |
| 炼丹 | 40 个最低配方，按阶段和职业等级解锁 |
| 锻造 | 普通装备约 452 个配方，首领装备由图纸控制 |
| 五行/聚灵阵 | 使用当前阶段材料，不再固定使用低阶四材料 |
| 灵宠 | 宠物蛋/碎片分布在精英、首领、副本，不全部塞进礼包 |
| 副本 | 10 个副本、30 层地图，每个副本独立凭证和事件组 |
| 宗门 | 宗门奖励可引用阶段精魄、特殊材料和绑定装备 |
| 世界 Boss | 只引用 S10 终局特殊装备和 `itm_quality_crystal` 等终局道具 |

## 12. 入库前验收标准

### ID 与引用

- 所有 ID 唯一。
- 所有地图刷怪 ID 存在。
- 所有怪物掉落的道具和装备 ID 存在。
- 所有任务、成就、商店、礼包、签到、兑换码奖励 ID 存在。
- 所有配方材料和产物 ID 存在。
- 所有种子都有作物模板。
- 所有宠物蛋或碎片都有宠物来源。

### 等级与地图

- 主线地图推荐等级单调递增。
- 材料图不会被自动推荐逻辑当成主线图。
- 普通地图 `NextMapId` 为空。
- 副本链全部是 `fuben_` 地图且不跨副本。
- 副本层数、入口和每日次数配置完整。

### 掉落与经济

- 每张材料图至少有一个独占怪物。
- 每个阶段四类基础材料都有稳定来源。
- 每个阶段的重要材料都有至少一个消耗点。
- 普通怪不会掉落终局装备。
- 首领专属装备不会出现在普通怪掉落池。
- 每个独立掉落组概率在 0-100% 范围内。
- 需要必掉的材料不能依赖单一随机池。

### 纯净基线

- 数据库中没有旧版 `map_001`、`item_001`、`item_003`、`monster_001` 等内容。
- 数据库中没有旧版本的旧地图链和旧副本事件。
- 项目启动只读取数据库，不执行任何 Seed、Sync、Repair 或旧 ID 回填。
- 启动前后核心模板表行数一致。
- 导出的数据库和 SQL 可以在空环境中还原并正常启动。

## 13. 下一步实施顺序

1. 评审本文件的内容规模和 ID 规则。
2. 先修改地图类型、怪物类型和掉落组结构。
3. 生成 224 个道具模板及扩展配置。
4. 生成 616 个装备模板和普通装备配方。
5. 生成约 160 个怪物模板及掉落组。
6. 生成 90 张普通地图和 30 张副本层地图。
7. 生成炼丹、灵田、灵宠、五行、洗练、任务、商店等引用配置。
8. 在空数据库导入并执行完整性检查。
9. 清理所有玩家和运行数据。
10. 导出 `game-baseline.db` 与 `game-baseline.sql`。
11. 删除项目启动种子和旧内容 fallback。
