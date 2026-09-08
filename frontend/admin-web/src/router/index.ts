import { createRouter, createWebHistory } from 'vue-router'
import { ADMIN_PERMISSIONS, ADMIN_USER_MANAGEMENT_ROLES } from '@/constants/admin'
import { useAuthStore } from '@/stores/auth'
import LoginLayout from '@/layouts/LoginLayout.vue'
import AdminLayout from '@/layouts/AdminLayout.vue'
import LoginView from '@/modules/auth/views/LoginView.vue'
import DashboardView from '@/modules/dashboard/views/DashboardView.vue'
import AdminUsersView from '@/modules/admin-users/views/AdminUsersView.vue'
import RuntimeConfigsView from '@/modules/runtime-configs/views/RuntimeConfigsView.vue'
import PlayersView from '@/modules/players/views/PlayersView.vue'
import OfflineBattlesView from '@/modules/offline-battles/views/OfflineBattlesView.vue'
import MapsView from '@/modules/maps/views/MapsView.vue'
import MonstersView from '@/modules/monsters/views/MonstersView.vue'
import WorldBossView from '@/modules/world-boss/views/WorldBossView.vue'
import SkillsView from '@/modules/skills/views/SkillsView.vue'
import BuffsView from '@/modules/buffs/views/BuffsView.vue'
import ItemsView from '@/modules/items/views/ItemsView.vue'
import DungeonsView from '@/modules/dungeons/views/DungeonsView.vue'
import DungeonInstancesView from '@/modules/dungeon-instances/views/DungeonInstancesView.vue'
import DungeonEventTypesView from '@/modules/dungeon-instances/views/DungeonEventTypesView.vue'
import DungeonEventGroupsView from '@/modules/dungeon-instances/views/DungeonEventGroupsView.vue'
import EquipmentsView from '@/modules/equipments/views/EquipmentsView.vue'
import PetsView from '@/modules/pets/views/PetsView.vue'
import AttributePointConfigsView from '@/modules/growth/views/AttributePointConfigsView.vue'
import PlayerLevelConfigsView from '@/modules/growth/views/PlayerLevelConfigsView.vue'
import RealmLevelConfigsView from '@/modules/growth/views/RealmLevelConfigsView.vue'
import CropsView from '@/modules/crops/views/CropsView.vue'
import FiveElementsView from '@/modules/five-elements/views/FiveElementsView.vue'
import FiveElementRulesView from '@/modules/five-element-rules/views/FiveElementRulesView.vue'
import SpiritFieldsView from '@/modules/spirit-fields/views/SpiritFieldsView.vue'
import SpiritFieldRulesView from '@/modules/spirit-field-rules/views/SpiritFieldRulesView.vue'
import ElementRulesView from '@/modules/element-rules/views/ElementRulesView.vue'
import QuestsView from '@/modules/quests/views/QuestsView.vue'
import AchievementsView from '@/modules/achievements/views/AchievementsView.vue'
import ShopConfigsView from '@/modules/shops/views/ShopConfigsView.vue'
import RankingConfigsView from '@/modules/rankings/views/RankingConfigsView.vue'
import CheckInConfigsView from '@/modules/checkin/views/CheckInConfigsView.vue'
import RedeemCodesView from '@/modules/redeem/views/RedeemCodesView.vue'
import StarterPackagesView from '@/modules/starter-packages/views/StarterPackagesView.vue'
import AlchemyRulesView from '@/modules/alchemy/views/AlchemyRulesView.vue'
import AlchemyRecipesView from '@/modules/alchemy/views/AlchemyRecipesView.vue'
import AlchemySystemsView from '@/modules/alchemy/views/AlchemySystemsView.vue'
import ForgeRulesView from '@/modules/forge/views/ForgeRulesView.vue'
import EquipmentRerollRulesView from '@/modules/equipment-reroll-rules/views/EquipmentRerollRulesView.vue'
import EquipmentDecomposeRulesView from '@/modules/equipment-decompose-rules/views/EquipmentDecomposeRulesView.vue'
import ForgeRecipesView from '@/modules/forge/views/ForgeRecipesView.vue'
import ForgeSystemsView from '@/modules/forge/views/ForgeSystemsView.vue'
import AuditLogsView from '@/modules/audit/views/AuditLogsView.vue'
import SectsView from '@/modules/sects/views/SectsView.vue'
import HeartSutrasView from '@/modules/heart-sutras/views/HeartSutrasView.vue'
import SectBossView from '@/modules/sect-boss/views/SectBossView.vue'
import SectShopView from '@/modules/sect-shop/views/SectShopView.vue'
import SectBlessingsView from '@/modules/sect-blessings/views/SectBlessingsView.vue'
import SectGuildsView from '@/modules/sect-guilds/views/SectGuildsView.vue'
import TextCollectionsView from '@/modules/collections/views/TextCollectionsView.vue'
import ImageCollectionsView from '@/modules/collections/views/ImageCollectionsView.vue'
import LotteryPoolsView from '@/modules/lottery/views/LotteryPoolsView.vue'
import LotteryLogsView from '@/modules/lottery/views/LotteryLogsView.vue'
import FavorabilityView from '@/modules/favorability/views/FavorabilityView.vue'
import MailView from '@/modules/mail/views/MailView.vue'
import TitleView from '@/modules/title/views/TitleView.vue'
import ArenaView from '@/modules/arena/views/ArenaView.vue'
import TowerView from '@/modules/tower/views/TowerView.vue'
import GemView from '@/modules/gem/views/GemView.vue'
import MarketView from '@/modules/market/views/MarketView.vue'
import FeedbackView from '@/modules/feedback/views/FeedbackView.vue'

