using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SourceGenerator.Common;
using SourceGenerator.Library;

namespace SourceGenerator.Test;

public class UnitTest1
{
    [Fact]
    public void TestAutoProperty()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Property]
        private string _test;

        [Property]
        private string _test2, _test3;

        private const string test4 = ""test4"";
    }
}";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public string Test { get => _test; set => _test = value; }
        public string Test2 { get => _test2; set => _test2 = value; }
        public string Test3 { get => _test3; set => _test3 = value; }
    }
}";

        var actual = Run<AutoPropertyGenerator>(source);

        Assert.Equal(expected, actual[0]);
    }

    [Fact]
    public void TestAutoOptions()
    {
        var source = @"
using SourceGenerator.Common;

namespace compilation
{
    [Options(Path = ""appsettings.json"")]
    public partial class AppSettings
    {
    }
}";

        var expected = @"// Auto-generated code

namespace compilation
{
    public partial class AppSettings
    {
        public string Test { get; set; }
        public int Test2 { get; set; }
        public bool Test3 { get; set; }
        public Test4 Test4 { get; set; }
        public decimal Test7 { get; set; }
        public decimal Test7_1 { get; set; }
    }

    public partial class Test4
    {
        public string Test41 { get; set; }
    }

}";

        var actual = Run<AutoOptionsGenerator>(source);

        Assert.Equal(expected, actual[0]);
    }

    [Fact]
    public void TestAutoArgs()
    {
        var source1 = @"
using SourceGenerator.Common;
using System.Collections.Generic;

namespace SourceGenerator.Demo
{
    public class UserClass
    {
    }

    [Service]
    public partial class UserClass2
    {
        private readonly UserClass _test;

        private readonly UserClass _test2, _test3;

        [Value]
        private readonly UserClass _test31;

        private const string test4 = ""test4"";

        private string test5;

        private readonly Dictionary<string, string> dic;

        public string test6;

        [Ignore]
        private readonly UserClass3 _test7;

        private UserClass2(UserClass3 test7)
        {
            this._test7 = test7;
        }
    }

    public class UserClass3
    {
    }
}";

        var expected = @"// Auto-generated code
using SourceGenerator.Common;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace SourceGenerator.Demo
{
    public partial class UserClass2
    {
        public UserClass2(UserClass a0, UserClass a1, UserClass a2, IOptions<UserClass> a3, UserClass3 a4) : this(a4)
        {
            this._test = a0;
            this._test2 = a1;
            this._test3 = a2;
            this._test31 = a3.Value;
        }
    }
}";

        var actual = Run<AutoArgsGenerator>(source1);

        Assert.Equal(expected, actual[0]);
    }

    [Fact]
    public void TestAutoService()
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

    private static List<string> Run<T>(params string[] sources) where T : ISourceGenerator, new()
    {
        var inputCompilation = CreateCompilation(sources);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new T());

        driver = driver
            .WithUpdatedAnalyzerConfigOptions(
                CreateAnalyzerConfigOptionsProvider(new Dictionary<string, string>()
                {
                    ["build_property.projectdir"] =
                        AppContext.BaseDirectory[..AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal)]
                })
            )
            .RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();

        var exception = runResult.Results.Select(m => m.Exception).FirstOrDefault();
        if (exception != null)
        {
            throw new Exception(exception.Message, exception);
        }

        var codes = runResult.GeneratedTrees.Select(m => m.GetText().ToString()).ToList();

        foreach (var diagnostic in outputCompilation.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                throw new Exception(diagnostic.ToString());
            }
        }

        var outDiagnostics = outputCompilation.GetDiagnostics();
        foreach (var diagnostic in outDiagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                throw new Exception(diagnostic.ToString());
            }
        }

        return codes;
    }

    private static Compilation CreateCompilation(IEnumerable<string> sources)
    {
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var assemblies = ReferenceAssemblies.NetStandard.NetStandard20.ResolveAsync(null, CancellationToken.None)
            .Result.ToList();
        assemblies.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
        assemblies.Add(MetadataReference.CreateFromFile(typeof(PropertyAttribute).Assembly.Location));
        assemblies.Add(MetadataReference.CreateFromFile(typeof(IOptions<>).Assembly.Location));

        var syntaxTrees = sources.Select(m => CSharpSyntaxTree.ParseText(m));
        var compilation = CSharpCompilation.Create("compilation", syntaxTrees, assemblies,
            options
        );

        return compilation;
    }

    private static AnalyzerConfigOptionsProvider
        CreateAnalyzerConfigOptionsProvider(Dictionary<string, string> dictionary) =>
        new MockAnalyzerConfigOptionsProvider(new MockAnalyzerConfigOptions(dictionary));

    public class MockAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        public MockAnalyzerConfigOptionsProvider(AnalyzerConfigOptions options)
        {
            GlobalOptions = options;
        }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return GlobalOptions;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return GlobalOptions;
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }
    }

    public class MockAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _dictionary;

        public MockAnalyzerConfigOptions(Dictionary<string, string> dictionary)
        {
            this._dictionary = dictionary;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}