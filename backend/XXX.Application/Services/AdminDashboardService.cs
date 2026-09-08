using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台首页服务。
    /// 当前先提供基础数据总览，后续可继续扩展运营统计与异常信息。
    /// </summary>
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<AdminUserEntity> _adminUserRepository;
        private readonly IRepository<MapTemplateEntity> _mapTemplateRepository;
        private readonly IRepository<MonsterTemplateEntity> _monsterTemplateRepository;
        private readonly IRepository<ItemTemplateEntity> _itemTemplateRepository;
        private readonly IRepository<EquipmentTemplateEntity> _equipmentTemplateRepository;

        /// <summary>
        /// 初始化管理后台首页服务。
        /// </summary>
        public AdminDashboardService(
            IRepository<UserEntity> userRepository,
            IRepository<AdminUserEntity> adminUserRepository,
            IRepository<MapTemplateEntity> mapTemplateRepository,
            IRepository<MonsterTemplateEntity> monsterTemplateRepository,
            IRepository<ItemTemplateEntity> itemTemplateRepository,
            IRepository<EquipmentTemplateEntity> equipmentTemplateRepository)
        {
            _userRepository = userRepository;
            _adminUserRepository = adminUserRepository;
            _mapTemplateRepository = mapTemplateRepository;
            _monsterTemplateRepository = monsterTemplateRepository;
            _itemTemplateRepository = itemTemplateRepository;
            _equipmentTemplateRepository = equipmentTemplateRepository;
        }

        /// <summary>
        /// 获取后台首页摘要数据。
        /// </summary>
        public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
        {
            var playerCountTask = _userRepository.CountAsync(user => !user.IsDeleted);
            var adminCountTask = _adminUserRepository.CountAsync(admin => !admin.IsDeleted && admin.IsActive);
            var mapCountTask = _mapTemplateRepository.CountAsync(_ => true);
            var monsterCountTask = _monsterTemplateRepository.CountAsync(_ => true);
            var itemCountTask = _itemTemplateRepository.CountAsync(_ => true);
            var equipmentCountTask = _equipmentTemplateRepository.CountAsync(_ => true);

            await Task.WhenAll(
                playerCountTask,
                adminCountTask,
                mapCountTask,
                monsterCountTask,
                itemCountTask,
                equipmentCountTask);

            return new AdminDashboardSummaryDto
            {
                PlayerCount = playerCountTask.Result,
                AdminCount = adminCountTask.Result,
                MapTemplateCount = mapCountTask.Result,
                MonsterTemplateCount = monsterCountTask.Result,
                ItemTemplateCount = itemCountTask.Result,
                EquipmentTemplateCount = equipmentCountTask.Result
            };
        }
    }
}
