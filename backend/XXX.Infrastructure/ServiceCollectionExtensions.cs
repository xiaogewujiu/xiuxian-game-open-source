using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XXX.Infrastructure.Authentication;
using XXX.Infrastructure.Cache;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Infrastructure.SeedData;

namespace XXX.Infrastructure
{
    /// <summary>
    /// 基础设施层依赖注入扩展。
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册基础设施服务。
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped(provider =>
            {
                var logger = provider.GetService<ILogger<DbContext>>();
                return new DbContext(configuration, logger);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.Configure<AdminSeedOptions>(configuration.GetSection("Admin"));
            services.Configure<SeedStartupOptions>(configuration.GetSection("SeedStartup"));
            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<ISeedDataService>(provider =>
            {
                var dbContext = provider.GetRequiredService<DbContext>();
                var logger = provider.GetService<ILogger<SeedDataService>>();
                var balanceCatalogSyncService = provider.GetRequiredService<BalanceCatalogSyncService>();
                var adminSeedOptions = provider.GetRequiredService<IOptions<AdminSeedOptions>>();
                var startupOptions = provider.GetRequiredService<IOptions<SeedStartupOptions>>();
                var hostEnvironment = provider.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
                return new SeedDataService(dbContext, balanceCatalogSyncService, adminSeedOptions, startupOptions, hostEnvironment, "SeedData", logger);
            });

            services.AddScoped<BalanceCatalogSyncService>(provider =>
            {
                var dbContext = provider.GetRequiredService<DbContext>();
                var logger = provider.GetService<ILogger<BalanceCatalogSyncService>>();
                return new BalanceCatalogSyncService(dbContext, logger);
            });

            services.AddScoped<IRuntimeTemplateLoader>(provider =>
            {
                var dbContext = provider.GetRequiredService<DbContext>();
                var logger = provider.GetService<ILogger<RuntimeTemplateLoader>>();
                return new RuntimeTemplateLoader(dbContext, logger);
            });

            return services;
        }

        /// <summary>
        /// 注册通用仓储。
        /// </summary>
        public static IServiceCollection AddRepository<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddScoped<IRepository<T>, Repository<T>>();
            return services;
        }
    }
}
