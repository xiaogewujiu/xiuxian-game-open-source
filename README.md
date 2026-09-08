# 修仙问道

一个以东方玄幻为主题的网页修仙游戏，包含玩家游戏端、管理端和 ASP.NET Core 后端。地图、怪物、掉落、道具、装备、技能、任务、成就、副本、秘境、商店和生产系统等内容以 SQLite 数据库中的配置为准，后端负责规则执行和运行数据保存，前端负责交互与展示。

## 目录

~~~
.
├── backend/                         # .NET 8 后端源码和本地数据库
│   ├── XXX.Core/                    # 核心实体、战斗和领域规则
│   ├── XXX.Application/             # DTO、服务、接口和业务编排
│   ├── XXX.Infrastructure/          # SQLite、SqlSugar、认证和运行时配置
│   ├── XXX.WebApi/                  # Web API、SignalR 和启动入口
│   ├── XXX.sln
│   ├── database_schema.sql
│   └── 纯净版内容导入基准_v1.sql
├── frontend/
│   ├── xiuxian-game/                # 玩家游戏端，Vue 3 + Vite
│   └── admin-web/                   # 管理端，Vue 3 + TypeScript + Vite
├── deploy/                          # API、前端发布文件和 Docker 配置
└── README.md
~~~

## 核心功能

### 玩家和修炼

- 注册、登录、刷新令牌、退出登录和账号检查
- 角色资料、头像、等级、境界、修为和肉身境界
- 基础属性点分配、属性重置和境界突破
- 生命、法力、物攻、法攻、物防、法防、速度、暴击、闪避、命中等战斗属性
- 游戏帮助、更新日志、建议反馈和问题反馈
- 设置装备自动出售时保留的最低等级和最低品质

### 地图、战斗和挂机

- 普通地图探索和自动战斗
- 每个阶段可配置主线图、材料图、精英图和首领图
- 地图进入条件、推荐等级、怪物、掉落、经验和金币
- 普通攻击、职业技能、Buff、Debuff、元素关系、目标选择和战斗日志
- 装备、道具、技能、宠物和其他系统加成参与实际战斗属性计算
- 在线战斗结算装备、道具、金币和经验
- 离线战斗由玩家选择本次时长，单次最多 48 小时
- 背包满时跳过无法进入背包的装备，不终止离线战斗、副本或秘境处理

### 装备和背包

- 武器、防具、法宝、首饰等装备部位
- 普通、优秀、精良、史诗、传说等品质
- 装备等级、基础属性、随机属性和装备比较
- 穿戴、卸下、强化、洗炼、宝石镶嵌和拆卸
- 按装备品质配置分解规则，产出自动合并到道具堆叠
- 单件/批量出售、单件/批量分解和出售金币返回
- 道具和装备只能由玩家主动锁定或解锁
- 锁定内容不能出售、丢弃、分解或交易
- 已穿戴或已上架市场的装备不会被自动出售
- 自动出售遵守锁定、保留等级和保留品质设置
- 同一种道具按 PlayerId + ItemId 合并为一个堆叠格
- 锁定道具再次获得时合并到原锁定堆叠，不产生第二格
- 道具格、装备格分别计算容量，可使用扩容道具增加容量

### 技能和 Buff

- warrior、mage、body 三个职业的技能体系
- 每个职业拥有基础技能和可通过技能书解锁的其他技能
- 技能等级、技能升级材料和技能书碎片
- 技能书使用、技能书分解和技能碎片升级
- 技能释放概率、伤害、治疗、控制、增益和减益由后端执行
- 怪物同样可以配置技能和 Buff，战斗强度不只依赖属性倍率

### 炼丹和锻造

- 炼丹师、锻造师独立等级、经验、熟练度和制作任务
- 炼丹配方、锻造图纸统一使用道具解锁
- 一张配方卷轴或图纸只绑定一条配方
- 使用后永久解锁，重复使用仍消耗道具但不重复写入配方
- 解锁时不检查玩家等级，实际制作时检查职业等级、金币、材料和任务状态
- 炼丹材料、锻造材料和产物使用数据库道具模板
- 管理端维护配方、材料、产物、职业等级和卷轴绑定关系

### 灵田、五行和宝石

