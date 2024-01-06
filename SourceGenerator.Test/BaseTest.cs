using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SourceGenerator.Common;

namespace SourceGenerator.Test;

public abstract class BaseTest
{
    public List<string> Run<T>(params string[] sources) where T : ISourceGenerator, new()
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
                throw new Exception(diagnostic.GetMessage());
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

    private class MockAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
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

    private class MockAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _dictionary;

        public MockAnalyzerConfigOptions(Dictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
}