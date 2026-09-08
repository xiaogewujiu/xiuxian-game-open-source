using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 宗门服务接口
    /// 提供宗门浏览、加入、心法修炼、捐献、切磋、大比、Boss、商店、福利等宗门系统功能
    /// </summary>
    public interface ISectService
    {
        /// <summary>
        /// 获取所有可用宗门列表
        /// </summary>
        /// <param name="playerId">当前玩家ID</param>
        /// <returns>宗门列表，包含成员数和是否已加入</returns>
        Task<List<SectTemplateDto>> GetAvailableSectsAsync(string playerId);

        /// <summary>
        /// 获取宗门详情
        /// </summary>
        /// <param name="playerId">当前玩家ID</param>
        /// <param name="sectId">宗门ID</param>
        /// <returns>宗门详情，包含心法列表和公会信息</returns>
        Task<SectDetailDto?> GetSectDetailAsync(string playerId, string sectId);

        /// <summary>
        /// 加入宗门
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="sectId">宗门模板ID</param>
        /// <returns>是否加入成功</returns>
        Task<bool> JoinSectAsync(string playerId, string sectId);

        /// <summary>
        /// 获取玩家心法进度列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>心法进度列表</returns>
        Task<List<PlayerSutraProgressDto>> GetPlayerSutrasAsync(string playerId);

        /// <summary>
        /// 升级心法层级
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="sutraId">心法ID</param>
        /// <returns>升级结果</returns>
        Task<HeartSutraUpgradeResultDto> UpgradeSutraLayerAsync(string playerId, string sutraId);

        /// <summary>
        /// 获取捐献状态
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>捐献状态信息</returns>
        Task<DonationStatusDto> GetDonationStatusAsync(string playerId);

        /// <summary>
        /// 执行捐献
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>捐献结果</returns>
        Task<DonationResultDto> DonateAsync(string playerId, long goldAmount = 0);

        /// <summary>
        /// 获取宗门弟子列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>弟子列表</returns>
        Task<List<SectDiscipleDto>> GetDisciplesAsync(string playerId);

        /// <summary>
        /// 切磋
        /// </summary>
        /// <param name="playerId">发起切磋的玩家ID</param>
        /// <param name="targetPlayerId">目标玩家ID</param>
        /// <returns>切磋结果</returns>
        Task<SparResultDto> SparAsync(string playerId, string targetPlayerId);

        /// <summary>
        /// 获取宗门大比状态
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>大比状态信息</returns>
        Task<SectTournamentStatusDto?> GetSectTournamentStatusAsync(string playerId);

        /// <summary>
        /// 获取宗门大比对战记录
        /// </summary>
        /// <param name="tournamentId">大比ID</param>
        /// <returns>对战记录列表</returns>
        Task<List<SectTournamentMatchDto>> GetSectTournamentMatchesAsync(string tournamentId);

        /// <summary>
        /// 领取宗门大比奖励
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="tournamentId">大比ID</param>
        /// <returns>奖励信息</returns>
        Task<SectTournamentRewardDto?> ClaimSectTournamentRewardAsync(string playerId, string tournamentId);

        /// <summary>
        /// 获取天骄赛状态
        /// </summary>
        /// <returns>天骄赛状态信息</returns>
        Task<GeniusTournamentStatusDto?> GetGeniusTournamentStatusAsync();

        /// <summary>
        /// 获取天骄赛对战记录
        /// </summary>
        /// <param name="tournamentId">天骄赛ID</param>
        /// <returns>对战记录列表</returns>
        Task<List<GeniusTournamentMatchDto>> GetGeniusTournamentMatchesAsync(string tournamentId);

        /// <summary>
        /// 领取天骄赛奖励
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="tournamentId">天骄赛ID</param>
        /// <returns>奖励信息</returns>
        Task<GeniusTournamentRewardDto?> ClaimGeniusTournamentRewardAsync(string playerId, string tournamentId);

        /// <summary>
        /// 获取宗门Boss状态
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>Boss状态信息</returns>
        Task<SectBossStatusDto?> GetSectBossStatusAsync(string playerId);

        /// <summary>
        /// 攻击宗门Boss
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>攻击结果</returns>
        Task<SectBossActionResultDto> AttackSectBossAsync(string playerId);

        /// <summary>
        /// 领取宗门Boss奖励
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>奖励信息</returns>
        Task<SectBossRewardDto?> ClaimSectBossRewardAsync(string playerId);

        /// <summary>
        /// 获取宗门商店商品列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>商品列表</returns>
        Task<List<SectShopItemDto>> GetSectShopItemsAsync(string playerId);

        /// <summary>
        /// 购买宗门商店商品
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="shopItemId">商品ID</param>
        /// <returns>购买结果</returns>
        Task<SectShopPurchaseResultDto> PurchaseSectShopItemAsync(string playerId, string shopItemId);

        /// <summary>
        /// 获取宗门福利列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>福利列表</returns>
        Task<List<SectBlessingDto>> GetSectBlessingsAsync(string playerId);

        /// <summary>
        /// 领取宗门福利
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="blessingId">福利ID</param>
        /// <returns>是否领取成功</returns>
        Task<bool> ClaimSectBlessingAsync(string playerId, string blessingId);

        /// <summary>
        /// 获取宗门任务列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>任务列表</returns>
        Task<List<SectTaskDto>> GetSectTasksAsync(string playerId);
    }
}