- 灵田种植、作物成长、收获和催熟
- 催熟材料、成长时间和产出由数据库配置
- 五行聚灵阵、五行属性、升级和资源产出
- 宝石模板、镶嵌、拆卸、合成和返还

### 副本和秘境

- 组队副本、队伍创建、加入、成员状态和副本战斗
- 副本地图、阶段、事件组、事件类型、怪物和奖励配置
- 秘境阶段探索、战斗、资源采集、商人、增益、减益和空事件
- 副本与秘境最终结算装备、道具、令牌、经验和金币
- 事件权重按万分比计算
- 未命中有效事件时，从空事件池随机选择提示，避免重复显示同一句“无事发生”

### 长期和社交内容

- 宠物获取、培养、喂养、进化和出战
- 任务、成就、签到、兑换码、邮件和新手礼包
- 商店、市场上架、购买、取消、成交和交易记录
- 队伍、宗门、宗门商店和宗门活动
- 好感度、赠礼、关系等级和相关奖励
- 竞技场、排行榜、塔、世界首领和图鉴/收藏

### 管理端

管理端用于维护数据库配置和处理运营操作，主要模块包括：

- 玩家、管理员、角色权限和审计日志
- 道具、装备、地图、怪物、技能、Buff 和宠物
- 任务、成就、签到、礼包、邮件、兑换码和商店
- 炼丹配方、锻造配方、职业等级和制作系统
- 强化、洗炼、分解、宝石、五行和成长规则
- 副本模板、秘境模板、事件组、事件类型和阶段奖励
- 灵田、作物、宗门、好感度、竞技场、排行榜、塔和世界首领
- 图片上传、装备/道具图片管理和 AI 图标生成入口

## 技术栈

- 后端：.NET 8、ASP.NET Core Web API、SqlSugar、SQLite、JWT、SignalR
- 游戏端：Vue 3、Vue Router、Vite、SignalR Client、原生 CSS
- 管理端：Vue 3、TypeScript、Vue Router、Pinia、Vite
- 部署：Docker、Nginx、Docker Compose（可选）

## 数据库和种子数据

当前版本以数据库作为游戏模板和运行配置的来源。启动时会读取并校验道具、装备、怪物、地图、技能、Buff、副本和宠物等必要模板；缺少必要数据时启动失败，不会自动恢复旧种子。

数据库文件：

~~~
backend/XXX.WebApi/Data/game-dev.db
deploy/api/Data/game-dev.db
~~~

两份文件是本发布目录中的同一份本地基准库。数据库包含模板配置和本地运行数据。正式运营时请先备份，再按需要清理玩家运行数据，不要直接覆盖正在使用的生产库。

内容和结构参考：

- backend/纯净版内容导入基准_v1.sql
- backend/纯净版内容导入清单_v1.json
- backend/database_schema.sql
- backend/SEED_DATA_CATALOG.md
- backend/XXX.Core/Data/ 下的系统设计方案

旧种子代码不再作为启动时的数据恢复来源。新增内容应通过管理端、数据库迁移脚本或新的基准库完成。

## 本地运行

环境要求：

- .NET SDK 8.0
- Node.js 18 或更高版本
- npm
- SQLite 查看工具（可选）

### 后端

~~~
cd backend
dotnet restore .\XXX.sln
dotnet run --project .\XXX.WebApi\XXX.WebApi.csproj
~~~

默认地址：

~~~
http://127.0.0.1:5247
http://127.0.0.1:5247/swagger/index.html
~~~

### 游戏端

~~~
cd frontend\xiuxian-game
npm install
npm run dev
~~~

默认地址：http://localhost:3000/。

游戏端 API 默认使用 http://127.0.0.1:5247，也可以配置：

~~~
$env:VITE_API_BASE_URL = "http://127.0.0.1:5247"
npm run dev
~~~

### 管理端

~~~
cd frontend\admin-web
npm install
npm run dev
~~~

默认地址：http://localhost:5176/。

管理端 API 配置：

~~~
$env:VITE_ADMIN_API_BASE_URL = "http://127.0.0.1:5247"
npm run dev
~~~

### 构建

~~~
cd backend
dotnet build .\XXX.sln --no-restore

cd ..\frontend\xiuxian-game
npm run build

