using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

        private const string test4;
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
    public void TestAutoAppSettings()
    {
        var expected = @"// Auto-generated code

namespace compilation
{
    public class AppSettings
    {
        public const string Test = ""1"";
        public const int Test2 = 2;
        public const bool Test3 = true;
        public const decimal Test7 = 7.0;
    }
}";

        var actual = Run<AutoAppSettingsGenerator>("");

        Assert.Equal(expected, actual[0]);
    }

    [Fact]
    public void TestAutoArgs()
    {
        var source1 = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Args]
        private string _test;

        [Args]
        private string _test2, _test3;

        private const string test4;
    }

    [Args]
    public partial class UserClass2
    {
        private string _test;

        private string _test2, _test3;

        private const string test4;

        public string test5;

        [ArgsIgnore]
        public string test6;
    }
}";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(string a0, string a1, string a2)
        {
            this._test = a0;
            this._test2 = a1;
            this._test3 = a2;
        }
    }
}";
        var expected2 = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass2
    {
        public UserClass2(string a0, string a1, string a2)
        {
            this._test = a0;
            this._test2 = a1;
            this._test3 = a2;
        }
    }
}";

        var actual = Run<AutoArgsGenerator>(source1);

        Assert.Equal(expected, actual[0]);
        Assert.Equal(expected2, actual[1]);
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

        return runResult.GeneratedTrees.Select(m => m.GetText().ToString()).ToList();
    }

    private static Compilation CreateCompilation(IEnumerable<string> sources)
    {
        var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        var reference = MetadataReference.CreateFromFile(typeof(PropertyAttribute).Assembly.Location);
        var syntaxTrees = sources.Select(m => CSharpSyntaxTree.ParseText(m));
        var compilation = CSharpCompilation.Create("compilation", syntaxTrees, new[] { reference },
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

        public override bool TryGetValue(string key, out string? value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}