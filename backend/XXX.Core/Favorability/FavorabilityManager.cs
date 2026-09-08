using System.Text.Json;
using XXX.Entity;

namespace XXX.Favorability
{
    /// <summary>
    /// 好感度赠送结果。
    /// </summary>
    public class FavorabilityGiftResult
    {
        public bool Success { get; set; }
        public int NewValue { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelColor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 好感度等级信息。
    /// </summary>
    public class FavorabilityLevelInfo
    {
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// 好感度管理器。
    /// 负责好感度相关的核心业务逻辑。
    /// </summary>
    public class FavorabilityManager
    {
        /// <summary>
        /// 校验赠送请求。
        /// </summary>
        public static (bool valid, string error, ItemFavorabilityGiftConfig? config) ValidateGift(
            ItemTable? itemTemplate, int inventoryCount, Dictionary<string, int> todayGifts, string itemId)
        {
            if (itemTemplate == null)
                return (false, "道具不存在", null);

            if (itemTemplate.FavorabilityGiftConfig == null)
                return (false, "该道具不是好感度道具", null);

            var config = itemTemplate.FavorabilityGiftConfig;

            if (!config.CanGift)
                return (false, "该道具不可赠送", null);

            if (inventoryCount < 1)
                return (false, "道具数量不足", null);

            int usedToday = todayGifts.TryGetValue(itemId, out var count) ? count : 0;
            if (usedToday >= config.DailyLimit)
                return (false, $"该道具今日赠送次数已达上限（{config.DailyLimit}次）", null);

            return (true, string.Empty, config);
        }

        /// <summary>
        /// 更新今日赠送记录。
        /// </summary>
        public static string UpdateTodayGiftJson(string? todayGiftJson, string itemId)
        {
            var gifts = string.IsNullOrWhiteSpace(todayGiftJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(todayGiftJson) ?? new Dictionary<string, int>();

            gifts[itemId] = gifts.TryGetValue(itemId, out var count) ? count + 1 : 1;
            return JsonSerializer.Serialize(gifts);
        }

        /// <summary>
        /// 解析今日赠送记录。
        /// </summary>
        public static Dictionary<string, int> ParseTodayGifts(string? todayGiftJson)
        {
            if (string.IsNullOrWhiteSpace(todayGiftJson))
                return new Dictionary<string, int>();

            return JsonSerializer.Deserialize<Dictionary<string, int>>(todayGiftJson)
                   ?? new Dictionary<string, int>();
        }

        /// <summary>
        /// 根据好感度值获取等级信息。
        /// </summary>
        public static FavorabilityLevelInfo GetLevelInfo(int value, List<FavorabilityLevelConfigEntity> levels)
        {
            var matched = levels
                .Where(l => value >= l.MinValue && value <= l.MaxValue)
                .OrderBy(l => l.SortOrder)
                .FirstOrDefault();

            if (matched != null)
            {
                return new FavorabilityLevelInfo
                {
                    Level = matched.Level,
                    Name = matched.Name,
                    Color = matched.Color
                };
            }

            return new FavorabilityLevelInfo
            {
                Level = 0,
                Name = "未知",
                Color = "#CCCCCC"
            };
        }
    }
}
