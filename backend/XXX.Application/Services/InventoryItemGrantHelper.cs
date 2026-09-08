using SqlSugar;
using XXX.Entity;

namespace XXX.Application.Services;

internal static class InventoryItemGrantHelper
{
    public static async Task<InventoryItemEntity> AddOrMergeAsync(
        ISqlSugarClient db,
        string playerId,
        string itemId,
        int quantity,
        string fullMessage)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new InvalidOperationException("玩家编号不能为空。");
        }

        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new InvalidOperationException("道具编号不能为空。");
        }

        if (quantity <= 0)
        {
            throw new InvalidOperationException("道具数量必须大于 0。");
        }

        var rows = await db.Queryable<InventoryItemEntity>()
            .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
            .OrderByDescending(item => item.IsLocked)
            .OrderBy(item => item.Id)
            .ToListAsync();

        var keeper = rows.FirstOrDefault();
        if (keeper == null)
        {
            var currentSlotCount = await db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId)
                .CountAsync();
            var capacity = await InventoryCapacityRules.GetItemCapacityAsync(db, playerId);
            if (currentSlotCount >= capacity)
            {
                throw new InvalidOperationException(fullMessage);
            }

            keeper = new InventoryItemEntity
            {
                PlayerId = playerId,
                ItemId = itemId,
                Quantity = quantity,
                IsLocked = false,
                AcquiredTime = DateTime.Now
            };
            await db.Insertable(keeper).ExecuteCommandAsync();
            return keeper;
        }

        keeper.Quantity = checked(rows.Sum(item => item.Quantity) + quantity);
        keeper.IsLocked = rows.Any(item => item.IsLocked);
        keeper.AcquiredTime = rows.Min(item => item.AcquiredTime);
        await db.Updateable(keeper).ExecuteCommandAsync();

        foreach (var duplicate in rows.Skip(1))
        {
            await db.Deleteable<InventoryItemEntity>()
                .Where(item => item.Id == duplicate.Id)
                .ExecuteCommandAsync();
        }

        return keeper;
    }
}