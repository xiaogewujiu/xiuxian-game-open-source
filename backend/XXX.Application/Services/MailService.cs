using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Balance;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX;

namespace XXX.Application.Services
{
    /// <summary>
    /// 玩家邮件服务。
    /// </summary>
    public class MailService : IMailService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<MailMessageEntity> _mailRepository;
        private readonly IRepository<MailGlobalClaimRecordEntity> _claimRecordRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly ITitleService _titleService;
        private readonly ILogger<MailService> _logger;

        public MailService(
            DbContext dbContext,
            IRepository<MailMessageEntity> mailRepository,
            IRepository<MailGlobalClaimRecordEntity> claimRecordRepository,
            IRepository<UserEntity> userRepository,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            ITitleService titleService,
            ILogger<MailService> logger)
        {
            _dbContext = dbContext;
            _mailRepository = mailRepository;
            _claimRecordRepository = claimRecordRepository;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
            _equipmentRepository = equipmentRepository;
            _titleService = titleService;
            _logger = logger;
        }

        /// <summary>
        /// 获取邮件列表。
        /// </summary>
        public async Task<MailPagedResultDto> GetMailsAsync(string playerId, int page = 1, int pageSize = 20)
        {
            var now = DateTime.Now;

            // 查询个人邮件 + 未过期全服邮件
            var query = _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => (m.RecipientId == playerId || (m.IsGlobal && m.ExpireAt > now)))
                .OrderByDescending(m => m.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 全服邮件需要检查领取状态和已读状态
            var globalMailIds = items.Where(m => m.IsGlobal).Select(m => m.Id).ToList();
            var claimedRecords = globalMailIds.Count > 0
                ? await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => globalMailIds.Contains(r.MailId) && r.PlayerId == playerId)
                    .ToListAsync()
                : new List<MailGlobalClaimRecordEntity>();
            var claimedMailIds = claimedRecords.Where(r => r.HasClaimed).Select(r => r.MailId).ToHashSet();
            var readMailIds = claimedRecords.Where(r => r.IsRead).Select(r => r.MailId).ToHashSet();

