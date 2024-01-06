using SourceGenerator.Library;
using SourceGenerator.Library.Utils;
using Xunit;

namespace SourceGenerator.Test;

public class StringUtilsTest
{
    [Fact]
    public void Test()
    {
        var result = StringUtils.SplitName("_aaaBbbCccDddeee");
        Assert.Equal(4, result.Count);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("bbb", result[1]);
        Assert.Equal("ccc", result[2]);
        Assert.Equal("dddeee", result[3]);

        result = StringUtils.SplitName("aaa_Bbb_CCC_dddeee");
        Assert.Equal(4, result.Count);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("bbb", result[1]);
        Assert.Equal("ccc", result[2]);
        Assert.Equal("dddeee", result[3]);

        Assert.Equal("aaaBbbCccDddeee", StringUtils.ToCamelCase("_aaaBbbCccDddeee"));
        Assert.Equal("AaaBbbCccDddeee", StringUtils.ToPascalCase("_aaaBbbCccDddeee"));
        Assert.Equal("_aaaBbbCccDddeee", StringUtils.ToFieldName("_aaaBbbCccDddeee"));
    }
}