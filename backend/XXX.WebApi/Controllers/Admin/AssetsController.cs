#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Security;
using XXX.WebApi.Services;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/assets")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AssetsController : ControllerBase
    {
        private readonly LocalAssetStorageService _assetStorageService;
        private readonly AiIconGenerationService _iconGenerationService;

        public AssetsController(LocalAssetStorageService assetStorageService, AiIconGenerationService iconGenerationService)
        {
            _assetStorageService = assetStorageService;
            _iconGenerationService = iconGenerationService;
        }

        [HttpPost("upload")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AssetUploadResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AssetUploadResultDto>>> Upload([FromForm] IFormFile file, [FromForm] string category)
        {
            try
            {
                var result = await _assetStorageService.SaveImageAsync(file, category, HttpContext.RequestAborted);
                return Ok(ApiResponse<AssetUploadResultDto>.Ok(result, "图片上传成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AssetUploadResultDto>.Fail(ex.Message));
            }
        }

        [HttpPost("upload-collection-image")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<CollectionImageUploadResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CollectionImageUploadResultDto>>> UploadCollectionImage([FromForm] IFormFile file)
        {
            try
            {
                var result = await _assetStorageService.SaveImageWithThumbnailAsync(file, HttpContext.RequestAborted);
                return Ok(ApiResponse<CollectionImageUploadResultDto>.Ok(result, "图片上传成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CollectionImageUploadResultDto>.Fail(ex.Message));
            }
        }

        [HttpPost("generate-icon")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AssetUploadResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AssetUploadResultDto>>> GenerateIcon([FromBody] GenerateIconRequest request)
        {
            try
            {
                var result = await _iconGenerationService.GenerateAsync(
                    request.Prompt,
                    request.Category,
                    request.OldPath,
                    HttpContext.RequestAborted);

                return Ok(ApiResponse<AssetUploadResultDto>.Ok(new AssetUploadResultDto
                {
                    RelativePath = result.RelativePath,
                    Url = result.Url,
                    FileName = result.FileName
                }, "图标生成成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AssetUploadResultDto>.Fail(ex.Message));
            }
        }
    }

    public class GenerateIconRequest
    {
        public string Prompt { get; set; } = "";
        public string Category { get; set; } = "misc";
        public string? OldPath { get; set; }
    }
}
#pragma warning restore CS1591
