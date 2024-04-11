using Xunit;
using SourceGenerator.Library.Generators;

namespace SourceGenerator.Test;

public class AutoOptionsGeneratorTest : BaseTest
{
    [Fact]
    public void Test()
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
}
".ReplaceLineEndings();

        var actual = Run<AutoOptionsGenerator>(source);

        Assert.Equal(expected, actual[1]);
    }
}