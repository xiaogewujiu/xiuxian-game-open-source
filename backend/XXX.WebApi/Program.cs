using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using XXX.Application;
using XXX.Application.Interfaces;
using XXX.Application.Security;
using XXX.Entity;
using XXX.Infrastructure;
using XXX.Infrastructure.Authentication;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.SeedData;
using XXX.WebApi.Hubs;
using XXX.WebApi.HostedServices;
using XXX.WebApi.Middleware;
using XXX.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "ProtectionKeys");
var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

Directory.CreateDirectory(dataProtectionKeysPath);
Directory.CreateDirectory(webRootPath);
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));

// 中文注释：
// 当前开发环境下直接跑 `dotnet run` 时，Windows EventLog provider 可能因为权限不足抛异常，
// 反而把本应只记录到日志里的启动告警升级成进程崩溃。
// 这里显式收口到控制台 / 调试输出，既保留后端日志，也避免被系统事件日志写权限卡死。
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("XXX.WebApi");

// 中文注释：
// DPAPI 仅支持 Windows。开发机继续使用系统保护；
// 若以后切到 Linux / 容器，至少不会因为这里直接调用而启动失败。
if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapi();
}

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddSignalR();
builder.Services.AddHostedService<OfflineBattleWorker>();

builder.Services.AddHostedService<BattleProgressWorker>();
builder.Services.AddHostedService<DungeonInstanceWorker>();
builder.Services.AddHostedService<WorldBossWorker>();
builder.Services.AddHostedService<SectTournamentWorker>();
builder.Services.AddHostedService<DailySectTaskResetWorker>();
builder.Services.AddHostedService<PillEffectExpiryWorker>();
builder.Services.AddHostedService<FavorabilityResetService>();
builder.Services.AddHostedService<MailCleanupWorker>();
builder.Services.AddSingleton<LocalAssetStorageService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AiIconGenerationService>();

