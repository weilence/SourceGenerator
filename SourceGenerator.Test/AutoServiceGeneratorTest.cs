using SourceGenerator.Library.Generators;
using Xunit;

namespace SourceGenerator.Test;

public class AutoServiceGeneratorTest : BaseTest
{
    [Fact]
    public void Test()
    {
        var expected = @"// Auto-generated code
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AutoServiceExtension
    {
        public static IServiceCollection AddAutoServices(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            AddService(services, typeof(SourceGenerator.Demo.AutoServiceClass), typeof(SourceGenerator.Demo.AutoServiceClass), lifetime);
            AddService(services, typeof(SourceGenerator.Demo2.AutoServiceClass2), typeof(SourceGenerator.Demo2.AutoServiceClass2), lifetime);
            AddService(services, typeof(SourceGenerator.Demo2.IAutoServiceClass), typeof(SourceGenerator.Demo2.AutoServiceClass3), lifetime);
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
}";
        var source = new[]
        {
            @"using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Service]
public class AutoServiceClass
{
    
}",
            @"using SourceGenerator.Common;

namespace SourceGenerator.Demo2;

[Service]
public class AutoServiceClass2
{
    
}

public interface IAutoServiceClass
{
}

[Service]
public class AutoServiceClass3 : IAutoServiceClass
{
    
}"
        };
        var actual = Run<AutoServiceGenerator>(source);

        Assert.Equal(expected, actual[0]);
    }
}