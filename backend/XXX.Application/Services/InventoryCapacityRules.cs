using SqlSugar;
using XXX.Entity;

namespace XXX.Application.Services;

/// <summary>
/// 统一的玩家背包容量规则。
/// 道具和装备是两类独立背包，各自最多占用 100 格；可堆叠道具按一条背包记录占一格。
/// 已穿戴装备不占用装备背包格子。
/// </summary>
public static class InventoryCapacityRules
{
    public const int DefaultCapacity = 100;
    public const int MaximumCapacity = 200;
    public const int ExpansionSlots = 5;

    public static async Task<int> GetItemCapacityAsync(ISqlSugarClient db, string playerId)
    {
        var player = await db.Queryable<UserEntity>()
            .Where(user => user.GID == playerId && !user.IsDeleted)
            .Select(user => new { user.ItemInventoryCapacity })
            .FirstAsync();

        return Math.Clamp(player?.ItemInventoryCapacity ?? DefaultCapacity, DefaultCapacity, MaximumCapacity);
    }

    public static async Task<int> GetEquipmentCapacityAsync(ISqlSugarClient db, string playerId)
    {
        var player = await db.Queryable<UserEntity>()
            .Where(user => user.GID == playerId && !user.IsDeleted)
            .Select(user => new { user.EquipmentInventoryCapacity })
            .FirstAsync();

        return Math.Clamp(player?.EquipmentInventoryCapacity ?? DefaultCapacity, DefaultCapacity, MaximumCapacity);
    }

    public static async Task<bool> HasItemSlotsAsync(
        ISqlSugarClient db,
        string playerId,
        int additionalSlots = 1)
    {
        if (additionalSlots <= 0)
        {
            return true;
        }

        var usedSlots = await db.Queryable<InventoryItemEntity>()
            .Where(item => item.PlayerId == playerId)
            .CountAsync();
        var capacity = await GetItemCapacityAsync(db, playerId);

        return usedSlots + additionalSlots <= capacity;
    }

    public static async Task<bool> HasEquipmentSlotsAsync(
        ISqlSugarClient db,
        string playerId,
        int additionalSlots = 1)
    {
        if (additionalSlots <= 0)
        {
            return true;
        }

        var usedSlots = await db.Queryable<EquipmentInstanceEntity>()
            .Where(equipment => equipment.PlayerId == playerId && !equipment.IsEquipped)
            .CountAsync();
        var capacity = await GetEquipmentCapacityAsync(db, playerId);

        return usedSlots + additionalSlots <= capacity;
    }
}
