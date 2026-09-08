using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;
using XXX.WebApi.Services;

namespace XXX.WebApi.Controllers
{
    /// <summary>玩家建议反馈控制器。</summary>
    [ApiController]
    [Route("api/feedback")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;
        private readonly LocalAssetStorageService _assetStorage;

        /// <summary>初始化玩家反馈控制器。</summary>
        public FeedbackController(IFeedbackService service, LocalAssetStorageService assetStorage) { _service = service; _assetStorage = assetStorage; }
        /// <summary>获取当前玩家自己的反馈列表。</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FeedbackListItemDto>>>> GetList()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));
            return Ok(ApiResponse<List<FeedbackListItemDto>>.Ok(await _service.GetMyListAsync(playerId)));
        }
        /// <summary>获取当前玩家自己的反馈详情。</summary>
        [HttpGet("{feedbackId:long}")]
        public async Task<ActionResult<ApiResponse<FeedbackDetailDto>>> GetDetail(long feedbackId)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));
            var result = await _service.GetMyDetailAsync(playerId, feedbackId);
            return result == null ? NotFound(ApiResponse<FeedbackDetailDto>.Fail("反馈不存在或无权查看。")) : Ok(ApiResponse<FeedbackDetailDto>.Ok(result));
        }
        /// <summary>提交玩家反馈。</summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<FeedbackDetailDto>>> Create([FromBody] CreateFeedbackRequestDto request)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));
            try { return Ok(ApiResponse<FeedbackDetailDto>.Ok(await _service.CreateAsync(playerId, request), "反馈提交成功。")); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<FeedbackDetailDto>.Fail(ex.Message)); }
        }
        /// <summary>上传玩家反馈图片。</summary>
        [HttpPost("attachments")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<ActionResult<ApiResponse<FeedbackAttachmentUploadResultDto>>> UploadAttachment([FromForm] IFormFile file)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));
            try
            {
                var result = await _assetStorage.SaveImageAsync(file, "feedback", HttpContext.RequestAborted);
                return Ok(ApiResponse<FeedbackAttachmentUploadResultDto>.Ok(new FeedbackAttachmentUploadResultDto { RelativePath = result.RelativePath, Url = result.Url, OriginalFileName = result.FileName, ContentType = file.ContentType, FileSize = file.Length }, "图片上传成功。"));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<FeedbackAttachmentUploadResultDto>.Fail(ex.Message)); }
        }
        /// <summary>删除当前玩家刚上传的反馈临时图片。</summary>
        [HttpDelete("attachments")]
        public ActionResult<ApiResponse> DeleteAttachment([FromBody] DeleteFeedbackAttachmentRequest request)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));
            return _assetStorage.DeleteFeedbackImage(request.RelativePath) ? Ok(ApiResponse.Ok("图片已删除。")) : NotFound(ApiResponse.Fail("图片不存在。"));
        }
    }

    /// <summary>删除反馈附件请求。</summary>
    public class DeleteFeedbackAttachmentRequest
    {
        /// <summary>图片相对路径。</summary>
        public string RelativePath { get; set; } = string.Empty;
    }
}