builder.Services.AddRepository<UserEntity>();
builder.Services.AddRepository<PlayerAttributeAllocationEntity>();
builder.Services.AddRepository<PlayerLevelConfigEntity>();
builder.Services.AddRepository<AttributePointConfigEntity>();
builder.Services.AddRepository<ItemChestConfigEntity>();
builder.Services.AddRepository<ItemChestRewardEntryEntity>();
builder.Services.AddRepository<ItemSkillBookConfigEntity>();
builder.Services.AddRepository<ItemRecipeUnlockConfigEntity>();
builder.Services.AddRepository<ItemPetEggConfigEntity>();
builder.Services.AddRepository<ItemPillConfigEntity>();
builder.Services.AddRepository<RealmLevelConfigEntity>();
builder.Services.AddRepository<AdminUserEntity>();
builder.Services.AddRepository<AdminAuditLogEntity>();
builder.Services.AddRepository<InventoryItemEntity>();
builder.Services.AddRepository<EquipmentInstanceEntity>();
builder.Services.AddRepository<PetInstanceEntity>();
builder.Services.AddRepository<PetTemplateEntity>();
builder.Services.AddRepository<SpiritFieldPlotEntity>();
builder.Services.AddRepository<SpiritFieldSystemEntity>();
builder.Services.AddRepository<SpiritFieldSystemConfigEntity>();
builder.Services.AddRepository<SpiritFieldSpeedUpItemConfigEntity>();
builder.Services.AddRepository<CropTemplateEntity>();
builder.Services.AddRepository<AlchemySystemEntity>();
builder.Services.AddRepository<AlchemyRecipeEntity>();
builder.Services.AddRepository<AlchemyProfessionLevelConfigEntity>();
builder.Services.AddRepository<AlchemyProfessionRuleConfigEntity>();
builder.Services.AddRepository<FiveElementArrayEntity>();
builder.Services.AddRepository<FiveElementLevelConfigEntity>();
builder.Services.AddRepository<FiveElementBranchUpgradeConfigEntity>();
builder.Services.AddRepository<FiveElementBranchRuleRangeEntity>();
builder.Services.AddRepository<ForgeSystemEntity>();
builder.Services.AddRepository<GuildEntity>();
builder.Services.AddRepository<GuildMemberEntity>();
builder.Services.AddRepository<RefreshTokenEntity>();
builder.Services.AddRepository<TokenBlacklistEntity>();
builder.Services.AddRepository<AchievementProgressEntity>();
builder.Services.AddRepository<AchievementConfigEntity>();
builder.Services.AddRepository<QuestProgressEntity>();
builder.Services.AddRepository<QuestConfigEntity>();
builder.Services.AddRepository<QuestCompletedRecordEntity>();
builder.Services.AddRepository<ShopDailyRecordEntity>();
builder.Services.AddRepository<DungeonDailyRecordEntity>();
builder.Services.AddRepository<ShopConfigEntity>();
builder.Services.AddRepository<ShopItemEntity>();
builder.Services.AddRepository<RankingEntryEntity>();
builder.Services.AddRepository<RankingConfigEntity>();
builder.Services.AddRepository<RankingRewardEntity>();
builder.Services.AddRepository<RankingHistoryEntity>();
builder.Services.AddRepository<CheckInRewardConfigEntity>();
builder.Services.AddRepository<RedeemCodeConfigEntity>();
builder.Services.AddRepository<ElementRelationRuleEntity>();
builder.Services.AddRepository<EquipmentSlotEntity>();
builder.Services.AddRepository<CurrencyRecordEntity>();
builder.Services.AddRepository<CheckInStateEntity>();
builder.Services.AddRepository<CheckInRecordEntity>();
builder.Services.AddRepository<RedeemCodeUsageEntity>();
builder.Services.AddRepository<StarterPackageConfigEntity>();
builder.Services.AddRepository<StarterPackageGrantItemEntity>();
builder.Services.AddRepository<StarterPackageGrantSkillEntity>();
builder.Services.AddRepository<ItemTemplateEntity>();
builder.Services.AddRepository<EquipmentTemplateEntity>();
builder.Services.AddRepository<MonsterTemplateEntity>();
builder.Services.AddRepository<MapTemplateEntity>();
builder.Services.AddRepository<SkillTemplateEntity>();
builder.Services.AddRepository<BuffTemplateEntity>();
builder.Services.AddRepository<ForgeRecipeEntity>();
builder.Services.AddRepository<ForgeProfessionLevelConfigEntity>();
builder.Services.AddRepository<ForgeProfessionRuleConfigEntity>();
builder.Services.AddRepository<DungeonTemplateEntity>();
builder.Services.AddRepository<DungeonInstanceTemplateEntity>();
builder.Services.AddRepository<DungeonInstanceEntity>();
builder.Services.AddRepository<DungeonInstanceDailyRecordEntity>();
builder.Services.AddRepository<DungeonRewardPoolEntity>();
builder.Services.AddRepository<DungeonEventConfigEntity>();
builder.Services.AddRepository<DungeonPartyEntity>();
builder.Services.AddRepository<DungeonEventGroupEntity>();
builder.Services.AddRepository<PartyEntity>();
builder.Services.AddRepository<PartyMemberEntity>();
builder.Services.AddRepository<PartyBattleRecordEntity>();
builder.Services.AddRepository<WorldBossTemplateEntity>();
builder.Services.AddRepository<WorldBossScheduleEntity>();
builder.Services.AddRepository<WorldBossInstanceEntity>();
builder.Services.AddRepository<WorldBossParticipantEntity>();
builder.Services.AddRepository<WorldBossRewardRecordEntity>();
builder.Services.AddRepository<WorldBossLogEntity>();
builder.Services.AddRepository<SectTemplateEntity>();
builder.Services.AddRepository<HeartSutraTemplateEntity>();
builder.Services.AddRepository<PlayerHeartSutraEntity>();
builder.Services.AddRepository<SectDonationRecordEntity>();
builder.Services.AddRepository<SectTournamentEntity>();
builder.Services.AddRepository<SectTournamentMatchEntity>();
builder.Services.AddRepository<GeniusTournamentEntity>();
builder.Services.AddRepository<GeniusTournamentMatchEntity>();
builder.Services.AddRepository<SectTournamentScheduleEntity>();
builder.Services.AddRepository<SectBossTemplateEntity>();
builder.Services.AddRepository<SectBossInstanceEntity>();
builder.Services.AddRepository<SectShopConfigEntity>();
builder.Services.AddRepository<SectShopItemEntity>();
builder.Services.AddRepository<SectBlessingConfigEntity>();
builder.Services.AddRepository<TextCollectionSeriesEntity>();
builder.Services.AddRepository<TextCollectionItemEntity>();
builder.Services.AddRepository<TextCollectionBonusEntity>();
builder.Services.AddRepository<ImageCollectionSeriesEntity>();
builder.Services.AddRepository<ImageCollectionItemEntity>();
builder.Services.AddRepository<ImageCollectionBonusEntity>();
builder.Services.AddRepository<LotteryPoolEntity>();
builder.Services.AddRepository<LotteryPrizeEntity>();
builder.Services.AddRepository<PlayerCollectionEntity>();
builder.Services.AddRepository<LotteryLogEntity>();
builder.Services.AddRepository<PlayerPillEffectEntity>();
builder.Services.AddRepository<PlayerFeedbackEntity>();
builder.Services.AddRepository<PlayerFeedbackAttachmentEntity>();
builder.Services.AddRepository<PlayerFeedbackStatusHistoryEntity>();
builder.Services.AddRepository<MailMessageEntity>();
builder.Services.AddRepository<MailGlobalClaimRecordEntity>();
builder.Services.AddRepository<TitleTemplateEntity>();
builder.Services.AddRepository<PlayerTitleEntity>();
builder.Services.AddRepository<ArenaPlayerEntity>();
builder.Services.AddRepository<ArenaBattleLogEntity>();
builder.Services.AddRepository<TowerProgressEntity>();
builder.Services.AddRepository<TowerFloorConfigEntity>();
builder.Services.AddRepository<TowerBattleLogEntity>();
builder.Services.AddRepository<MarketListingEntity>();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // 中文注释：
                    // 浏览器中的 SignalR WebSocket 握手通常通过 access_token 查询参数传 JWT，
                    // 这里仅对聊天 Hub 路径放行，避免影响普通 HTTP 接口的 Bearer Header 鉴权。
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/party")))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                    var accessToken = (context.SecurityToken as JwtSecurityToken)?.RawData;
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        var bearerToken = context.HttpContext.Request.Headers.Authorization.ToString();
                        if (bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = bearerToken["Bearer ".Length..].Trim();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        await jwtService.IsTokenBlacklistedAsync(accessToken))
                    {
                        context.Fail("当前登录令牌已失效。");
                        return;
                    }

                    var role = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                        ?? context.Principal?.FindFirst(JwtClaims.Role)?.Value
                        ?? string.Empty;

                    // 中文注释：
                    // 后台管理员和玩家共用一套 JWT 发行器，但封禁校验只作用于玩家账号。
                    // 管理后台账号走独立 AdminUsers 表，不应误命中 Users 表的封禁逻辑。
                    if (AdminRoleCatalog.AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? context.Principal?.FindFirst(JwtClaims.UserId)?.Value;
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return;
                    }

                    var userRepository = context.HttpContext.RequestServices.GetRequiredService<XXX.Infrastructure.Repositories.IRepository<UserEntity>>();
                    var user = await userRepository.GetByIdAsync(userId);
                    if (user == null)
                    {
                        context.Fail("当前账号不存在。");
                        return;
                    }

                    if (user.IsBanned && (!user.BanExpiresAt.HasValue || user.BanExpiresAt.Value > DateTime.Now))
                    {
                        context.Fail(string.IsNullOrWhiteSpace(user.BanReason)
                            ? "当前账号已被封禁。"
                            : $"当前账号已被封禁：{user.BanReason}");
                    }
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }

                    return Task.CompletedTask;
                }
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AdminRoleCatalog.AdminOnlyPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
    });

    options.AddPolicy(AdminPermissionCatalog.PlayerReadPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
        policy.RequireClaim(JwtClaims.Permission, AdminPermissionCatalog.PlayerRead);
    });

    options.AddPolicy(AdminPermissionCatalog.ConfigWritePolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
        policy.RequireClaim(JwtClaims.Permission, AdminPermissionCatalog.ConfigWrite);
    });

    options.AddPolicy(AdminPermissionCatalog.PlayerWritePolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
        policy.RequireClaim(JwtClaims.Permission, AdminPermissionCatalog.PlayerWrite);
    });

    options.AddPolicy(AdminPermissionCatalog.PlayerGrantPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
        policy.RequireClaim(JwtClaims.Permission, AdminPermissionCatalog.PlayerGrant);
    });

    options.AddPolicy(AdminPermissionCatalog.AuditReadPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(AdminRoleCatalog.AdminRoles);
        policy.RequireClaim(JwtClaims.Permission, AdminPermissionCatalog.AuditRead);
    });
});

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:3003", "http://localhost:3004", "http://localhost:3005", "http://localhost:3006", "http://localhost:5173" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    options.AddPolicy("auth", httpContext =>
    {
        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/register", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                });
        }

        return RateLimitPartition.GetNoLimiter("none");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "XXX Game API",
        Version = "v1",
        Description = "修仙游戏开发接口"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT 鉴权，格式：Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();
