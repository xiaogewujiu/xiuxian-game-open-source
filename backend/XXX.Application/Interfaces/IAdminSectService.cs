using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 后台宗门管理服务接口。
    /// </summary>
    public interface IAdminSectService
    {
        // ---- SectTemplate CRUD ----

        /// <summary>
        /// 获取宗门模板列表。
        /// </summary>
        Task<List<AdminSectTemplateListItemDto>> GetSectTemplatesAsync();

        /// <summary>
        /// 获取宗门模板详情。
        /// </summary>
        Task<AdminSectTemplateDetailDto?> GetSectTemplateAsync(string sectId);

        /// <summary>
        /// 保存宗门模板（新增或更新）。
        /// </summary>
        Task<AdminSectTemplateDetailDto> SaveSectTemplateAsync(AdminSectTemplateDetailDto request);

        /// <summary>
        /// 删除宗门模板。
        /// </summary>
        Task<bool> DeleteSectTemplateAsync(string sectId);

        // ---- HeartSutra CRUD ----

        /// <summary>
        /// 获取心法模板列表。
        /// </summary>
        Task<List<AdminHeartSutraListItemDto>> GetHeartSutrasAsync(string sectId = "");

        /// <summary>
        /// 获取心法模板详情。
        /// </summary>
        Task<AdminHeartSutraDetailDto?> GetHeartSutraAsync(string sutraId);

        /// <summary>
        /// 保存心法模板（新增或更新）。
        /// </summary>
        Task<AdminHeartSutraDetailDto> SaveHeartSutraAsync(AdminHeartSutraDetailDto request);

        /// <summary>
        /// 删除心法模板。
        /// </summary>
        Task<bool> DeleteHeartSutraAsync(string sutraId);

        // ---- SectBossTemplate CRUD ----

        /// <summary>
        /// 获取宗门Boss模板列表。
        /// </summary>
        Task<List<AdminSectBossTemplateListItemDto>> GetSectBossTemplatesAsync();

        /// <summary>
        /// 获取宗门Boss模板详情。
        /// </summary>
        Task<AdminSectBossTemplateDetailDto?> GetSectBossTemplateAsync(string bossId);

        /// <summary>
        /// 保存宗门Boss模板（新增或更新）。
        /// </summary>
        Task<AdminSectBossTemplateDetailDto> SaveSectBossTemplateAsync(AdminSectBossTemplateDetailDto request);

        /// <summary>
        /// 删除宗门Boss模板。
        /// </summary>
        Task<bool> DeleteSectBossTemplateAsync(string bossId);

        // ---- SectTournamentSchedule ----

        /// <summary>
        /// 获取宗门大比排期配置。
        /// </summary>
        Task<AdminSectTournamentScheduleDto> GetSectTournamentScheduleAsync();

        /// <summary>
        /// 保存宗门大比排期配置。
        /// </summary>
        Task<AdminSectTournamentScheduleDto> SaveSectTournamentScheduleAsync(AdminSectTournamentScheduleDto request);

        // ---- SectShopItem CRUD ----

        /// <summary>
        /// 获取宗门商店商品列表。
        /// </summary>
        Task<List<AdminSectShopItemListItemDto>> GetSectShopItemsAsync();

        /// <summary>
        /// 获取宗门商店商品详情。
        /// </summary>
        Task<AdminSectShopItemDetailDto?> GetSectShopItemAsync(string gid);

        /// <summary>
        /// 保存宗门商店商品（新增或更新）。
        /// </summary>
        Task<AdminSectShopItemDetailDto> SaveSectShopItemAsync(AdminSectShopItemDetailDto request);

        /// <summary>
        /// 删除宗门商店商品。
        /// </summary>
        Task<bool> DeleteSectShopItemAsync(string gid);

        // ---- SectBlessing CRUD ----

        /// <summary>
        /// 获取宗门福利列表。
        /// </summary>
        Task<List<AdminSectBlessingListItemDto>> GetSectBlessingsAsync();

        /// <summary>
        /// 获取宗门福利详情。
        /// </summary>
        Task<AdminSectBlessingDetailDto?> GetSectBlessingAsync(string blessingId);

        /// <summary>
        /// 保存宗门福利（新增或更新）。
        /// </summary>
        Task<AdminSectBlessingDetailDto> SaveSectBlessingAsync(AdminSectBlessingDetailDto request);

        /// <summary>
        /// 删除宗门福利。
        /// </summary>
        Task<bool> DeleteSectBlessingAsync(string blessingId);

        // ---- Guild admin ----

        /// <summary>
        /// 获取公会列表。
        /// </summary>
        Task<List<AdminGuildListItemDto>> GetGuildsAsync();

        /// <summary>
        /// 获取公会详情。
        /// </summary>
        Task<AdminGuildDetailDto?> GetGuildAsync(string guildId);
    }
}
