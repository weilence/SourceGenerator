# SourceGenerator.Library

[![NuGet](https://img.shields.io/nuget/v/SourceGenerator.Library.svg)](https://www.nuget.org/packages/SourceGenerator.Library/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A powerful C# Source Generator library that automatically generates repetitive code to improve development productivity. This library leverages the [.NET Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) technology to provide compile-time code generation.

## Features

- **Incremental Source Generation**: Built with .NET's modern **Incremental Generator** technology for optimal performance and minimal compilation overhead
- **AutoOptions Generator**: Automatically generates strongly-typed configuration classes from JSON files
- **AutoArgs Generator**: Generates constructors automatically based on class fields with advanced features
- **AutoService Generator**: Automatically registers services with dependency injection container
- **Zero Runtime Overhead**: All code generation happens at compile time
- **IDE Integration**: Full IntelliSense support for generated code
- **High Performance**: Utilizes incremental compilation to only regenerate code when necessary, significantly reducing build times

## Installation

Install the NuGet package in your project:

```bash
dotnet add package SourceGenerator.Library
```

Or via Package Manager Console:

```powershell
Install-Package SourceGenerator.Library
```

**Important**: After installing or updating `SourceGenerator.Library`, you may need to close and reopen your IDE to enable IntelliSense for the generated code.

## Generators

### 1. AutoOptions Generator

This generator creates strongly-typed configuration classes from JSON files (like `appsettings.json`), providing compile-time safety and IntelliSense support for configuration values.

#### Usage

```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Options(Path = "appsettings.json")]
    public partial class AppSettings
    {
        // Properties will be auto-generated based on JSON structure
    }
}
```

#### Example

**appsettings.json:**
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

**Generated Code:**
```csharp
// Auto-generated code
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

### 2. AutoArgs Generator

Automatically generates constructors for classes based on their private readonly fields. This generator includes advanced features like automatic logger injection and keyed services support.

#### Requirements
- Target class must be `public partial`
- Only `private readonly`, `non-static`, `non-const` fields are processed
- Fields with initializers are ignored

#### Special Attributes

- `[Args]`: Apply to class to generate constructor for all eligible fields
- `[Ignore]`: Skip specific fields when using class-level `[Args]`
- `[Logger]`: Automatically inject `ILogger<T>` into the constructor
- `[Key("key")]`: Use keyed service injection for specific fields
- `[PostConstruct]`: Mark a private method to be automatically called after constructor field initialization

#### Usage Examples

**Basic Usage:**
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
        private readonly string _cache; // This field will be ignored
    }
}
```

**With Logger Injection:**
```csharp
using SourceGenerator.Common;

namespace MyProject
{
    [Args]
    [Logger]
    public partial class UserService
    {
        private readonly IUserRepository _userRepository;
        // ILogger<UserService> _logger will be automatically added
    }
}
```

**With Keyed Services:**
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

**With PostConstruct Initialization:**
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
            // Initialization logic called automatically after constructor
            // Can use injected dependencies and additional parameters
        }
    }
}
```

**Generated Constructor:**
```csharp
// Auto-generated code
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

#### Parameter Naming Convention

The generator automatically creates meaningful parameter names based on field names:
- `_userRepository` → `userRepository`
- `_emailService` → `emailService`
- `_logger` → `logger`

This makes the generated constructors more readable and follows C# naming conventions.

### 3. AutoService Generator

Automatically registers services with the dependency injection container and generates extension methods for easy service registration.

#### Usage

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
        // Register as self-type
    }
}
```

**Generated Extension Method:**
```csharp
// Auto-generated code
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

**Register Services in Program.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register all services marked with [Service] attribute
builder.Services.AddAutoServices();

var app = builder.Build();
```

## Project Structure

```
SourceGenerator/
├── SourceGenerator.Library/     # Core source generator library
├── SourceGenerator.Demo/        # Example usage and demonstrations
├── SourceGenerator.Test/        # Unit tests
└── README.md                   # This file
```

## Technical Implementation

### Incremental Source Generation

This library is built using **.NET Incremental Source Generators**, which provide significant performance improvements over traditional source generators:

- **Incremental Compilation**: Only regenerates code when input files actually change, dramatically reducing compilation time
- **Caching**: Results are cached between builds, eliminating unnecessary work
- **Dependency Tracking**: Precisely tracks file dependencies to minimize regeneration scope  
- **IDE Performance**: Provides faster IntelliSense and better IDE responsiveness
- **Build Optimization**: Reduces overall solution build times, especially in large projects

The incremental approach means that your development workflow remains fast even as your project grows, making this library suitable for both small applications and large enterprise solutions.

## Requirements

- .NET Standard 2.0 or higher
- C# 9.0 or later (for source generator support)
- Visual Studio 2019 16.9+ or VS Code with C# extension

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Microsoft.CodeAnalysis](https://github.com/dotnet/roslyn) Roslyn analyzers
- Inspired by the .NET Source Generator ecosystem

## Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/Weilence/SourceGenerator/issues) page
2. Create a new issue with detailed information
3. Provide sample code that reproduces the problem

---

For more examples and advanced usage, check out the `SourceGenerator.Demo` project in this repository.