app.Logger.LogInformation("DataProtection keys will be persisted to {KeyPath}", dataProtectionKeysPath);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "XXX Game API V1");
        options.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath)
});
app.UseCors("RestrictedCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminAuditLoggingMiddleware>();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat").RequireCors("RestrictedCors");
app.MapHub<PartyHub>("/hubs/party").RequireCors("RestrictedCors");

static async Task RunStartupStageAsync(string stageName, Func<Task> action, ILogger logger)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await action();
    logger.LogInformation("Startup stage {StageName} completed in {ElapsedMs} ms", stageName, stopwatch.ElapsedMilliseconds);
}

using (var scope = app.Services.CreateScope())
{
    var startupStopwatch = System.Diagnostics.Stopwatch.StartNew();
    var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
    dbContext.InitDatabase();

    // 中文注释：
    // 模板类数据在开发环境启动时就主动落进 SQLite，
    // 这样前端第一次打开灵宠、炼丹等弹窗时读取到的就是数据库真实数据，
    // 而不是等到某个业务接口第一次命中后才临时补写。
    // 纯净版：不解析 ISeedDataService，避免启动阶段进入任何种子处理链。
    // var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
    var runtimeTemplateLoader = scope.ServiceProvider.GetRequiredService<IRuntimeTemplateLoader>();
    var growthConfigRuntimeService = scope.ServiceProvider.GetRequiredService<IGrowthConfigRuntimeService>();
    var fiveElementRuleRuntimeService = scope.ServiceProvider.GetRequiredService<IFiveElementRuleRuntimeService>();
    var spiritFieldRuleRuntimeService = scope.ServiceProvider.GetRequiredService<ISpiritFieldRuleRuntimeService>();
    var alchemyProfessionRuleRuntimeService = scope.ServiceProvider.GetRequiredService<IAlchemyProfessionRuleRuntimeService>();
    var forgeProfessionRuleRuntimeService = scope.ServiceProvider.GetRequiredService<IForgeProfessionRuleRuntimeService>();
    var elementRelationRuleRuntimeService = scope.ServiceProvider.GetRequiredService<IElementRelationRuleRuntimeService>();
    var equipmentRerollRuleRuntimeService = scope.ServiceProvider.GetRequiredService<IEquipmentRerollRuleRuntimeService>();
    // 纯净版：项目启动只读取数据库，不自动执行任何种子、同步、修复或旧 ID 回填。
    // await RunStartupStageAsync("seed-bootstrap", () => seedDataService.InitializeAsync(), app.Logger);
    await RunStartupStageAsync("runtime-templates", () => runtimeTemplateLoader.LoadAsync(), app.Logger);
    await RunStartupStageAsync("growth-runtime", () => growthConfigRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("five-element-runtime", () => fiveElementRuleRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("spirit-field-runtime", () => spiritFieldRuleRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("alchemy-runtime", () => alchemyProfessionRuleRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("forge-runtime", () => forgeProfessionRuleRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("element-relation-runtime", () => elementRelationRuleRuntimeService.LoadAsync(), app.Logger);
    await RunStartupStageAsync("equipment-reroll-runtime", () => equipmentRerollRuleRuntimeService.LoadAsync(), app.Logger);

    // 中文注释：开发版只初始化可玩闭环必须依赖的系统，避免非核心系统阻塞启动。
    var shopService = scope.ServiceProvider.GetRequiredService<IShopService>();
    var rankingService = scope.ServiceProvider.GetRequiredService<IRankingService>();

    await RunStartupStageAsync("shop-cache", () => shopService.InitializeAsync(), app.Logger);
    await RunStartupStageAsync("ranking-cache", () => rankingService.InitializeAsync(), app.Logger);
    app.Logger.LogInformation("Startup bootstrap pipeline completed in {ElapsedMs} ms", startupStopwatch.ElapsedMilliseconds);
}

app.Run();


