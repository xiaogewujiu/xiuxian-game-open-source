#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/equipment-decompose-rules")]
[Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
public class EquipmentDecomposeRulesController : ControllerBase
{
    private readonly IAdminEquipmentDecomposeRuleService _service;

    public EquipmentDecomposeRulesController(IAdminEquipmentDecomposeRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminEquipmentDecomposeRuleDto>>>> Get()
        => Ok(ApiResponse<List<AdminEquipmentDecomposeRuleDto>>.Ok(await _service.GetRulesAsync()));

    [HttpPost]
    [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
    public async Task<ActionResult<ApiResponse>> Save([FromBody] AdminEquipmentDecomposeRuleDto request)
    {
        await _service.SaveRuleAsync(request);
        return Ok(ApiResponse.Ok("装备分解规则保存成功"));
    }

    [HttpDelete("{gid:long}")]
    [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
    public async Task<ActionResult<ApiResponse>> Delete(long gid)
    {
        await _service.DeleteRuleAsync(gid);
        return Ok(ApiResponse.Ok("装备分解规则删除成功"));
    }
}
#pragma warning restore CS1591
