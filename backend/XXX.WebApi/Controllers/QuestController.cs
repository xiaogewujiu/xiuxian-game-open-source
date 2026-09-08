using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Quest;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 任务系统接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuestController : ControllerBase
    {
        private readonly IQuestService _questService;
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// 初始化任务控制器。
        /// </summary>
        public QuestController(IQuestService questService, IRepository<UserEntity> userRepository)
        {
            _questService = questService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前玩家可接取的任务配置。
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<List<QuestConfig>>>> GetAvailable()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var quests = await _questService.GetAvailableQuestsAsync(player);
            return Ok(ApiResponse<List<QuestConfig>>.Ok(quests));
        }

        /// <summary>
        /// 获取当前玩家可接任务的前端总览结构。
        /// </summary>
        [HttpGet("available-overview")]
        public async Task<ActionResult<ApiResponse<List<QuestDto>>>> GetAvailableOverview()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var quests = await _questService.GetAvailableQuestsAsync(player);
            var overview = quests
                .OrderBy(quest => quest.SortOrder)
                .Select(config => MapQuestToDto(config, null))
                .ToList();

            return Ok(ApiResponse<List<QuestDto>>.Ok(overview));
        }

        /// <summary>
        /// 获取当前进行中的任务进度。
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<QuestProgress>>>> GetActive()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var quests = await _questService.GetPlayerInProgressQuestsAsync(playerId);
            return Ok(ApiResponse<List<QuestProgress>>.Ok(quests));
        }

        /// <summary>
        /// 获取当前进行中任务的前端总览结构。
        /// </summary>
        [HttpGet("active-overview")]
        public async Task<ActionResult<ApiResponse<List<QuestDto>>>> GetActiveOverview()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var quests = await _questService.GetPlayerInProgressQuestsAsync(playerId);
            var overview = await MapProgressListToDtoAsync(quests);
            return Ok(ApiResponse<List<QuestDto>>.Ok(overview));
        }

        /// <summary>
        /// 获取当前已完成任务的进度记录。
        /// </summary>
        [HttpGet("completed")]
        public async Task<ActionResult<ApiResponse<List<QuestProgress>>>> GetCompleted()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var quests = await _questService.GetPlayerCompletedQuestsAsync(playerId);
            return Ok(ApiResponse<List<QuestProgress>>.Ok(quests));
        }

        /// <summary>
        /// 获取当前已完成任务的前端总览结构。
        /// </summary>
        [HttpGet("completed-overview")]
        public async Task<ActionResult<ApiResponse<List<QuestDto>>>> GetCompletedOverview()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var quests = await _questService.GetPlayerCompletedQuestsAsync(playerId);
            var overview = await MapProgressListToDtoAsync(quests);
            return Ok(ApiResponse<List<QuestDto>>.Ok(overview));
        }

        /// <summary>
        /// 获取单条任务配置详情。
        /// </summary>
        [HttpGet("{questId}")]
        public async Task<ActionResult<ApiResponse<QuestConfig>>> GetDetail(string questId)
        {
            var quest = await _questService.GetQuestConfigAsync(questId);
            if (quest == null)
            {
                return NotFound(ApiResponse<QuestConfig>.Fail("任务不存在"));
            }

            return Ok(ApiResponse<QuestConfig>.Ok(quest));
        }

        /// <summary>
        /// 接取任务。
        /// </summary>
        [HttpPost("accept")]
        public async Task<ActionResult<ApiResponse<QuestOperationResult>>> Accept([FromBody] AcceptQuestRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var result = await _questService.AcceptQuestAsync(player, request.QuestId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<QuestOperationResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<QuestOperationResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 提交任务。
        /// </summary>
        [HttpPost("submit")]
        public async Task<ActionResult<ApiResponse<QuestSubmitResult>>> Submit([FromBody] SubmitQuestRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                return NotFound(ApiResponse.Fail("玩家不存在"));
            }

            var result = await _questService.SubmitQuestAsync(player, request.QuestId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<QuestSubmitResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<QuestSubmitResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 放弃任务。
        /// </summary>
        [HttpDelete("{questId}")]
        public async Task<ActionResult<ApiResponse<QuestOperationResult>>> Abandon(string questId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _questService.AbandonQuestAsync(playerId, questId);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<QuestOperationResult>.Fail(result.Message));
            }

            return Ok(ApiResponse<QuestOperationResult>.Ok(result, result.Message));
        }

        /// <summary>
        /// 将任务进度列表映射成前端总览 DTO。
        /// </summary>
        private async Task<List<QuestDto>> MapProgressListToDtoAsync(List<QuestProgress> progresses)
        {
            var sortedProgresses = progresses
                .OrderBy(progress => progress.AcceptTime)
                .ToList();

            var questIds = sortedProgresses
                .Select(progress => progress.QuestId)
                .Distinct()
                .ToList();

            var configLookup = new Dictionary<string, QuestConfig>();
            foreach (var questId in questIds)
            {
                var config = await _questService.GetQuestConfigAsync(questId);
                if (config != null)
                {
                    configLookup[questId] = config;
                }
            }

            return sortedProgresses
                .Where(progress => configLookup.ContainsKey(progress.QuestId))
                .Select(progress => MapQuestToDto(configLookup[progress.QuestId], progress))
                .OrderBy(dto => dto.SortOrder)
                .ToList();
        }

        /// <summary>
        /// 将任务配置和玩家进度映射成前端总览 DTO。
        /// </summary>
        private static QuestDto MapQuestToDto(QuestConfig config, QuestProgress? progress)
        {
            var effectiveProgress = progress ?? new QuestProgress
            {
                QuestId = config.QuestId,
                Status = XXX.Quest.QuestStatus.NotAccepted
            };

            var objectives = config.Objectives
                .Select((objective, index) =>
                {
                    var currentProgress = Math.Max(0, effectiveProgress.GetObjectiveProgress(index));
                    var targetProgress = Math.Max(1, objective.TargetCount);
                    var progressPercent = Math.Min(100d, currentProgress * 100d / targetProgress);

                    return new QuestObjectiveDto
                    {
                        Index = index,
                        ObjectiveType = objective.ObjectiveType.ToString(),
                        ObjectiveTypeValue = (int)objective.ObjectiveType,
                        TargetId = objective.TargetId,
                        CurrentProgress = currentProgress,
                        TargetProgress = objective.TargetCount,
                        ProgressPercent = progressPercent,
                        IsCompleted = currentProgress >= objective.TargetCount,
                        Description = objective.Description
                    };
                })
                .ToList();

            var currentProgressTotal = objectives.Sum(objective => Math.Min(objective.CurrentProgress, Math.Max(0, objective.TargetProgress)));
            var targetProgressTotal = objectives.Sum(objective => Math.Max(0, objective.TargetProgress));

            return new QuestDto
            {
                QuestId = config.QuestId,
                Name = config.QuestName,
                Description = config.Description,
                Type = config.QuestType.ToString(),
                TypeValue = (int)config.QuestType,
                Icon = config.Icon,
                RequiredLevel = config.RequiredLevel,
                Status = effectiveProgress.Status.ToString(),
                StatusValue = (int)effectiveProgress.Status,
                CurrentProgress = currentProgressTotal,
                TargetProgress = targetProgressTotal,
                ProgressPercent = progress != null ? Math.Min(100d, config.GetProgressPercent(effectiveProgress)) : 0d,
                SortOrder = config.SortOrder,
                AutoSubmit = config.AutoSubmit,
                TimeLimit = config.TimeLimit,
                Objectives = objectives,
                Rewards = MapRewards(config.Rewards),
                AcceptTime = progress?.AcceptTime,
                CompleteTime = progress?.CompleteTime,
                SubmitTime = progress?.SubmitTime
            };
        }

        /// <summary>
        /// 将任务奖励映射成统一展示结构。
        /// </summary>
        private static List<QuestRewardDto> MapRewards(QuestReward reward)
        {
            var rewards = new List<QuestRewardDto>();

            void AddCurrencyReward(string rewardType, string name, long quantity)
            {
                if (quantity <= 0)
                {
                    return;
                }

                rewards.Add(new QuestRewardDto
                {
                    RewardType = rewardType,
                    Name = name,
                    Quantity = quantity > int.MaxValue ? int.MaxValue : (int)quantity
                });
            }

            AddCurrencyReward("Exp", "修为", reward.Exp);
            AddCurrencyReward("Gold", "金币", reward.Gold);
            AddCurrencyReward("SpiritStone", "灵石", reward.SpiritStone);
            AddCurrencyReward("Honor", "荣誉", reward.Honor);
            AddCurrencyReward("GuildContribution", "宗门贡献", reward.GuildContribution);

            foreach (var itemReward in reward.Items)
            {
                rewards.Add(new QuestRewardDto
                {
                    RewardType = "Item",
                    ItemId = itemReward.Key,
                    Name = ResolveQuestItemName(itemReward.Key),
                    Quantity = itemReward.Value
                });
            }

            foreach (var equipmentId in reward.EquipmentIds)
            {
                rewards.Add(new QuestRewardDto
                {
                    RewardType = "Equipment",
                    EquipmentId = equipmentId,
                    Name = ResolveQuestEquipmentName(equipmentId),
                    Quantity = 1
                });
            }

            return rewards;
        }

        private static string ResolveQuestItemName(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId) &&
                XXX.GameData.Items.TryGetValue(itemId, out var item)
                ? item.Name
                : itemId;
        }

        private static string ResolveQuestEquipmentName(int equipmentId)
        {
            return XXX.GameData.EquipmentTemplates.TryGetValue(equipmentId, out var equipment)
                ? equipment.Name
                : $"装备 {equipmentId}";
        }
    }
}
