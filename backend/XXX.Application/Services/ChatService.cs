using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 聊天服务实现。
    /// </summary>
    public class ChatService : IChatService
    {
        private const int MaxContentLength = 200;

        private readonly DbContext _dbContext;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<ChatService> _logger;

        /// <summary>
        /// 初始化聊天服务。
        /// </summary>
        public ChatService(
            DbContext dbContext,
            IRepository<UserEntity> userRepository,
            ILogger<ChatService> logger)
        {
            _dbContext = dbContext;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// 获取指定频道的历史聊天记录。
        /// </summary>
        public async Task<List<ChatMessageDto>> GetHistoryAsync(string playerId, string channelType, int count = 50)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var normalizedChannel = NormalizeChannelType(channelType);
            EnsureChannelAccess(player, normalizedChannel, isWrite: false);

            var safeCount = Math.Clamp(count, 1, 100);
            var messages = await _dbContext.Db.Queryable<ChatMessageEntity>()
                .Where(message => message.ChannelType == normalizedChannel)
                .OrderByDescending(message => message.SendTime)
                .Take(safeCount)
                .ToListAsync();

            // 批量查询发送者称号
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var titleMap = await GetPlayerTitlesAsync(senderIds);

            return messages
                .OrderBy(message => message.SendTime)
                .Select(m => MapToDto(m, titleMap.GetValueOrDefault(m.SenderId)))
                .ToList();
        }

        /// <summary>
        /// 发送一条聊天消息。
        /// </summary>
        public async Task<ChatMessageDto> SendMessageAsync(string playerId, SendMessageRequestDto request)
        {
            var player = await GetRequiredPlayerAsync(playerId);
            var normalizedChannel = NormalizeChannelType(request.ChannelType);
            EnsureChannelAccess(player, normalizedChannel, isWrite: true);

            var content = NormalizeContent(request.Content);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("聊天内容不能为空。");
            }

            var entity = new ChatMessageEntity
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ChannelType = normalizedChannel,
                SenderId = playerId,
                SenderName = player.Name,
                Content = content,
                SendTime = DateTime.Now
            };

            await _dbContext.Db.Insertable(entity).ExecuteCommandAsync();
            _logger.LogInformation(
                "Player {PlayerId} sent chat message to {ChannelType}. messageId={MessageId}",
                playerId,
                normalizedChannel,
                entity.MessageId);

            // 查询发送者称号
            var titleInfo = await GetPlayerTitleAsync(playerId);
            return MapToDto(entity, titleInfo);
        }

        private async Task<UserEntity> GetRequiredPlayerAsync(string playerId)
        {
            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在，无法使用聊天功能。");
            }

            return player;
        }

        private static void EnsureChannelAccess(UserEntity player, string channelType, bool isWrite)
        {
            switch (channelType)
            {
                case "world":
                    return;

                case "sect":
                    if (string.IsNullOrWhiteSpace(player.GuildId))
                    {
                        throw new InvalidOperationException("未加入公会，无法访问宗门频道。");
                    }

                    return;

                case "system":
                    if (isWrite)
                    {
                        throw new InvalidOperationException("系统频道只允许系统消息发送。");
                    }

                    return;

                default:
                    throw new InvalidOperationException("不支持的聊天频道。");
            }
        }

        private static string NormalizeChannelType(string? channelType)
        {
            var normalized = (channelType ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "world" => "world",
                "sect" => "sect",
                "system" => "system",
                _ => "world"
            };
        }

        private static string NormalizeContent(string? content)
        {
            var normalized = (content ?? string.Empty).Trim();
            if (normalized.Length > MaxContentLength)
            {
                normalized = normalized[..MaxContentLength];
            }

            return normalized;
        }

        private static ChatMessageDto MapToDto(ChatMessageEntity entity, (string? Title, string? Rarity, string? Icon)? titleInfo = null)
        {
            return new ChatMessageDto
            {
                MessageId = entity.MessageId,
                SenderId = entity.SenderId,
                SenderName = entity.SenderName,
                ChannelType = entity.ChannelType,
                Content = entity.Content,
                SendTime = entity.SendTime,
                SenderTitle = titleInfo?.Title,
                SenderTitleRarity = titleInfo?.Rarity,
                SenderTitleIcon = titleInfo?.Icon
            };
        }

        private async Task<(string? Title, string? Rarity, string? Icon)?> GetPlayerTitleAsync(string playerId)
        {
            var playerTitle = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(pt => pt.PlayerId == playerId && pt.IsEquipped)
                .FirstAsync();

            if (playerTitle == null) return null;

            var template = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => t.TitleId == playerTitle.TitleId)
                .FirstAsync();

            if (template == null) return null;

            return (template.Name, template.Rarity, template.IconPath);
        }

        private async Task<Dictionary<string, (string? Title, string? Rarity, string? Icon)>> GetPlayerTitlesAsync(List<string> playerIds)
        {
            if (playerIds.Count == 0) return new Dictionary<string, (string? Title, string? Rarity, string? Icon)>();

            var playerTitles = await _dbContext.Db.Queryable<PlayerTitleEntity>()
                .Where(pt => playerIds.Contains(pt.PlayerId) && pt.IsEquipped)
                .ToListAsync();

            if (playerTitles.Count == 0) return new Dictionary<string, (string? Title, string? Rarity, string? Icon)>();

            var titleIds = playerTitles.Select(pt => pt.TitleId).Distinct().ToList();
            var templates = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                .Where(t => titleIds.Contains(t.TitleId))
                .ToListAsync();

            var templateMap = templates.ToDictionary(t => t.TitleId, t => (t.Name, t.Rarity, t.IconPath));
            var result = new Dictionary<string, (string? Title, string? Rarity, string? Icon)>();

            foreach (var pt in playerTitles)
            {
                if (templateMap.TryGetValue(pt.TitleId, out var info))
                {
                    result[pt.PlayerId] = info;
                }
            }

            return result;
        }
    }
}