type AdminRouteMeta = {
  requiresAuth?: boolean
  permission?: string
  roles?: string[]
}

// 管理后台只有“登录布局”和“后台主布局”两层。
// 实际业务页全部挂在后台主布局下面，便于统一做权限守卫。
const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      component: LoginLayout,
      children: [
        { path: '', name: 'admin-login', component: LoginView }
      ]
    },
    {
      path: '/',
      component: AdminLayout,
      meta: { requiresAuth: true },
      children: [
        { path: '', name: 'admin-dashboard', component: DashboardView },
        { path: 'runtime-configs', name: 'admin-runtime-configs', component: RuntimeConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'admin-users', name: 'admin-admin-users', component: AdminUsersView, meta: { roles: [...ADMIN_USER_MANAGEMENT_ROLES] } },
        { path: 'players', name: 'admin-players', component: PlayersView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'offline-battles', name: 'admin-offline-battles', component: OfflineBattlesView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'maps', name: 'admin-maps', component: MapsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'monsters', name: 'admin-monsters', component: MonstersView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'world-boss', name: 'admin-world-boss', component: WorldBossView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'skills', name: 'admin-skills', component: SkillsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'buffs', name: 'admin-buffs', component: BuffsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'items', name: 'admin-items', component: ItemsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'dungeons', name: 'admin-dungeons', component: DungeonsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'dungeon-instances', name: 'admin-dungeon-instances', component: DungeonInstancesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'dungeon-instances/event-types', name: 'admin-dungeon-event-types', component: DungeonEventTypesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'dungeon-instances/event-groups', name: 'admin-dungeon-event-groups', component: DungeonEventGroupsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'sects', name: 'admin-sects', component: SectsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'heart-sutras', name: 'admin-heart-sutras', component: HeartSutrasView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'sect-boss', name: 'admin-sect-boss', component: SectBossView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'sect-shop', name: 'admin-sect-shop', component: SectShopView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'sect-blessings', name: 'admin-sect-blessings', component: SectBlessingsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'sect-guilds', name: 'admin-sect-guilds', component: SectGuildsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'equipments', name: 'admin-equipments', component: EquipmentsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'pets', name: 'admin-pets', component: PetsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'five-elements', name: 'admin-five-elements', component: FiveElementsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'five-element-rules', name: 'admin-five-element-rules', component: FiveElementRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'attribute-point-configs', name: 'admin-attribute-point-configs', component: AttributePointConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'player-level-configs', name: 'admin-player-level-configs', component: PlayerLevelConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'realm-level-configs', name: 'admin-realm-level-configs', component: RealmLevelConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'spirit-fields', name: 'admin-spirit-fields', component: SpiritFieldsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'spirit-field-rules', name: 'admin-spirit-field-rules', component: SpiritFieldRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'element-rules', name: 'admin-element-rules', component: ElementRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'crops', name: 'admin-crops', component: CropsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'quests', name: 'admin-quests', component: QuestsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'achievements', name: 'admin-achievements', component: AchievementsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'shop-configs', name: 'admin-shop-configs', component: ShopConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'ranking-configs', name: 'admin-ranking-configs', component: RankingConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'checkin-configs', name: 'admin-checkin-configs', component: CheckInConfigsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'redeem-codes', name: 'admin-redeem-codes', component: RedeemCodesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'starter-packages', name: 'admin-starter-packages', component: StarterPackagesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'alchemy-rules', name: 'admin-alchemy-rules', component: AlchemyRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'alchemy-systems', name: 'admin-alchemy-systems', component: AlchemySystemsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'alchemy-recipes', name: 'admin-alchemy-recipes', component: AlchemyRecipesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'forge-rules', name: 'admin-forge-rules', component: ForgeRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'forge-systems', name: 'admin-forge-systems', component: ForgeSystemsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'forge-recipes', name: 'admin-forge-recipes', component: ForgeRecipesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'equipment-reroll-rules', name: 'admin-equipment-reroll-rules', component: EquipmentRerollRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'equipment-decompose-rules', name: 'admin-equipment-decompose-rules', component: EquipmentDecomposeRulesView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'audit-logs', name: 'admin-audit-logs', component: AuditLogsView, meta: { permission: ADMIN_PERMISSIONS.auditRead } },
        { path: 'text-collections', name: 'admin-text-collections', component: TextCollectionsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'image-collections', name: 'admin-image-collections', component: ImageCollectionsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'lottery-pools', name: 'admin-lottery-pools', component: LotteryPoolsView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'lottery-logs', name: 'admin-lottery-logs', component: LotteryLogsView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'favorability', name: 'admin-favorability', component: FavorabilityView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'feedback', name: 'admin-feedback', component: FeedbackView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'mail', name: 'admin-mail', component: MailView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'title', name: 'admin-title', component: TitleView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'arena', name: 'admin-arena', component: ArenaView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'tower', name: 'admin-tower', component: TowerView, meta: { permission: ADMIN_PERMISSIONS.playerRead } },
        { path: 'gem', name: 'admin-gem', component: GemView, meta: { permission: ADMIN_PERMISSIONS.configRead } },
        { path: 'market', name: 'admin-market', component: MarketView, meta: { permission: ADMIN_PERMISSIONS.configRead } }
      ]
    }
  ]
})

// 后台统一路由守卫：
// 1. 先补全当前管理员资料。
// 2. 再处理登录态拦截。
// 3. 最后按角色和权限做页面级访问控制。
router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  if (authStore.isAuthenticated && !authStore.currentUser) {
    try {
      await authStore.loadCurrentUser()
    } catch {
      authStore.clearSession()
    }
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'admin-login' }
  }

  if (to.name === 'admin-login' && authStore.isAuthenticated) {
    return { name: 'admin-dashboard' }
  }

  // 菜单显隐之外，路由也要执行同一套权限检查，避免手工输入地址绕过前端限制。
  const meta = to.meta as AdminRouteMeta
  if (meta.roles?.length && !meta.roles.includes(authStore.role)) {
    return { name: 'admin-dashboard' }
  }

  if (meta.permission && !authStore.permissions.includes(meta.permission)) {
    return { name: 'admin-dashboard' }
  }

  return true
})

export default router
