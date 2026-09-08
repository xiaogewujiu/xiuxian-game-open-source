using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    /// <summary>
    /// 后台宗门管理控制器。
    /// 提供宗门模板、心法、Boss、排期、商店、福利和公会的后台管理入口。
    /// </summary>
    [ApiController]
    [Route("api/admin/sect")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AdminSectController : ControllerBase
    {
        private readonly IAdminSectService _adminSectService;

        /// <summary>
        /// 初始化后台宗门管理控制器。
        /// </summary>
        public AdminSectController(IAdminSectService adminSectService)
        {
            _adminSectService = adminSectService;
        }

        #region SectTemplate

        /// <summary>
        /// 获取宗门模板列表。
        /// </summary>
        [HttpGet("templates")]
        public async Task<ActionResult<ApiResponse<List<AdminSectTemplateListItemDto>>>> GetSectTemplates()
        {
            var result = await _adminSectService.GetSectTemplatesAsync();
            return Ok(ApiResponse<List<AdminSectTemplateListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取宗门模板详情。
        /// </summary>
        [HttpGet("templates/{sectId}")]
        public async Task<ActionResult<ApiResponse<AdminSectTemplateDetailDto>>> GetSectTemplate(string sectId)
        {
            var result = await _adminSectService.GetSectTemplateAsync(sectId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminSectTemplateDetailDto>.Fail("宗门模板不存在。"));
            }

            return Ok(ApiResponse<AdminSectTemplateDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存宗门模板。
        /// </summary>
        [HttpPost("templates")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSectTemplateDetailDto>>> SaveSectTemplate([FromBody] AdminSectTemplateDetailDto request)
        {
            try
            {
                var result = await _adminSectService.SaveSectTemplateAsync(request);
                return Ok(ApiResponse<AdminSectTemplateDetailDto>.Ok(result, "宗门模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSectTemplateDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除宗门模板。
        /// </summary>
        [HttpDelete("templates/{sectId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteSectTemplate(string sectId)
        {
            var deleted = await _adminSectService.DeleteSectTemplateAsync(sectId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("宗门模板不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("宗门模板删除成功"));
        }

        #endregion

        #region HeartSutra

        /// <summary>
        /// 获取心法模板列表。
        /// </summary>
        [HttpGet("heart-sutras")]
        public async Task<ActionResult<ApiResponse<List<AdminHeartSutraListItemDto>>>> GetHeartSutras([FromQuery] string? sectId)
        {
            var result = await _adminSectService.GetHeartSutrasAsync(sectId ?? "");
            return Ok(ApiResponse<List<AdminHeartSutraListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取心法模板详情。
        /// </summary>
        [HttpGet("heart-sutras/{sutraId}")]
        public async Task<ActionResult<ApiResponse<AdminHeartSutraDetailDto>>> GetHeartSutra(string sutraId)
        {
            var result = await _adminSectService.GetHeartSutraAsync(sutraId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminHeartSutraDetailDto>.Fail("心法模板不存在。"));
            }

            return Ok(ApiResponse<AdminHeartSutraDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存心法模板。
        /// </summary>
        [HttpPost("heart-sutras")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminHeartSutraDetailDto>>> SaveHeartSutra([FromBody] AdminHeartSutraDetailDto request)
        {
            try
            {
                var result = await _adminSectService.SaveHeartSutraAsync(request);
                return Ok(ApiResponse<AdminHeartSutraDetailDto>.Ok(result, "心法模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminHeartSutraDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除心法模板。
        /// </summary>
        [HttpDelete("heart-sutras/{sutraId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteHeartSutra(string sutraId)
        {
            var deleted = await _adminSectService.DeleteHeartSutraAsync(sutraId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("心法模板不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("心法模板删除成功"));
        }

        #endregion

        #region SectBossTemplate

        /// <summary>
        /// 获取宗门Boss模板列表。
        /// </summary>
        [HttpGet("boss-templates")]
        public async Task<ActionResult<ApiResponse<List<AdminSectBossTemplateListItemDto>>>> GetSectBossTemplates()
        {
            var result = await _adminSectService.GetSectBossTemplatesAsync();
            return Ok(ApiResponse<List<AdminSectBossTemplateListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取宗门Boss模板详情。
        /// </summary>
        [HttpGet("boss-templates/{bossId}")]
        public async Task<ActionResult<ApiResponse<AdminSectBossTemplateDetailDto>>> GetSectBossTemplate(string bossId)
        {
            var result = await _adminSectService.GetSectBossTemplateAsync(bossId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminSectBossTemplateDetailDto>.Fail("Boss模板不存在。"));
            }

            return Ok(ApiResponse<AdminSectBossTemplateDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存宗门Boss模板。
        /// </summary>
        [HttpPost("boss-templates")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSectBossTemplateDetailDto>>> SaveSectBossTemplate([FromBody] AdminSectBossTemplateDetailDto request)
        {
            try
            {
                var result = await _adminSectService.SaveSectBossTemplateAsync(request);
                return Ok(ApiResponse<AdminSectBossTemplateDetailDto>.Ok(result, "Boss模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSectBossTemplateDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除宗门Boss模板。
        /// </summary>
        [HttpDelete("boss-templates/{bossId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteSectBossTemplate(string bossId)
        {
            var deleted = await _adminSectService.DeleteSectBossTemplateAsync(bossId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("Boss模板不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("Boss模板删除成功"));
        }

        #endregion

        #region SectTournamentSchedule

        /// <summary>
        /// 获取宗门大比排期配置。
        /// </summary>
        [HttpGet("tournament-schedule")]
        public async Task<ActionResult<ApiResponse<AdminSectTournamentScheduleDto>>> GetSectTournamentSchedule()
        {
            var result = await _adminSectService.GetSectTournamentScheduleAsync();
            return Ok(ApiResponse<AdminSectTournamentScheduleDto>.Ok(result));
        }

        /// <summary>
        /// 保存宗门大比排期配置。
        /// </summary>
        [HttpPost("tournament-schedule")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSectTournamentScheduleDto>>> SaveSectTournamentSchedule([FromBody] AdminSectTournamentScheduleDto request)
        {
            var result = await _adminSectService.SaveSectTournamentScheduleAsync(request);
            return Ok(ApiResponse<AdminSectTournamentScheduleDto>.Ok(result, "排期配置保存成功"));
        }

        #endregion

        #region SectShopItem

        /// <summary>
        /// 获取宗门商店商品列表。
        /// </summary>
        [HttpGet("shop-items")]
        public async Task<ActionResult<ApiResponse<List<AdminSectShopItemListItemDto>>>> GetSectShopItems()
        {
            var result = await _adminSectService.GetSectShopItemsAsync();
            return Ok(ApiResponse<List<AdminSectShopItemListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取宗门商店商品详情。
        /// </summary>
        [HttpGet("shop-items/{gid}")]
        public async Task<ActionResult<ApiResponse<AdminSectShopItemDetailDto>>> GetSectShopItem(string gid)
        {
            var result = await _adminSectService.GetSectShopItemAsync(gid);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminSectShopItemDetailDto>.Fail("商品不存在。"));
            }

            return Ok(ApiResponse<AdminSectShopItemDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存宗门商店商品。
        /// </summary>
        [HttpPost("shop-items")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSectShopItemDetailDto>>> SaveSectShopItem([FromBody] AdminSectShopItemDetailDto request)
        {
            try
            {
                var result = await _adminSectService.SaveSectShopItemAsync(request);
                return Ok(ApiResponse<AdminSectShopItemDetailDto>.Ok(result, "商品保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSectShopItemDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除宗门商店商品。
        /// </summary>
        [HttpDelete("shop-items/{gid}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteSectShopItem(string gid)
        {
            var deleted = await _adminSectService.DeleteSectShopItemAsync(gid);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("商品不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("商品删除成功"));
        }

        #endregion

        #region SectBlessing

        /// <summary>
        /// 获取宗门福利列表。
        /// </summary>
        [HttpGet("blessings")]
        public async Task<ActionResult<ApiResponse<List<AdminSectBlessingListItemDto>>>> GetSectBlessings()
        {
            var result = await _adminSectService.GetSectBlessingsAsync();
            return Ok(ApiResponse<List<AdminSectBlessingListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取宗门福利详情。
        /// </summary>
        [HttpGet("blessings/{blessingId}")]
        public async Task<ActionResult<ApiResponse<AdminSectBlessingDetailDto>>> GetSectBlessing(string blessingId)
        {
            var result = await _adminSectService.GetSectBlessingAsync(blessingId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminSectBlessingDetailDto>.Fail("福利配置不存在。"));
            }

            return Ok(ApiResponse<AdminSectBlessingDetailDto>.Ok(result));
        }

        /// <summary>
        /// 保存宗门福利。
        /// </summary>
        [HttpPost("blessings")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse<AdminSectBlessingDetailDto>>> SaveSectBlessing([FromBody] AdminSectBlessingDetailDto request)
        {
            try
            {
                var result = await _adminSectService.SaveSectBlessingAsync(request);
                return Ok(ApiResponse<AdminSectBlessingDetailDto>.Ok(result, "福利配置保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminSectBlessingDetailDto>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除宗门福利。
        /// </summary>
        [HttpDelete("blessings/{blessingId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        public async Task<ActionResult<ApiResponse>> DeleteSectBlessing(string blessingId)
        {
            var deleted = await _adminSectService.DeleteSectBlessingAsync(blessingId);
            if (!deleted)
            {
                return NotFound(ApiResponse.Fail("福利配置不存在或删除失败。"));
            }

            return Ok(ApiResponse.Ok("福利配置删除成功"));
        }

        #endregion

        #region Guild admin

        /// <summary>
        /// 获取公会列表。
        /// </summary>
        [HttpGet("guilds")]
        public async Task<ActionResult<ApiResponse<List<AdminGuildListItemDto>>>> GetGuilds()
        {
            var result = await _adminSectService.GetGuildsAsync();
            return Ok(ApiResponse<List<AdminGuildListItemDto>>.Ok(result));
        }

        /// <summary>
        /// 获取公会详情。
        /// </summary>
        [HttpGet("guilds/{guildId}")]
        public async Task<ActionResult<ApiResponse<AdminGuildDetailDto>>> GetGuild(string guildId)
        {
            var result = await _adminSectService.GetGuildAsync(guildId);
            if (result == null)
            {
                return NotFound(ApiResponse<AdminGuildDetailDto>.Fail("公会不存在。"));
            }

            return Ok(ApiResponse<AdminGuildDetailDto>.Ok(result));
        }

        #endregion
    }
}
