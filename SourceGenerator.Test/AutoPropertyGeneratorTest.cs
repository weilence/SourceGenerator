using SourceGenerator.Library.Generators;
using Xunit;

namespace SourceGenerator.Test;

public class AutoPropertyGeneratorTest : BaseTest
{
    [Fact]
    public void Test()
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
}
".ReplaceLineEndings();

        var actual = Run<AutoPropertyGenerator>(source);

        Assert.Equal(expected, actual[0]);
    }
}