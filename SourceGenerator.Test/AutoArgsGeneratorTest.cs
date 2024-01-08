using System.Linq;
using SourceGenerator.Library.Generators;
using Xunit;

namespace SourceGenerator.Test;

public class AutoArgsGeneratorTest : BaseTest
{
    [Fact]
    public void Test()
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
}
".ReplaceLineEndings();

        var actual = Run<AutoArgsGenerator>(source1).FirstOrDefault();

        Assert.Equal(expected, actual);
    }
}