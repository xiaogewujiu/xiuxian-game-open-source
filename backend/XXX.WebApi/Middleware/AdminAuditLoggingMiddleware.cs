using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using XXX.Entity;
using XXX.Infrastructure.Authentication;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.WebApi.Middleware
{
    /// <summary>
    /// 管理后台审计日志中间件。
    /// 记录后台非 GET 请求的请求体、响应体以及资源前后快照，作为第一版审计入口。
    /// </summary>
    public class AdminAuditLoggingMiddleware
    {
        private static readonly JsonSerializerOptions SnapshotJsonOptions = new(JsonSerializerDefaults.Web);
        private readonly RequestDelegate _next;

        /// <summary>
        /// 初始化审计日志中间件。
        /// </summary>
        public AdminAuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行审计日志记录。
        /// </summary>
        public async Task InvokeAsync(HttpContext context, IRepository<AdminAuditLogEntity> auditRepository, DbContext dbContext)
        {
            if (!ShouldLog(context.Request))
            {
                await _next(context);
                return;
            }

            var requestPath = context.Request.Path.Value ?? string.Empty;
            var resourceKey = ResolveResourceKey(requestPath);
            var requestJson = SanitizeJsonPayload(await ReadRequestBodyAsync(context.Request));
            var targetId = ResolveTargetId(context, resourceKey, requestJson, null);
            var beforeSnapshotJson = SerializeSnapshot(await LoadSnapshotAsync(dbContext, resourceKey, targetId));

            var originalResponseBody = context.Response.Body;
            await using var responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            Exception? exception = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                if (context.Response.StatusCode < StatusCodes.Status400BadRequest)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }

            responseBuffer.Position = 0;
            var responseJson = SanitizeJsonPayload(await ReadResponseBodyAsync(responseBuffer));
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            var afterTargetId = ResolveTargetId(context, resourceKey, requestJson, responseJson) ?? targetId;
            var afterSnapshotJson = SerializeSnapshot(await LoadSnapshotAsync(dbContext, resourceKey, afterTargetId));

            var log = new AdminAuditLogEntity
            {
                LogId = Guid.NewGuid().ToString("N"),
                OperatorId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst(JwtClaims.UserId)?.Value,
                OperatorName = context.User.FindFirst(ClaimTypes.Name)?.Value
                    ?? context.User.FindFirst(JwtClaims.UserName)?.Value,
                OperatorRole = context.User.FindFirst(ClaimTypes.Role)?.Value
                    ?? context.User.FindFirst(JwtClaims.Role)?.Value,
                HttpMethod = context.Request.Method,
                Path = requestPath,
                ResourceKey = resourceKey,
                TargetId = afterTargetId,
                RequestJson = requestJson,
                ResponseJson = responseJson,
                BeforeJson = beforeSnapshotJson,
                AfterJson = afterSnapshotJson,
                DiffJson = BuildDiffJson(beforeSnapshotJson, afterSnapshotJson),
                StatusCode = context.Response.StatusCode,
                Success = exception == null && context.Response.StatusCode < StatusCodes.Status400BadRequest,
                ErrorMessage = exception?.Message ?? (context.Response.StatusCode >= StatusCodes.Status400BadRequest ? $"HTTP {context.Response.StatusCode}" : null),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                CreateTime = DateTime.Now
            };

            await auditRepository.AddAsync(log);

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        private static bool ShouldLog(HttpRequest request)
        {
            if (request.Path.Value == null)
            {
                return false;
            }

            if (!request.Path.Value.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !HttpMethods.IsGet(request.Method);
        }

        private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return string.IsNullOrWhiteSpace(body) ? null : body;
        }

        private static async Task<string?> ReadResponseBodyAsync(Stream responseBody)
        {
            using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            return string.IsNullOrWhiteSpace(body) ? null : body;
        }

        private static string? ResolveResourceKey(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 3 ? segments[2] : null;
        }

        private static string? ResolveTargetId(HttpContext context, string? resourceKey, string? requestJson, string? responseJson)
        {
            foreach (var candidateKey in GetCandidateIdKeys(resourceKey))
            {
                if (context.Request.RouteValues.TryGetValue(candidateKey, out var routeValue) && routeValue != null)
                {
                    return routeValue.ToString();
                }
            }

            return ResolveTargetIdFromJson(resourceKey, requestJson)
                ?? ResolveTargetIdFromJson(resourceKey, responseJson)
                ?? ResolveDefaultTargetId(context, resourceKey);
        }

        private static string? ResolveTargetIdFromJson(string? resourceKey, string? json)
        {
            if (string.IsNullOrWhiteSpace(resourceKey) || string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (TryResolveTargetIdFromElement(resourceKey, root, out var directTargetId))
                {
                    return directTargetId;
                }

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataElement))
                {
                    if (TryResolveTargetIdFromElement(resourceKey, dataElement, out var nestedTargetId))
                    {
                        return nestedTargetId;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool TryResolveTargetIdFromElement(string resourceKey, JsonElement element, out string? targetId)
        {
            targetId = null;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var candidateKey in GetCandidateIdKeys(resourceKey))
            {
                if (!TryGetPropertyIgnoreCase(element, candidateKey, out var valueElement))
                {
                    continue;
                }

                targetId = JsonElementToString(valueElement);
                if (!string.IsNullOrWhiteSpace(targetId))
                {
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<string> GetCandidateIdKeys(string? resourceKey)
        {
            return (resourceKey ?? string.Empty).ToLowerInvariant() switch
            {
                "maps" => new[] { "mapId" },
                "monsters" => new[] { "monsterId" },
                "items" => new[] { "itemId" },
                "dungeons" => new[] { "dungeonId" },
                "equipments" => new[] { "equipmentId" },
                "players" => new[] { "playerId", "gid" },
                "pets" => new[] { "templateId" },
                "crops" => new[] { "templateId" },
                "quests" => new[] { "questId" },
                "achievements" => new[] { "achievementId" },
                "attribute-point-configs" => new[] { "configId" },
                "player-level-configs" => new[] { "level" },
                "realm-level-configs" => new[] { "level" },
                "five-element-rules" => new[] { "arrayLevel", "gid" },
                "spirit-field-rules" => new[] { "itemId", "configId" },
                "alchemy-rules" => new[] { "level", "configId" },
                "forge-rules" => new[] { "level", "configId" },
                "element-rules" => Array.Empty<string>(),
                "shop-configs" => new[] { "shopId" },
                "shop-items" => new[] { "gid" },
                "runtime-configs" => new[] { "domain" },
                "ranking-configs" => new[] { "rankingId" },
                "ranking-rewards" => new[] { "gid" },
                "checkin-configs" => new[] { "continuousDay" },
                "redeem-codes" => new[] { "code" },
                "starter-packages" => new[] { "packageId" },
                "pills" => new[] { "templateId" },
                "alchemy-recipes" => new[] { "recipeId" },
                "forge-recipes" => new[] { "recipeId" },
                "admin-users" => new[] { "adminId" },
                "skills" => new[] { "skillId" },
                "buffs" => new[] { "buffId" },
                _ => Array.Empty<string>()
            };
        }

        private static string? ResolveDefaultTargetId(HttpContext context, string? resourceKey)
        {
            return (resourceKey ?? string.Empty).ToLowerInvariant() switch
            {
                "attribute-point-configs" => "bundle",
                "element-rules" => "matrix",
                "spirit-field-rules" when context.Request.Path.Value?.Contains("/system", StringComparison.OrdinalIgnoreCase) == true => "default",
                "alchemy-rules" when context.Request.Path.Value?.Contains("/config", StringComparison.OrdinalIgnoreCase) == true => "default",
                "forge-rules" when context.Request.Path.Value?.Contains("/config", StringComparison.OrdinalIgnoreCase) == true => "default",
                "runtime-configs" when context.Request.Path.Value?.Contains("/refresh-all", StringComparison.OrdinalIgnoreCase) == true => "all",
                _ => null
            };
        }

        private static async Task<object?> LoadSnapshotAsync(DbContext dbContext, string? resourceKey, string? targetId)
        {
            if (string.IsNullOrWhiteSpace(resourceKey) || string.IsNullOrWhiteSpace(targetId))
            {
                return null;
            }

            return (resourceKey ?? string.Empty).ToLowerInvariant() switch
            {
                "maps" => await dbContext.Db.Queryable<MapTemplateEntity>().InSingleAsync(targetId),
                "monsters" => await dbContext.Db.Queryable<MonsterTemplateEntity>().InSingleAsync(targetId),
                "items" => await dbContext.Db.Queryable<ItemTemplateEntity>().InSingleAsync(targetId),
                "dungeons" => await dbContext.Db.Queryable<DungeonTemplateEntity>().InSingleAsync(targetId),
                "equipments" when int.TryParse(targetId, out var equipmentId) => await dbContext.Db.Queryable<EquipmentTemplateEntity>().InSingleAsync(equipmentId),
                "players" => await dbContext.Db.Queryable<UserEntity>().InSingleAsync(targetId),
                "pets" => await dbContext.Db.Queryable<PetTemplateEntity>().InSingleAsync(targetId),
                "crops" => await dbContext.Db.Queryable<CropTemplateEntity>().InSingleAsync(targetId),
                "quests" => await dbContext.Db.Queryable<QuestConfigEntity>().InSingleAsync(targetId),
                "achievements" => await dbContext.Db.Queryable<AchievementConfigEntity>().InSingleAsync(targetId),
                "attribute-point-configs" when string.Equals(targetId, "bundle", StringComparison.OrdinalIgnoreCase) => await dbContext.Db.Queryable<AttributePointConfigEntity>()
                    .OrderBy(item => item.SortOrder)
                    .OrderBy(item => item.LevelStart)
                    .OrderBy(item => item.Key)
                    .ToListAsync(),
                "player-level-configs" when int.TryParse(targetId, out var playerLevel) => await dbContext.Db.Queryable<PlayerLevelConfigEntity>().InSingleAsync(playerLevel),
                "realm-level-configs" when int.TryParse(targetId, out var realmLevel) => await dbContext.Db.Queryable<RealmLevelConfigEntity>().InSingleAsync(realmLevel),
                "five-element-rules" when int.TryParse(targetId, out var arrayLevel) => await dbContext.Db.Queryable<FiveElementLevelConfigEntity>().InSingleAsync(arrayLevel),
                "five-element-rules" => await dbContext.Db.Queryable<FiveElementBranchUpgradeConfigEntity>().InSingleAsync(targetId),
                "spirit-field-rules" when string.Equals(targetId, "default", StringComparison.OrdinalIgnoreCase) => await dbContext.Db.Queryable<SpiritFieldSystemConfigEntity>().InSingleAsync(targetId),
                "spirit-field-rules" => await dbContext.Db.Queryable<SpiritFieldSpeedUpItemConfigEntity>().InSingleAsync(targetId),
                "alchemy-rules" when int.TryParse(targetId, out var alchemyLevel) => await dbContext.Db.Queryable<AlchemyProfessionLevelConfigEntity>().InSingleAsync(alchemyLevel),
                "alchemy-rules" => await dbContext.Db.Queryable<AlchemyProfessionRuleConfigEntity>().InSingleAsync(targetId),
                "forge-rules" when int.TryParse(targetId, out var forgeLevel) => await dbContext.Db.Queryable<ForgeProfessionLevelConfigEntity>().InSingleAsync(forgeLevel),
                "forge-rules" => await dbContext.Db.Queryable<ForgeProfessionRuleConfigEntity>().InSingleAsync(targetId),
                "element-rules" when string.Equals(targetId, "matrix", StringComparison.OrdinalIgnoreCase) => await dbContext.Db.Queryable<ElementRelationRuleEntity>()
                    .OrderBy(item => item.SortOrder)
                    .ToListAsync(),
                "shop-configs" => await dbContext.Db.Queryable<ShopConfigEntity>().InSingleAsync(targetId),
                "shop-items" => await dbContext.Db.Queryable<ShopItemEntity>().InSingleAsync(targetId),
                "runtime-configs" when string.Equals(targetId, "all", StringComparison.OrdinalIgnoreCase) => await dbContext.Db.Queryable<SystemConfigVersionEntity>()
                    .OrderBy(item => item.ConfigDomain)
                    .ToListAsync(),
                "runtime-configs" => await dbContext.Db.Queryable<SystemConfigVersionEntity>()
                    .FirstAsync(item => item.ConfigDomain == targetId),
                "ranking-configs" => await dbContext.Db.Queryable<RankingConfigEntity>().InSingleAsync(targetId),
                "ranking-rewards" => await dbContext.Db.Queryable<RankingRewardEntity>().InSingleAsync(targetId),
                "checkin-configs" when int.TryParse(targetId, out var continuousDay) => await dbContext.Db.Queryable<CheckInRewardConfigEntity>().InSingleAsync(continuousDay),
                "redeem-codes" => await dbContext.Db.Queryable<RedeemCodeConfigEntity>().InSingleAsync(targetId),
                "starter-packages" => await dbContext.Db.Queryable<StarterPackageConfigEntity>().InSingleAsync(targetId),
                "alchemy-recipes" => await dbContext.Db.Queryable<AlchemyRecipeEntity>().InSingleAsync(targetId),
                "forge-recipes" => await dbContext.Db.Queryable<ForgeRecipeEntity>().InSingleAsync(targetId),
                "admin-users" => await dbContext.Db.Queryable<AdminUserEntity>().InSingleAsync(targetId),
                "skills" when int.TryParse(targetId, out var skillId) => await dbContext.Db.Queryable<SkillTemplateEntity>().InSingleAsync(skillId),
                "buffs" => await dbContext.Db.Queryable<BuffTemplateEntity>().InSingleAsync(targetId),
                _ => null
            };
        }

        private static string? SerializeSnapshot(object? snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            var json = JsonSerializer.Serialize(snapshot, SnapshotJsonOptions);
            return SanitizeJsonPayload(json);
        }

        private static string? SanitizeJsonPayload(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var node = JsonNode.Parse(json);
                RedactSensitiveFields(node);
                return node?.ToJsonString();
            }
            catch
            {
                return json;
            }
        }

        private static void RedactSensitiveFields(JsonNode? node)
        {
            if (node is JsonObject jsonObject)
            {
                foreach (var entry in jsonObject.ToList())
                {
                    if (entry.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                        entry.Key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                        entry.Key.Contains("secret", StringComparison.OrdinalIgnoreCase))
                    {
                        jsonObject[entry.Key] = "[REDACTED]";
                        continue;
                    }

                    RedactSensitiveFields(entry.Value);
                }

                return;
            }

            if (node is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    RedactSensitiveFields(item);
                }
            }
        }

        private static string? BuildDiffJson(string? beforeJson, string? afterJson)
        {
            var beforeMap = FlattenJsonPayload(beforeJson);
            var afterMap = FlattenJsonPayload(afterJson);
            var allKeys = beforeMap.Keys
                .Union(afterMap.Keys, StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();

            var diffs = allKeys
                .Where(path => !string.Equals(beforeMap.GetValueOrDefault(path), afterMap.GetValueOrDefault(path), StringComparison.Ordinal))
                .Select(path => new AuditDiffEntry
                {
                    Path = path,
                    Before = beforeMap.GetValueOrDefault(path),
                    After = afterMap.GetValueOrDefault(path)
                })
                .ToList();

            return diffs.Count == 0 ? null : JsonSerializer.Serialize(diffs, SnapshotJsonOptions);
        }

        private static Dictionary<string, string?> FlattenJsonPayload(string? json)
        {
            var values = new Dictionary<string, string?>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(json))
            {
                return values;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                FlattenJsonElement(document.RootElement, "$", values);
            }
            catch
            {
                values["$"] = json;
            }

            return values;
        }

        private static void FlattenJsonElement(JsonElement element, string path, Dictionary<string, string?> values)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var hasProperties = false;
                    foreach (var property in element.EnumerateObject())
                    {
                        hasProperties = true;
                        FlattenJsonElement(property.Value, path == "$" ? property.Name : $"{path}.{property.Name}", values);
                    }

                    if (!hasProperties)
                    {
                        values[path] = "{}";
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        FlattenJsonElement(item, $"{path}[{index}]", values);
                        index++;
                    }

                    if (index == 0)
                    {
                        values[path] = "[]";
                    }
                    break;

                case JsonValueKind.String:
                    values[path] = element.GetString();
                    break;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    values[path] = null;
                    break;

                default:
                    values[path] = element.GetRawText();
                    break;
            }
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string? JsonElementToString(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => bool.TrueString,
                JsonValueKind.False => bool.FalseString,
                _ => null
            };
        }

        private sealed class AuditDiffEntry
        {
            /// <summary>
            /// 发生变化的字段路径。
            /// </summary>
            public string Path { get; set; } = string.Empty;

            /// <summary>
            /// 变更前值。
            /// </summary>
            public string? Before { get; set; }

            /// <summary>
            /// 变更后值。
            /// </summary>
            public string? After { get; set; }
        }
    }
}
