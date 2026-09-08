# 修仙问道 - 东方玄幻挂机修仙游戏前端

一个基于 Vue 3 + Vite 构建的东方修仙题材网页回合制挂机游戏前端。

## 项目特性

- 🎮 **完整的游戏系统**：修炼、战斗、装备、技能、宠物、炼丹、锻造等
- 🎨 **东方修仙风格UI**：深色背景 + 玉石色/金色配色，符合修仙题材
- 📱 **响应式设计**：适配不同屏幕尺寸
- 🔧 **模块化架构**：清晰的代码结构，方便后期接入真实后端
- 💾 **纯前端模拟**：所有数据使用模拟数据，无需后端即可运行

## 技术栈

- **前端框架**：Vue 3 (Composition API)
- **构建工具**：Vite
- **路由管理**：Vue Router
- **样式方案**：原生 CSS + CSS 变量

## 项目结构

```
xiuxian-game/
├── public/                  # 静态资源
├── src/
│   ├── assets/             # 图片、字体等资源
│   ├── components/         # Vue 组件
│   │   ├── common/         # 通用组件
│   │   │   ├── XiuXianModal.vue    # 修仙风格Modal
│   │   │   ├── XiuXianTooltip.vue  # Tooltip组件
│   │   │   └── XiuXianProgress.vue # 进度条组件
│   │   ├── layout/         # 布局组件
│   │   │   ├── HeaderBar.vue       # 顶部信息栏
│   │   │   ├── SidebarMenu.vue     # 左侧菜单
│   │   │   ├── RightPanel.vue      # 右侧面板
│   │   │   └── BottomBar.vue       # 底部快捷栏
│   │   ├── modals/         # 弹窗组件
│   │   │   ├── CheckInModal.vue    # 签到系统
│   │   │   ├── RankingsModal.vue   # 排行榜
│   │   │   ├── TeamModal.vue       # 组队系统
│   │   │   ├── RedeemModal.vue     # 兑换码
│   │   │   ├── PetModal.vue        # 宠物系统
│   │   │   ├── ShopModal.vue       # 商店系统
│   │   │   ├── ForgeModal.vue      # 锻造系统
│   │   │   ├── AlchemyModal.vue    # 炼丹系统
│   │   │   ├── ChangelogModal.vue  # 更新日志
│   │   │   └── MapSelectModal.vue  # 地图选择
│   │   └── panels/         # 功能面板
│   │       ├── CultivationPanel.vue  # 修炼系统
│   │       ├── BattlePanel.vue       # 战斗系统
│   │       ├── StatsPanel.vue        # 属性面板
│   │       ├── SkillEquipPanel.vue   # 技能装备
│   │       └── InventoryPanel.vue    # 背包系统
│   ├── data/               # 模拟数据
│   │   └── mockData.js     # 所有模拟数据
│   ├── router/             # 路由配置
│   │   └── index.js
│   ├── styles/             # 全局样式
│   │   ├── index.css       # 样式入口
│   │   └── variables.css   # CSS变量定义
│   ├── views/              # 页面视图
│   │   ├── LoginView.vue   # 登录页面
│   │   └── MainView.vue    # 游戏主页面
│   ├── App.vue             # 根组件
│   └── main.js             # 入口文件
├── index.html
├── package.json
├── vite.config.js
└── README.md
```

## 核心系统模块

### 1. 人物修炼（挂机系统）
- 显示修为境界（练气、筑基、金丹等）
- 肉体境界显示
- 经验池自动增长（每10秒+10）
- 实时进度条动画
- 突破功能

### 2. 战斗系统
- 地图背景展示
- 左侧敌方单位 / 右侧友方单位
- 血条(HP) / 蓝条(MP)显示
- 战斗日志滚动显示
- 普通攻击、技能释放、物品使用、逃跑
- 自动战斗功能

### 3. 地图选择系统
- 地图列表（卡片形式）
- 地图详细信息（推荐境界、敌人信息、掉落展示）
- 进入限制（最低修为、最低肉体）
- 解锁状态显示

### 4. 人物属性面板
- 基础属性（生命、法力、攻击、防御）
- 攻击属性（物攻、法攻、命中、破甲）
- 防御属性（物防、法防、闪避、格挡）
- 特殊属性（暴击、反击率、连击率等）

### 5. 技能与装备展示
- 已装备技能列表（6个槽位）
- 技能库列表
- Tooltip显示技能详情
- 当前穿戴装备展示
- 悬停显示装备详细属性

### 6. 背包系统
- 装备背包：网格形式展示
- 道具背包：使用、丢弃
- 品级筛选（白/绿/蓝/紫/橙/红）
- 五行属性筛选（金木水火土）
- 名称搜索
- 装备操作：穿戴、强化、鉴定、洗练、捐献、绑定/解绑、卖出

### 7. 弹窗系统
- **签到系统**：月历视图、签到按钮、连续签到奖励
- **排行榜**：境界榜、肉体榜、金币榜、灵石榜
- **组队系统**：队伍列表、创建队伍、加入队伍
- **兑换码**：输入兑换码、显示兑换结果
- **宠物系统**：宠物列表、查看详细属性
- **商店系统**：商品列表、购买功能
- **锻造系统**：选择图纸、打造动画、结果显示
- **炼丹系统**：选择丹方、燃烧动画、结果显示
- **更新日志**：版本更新内容展示

## UI风格特点

- **深色主题**：主背景使用深蓝黑色系
- **玉石色/金色点缀**：强调色使用玉石绿和金色
- **品级颜色区分**：
  - 普通(白) - #9e9e9e
  - 优秀(绿) - #4caf50
  - 稀有(蓝) - #2196f3
  - 史诗(紫) - #9c27b0
  - 传说(橙) - #ff9800
  - 神话(红) - #f44336
- **半透明面板**：使用 backdrop-filter 实现毛玻璃效果
- **符文边框**：四角装饰性边框设计
- **灵气特效**：发光、脉冲、漂浮等动画效果

## 快速开始

### 安装依赖

```bash
cd xiuxian-game
npm install
```

### 启动开发服务器

```bash
npm run dev
```

访问 http://localhost:3000

### 构建生产版本

```bash
npm run build
```

## 开发说明

### 添加新的弹窗

1. 在 `src/components/modals/` 创建新的弹窗组件
2. 在 `src/views/MainView.vue` 中导入并注册组件
3. 在 `modals` 响应式对象中添加对应的显示状态
4. 通过 `provide('openModal')` 在子组件中打开弹窗

### 接入真实后端

1. 在 `src/data/` 目录下创建 API 接口文件
2. 使用 axios 或 fetch 替换模拟数据
3. 建议使用 Pinia 或 Vuex 进行状态管理
4. 模拟数据文件 `mockData.js` 可作为接口响应格式参考

### 样式定制

全局样式变量定义在 `src/styles/variables.css`，包括：
- 颜色变量（主题色、品质色、五行色等）
- 间距变量
- 圆角变量
- 阴影变量

## 浏览器支持

- Chrome / Edge 88+
- Firefox 78+
- Safari 14+

## 许可证

MIT
