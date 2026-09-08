namespace XXX.Application.DTOs
{
    /// <summary>
    /// 赠送好感度道具请求。
    /// </summary>
    public class GiftFavorabilityRequest
    {
        /// <summary>
        /// 目标玩家ID。
        /// </summary>
        public string TargetPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 道具ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 赠送好感度道具响应。
    /// </summary>
    public class GiftFavorabilityResponse
    {
        public bool Success { get; set; }
        public int NewValue { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelColor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 好感度列表项。
    /// </summary>
    public class FavorabilityListItemDto
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public int Value { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelColor { get; set; } = string.Empty;
        public DateTime? LastGiftTime { get; set; }
        public List<FavorabilityGiftRecordDto> RecentGifts { get; set; } = [];
    }

    /// <summary>
    /// 好感度赠送记录。
    /// </summary>
    public class FavorabilityGiftRecordDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int Change { get; set; }
        public DateTime Time { get; set; }
        public string? FromPlayerName { get; set; }
        public string? ToPlayerName { get; set; }
    }

    /// <summary>
    /// 好感度日志查询请求。
    /// </summary>
    public class FavorabilityLogQueryRequest
    {
        public string? TargetPlayerId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 好感度等级DTO（与前端 AdminFavorabilityLevelConfig 对应）。
    /// </summary>
    public class FavorabilityLevelDto
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string Color { get; set; } = "#CCCCCC";
        public int SortOrder { get; set; }
        public string? Description { get; set; }
        public string? RewardJson { get; set; }
        public bool IsBuiltIn { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台赠送记录DTO（与前端 AdminFavorabilityGiftLog 对应）。
    /// </summary>
    public class AdminFavorabilityGiftLogDto
    {
        public long Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string TargetPlayerId { get; set; } = string.Empty;
        public string TargetPlayerName { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int FavorabilityChange { get; set; }
        public DateTime GiftTime { get; set; }
    }

    /// <summary>
    /// 后台好感度关系DTO。
    /// </summary>
    public class AdminFavorabilityRelationDto
    {
        public long Id { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string TargetPlayerId { get; set; } = string.Empty;
        public string TargetPlayerName { get; set; } = string.Empty;
        public int Value { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public DateTime? LastGiftTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    /// <summary>
    /// 后台修改好感度请求。
    /// </summary>
    public class AdminFavorabilityUpdateRequest
    {
        public long Id { get; set; }
        public int NewValue { get; set; }
    }

    /// <summary>
    /// 后台好感度统计DTO。
    /// </summary>
    public class AdminFavorabilityStatsDto
    {
        public int TotalRelations { get; set; }
        public int TotalGifts { get; set; }
        public List<AdminPopularPlayerDto> MostGiftedPlayers { get; set; } = [];
    }

    /// <summary>
    /// 热门玩家DTO。
    /// </summary>
    public class AdminPopularPlayerDto
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int GiftCount { get; set; }
    }

    /// <summary>
    /// 好感度排行榜项。
    /// </summary>
    public class FavorabilityLeaderboardDto
    {
        public int Rank { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public int TotalFavorability { get; set; }
        public int RelationCount { get; set; }
    }

    /// <summary>
    /// 深度友谊排行项。
    /// </summary>
    public class DeepFriendshipDto
    {
        public int Rank { get; set; }
        public string Player1Id { get; set; } = string.Empty;
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Id { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
        public int CombinedValue { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelColor { get; set; } = string.Empty;
    }

    /// <summary>
    /// 好感度成就进度。
    /// </summary>
    public class FavorabilityAchievementProgressDto
    {
        public int TotalGiftSent { get; set; }
        public int TotalGiftReceived { get; set; }
        public int UniqueFriends { get; set; }
        public int MaxFavorability { get; set; }
        public int RelationsAtMaxLevel { get; set; }
        public List<FavorabilityAchievementDto> Achievements { get; set; } = [];
    }

    /// <summary>
    /// 好感度成就项。
    /// </summary>
    public class FavorabilityAchievementDto
    {
        public string AchievementId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TargetValue { get; set; }
        public int CurrentValue { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsClaimed { get; set; }
        public string? RewardJson { get; set; }
    }

    /// <summary>
    /// 好感度成就奖励领取结果。
    /// </summary>
    public class FavorabilityAchievementRewardDto
    {
        public string AchievementId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RewardJson { get; set; }
    }
}
