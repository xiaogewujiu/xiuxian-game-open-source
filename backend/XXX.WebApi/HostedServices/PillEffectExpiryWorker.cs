using System.Data.Common;
using Microsoft.Data.Sqlite;
using SqlSugar;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.WebApi.HostedServices
{
    /// <summary>
    /// 丹药限时效果过期清理后台服务。
    /// 每 30 秒扫描一次已过期的临时丹药效果，自动回滚属性加成并删除记录。
    /// </summary>
    public sealed class PillEffectExpiryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PillEffectExpiryWorker> _logger;

        public PillEffectExpiryWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<PillEffectExpiryWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider.GetRequiredService<DbContext>().Db;
                    var syncService = scope.ServiceProvider.GetRequiredService<IGameSyncService>();
                    var attributeService = scope.ServiceProvider.GetRequiredService<IPlayerAttributeService>();

                    await ProcessExpiredEffectsAsync(db, syncService, attributeService, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "丹药过期效果清理 Tick 执行失败。");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 扫描并处理所有已过期的临时丹药效果。
        /// </summary>
        private async Task ProcessExpiredEffectsAsync(
            ISqlSugarClient db,
            IGameSyncService syncService,
            IPlayerAttributeService attributeService,
            CancellationToken ct)
        {
            const int batchSize = 100;
            var now = DateTime.Now;

            while (!ct.IsCancellationRequested)
            {
                // 查询一批已过期的临时效果记录。
                var expiredRecords = await db.Queryable<PlayerPillEffectEntity>()
                    .Where(e => e.IsTemporary && e.ExpiresAt != null && e.ExpiresAt <= now)
                    .Take(batchSize)
                    .ToListAsync();

                if (expiredRecords.Count == 0)
                {
                    break;
                }

                foreach (var record in expiredRecords)
                {
                    try
                    {
                        await ReverseEffectAsync(db, syncService, attributeService, record);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "回滚过期丹药效果失败，记录 Id={RecordId}，玩家={PlayerId}。",
                            record.Id, record.PlayerId);
                    }
                }
            }
        }

        /// <summary>
        /// 回滚单条过期丹药效果：撤销属性加成、删除记录、同步玩家数据。
        /// </summary>
        private async Task ReverseEffectAsync(
            ISqlSugarClient db,
            IGameSyncService syncService,
            IPlayerAttributeService attributeService,
            PlayerPillEffectEntity record)
        {
            var player = await db.Queryable<UserEntity>()
                .Where(u => u.GID == record.PlayerId)
                .FirstAsync();

            if (player == null)
            {
                // 玩家不存在，直接删除过期记录。
                await db.Deleteable<PlayerPillEffectEntity>()
                    .Where(e => e.Id == record.Id)
                    .ExecuteCommandAsync();
                return;
            }

            var effectType = (ItemPillEffectType)record.EffectType;
            string? updatedPillBonusJson = null;

            switch (effectType)
            {
                case ItemPillEffectType.BreakthroughChance:
                {
                    var newValue = Math.Max(0, player.BreakthroughBonusPercent - (int)record.BonusValue);
                    await db.Ado.ExecuteCommandAsync(
                        "UPDATE Users SET BreakthroughBonusPercent = @val WHERE GID = @gid",
                        new { val = newValue, gid = player.GID });
                    break;
                }

                case ItemPillEffectType.AddAttribute:
                {
                    var attrType = ResolveAttributeTypeFromName(record.BonusType ?? string.Empty);
                    if (attrType.HasValue)
                    {
                        var pillBonuses = player.PillBonusAttributes ?? [];
                        var target = pillBonuses.FirstOrDefault(b => b.type == attrType.Value);
                        if (target != null)
                        {
                            target.value = Math.Max(0, target.value - (float)record.BonusValue);
                            if (target.value <= 0)
                            {
                                pillBonuses.Remove(target);
                            }
                        }

                        updatedPillBonusJson = System.Text.Json.JsonSerializer.Serialize(pillBonuses);
                    }

                    break;
                }

                default:
                    // AddExp 等其它类型不做回滚，直接删除记录。
                    break;
            }

            // 删除过期记录。
            await db.Deleteable<PlayerPillEffectEntity>()
                .Where(e => e.Id == record.Id)
                .ExecuteCommandAsync();

            // 用原始 ADO.NET 更新 PillBonusAttributesJson，彻底绕过 SqlSugar 实体跟踪。
            if (updatedPillBonusJson != null)
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection");
                await using var conn = new SqliteConnection(connStr);
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Users SET PillBonusAttributesJson = @json WHERE GID = @gid";
                cmd.Parameters.AddWithValue("@json", updatedPillBonusJson);
                cmd.Parameters.AddWithValue("@gid", player.GID);
                var affected = await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation(
                    "ADO.NET 更新过期丹药 PillBonusAttributesJson: PlayerId={PlayerId}, Affected={Affected}",
                    player.GID, affected);
            }

            // 创建新 scope 重算属性，确保读取到最新的数据库值。
            await using var recalcScope = _scopeFactory.CreateAsyncScope();
            var recalcService = recalcScope.ServiceProvider.GetRequiredService<IPlayerAttributeService>();
            await recalcService.RecalculatePlayerAttributesAsync(record.PlayerId);
        }

        /// <summary>
        /// 将中文属性名称反向映射为 AttributeType 枚举值。
        /// </summary>
        private static AttributeType? ResolveAttributeTypeFromName(string name)
        {
            return name switch
            {
                "生命" => AttributeType.Type1,
                "法力" => AttributeType.Type2,
                "物攻" => AttributeType.Type3,
                "法攻" => AttributeType.Type4,
                "物防" => AttributeType.Type5,
                "法防" => AttributeType.Type6,
                "速度" => AttributeType.Type7,
                "命中" => AttributeType.Type8,
                "闪避" => AttributeType.Type9,
                "暴击" => AttributeType.Type10,
                "暴伤" => AttributeType.Type11,
                "连击" => AttributeType.Type12,
                "反击" => AttributeType.Type13,
                "破甲" => AttributeType.Type14,
                "增伤" => AttributeType.Type15,
                _ => null
            };
        }
    }
}
