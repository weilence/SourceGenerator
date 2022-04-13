using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerator.Common;
using SourceGenerator.Library;

namespace SourceGenerator.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Create the 'input' compilation that the generator will act on
        Compilation inputCompilation = CreateCompilation(@"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Property]
        private string _test = ""test"";

        [Property]
        private string _test2;
    }
}
");

        AutoPropertyGenerator generator = new AutoPropertyGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation,
            out var diagnostics);

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        Assert.Equal(@"// Auto-generated code

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public string Test { get => _test; set => _test = value; }
        public string Test2 { get => _test2; set => _test2 = value; }
    }
}
", runResult.GeneratedTrees[0].GetText().ToString());
    }

    private static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(PropertyAttribute).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}