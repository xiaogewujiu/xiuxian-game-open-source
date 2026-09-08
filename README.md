# 修仙游戏

这是一个包含后端、游戏端和管理端的修仙类游戏项目。

## 目录

- `backend`：ASP.NET Core 后端与本地 SQLite 数据库
- `frontend/xiuxian-game`：游戏端
- `frontend/admin-web`：管理端
- `deploy`：部署产物与 Nginx 配置

## 本地运行

后端使用 .NET 8，前端使用 Node.js 与 npm。数据库基准文件位于：

```text
backend/XXX.WebApi/Data/game-dev.db
```

前端分别进入 `frontend/xiuxian-game` 和 `frontend/admin-web` 执行依赖安装与构建。

## 配置

运行环境可以通过 ASP.NET Core 配置覆盖数据库、JWT、管理员和 AI 图标服务配置。AI 图标服务默认不配置 Key，需要使用时通过环境变量提供 `AiIcon__ApiKey`。

