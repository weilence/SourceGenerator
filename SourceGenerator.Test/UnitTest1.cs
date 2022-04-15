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
    }
}";
        var expected = @"// Auto-generated code

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

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestAutoAppSettings()
    {
        var expected = @"// Auto-generated code

namespace compilation
{
    public class AppSettings
    {
        public const string Test3 = ""test3"";
    }
}";

        var actual = Run<AutoAppSettingsGenerator>("");

        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void TestAutoArgs()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Args]
        private string _test;

        [Args]
        private string _test2, _test3;
    }
}";
        var expected = @"// Auto-generated code

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(string test, string test2, string test3)
        {
            this._test = test;
            this._test2 = test2;
            this._test3 = test3;
        }
    }
}";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual);
    }

    private static string Run<T>(string source) where T : ISourceGenerator, new()
    {
        var inputCompilation = CreateCompilation(source);

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

        return runResult.GeneratedTrees[0].GetText().ToString();
    }

    private static Compilation CreateCompilation(string source)
    {
        var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        var reference = MetadataReference.CreateFromFile(typeof(PropertyAttribute).Assembly.Location);
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("compilation", new[] { syntaxTree }, new[] { reference },
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