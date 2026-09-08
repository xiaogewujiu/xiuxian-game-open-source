"""按地图类型与内容阶段，将怪物共享掉落池调整到 20%～40%区间。

本脚本只修改当前运行数据库 MonsterTemplates 的三类掉落 Rate：
- 保持每条道具、装备、图鉴之间的原始相对比例；
- 每个怪物根据其实际关联地图得到目标总权重；
- 目标总权重分布在 2000～4000，不统一为同一个值；
- 不修改装备模板、地图刷怪规则、战斗代码或接口。
"""
from __future__ import annotations

import json
import shutil
import sqlite3
from collections import defaultdict
from pathlib import Path

DB_PATH = Path("XXX.WebApi/Data/game-dev.db")
BACKUP_PATH = Path(".hermes/game-dev.before-drop-rate-gradient-20260904.db")


def array(value: str | None) -> list[dict]:
    """把数据库 JSON 字段读取为对象数组。"""
    try:
        data = json.loads(value or "[]")
    except json.JSONDecodeError:
        return []
    return data if isinstance(data, list) else []


def stage_from_map(map_id: str) -> int:
    """从地图编号读取内容阶段。"""
    try:
        return int(map_id.split("_")[1][1:])
    except (IndexError, ValueError):
        return 1


def map_target(map_id: str) -> int:
    """按地图类型和阶段返回单只怪物目标掉落总权重。"""
    stage = stage_from_map(map_id)
    progress = min(stage - 1, 9)
    if map_id.startswith("fuben_"):
        layer = int(map_id.split("_")[-1])
        return min(4000, 2600 + progress * 120 + max(0, layer - 1) * 300)
    if "_boss_" in map_id:
        return min(4000, 3300 + progress * 80)
    if "_elite_" in map_id:
        return min(4000, 3000 + progress * 90)
    if "_crystal_" in map_id or "_ore_" in map_id:
        return min(4000, 2500 + progress * 100)
    if "_wood_" in map_id or "_hide_" in map_id:
        return min(4000, 2300 + progress * 100)
    if "_main_01" in map_id:
        return min(3200, 2000 + progress * 120)
    if "_main_04" in map_id:
        return min(4000, 2600 + progress * 140)
    if "_main_07" in map_id:
        return min(4000, 3000 + progress * 110)
    return min(4000, 2400 + progress * 100)


def scale_rows(rows: list[dict], target: int) -> None:
    """按原始正数权重比例缩放掉落条目，并补齐整数余数。"""
    positive = [row for row in rows if int(row.get("Rate", 0) or 0) > 0]
    original_total = sum(int(row.get("Rate", 0) or 0) for row in positive)
    if original_total <= 0:
        return
    assigned = 0
    for index, row in enumerate(positive):
        if index == len(positive) - 1:
            new_rate = target - assigned
        else:
            new_rate = int(int(row.get("Rate", 0) or 0) * target / original_total)
            assigned += new_rate
        row["Rate"] = max(0, new_rate)


def main() -> None:
    """备份当前数据库、按怪物地图来源设置梯度掉落率并校验。"""
    if not DB_PATH.exists():
        raise SystemExit(f"数据库不存在：{DB_PATH}")
    shutil.copy2(DB_PATH, BACKUP_PATH)

    connection = sqlite3.connect(DB_PATH)
    connection.row_factory = sqlite3.Row
    try:
        monster_rows = {
            row["MonsterId"]: {
                "items": array(row["ItemDropsJson"]),
                "equipment": array(row["EquipmentDropsJson"]),
                "collections": array(row["CollectionDropsJson"]),
            }
            for row in connection.execute("SELECT MonsterId, ItemDropsJson, EquipmentDropsJson, CollectionDropsJson FROM MonsterTemplates")
        }
        map_monsters = {
            row["MapId"]: [
                str(rule.get("MonsterTemplateId", "")).strip()
                for rule in array(row["SpawnRulesJson"])
                if str(rule.get("MonsterTemplateId", "")).strip()
            ]
            for row in connection.execute("SELECT MapId, SpawnRulesJson FROM MapTemplates")
        }

        monster_targets: defaultdict[str, list[int]] = defaultdict(list)
        for map_id, monster_ids in map_monsters.items():
            for monster_id in monster_ids:
                if monster_id in monster_rows:
                    monster_targets[monster_id].append(map_target(map_id))

        target_by_monster = {
            monster_id: max(targets) if targets else 2000
            for monster_id, targets in monster_targets.items()
        }
        # 未被地图引用的怪物保留最低目标，避免后台或其他入口的配置出现零权重。
        for monster_id in monster_rows:
            target_by_monster.setdefault(monster_id, 2000)

        connection.execute("BEGIN")
        for monster_id, groups in monster_rows.items():
            all_rows = [*groups["items"], *groups["equipment"], *groups["collections"]]
            scale_rows(all_rows, target_by_monster[monster_id])
            item_count = len(groups["items"])
            equipment_count = len(groups["equipment"])
            groups["items"] = all_rows[:item_count]
            groups["equipment"] = all_rows[item_count:item_count + equipment_count]
            groups["collections"] = all_rows[item_count + equipment_count:]
            connection.execute(
                "UPDATE MonsterTemplates SET ItemDropsJson = ?, EquipmentDropsJson = ?, CollectionDropsJson = ? WHERE MonsterId = ?",
                (
                    json.dumps(groups["items"], ensure_ascii=False, separators=(",", ":")),
                    json.dumps(groups["equipment"], ensure_ascii=False, separators=(",", ":")),
                    json.dumps(groups["collections"], ensure_ascii=False, separators=(",", ":")),
                    monster_id,
                ),
            )
        connection.commit()

        totals = {}
        for row in connection.execute("SELECT MonsterId, ItemDropsJson, EquipmentDropsJson, CollectionDropsJson FROM MonsterTemplates"):
            total = sum(
                int(drop.get("Rate", 0) or 0)
                for value in (row["ItemDropsJson"], row["EquipmentDropsJson"], row["CollectionDropsJson"])
                for drop in array(value)
                if int(drop.get("Rate", 0) or 0) > 0
            )
            totals[row["MonsterId"]] = total
        invalid = {monster_id: total for monster_id, total in totals.items() if total < 2000 or total > 4000}
        print(json.dumps({
            "database": str(DB_PATH),
            "backup": str(BACKUP_PATH),
            "monster_count": len(totals),
            "target_weight_min": min(totals.values()),
            "target_weight_max": max(totals.values()),
            "target_weight_distribution": {str(value): list(totals.values()).count(value) for value in sorted(set(totals.values()))},
            "invalid_monsters": invalid,
        }, ensure_ascii=False, indent=2))
        if invalid:
            raise SystemExit("存在超出 2000～4000 区间的怪物掉落池")
    finally:
        connection.close()


if __name__ == "__main__":
    main()
