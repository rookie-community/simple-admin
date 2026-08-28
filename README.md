# Simple Admin

简要说明

## 项目简介
`Simple Admin` 是一个基于ABP vNext的通用权限管理框架，包含WPF与Web的简单示例。

## 目录结构

```
Simple Admin/
├── Admin.Application/           # AppService、业务编排
├── Admin.Application.Contracts/ # DTO、应用/基础设施接口
├── Admin.DbMigrator/            # 数据迁移工具
├── Admin.Desktop/               # 基于HandyControl的WPF客户端，接口服务由Host提供
├── Admin.Domain/                # 实体、领域规则
├── Admin.Domain.Shared/         # 枚举等共享常量
├── Admin.EntityFrameworkCore/   # DbContext、Migrations
├── Admin.MongoDB/               # MongoDB
├── Admin.HttpApi.Host/          # Api 接口服务
├── Admin.HttpApi/               # Controller、Filter
├── Admin.HttpApi.Client/        # Http Proxy
├── Admin.Quartz/                # Quartz定时器、BackgroudJobs、BackgroundWorkers
├── Admin.Test/                  # 单元测试
└── Admin.Web/                   # 基于Layui的Web版本
```

## 贡献
欢迎提交 Issue / Pull Request。请在提交前：
- 保持代码风格一致
- 添加必要的注释与单元测试（如适用）

## 许可证
[Apache-2.0](./LICENSE.txt)

## 联系
如需帮助或讨论，请在仓库Issue 中创建条目。