            // 计算未读数：个人邮件未读 + 全服邮件未读（不在readMailIds中的）
            var personalUnread = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.RecipientId == playerId && !m.IsRead)
                .CountAsync();
            var globalMailIdsAll = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.IsGlobal && m.ExpireAt > now)
                .Select(m => m.Id)
                .ToListAsync();
            var globalReadIds = globalMailIdsAll.Count > 0
                ? (await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => globalMailIdsAll.Contains(r.MailId) && r.PlayerId == playerId && r.IsRead)
                    .Select(r => r.MailId)
                    .ToListAsync()).ToHashSet()
                : new HashSet<long>();
            var globalUnread = globalMailIdsAll.Count(id => !globalReadIds.Contains(id));
            var unreadCount = personalUnread + globalUnread;

            var dtos = items.Select(m => MapToListItem(m, playerId, claimedMailIds, readMailIds)).ToList();

            return new MailPagedResultDto
            {
                Items = dtos,
                Total = total,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        public async Task<MailDetailDto?> GetMailDetailAsync(string playerId, long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null) return null;

            // 校验权限：个人邮件或全服邮件
            if (mail.RecipientId != playerId && !mail.IsGlobal) return null;
            if (mail.IsGlobal && mail.ExpireAt <= DateTime.Now) return null;

            var claimedMailIds = new HashSet<long>();
            var readMailIds = new HashSet<long>();
            if (mail.IsGlobal)
            {
                var record = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => r.MailId == mailId && r.PlayerId == playerId)
                    .FirstAsync();
                if (record != null)
                {
                    if (record.HasClaimed) claimedMailIds.Add(mailId);
                    if (record.IsRead) readMailIds.Add(mailId);
                }
            }

            var dto = MapToDetail(mail, playerId, claimedMailIds, readMailIds);
            dto.ResolvedAttachmentsJson = ResolveAttachmentNames(dto.AttachmentsJson);
            return dto;
        }

        private string? ResolveAttachmentNames(string? attachmentsJson)
        {
            if (string.IsNullOrWhiteSpace(attachmentsJson)) return null;

            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(attachmentsJson);
                var root = doc.RootElement;
                var resolved = new System.Text.Json.Nodes.JsonObject();

                if (root.TryGetProperty("gold", out var gold))
                    resolved["gold"] = gold.GetInt32();
                if (root.TryGetProperty("spiritStone", out var ss))
                    resolved["spiritStone"] = ss.GetInt32();

                if (root.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var arr = new System.Text.Json.Nodes.JsonArray();
                    foreach (var item in items.EnumerateArray())
                    {
                        var itemId = item.GetProperty("itemId").GetString() ?? "";
                        var quantity = item.GetProperty("quantity").GetInt32();
                        var name = GameData.Items.TryGetValue(itemId, out var tpl) ? tpl.Name : itemId;
                        arr.Add(new System.Text.Json.Nodes.JsonObject
                        {
                            ["itemId"] = itemId,
                            ["name"] = name,
                            ["quantity"] = quantity
                        });
                    }
                    resolved["items"] = arr;
                }

                if (root.TryGetProperty("equipment", out var equips) && equips.GetArrayLength() > 0)
                {
                    var arr = new System.Text.Json.Nodes.JsonArray();
                    foreach (var eq in equips.EnumerateArray())
                    {
                        var tidElem = eq.GetProperty("templateId");
                        int templateId;
                        if (tidElem.ValueKind == System.Text.Json.JsonValueKind.String)
                            int.TryParse(tidElem.GetString(), out templateId);
                        else
                            templateId = tidElem.GetInt32();
                        var quality = eq.TryGetProperty("quality", out var q) ? q.GetInt32() : 1;
                        var name = GameData.EquipmentTemplates.TryGetValue(templateId, out var tpl) ? tpl.Name : $"装备{templateId}";
                        arr.Add(new System.Text.Json.Nodes.JsonObject
                        {
                            ["templateId"] = templateId,
                            ["name"] = name,
                            ["quality"] = quality
                        });
                    }
                    resolved["equipment"] = arr;
                }

                if (root.TryGetProperty("titles", out var titles) && titles.GetArrayLength() > 0)
                {
                    var arr = new System.Text.Json.Nodes.JsonArray();
                    foreach (var t in titles.EnumerateArray())
                    {
                        var titleId = t.GetProperty("titleId").GetString() ?? "";
                        var template = _dbContext.Db.Queryable<TitleTemplateEntity>()
                            .Where(x => x.TitleId == titleId).First();
                        var name = template?.Name ?? titleId;
                        arr.Add(new System.Text.Json.Nodes.JsonObject
                        {
                            ["titleId"] = titleId,
                            ["name"] = name
                        });
                    }
                    resolved["titles"] = arr;
                }

                return resolved.ToJsonString();
            }
            catch
            {
                return attachmentsJson;
            }
        }

        /// <summary>
        /// 标记邮件已读。
        /// </summary>
        public async Task<bool> MarkAsReadAsync(string playerId, long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null) return false;
            if (mail.RecipientId != playerId && !mail.IsGlobal) return false;

            if (mail.IsGlobal)
            {
                // 全服邮件：在 ClaimRecord 中记录已读状态
                var record = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => r.MailId == mailId && r.PlayerId == playerId)
                    .FirstAsync();

                if (record != null)
                {
                    if (!record.IsRead)
                    {
                        record.IsRead = true;
                        await _dbContext.Db.Updateable(record).ExecuteCommandAsync();
                    }
                }
                else
                {
                    await _dbContext.Db.Insertable(new MailGlobalClaimRecordEntity
                    {
                        MailId = mailId,
                        PlayerId = playerId,
                        ClaimedAt = DateTime.Now,
                        IsRead = true,
                        HasClaimed = false
                    }).ExecuteCommandAsync();
                }
            }
            else if (!mail.IsRead)
            {
                // 个人邮件：直接更新共享行
                mail.IsRead = true;
                await _mailRepository.UpdateAsync(mail);
            }

            return true;
        }

        /// <summary>
        /// 领取邮件附件。
        /// </summary>
        public async Task<MailClaimResultDto> ClaimAttachmentsAsync(string playerId, long mailId)
        {
            _logger.LogInformation("ClaimAttachmentsAsync 被调用。PlayerId={PlayerId}, MailId={MailId}", playerId, mailId);

            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null)
                throw new InvalidOperationException("邮件不存在。");

            if (mail.RecipientId != playerId && !mail.IsGlobal)
                throw new InvalidOperationException("无权操作此邮件。");

            if (mail.IsGlobal && mail.ExpireAt <= DateTime.Now)
                throw new InvalidOperationException("邮件已过期。");

            // 检查全服邮件是否已领取
            if (mail.IsGlobal)
            {
                var existing = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => r.MailId == mailId && r.PlayerId == playerId && r.HasClaimed)
                    .FirstAsync();
                if (existing != null)
                    throw new InvalidOperationException("附件已领取。");
            }
            else if (mail.IsClaimed)
            {
                throw new InvalidOperationException("附件已领取。");
            }

            if (string.IsNullOrWhiteSpace(mail.AttachmentsJson))
                throw new InvalidOperationException("该邮件没有附件。");

            var attachments = JsonSerializer.Deserialize<MailAttachmentsDto>(mail.AttachmentsJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (attachments == null)
                throw new InvalidOperationException("附件数据解析失败。");

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null)
                throw new InvalidOperationException("玩家不存在。");

            var rewards = new List<RewardItemDto>();

            _logger.LogInformation("开始领取邮件附件。PlayerId={PlayerId}, MailId={MailId}, Gold={Gold}, SpiritStone={SS}, Items={ItemCount}, Equipment={EquipCount}, Titles={TitleCount}",
                playerId, mailId, attachments.Gold, attachments.SpiritStone,
                attachments.Items?.Count ?? 0, attachments.Equipment?.Count ?? 0, attachments.Titles?.Count ?? 0);

            _dbContext.BeginTransaction();
            try
            {
                // 发放金币
                if (attachments.Gold > 0)
                {
                    player.Gold += attachments.Gold;
                    player.TotalGoldEarned += attachments.Gold;
                    rewards.Add(new RewardItemDto { Type = "gold", Name = "金币", Count = attachments.Gold });
                }

                // 发放灵石
                if (attachments.SpiritStone > 0)
                {
                    player.SpiritStone += attachments.SpiritStone;
                    rewards.Add(new RewardItemDto { Type = "spiritStone", Name = "灵石", Count = attachments.SpiritStone });
                }

                // 发放道具
                if (attachments.Items != null)
                {
                    foreach (var item in attachments.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.ItemId) || item.Quantity <= 0) continue;

                        await InventoryItemGrantHelper.AddOrMergeAsync(
                            _dbContext.Db,
                            playerId,
                            item.ItemId,
                            item.Quantity,
                            "道具背包已满，无法领取邮件附件。");

                        var itemName = (await _dbContext.Db.Queryable<ItemTemplateEntity>()
                            .FirstAsync(t => t.ItemId == item.ItemId))?.Name ?? item.ItemId;
                        rewards.Add(new RewardItemDto { Type = "item", Name = itemName, Count = item.Quantity });
                    }
                }

                // 发放装备
                if (attachments.Equipment != null)
                {
                    var usedEquipmentSlots = await _dbContext.Db.Queryable<EquipmentInstanceEntity>()
                        .Where(equipment => equipment.PlayerId == playerId && !equipment.IsEquipped)
                        .CountAsync();
                    var equipmentCapacity = await InventoryCapacityRules.GetEquipmentCapacityAsync(_dbContext.Db, playerId);
                    foreach (var equip in attachments.Equipment)
                    {
                        if (string.IsNullOrWhiteSpace(equip.TemplateId)) continue;

                        if (!int.TryParse(equip.TemplateId, out var templateId)) continue;
                        if (!GameData.EquipmentTemplates.TryGetValue(templateId, out var template)) continue;

                        if (usedEquipmentSlots >= equipmentCapacity)
                        {
                            throw new InvalidOperationException("装备背包已满，无法领取邮件附件。");
                        }

                        var equipmentInstance = new EquipmentInstance
                        {
                            InstanceId = Guid.NewGuid().ToString("N"),
                            Template = template,
                            EnhanceLevel = 0,
                            RerolledAttrs = [],
                            RerollCount = 0,
                            Type1 = EquipmentBalanceHelper.RandomRange(template.MinType1, template.MaxType1),
                            Type2 = EquipmentBalanceHelper.RandomRange(template.MinType2, template.MaxType2),
                            Type3 = EquipmentBalanceHelper.RandomRange(template.MinType3, template.MaxType3),
                            Type4 = EquipmentBalanceHelper.RandomRange(template.MinType4, template.MaxType4),
                            Type5 = EquipmentBalanceHelper.RandomRange(template.MinType5, template.MaxType5),
                            Type6 = EquipmentBalanceHelper.RandomRange(template.MinType6, template.MaxType6),
                            Type7 = EquipmentBalanceHelper.RandomRange(template.MinType7, template.MaxType7),
                            Type8 = EquipmentBalanceHelper.RandomRange(template.MinType8, template.MaxType8) / 100f,
                            Type9 = EquipmentBalanceHelper.RandomRange(template.MinType9, template.MaxType9) / 100f,
                            Type10 = EquipmentBalanceHelper.RandomRange(template.MinType10, template.MaxType10) / 100f,
                            Type11 = EquipmentBalanceHelper.RandomRange(template.MinType11, template.MaxType11) / 100f,
                            Type12 = EquipmentBalanceHelper.RandomRange(template.MinType12, template.MaxType12) / 100f,
                            Type13 = EquipmentBalanceHelper.RandomRange(template.MinType13, template.MaxType13) / 100f,
                            Type14 = EquipmentBalanceHelper.RandomRange(template.MinType14, template.MaxType14) / 100f,
                            Type15 = EquipmentBalanceHelper.RandomRange(template.MinType15, template.MaxType15) / 100f
                        };

                        var entity = EquipmentBalanceHelper.CreateEntity(playerId, equipmentInstance, false);
                        await _equipmentRepository.AddAsync(entity);
                        usedEquipmentSlots++;

                        rewards.Add(new RewardItemDto { Type = "item", Name = template.Name, Count = 1 });
                    }
                }

                // 发放称号
                if (attachments.Titles != null)
                {
                    foreach (var titleAttach in attachments.Titles)
                    {
                        if (string.IsNullOrWhiteSpace(titleAttach.TitleId)) continue;

                        var granted = await _titleService.GrantTitleAsync(playerId, titleAttach.TitleId);
                        var titleTemplate = await _dbContext.Db.Queryable<TitleTemplateEntity>()
                            .Where(t => t.TitleId == titleAttach.TitleId).FirstAsync();
                        var titleName = titleTemplate?.Name ?? titleAttach.TitleId;
                        rewards.Add(new RewardItemDto
                        {
                            Type = "title",
                            Name = titleName,
                            Count = granted ? 1 : 0
                        });
                    }
                }

                // 更新玩家
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);

                // 标记领取状态和已读
                if (mail.IsGlobal)
                {
                    var existingRecord = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                        .Where(r => r.MailId == mailId && r.PlayerId == playerId)
                        .FirstAsync();
                    if (existingRecord != null)
                    {
                        existingRecord.HasClaimed = true;
                        existingRecord.ClaimedAt = DateTime.Now;
                        existingRecord.IsRead = true;
                        await _dbContext.Db.Updateable(existingRecord).ExecuteCommandAsync();
                    }
                    else
                    {
                        await _claimRecordRepository.AddAsync(new MailGlobalClaimRecordEntity
                        {
                            MailId = mailId,
                            PlayerId = playerId,
                            ClaimedAt = DateTime.Now,
                            IsRead = true,
                            HasClaimed = true
                        });
                    }
                }
                else
                {
                    mail.IsClaimed = true;
                    mail.IsRead = true;
                    await _mailRepository.UpdateAsync(mail);
                }

                _logger.LogInformation("准备提交事务。PlayerId={PlayerId}, MailId={MailId}, Gold={Gold}, Rewards={RewardCount}",
                    playerId, mailId, player.Gold, rewards.Count);

                _dbContext.CommitTransaction();

                _logger.LogInformation("邮件附件领取成功。PlayerId={PlayerId}, MailId={MailId}", playerId, mailId);

                return new MailClaimResultDto
                {
                    Success = true,
                    Message = "附件领取成功。",
                    Rewards = rewards
                };
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogError(ex, "邮件附件领取失败（已回滚）。PlayerId={PlayerId}, MailId={MailId}, Error={Error}", playerId, mailId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 一键领取所有邮件附件。
        /// </summary>
        public async Task<MailClaimAllResultDto> ClaimAllAsync(string playerId)
        {
            var now = DateTime.Now;
            var result = new MailClaimAllResultDto();

            // 查询所有有未领取附件的邮件
            var personalMails = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.RecipientId == playerId && !m.IsClaimed && m.AttachmentsJson != null && m.AttachmentsJson != "")
                .ToListAsync();

            var globalMails = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.IsGlobal && m.ExpireAt > now && m.AttachmentsJson != null && m.AttachmentsJson != "")
                .ToListAsync();

            // 过滤掉已领取的全服邮件
            var globalMailIds = globalMails.Select(m => m.Id).ToList();
            var claimedRecords = globalMailIds.Count > 0
                ? await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => globalMailIds.Contains(r.MailId) && r.PlayerId == playerId)
                    .ToListAsync()
                : new List<MailGlobalClaimRecordEntity>();
            var claimedMailIds = claimedRecords.Where(r => r.HasClaimed).Select(r => r.MailId).ToHashSet();

            var allMails = personalMails
                .Concat(globalMails.Where(m => !claimedMailIds.Contains(m.Id)))
                .ToList();

            foreach (var mail in allMails)
            {
                try
                {
                    var claimResult = await ClaimAttachmentsAsync(playerId, mail.Id);
                    result.ClaimedCount++;
                    result.TotalRewards.AddRange(claimResult.Rewards);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"邮件 {mail.Id}: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 删除邮件。
        /// </summary>
        public async Task<bool> DeleteMailAsync(string playerId, long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null) return false;

            // 只能删除个人邮件（全服邮件不能删除，只能忽略）
            if (mail.RecipientId != playerId) return false;

            await _mailRepository.DeleteAsync(mailId);
            return true;
        }

        /// <summary>
        /// 全部标为已读。
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string playerId)
        {
            var now = DateTime.Now;

            // 个人邮件：直接标记已读
            var personalMails = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.RecipientId == playerId && !m.IsRead)
                .ToListAsync();

            foreach (var mail in personalMails)
            {
                mail.IsRead = true;
            }

            if (personalMails.Count > 0)
            {
                await _mailRepository.UpdateRangeAsync(personalMails);
            }

            // 全服邮件：在 ClaimRecord 中标记已读
            var globalMails = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.IsGlobal && m.ExpireAt > now)
                .ToListAsync();

            var globalMailIds = globalMails.Select(m => m.Id).ToList();
            if (globalMailIds.Count > 0)
            {
                var existingRecords = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => globalMailIds.Contains(r.MailId) && r.PlayerId == playerId)
                    .ToListAsync();
                var existingMailIds = existingRecords.Select(r => r.MailId).ToHashSet();

                // 更新已有的未读记录
                var recordsToUpdate = existingRecords.Where(r => !r.IsRead).ToList();
                foreach (var record in recordsToUpdate)
                {
                    record.IsRead = true;
                }
                if (recordsToUpdate.Count > 0)
                {
                    await _dbContext.Db.Updateable(recordsToUpdate).ExecuteCommandAsync();
                }

                // 为没有记录的全服邮件创建已读记录
                var newRecords = globalMailIds
                    .Where(id => !existingMailIds.Contains(id))
                    .Select(id => new MailGlobalClaimRecordEntity
                    {
                        MailId = id,
                        PlayerId = playerId,
                        ClaimedAt = DateTime.Now,
                        IsRead = true
                    })
                    .ToList();

                if (newRecords.Count > 0)
                {
                    await _dbContext.Db.Insertable(newRecords).ExecuteCommandAsync();
                }
            }

            return personalMails.Count + globalMails.Count;
        }

        /// <summary>
        /// 发送邮件（系统内部调用）。
        /// </summary>
        public async Task<long> SendMailAsync(string? recipientId, string senderType, string senderName, string title, string content, string? attachmentsJson = null, bool isGlobal = false)
        {
            var entity = new MailMessageEntity
            {
                RecipientId = isGlobal ? null : recipientId,
                SenderType = senderType,
                SenderName = senderName,
                Title = title,
                Content = content,
                AttachmentsJson = attachmentsJson,
                IsRead = false,
                IsClaimed = false,
                IsGlobal = isGlobal,
                CreatedAt = DateTime.Now,
                ExpireAt = DateTime.Now.AddDays(30)
            };

            await _mailRepository.AddAsync(entity);
            _logger.LogInformation("邮件发送成功。MailId={MailId}, Recipient={Recipient}, IsGlobal={IsGlobal}", entity.Id, recipientId, isGlobal);
            return entity.Id;
        }

        /// <summary>
        /// 清理过期邮件。
        /// </summary>
        public async Task<int> CleanupExpiredMailsAsync()
        {
            var now = DateTime.Now;
            var expired = await _dbContext.Db.Queryable<MailMessageEntity>()
                .Where(m => m.ExpireAt <= now)
                .ToListAsync();

            if (expired.Count > 0)
            {
                foreach (var mail in expired)
                {
                    await _mailRepository.DeleteAsync(mail.Id);
                }

                // 清理对应的全服邮件领取记录
                var expiredIds = expired.Select(m => m.Id).ToList();
                var claimRecords = await _dbContext.Db.Queryable<MailGlobalClaimRecordEntity>()
                    .Where(r => expiredIds.Contains(r.MailId))
                    .ToListAsync();
                foreach (var record in claimRecords)
                {
                    await _claimRecordRepository.DeleteAsync(record.Id);
                }

                _logger.LogInformation("过期邮件清理完成。Count={Count}", expired.Count);
            }

            return expired.Count;
        }

        private static MailListItemDto MapToListItem(MailMessageEntity mail, string playerId, HashSet<long> claimedMailIds, HashSet<long> readMailIds)
        {
            var hasAttachments = !string.IsNullOrWhiteSpace(mail.AttachmentsJson);
            var isClaimed = mail.IsGlobal ? claimedMailIds.Contains(mail.Id) : mail.IsClaimed;
            var isRead = mail.IsGlobal ? readMailIds.Contains(mail.Id) : mail.IsRead;

            return new MailListItemDto
            {
                Id = mail.Id,
                Title = mail.Title,
                SenderName = mail.SenderName,
                SenderType = mail.SenderType,
                IsRead = isRead,
                IsClaimed = isClaimed,
                HasAttachments = hasAttachments,
                IsGlobal = mail.IsGlobal,
                CreatedAt = mail.CreatedAt,
                ExpireAt = mail.ExpireAt
            };
        }

        private static MailDetailDto MapToDetail(MailMessageEntity mail, string playerId, HashSet<long> claimedMailIds, HashSet<long> readMailIds)
        {
            var hasAttachments = !string.IsNullOrWhiteSpace(mail.AttachmentsJson);
            var isClaimed = mail.IsGlobal ? claimedMailIds.Contains(mail.Id) : mail.IsClaimed;
            var isRead = mail.IsGlobal ? readMailIds.Contains(mail.Id) : mail.IsRead;

            return new MailDetailDto
            {
                Id = mail.Id,
                Title = mail.Title,
                Content = mail.Content,
                SenderName = mail.SenderName,
                SenderType = mail.SenderType,
                AttachmentsJson = mail.AttachmentsJson,
                IsRead = isRead,
                IsClaimed = isClaimed,
                HasAttachments = hasAttachments,
                IsGlobal = mail.IsGlobal,
                CreatedAt = mail.CreatedAt,
                ExpireAt = mail.ExpireAt
            };
        }
    }
}