cd ..\admin-web
npm run build
~~~

## 配置说明

后端配置文件：

- backend/XXX.WebApi/appsettings.json
- deploy/api/appsettings.json

生产环境建议使用环境变量覆盖配置：

~~~
ASPNETCORE_URLS=http://+:8080
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=Data/game-dev.db;Cache=Shared
Jwt__SecretKey=长度至少32位的随机密钥
Jwt__Issuer=XXXGameAPI
Jwt__Audience=XXXGameClient
Admin__DefaultAccount=admin
Admin__DefaultPassword=首次部署时设置的管理员密码
AiIcon__ApiKey=可选的图片生成服务 Key
~~~

AI 图标生成不是核心游戏功能。未配置 AiIcon__ApiKey 时，生成图片接口不可用，但游戏端、管理端其他功能仍然可以运行；已有图片上传和展示不受影响。

前端构建变量：

~~~
VITE_API_BASE_URL       # 游戏端 API 地址
VITE_ADMIN_API_BASE_URL # 管理端 API 地址
~~~

## 推荐玩法流程

1. 注册账号并登录。
2. 创建角色并选择职业。
3. 通过修炼、战斗和任务获得等级、经验、金币和材料。
4. 从普通地图开始，逐步挑战材料图、精英图、首领图和副本。
5. 使用装备提升攻击、防御和生存能力，不需要的装备出售或分解。
6. 获得技能书后解锁技能，用技能书碎片升级技能。
7. 使用炼丹卷轴和锻造图纸解锁配方。
8. 收集材料制作药品、装备和其他生产产物。
9. 培养宠物、升级五行聚灵阵、种植灵田和镶嵌宝石。
10. 组队挑战副本，进入秘境处理阶段事件并领取最终奖励。
11. 开启离线战斗，回归后领取离线结算。

## Docker 部署

### Compose 部署

服务器安装 Docker 和 Docker Compose 后，将 deploy 目录上传：

~~~
mkdir -p /opt/xiuxian
scp -r deploy 用户名@服务器IP:/opt/xiuxian/
cd /opt/xiuxian/deploy
docker compose up -d
docker compose ps
docker compose logs --tail=200 api
~~~

旧版 Compose 使用 docker-compose up -d。

默认地址：

| 服务 | 地址 |
|---|---|
| 游戏端 | http://服务器IP/ |
| 管理端 | http://服务器IP:5176/ |
| API | http://服务器IP:5247/ |
| Swagger | http://服务器IP:5247/swagger/index.html |

### 不安装 Compose 的手动 Docker 部署

CentOS 7.6 只安装 Docker 时：

~~~
cd /home/xiuxian/deploy
docker network create xiuxian-net 2>/dev/null || true
~~~

启动 API：

~~~
docker build -f api.Dockerfile -t xiuxian-api .
docker rm -f xiuxian-api 2>/dev/null || true
docker run -d \
  --name xiuxian-api \
  --restart unless-stopped \
  --network xiuxian-net \
  -p 5247:8080 \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e 'ConnectionStrings__DefaultConnection=Data Source=Data/game-dev.db;Cache=Shared' \
  -v /home/xiuxian/deploy/api/Data:/app/Data \
  -v /home/xiuxian/deploy/api/wwwroot/uploads:/app/wwwroot/uploads \
  xiuxian-api
~~~

启动游戏端：

~~~
docker rm -f xiuxian-game 2>/dev/null || true
docker run -d \
  --name xiuxian-game \
  --restart unless-stopped \
  --network xiuxian-net \
  -p 80:80 \
  -v /home/xiuxian/deploy/game:/usr/share/nginx/html:ro \
  -v /home/xiuxian/deploy/game.nginx.conf:/etc/nginx/conf.d/default.conf:ro \
  nginx:1.27-alpine
~~~

启动管理端：

~~~
docker rm -f xiuxian-admin 2>/dev/null || true
docker run -d \
  --name xiuxian-admin \
  --restart unless-stopped \
  --network xiuxian-net \
  -p 5176:80 \
  -v /home/xiuxian/deploy/admin:/usr/share/nginx/html:ro \
  -v /home/xiuxian/deploy/admin.nginx.conf:/etc/nginx/conf.d/default.conf:ro \
  nginx:1.27-alpine
