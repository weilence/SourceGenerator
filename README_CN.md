# SourceGenerator.Library

[![NuGet](https://img.shields.io/nuget/v/SourceGenerator.Library.svg)](https://www.nuget.org/packages/SourceGenerator.Library/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

一个强大的 C# 源代码生成器库，可自动生成重复代码以提高开发效率。此库利用 [.NET 源代码生成器](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) 技术提供编译时代码生成。

## 功能特性

- **增量源代码生成**：采用 .NET 现代化的**增量生成器**技术构建，实现最佳性能和最小编译开销
- **AutoOptions 生成器**：从 JSON 文件自动生成强类型配置类
- **AutoArgs 生成器**：基于类字段自动生成构造函数，支持高级功能
- **AutoService 生成器**：自动注册服务到依赖注入容器
- **零运行时开销**：所有代码生成都在编译时完成
- **IDE 集成**：为生成的代码提供完整的 IntelliSense 支持
- **高性能**：利用增量编译技术，仅在必要时重新生成代码，显著减少构建时间

## 安装

在您的项目中安装 NuGet 包：

```bash
dotnet add package SourceGenerator.Library
```

或通过包管理器控制台：

```powershell
Install-Package SourceGenerator.Library
```

**重要提示**：安装或更新 `SourceGenerator.Library` 后，您可能需要关闭并重新打开 IDE 以启用生成代码的 IntelliSense。

## 代码生成器

### 1. AutoOptions 生成器

此生成器从 JSON 文件（如 `appsettings.json`）创建强类型配置类，提供编译时安全性和 IntelliSense 支持。

#### 用法

```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Options(Path = "appsettings.json")]
    public partial class AppSettings
    {
        // 属性将根据JSON结构自动生成
    }
}
```

#### 示例

**appsettings.json：**
```json
{
  "ConnectionString": "Server=localhost;Database=MyApp;",
  "MaxRetryCount": 3,
  "EnableLogging": true,
  "DatabaseOptions": {
    "Timeout": 30,
    "RetryCount": 3
  }
}
```

**生成的代码：**
```csharp
// 自动生成的代码
namespace MyProject
{
    public partial class AppSettings
    {
        public string ConnectionString { get; set; }
        public int MaxRetryCount { get; set; }
        public bool EnableLogging { get; set; }
        public DatabaseOptions DatabaseOptions { get; set; }
    }

    public partial class DatabaseOptions
    {
        public int Timeout { get; set; }
        public int RetryCount { get; set; }
    }
}
```

### 2. AutoArgs 生成器

基于类的私有只读字段自动生成构造函数。此生成器包含高级功能，如自动日志注入和键控服务支持。

#### 要求
- 目标类必须是 `public partial`
- 仅处理 `private readonly`、`non-static`、`non-const` 字段
- 带有初始化器的字段将被忽略

#### 特殊特性

- `[Args]`：应用于类以为所有符合条件的字段生成构造函数
- `[Ignore]`：在使用类级 `[Args]` 时跳过特定字段
- `[Logger]`：自动将 `ILogger<T>` 注入到构造函数中
- `[Key("key")]`：为特定字段使用键控服务注入
- `[PostConstruct]`：标记一个私有方法，在构造函数完成字段初始化后自动调用

#### 用法示例

**基本用法：**
```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Args]
    public partial class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        
        [Ignore]
        private readonly string _cache; // 此字段将被忽略
    }
}
```

**使用日志注入：**
```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Args]
    [Logger]
    public partial class UserService
    {
        private readonly IUserRepository _userRepository;
        // ILogger<UserService> _logger 将自动添加
    }
}
```

**使用键控服务：**
```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Args]
    public partial class NotificationService
    {
        [Key("email")]
        private readonly INotificationProvider _emailProvider;
        
        [Key("sms")]
        private readonly INotificationProvider _smsProvider;
    }
}
```

**使用 PostConstruct 初始化：**
```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Args]
    public partial class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        [PostConstruct]
        private void Initialize(string connectionString, int maxRetryCount)
        {
            // 在构造函数完成后自动调用的初始化逻辑
            // 可以使用已注入的依赖项和额外的参数
        }
    }
}
```

**生成的构造函数：**
```csharp
// 自动生成的代码
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject
{
    public partial class UserService
    {
        public UserService(IUserRepository userRepository, IConfiguration configuration, string connectionString, int maxRetryCount)
        {
            this._userRepository = userRepository;
            this._configuration = configuration;

            this.Initialize(connectionString, maxRetryCount);
        }
    }
}
```

#### 参数命名约定

生成器会根据字段名自动创建有意义的参数名：
- `_userRepository` → `userRepository`
- `_emailService` → `emailService`
- `_logger` → `logger`

这使得生成的构造函数更具可读性，并遵循 C# 命名约定。

### 3. AutoService 生成器

自动注册服务到依赖注入容器并生成便于服务注册的扩展方法。

#### 用法

```csharp
using SourceGenerator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject
{
    [Service(typeof(IUserService), Lifetime = ServiceLifetime.Scoped)]
    public partial class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
    }

    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class CacheService
    {
        // 注册为自身类型
    }
}
```

**生成的扩展方法：**
```csharp
// 自动生成的代码
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoServiceExtension
    {
        public static IServiceCollection AddAutoServices(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            AddService(services, typeof(IUserService), typeof(UserService), ServiceLifetime.Scoped);
            AddService(services, typeof(CacheService), typeof(CacheService), ServiceLifetime.Singleton);
            return services;
        }

        private static void AddService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(serviceType, implementationType);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(serviceType, implementationType);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(serviceType, implementationType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}
```

**在 Program.cs 中注册服务：**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 注册所有标记有 [Service] 特性的服务
builder.Services.AddAutoServices();

var app = builder.Build();
```

## 项目结构

```
SourceGenerator/
├── SourceGenerator.Library/     # 核心源代码生成器库
├── SourceGenerator.Demo/        # 使用示例和演示
├── SourceGenerator.Test/        # 单元测试
└── README.md                   # 本文件
```

## 技术实现

### 增量源代码生成

此库采用 **.NET 增量源代码生成器**构建，相比传统源代码生成器提供显著的性能提升：

- **增量编译**：仅在输入文件实际发生变化时才重新生成代码，大幅减少编译时间
- **缓存机制**：在构建之间缓存结果，消除不必要的工作
- **依赖追踪**：精确跟踪文件依赖关系，最小化重新生成的范围
- **IDE 性能**：提供更快的 IntelliSense 和更好的 IDE 响应速度
- **构建优化**：减少整体解决方案构建时间，特别是在大型项目中

增量方法意味着即使项目规模增长，您的开发工作流程仍然保持快速，使此库既适用于小型应用程序，也适用于大型企业解决方案。 

## 系统要求

- .NET Standard 2.0 或更高版本
- C# 9.0 或更高版本（用于源代码生成器支持）
- Visual Studio 2019 16.9+ 或带有 C# 扩展的 VS Code

## 贡献

欢迎贡献！请随时提交 Pull Request。对于重大更改，请先打开一个问题来讨论您想要更改的内容。

## 许可证

此项目根据 MIT 许可证授权 - 有关详细信息，请参阅 [LICENSE](LICENSE) 文件。

## 致谢

- 使用 [Microsoft.CodeAnalysis](https://github.com/dotnet/roslyn) Roslyn 分析器构建
- 受 .NET 源代码生成器生态系统启发

## 支持

如果您遇到任何问题或有疑问：

1. 查看 [Issues](https://github.com/Weilence/SourceGenerator/issues) 页面
2. 创建包含详细信息的新问题
3. 提供重现问题的示例代码

---

有关更多示例和高级用法，请查看此仓库中的 `SourceGenerator.Demo` 项目。