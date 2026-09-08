using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 装备控制器。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<EquipmentController> _logger;

        /// <summary>
        /// 初始化装备控制器。
        /// </summary>
        public EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger)
        {
            _equipmentService = equipmentService;
            _logger = logger;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取玩家全部装备。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EquipmentDto>>>> GetAll()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var equipments = await _equipmentService.GetPlayerEquipmentsAsync(playerId);
            return Ok(ApiResponse<List<EquipmentDto>>.Ok(equipments));
        }

        /// <summary>
        /// 获取已穿戴装备。
        /// </summary>
        [HttpGet("equipped")]
        public async Task<ActionResult<ApiResponse<List<EquipmentDto>>>> GetEquipped()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var equipments = await _equipmentService.GetEquippedItemsAsync(playerId);
            return Ok(ApiResponse<List<EquipmentDto>>.Ok(equipments));
        }

        /// <summary>
        /// 获取装备详情。
        /// </summary>
        [HttpGet("{equipmentId}")]
        public async Task<ActionResult<ApiResponse<EquipmentDto>>> GetDetail(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var equipment = await _equipmentService.GetEquipmentDetailAsync(playerId, equipmentId);
            if (equipment == null) return NotFound(ApiResponse<EquipmentDto>.Fail("装备不存在"));

            return Ok(ApiResponse<EquipmentDto>.Ok(equipment));
        }

        /// <summary>
        /// 穿戴装备。
        /// </summary>
        [HttpPost("equip")]
        public async Task<ActionResult<ApiResponse<bool>>> Equip([FromBody] EquipItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.EquipItemAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("穿戴失败，可能是装备不存在或等级不足"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "穿戴成功"));
        }

        /// <summary>
        /// 卸下装备。
        /// </summary>
        [HttpPost("unequip")]
        public async Task<ActionResult<ApiResponse<bool>>> Unequip([FromBody] UnequipItemRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.UnequipItemAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("卸下失败，当前部位没有已穿戴装备"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "卸下成功"));
        }

        /// <summary>
        /// 强化装备。
        /// </summary>
        [HttpPost("enhance")]
        public async Task<ActionResult<ApiResponse<EnhanceResultDto>>> Enhance([FromBody] EnhanceEquipmentRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.EnhanceEquipmentAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<EnhanceResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<EnhanceResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 对比装备。
        /// </summary>
        [HttpGet("compare/{equipmentId}")]
        public async Task<ActionResult<ApiResponse<EquipmentCompareDto>>> Compare(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.CompareEquipmentAsync(playerId, equipmentId);
            if (result == null) return NotFound(ApiResponse<EquipmentCompareDto>.Fail("装备不存在"));

            return Ok(ApiResponse<EquipmentCompareDto>.Ok(result));
        }

        /// <summary>
        /// 出售装备。
        /// </summary>
        [HttpPost("sell/{equipmentId}")]
        public async Task<ActionResult<ApiResponse<long>>> Sell(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var gold = await _equipmentService.SellEquipmentAsync(playerId, equipmentId);
            if (gold <= 0)
            {
                return BadRequest(ApiResponse<long>.Fail("出售失败，可能是装备不存在、已穿戴或已绑定"));
            }

            return Ok(ApiResponse<long>.Ok(gold, $"出售成功，获得 {gold} 金币"));
        }

        /// <summary>
        /// 批量出售装备。
        /// </summary>
        [HttpPost("sell-batch")]
        public async Task<ActionResult<ApiResponse<BatchSellEquipmentResultDto>>> SellBatch([FromBody] BatchSellEquipmentRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.SellEquipmentsAsync(playerId, request);
            if (result.SoldCount == 0)
            {
                return BadRequest(ApiResponse<BatchSellEquipmentResultDto>.Fail("没有可出售的装备"));
            }

            return Ok(ApiResponse<BatchSellEquipmentResultDto>.Ok(result, $"成功出售 {result.SoldCount} 件装备，获得 {result.GoldEarned} 金币"));
        }

        /// <summary>
        /// 分解一件或多件未穿戴、未锁定装备。
        /// </summary>
        [HttpPost("decompose")]
        public async Task<ActionResult<ApiResponse<EquipmentDecomposeResultDto>>> Decompose([FromBody] DecomposeEquipmentRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.DecomposeEquipmentsAsync(playerId, request);
            if (result.DecomposedCount == 0)
            {
                return BadRequest(ApiResponse<EquipmentDecomposeResultDto>.Fail("没有可分解的装备"));
            }

            return Ok(ApiResponse<EquipmentDecomposeResultDto>.Ok(result, $"成功分解 {result.DecomposedCount} 件装备"));
        }

        /// <summary>
        /// 获取可强化装备列表。
        /// </summary>
        [HttpGet("enhanceable")]
        public async Task<ActionResult<ApiResponse<List<EquipmentDto>>>> GetEnhanceable()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var equipments = await _equipmentService.GetEnhanceableEquipmentsAsync(playerId);
            return Ok(ApiResponse<List<EquipmentDto>>.Ok(equipments));
        }

        /// <summary>
        /// 获取强化成功率。
        /// </summary>
        [HttpGet("success-rate/{equipmentId}")]
        public async Task<ActionResult<ApiResponse<int>>> GetSuccessRate(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var rate = await _equipmentService.CalculateEnhanceSuccessRateAsync(playerId, equipmentId);
            return Ok(ApiResponse<int>.Ok(rate));
        }

        /// <summary>
        /// 锁定装备。
        /// </summary>
        [HttpPost("bind")]
        public async Task<ActionResult<ApiResponse<BindEquipmentResultDto>>> Bind([FromBody] BindEquipmentRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.BindEquipmentAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<BindEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<BindEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 解锁装备。
        /// </summary>
        [HttpPost("unlock")]
        public async Task<ActionResult<ApiResponse<BindEquipmentResultDto>>> Unlock([FromBody] BindEquipmentRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.UnlockEquipmentAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<BindEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<BindEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 捐献装备。
        /// </summary>
        [HttpPost("donate/{equipmentId}")]
        public async Task<ActionResult<ApiResponse<DonateEquipmentResultDto>>> Donate(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.DonateEquipmentAsync(playerId, equipmentId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<DonateEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<DonateEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 获取装备洗练预览。
        /// </summary>
        [HttpGet("reroll/preview/{equipmentId}")]
        public async Task<ActionResult<ApiResponse<EquipmentRerollPreviewDto>>> GetRerollPreview(string equipmentId)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.GetEquipmentRerollPreviewAsync(playerId, equipmentId);
            if (result == null)
            {
                return NotFound(ApiResponse<EquipmentRerollPreviewDto>.Fail("装备不存在"));
            }

            return Ok(ApiResponse<EquipmentRerollPreviewDto>.Ok(result));
        }

        /// <summary>
        /// 执行装备洗练（生成候选词条）。
        /// </summary>
        [HttpPost("reroll/roll")]
        public async Task<ActionResult<ApiResponse<RerollEquipmentResultDto>>> RollReroll([FromBody] RerollEquipmentRollRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.RollEquipmentRerollAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<RerollEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<RerollEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 接受洗练候选结果。
        /// </summary>
        [HttpPost("reroll/accept")]
        public async Task<ActionResult<ApiResponse<RerollEquipmentResultDto>>> AcceptReroll([FromBody] RerollEquipmentAcceptRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.AcceptEquipmentRerollAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<RerollEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<RerollEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 丢弃洗练候选结果。
        /// </summary>
        [HttpPost("reroll/discard")]
        public async Task<ActionResult<ApiResponse<RerollEquipmentResultDto>>> DiscardReroll([FromBody] RerollEquipmentDiscardRequestDto request)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _equipmentService.DiscardEquipmentRerollAsync(playerId, request);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<RerollEquipmentResultDto>.Fail(result.Message));
            }

            return Ok(ApiResponse<RerollEquipmentResultDto>.Ok(result, result.Message));
        }
    }
}
