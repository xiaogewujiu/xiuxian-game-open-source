using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 丹药效果查询接口。
    /// </summary>
    [ApiController]
    [Route("api/player/me")]
    [Authorize]
    public class PillEffectController : ControllerBase
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 初始化丹药效果控制器。
        /// </summary>
        public PillEffectController(DbContext dbContext) => _db = dbContext.Db;

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前玩家的丹药效果总览。
        /// 返回永久生效和当前仍在生效的限时丹药列表。
        /// </summary>
        [HttpGet("pill-effects")]
        public async Task<ActionResult<ApiResponse<PillEffectsDto>>> GetPillEffects()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var now = DateTime.Now;

            // 查询玩家丹药效果并关联物品模板，获取名称与品质信息。
            var records = await _db.Queryable<PlayerPillEffectEntity>()
                .LeftJoin<ItemTemplateEntity>((effect, tpl) => effect.ItemId == tpl.ItemId)
                .Where((effect, tpl) => effect.PlayerId == playerId)
                .Where((effect, tpl) => !effect.IsTemporary || effect.ExpiresAt == null || effect.ExpiresAt > now)
                .Select((effect, tpl) => new
                {
                    effect.ItemId,
                    TemplateName = tpl.Name,
                    TemplateQuality = tpl.Quality,
                    effect.EffectType,
                    effect.BonusType,
                    effect.BonusValue,
                    effect.UsageCount,
                    effect.IsTemporary,
                    effect.ExpiresAt
                })
                .ToListAsync();

            var result = new PillEffectsDto();

            foreach (var r in records)
            {
                var qualityName = GetQualityName(r.TemplateQuality);

                if (r.IsTemporary)
                {
                    result.TemporaryEffects.Add(new TemporaryPillEffectDto
                    {
                        ItemId = r.ItemId,
                        Name = r.TemplateName,
                        Rank = qualityName,
                        EffectType = r.EffectType,
                        BonusType = r.BonusType ?? string.Empty,
                        CurrentValue = r.BonusValue,
                        ExpiresAt = r.ExpiresAt ?? DateTime.MinValue
                    });
                }
                else
                {
                    result.PermanentEffects.Add(new PermanentPillEffectDto
                    {
                        ItemId = r.ItemId,
                        Name = r.TemplateName,
                        Rank = qualityName,
                        EffectType = r.EffectType,
                        BonusType = r.BonusType ?? string.Empty,
                        CumulativeValue = r.BonusValue * r.UsageCount,
                        UsageCount = r.UsageCount
                    });
                }
            }

            return Ok(ApiResponse<PillEffectsDto>.Ok(result));
        }

        /// <summary>
        /// 将品质整数值转换为品质名称。
        /// </summary>
        private static string GetQualityName(int quality) => quality switch
        {
            1 => "普通",
            2 => "优秀",
            3 => "稀有",
            4 => "史诗",
            5 => "传说",
            6 => "神话",
            7 => "神圣",
            _ => "普通"
        };
    }
}