~~~

手动方式要求 API 容器名为 xiuxian-api，且三个容器加入同一个 xiuxian-net 网络，因为 Nginx 配置通过容器名反向代理。

### 镜像拉取失败

CentOS 服务器访问 Docker Hub 超时时，可以先使用镜像加速地址：

~~~
docker pull m.daocloud.io/docker.io/library/nginx:1.27-alpine
docker tag m.daocloud.io/docker.io/library/nginx:1.27-alpine nginx:1.27-alpine
~~~

API 基础镜像也需要可访问：

~~~
docker pull m.daocloud.io/docker.io/library/aspnet:8.0
docker tag m.daocloud.io/docker.io/library/aspnet:8.0 mcr.microsoft.com/dotnet/aspnet:8.0
~~~

也可以在网络正常的机器构建后导出镜像：

~~~
docker save xiuxian-api -o xiuxian-api.tar
docker load -i xiuxian-api.tar
~~~

### 发布更新

更新前备份数据库：

~~~
cp /home/xiuxian/deploy/api/Data/game-dev.db \
   /home/xiuxian/deploy/api/Data/game-dev.db.backup-$(date +%Y%m%d-%H%M%S)
~~~

只更新前端静态文件：

~~~
docker restart xiuxian-game xiuxian-admin
~~~

更新 API 镜像：

~~~
docker build -f /home/xiuxian/deploy/api.Dockerfile \
  -t xiuxian-api /home/xiuxian/deploy
docker rm -f xiuxian-api
~~~

然后重新执行 API 的 docker run 命令。不要在没有备份的情况下覆盖 api/Data/game-dev.db。

### 常用检查命令

~~~
docker ps
docker logs --tail=200 xiuxian-api
docker logs --tail=100 xiuxian-game
docker logs --tail=100 xiuxian-admin
docker exec xiuxian-api ls -lh /app/Data
docker exec xiuxian-game nginx -t
docker inspect xiuxian-api
curl -I http://127.0.0.1/
curl -I http://127.0.0.1:5176/
curl -I http://127.0.0.1:5247/
~~~

常见问题：

- host not found in upstream：检查 API 容器是否名为 xiuxian-api，并确认三个容器在同一个 Docker 网络。
- not a directory：检查 Nginx 配置宿主机路径是否为文件，而不是同名目录。
- API 提示缺少模板：检查 /app/Data/game-dev.db 是否挂载正确，数据库是否包含必要模板。
- 图片 404：检查 api/wwwroot/uploads 是否挂载，数据库图片路径是否使用 /uploads/...。
- Swagger 404：生产环境可能关闭或不暴露 Swagger，以 API 日志和前端请求为准。
- 页面数据未刷新：关闭并重新打开对应弹窗，仍异常时检查 API 日志和浏览器控制台。

## 生产安全建议

1. 修改 Jwt SecretKey，使用长度至少 32 位的随机字符串。
2. 修改默认管理员账号和密码，并删除不需要的管理员。
3. 通过环境变量配置 AiIcon__ApiKey，不要把真实 Key 写入提交记录。
4. 生产环境使用独立数据库，不要直接使用开发库。
5. 限制 API、管理端和 Swagger 的公网访问范围。
6. 定期备份 api/Data/game-dev.db 和 api/wwwroot/uploads。
7. 不要把数据库备份、日志、Token 和隐私图片提交到公开仓库。
8. 修改数据库结构前先备份，并进行完整性检查。

## 开发约定

- 新增内容优先使用管理端配置或数据库基准脚本，不在业务代码中硬编码掉落和奖励。
- 新增奖励入口统一复用后端发放服务，确保堆叠、容量和锁定规则一致。
- 战斗数值修改必须同时检查人物属性、装备、技能、Buff、怪物技能和掉落预算。
- 前端只负责展示和提交操作，权限、资源校验、战斗结果和奖励结算由后端验证。
- 数据库结构变更同步更新 database_schema.sql 和部署说明。
- 修改完成后至少执行后端、游戏端和管理端构建。

## 许可证

当前项目未附带单独许可证文件。公开发布前请根据实际授权范围补充许可证；未声明许可证时，其他人默认不获得复制、修改和再分发代码的授权。
