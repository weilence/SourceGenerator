using System.Linq;
using SourceGenerator.Library.Generators;
using Xunit;

namespace SourceGenerator.Test;

public class AutoArgsGeneratorTest : BaseTest
{
    [Fact]
    public void Test_Base()
    {
        var source1 = @"
using SourceGenerator.Common;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Datetime = System.DateTime;

namespace SourceGenerator.Demo
{
    public class UserClass
    {
    }

    [Args, Logger]
    public partial class UserClass2
    {
        private readonly UserClass _test;

        private readonly UserClass _test2, _test3;

        private const string test4 = ""test4"";

        private string test5;

        private readonly Dictionary<string, string> dic = new();

        public string test6;

        [Ignore]
        private readonly UserClass3 _test7;

        private readonly string test8 = """";

        private readonly int test9 = 0;

        private readonly IOptions<UserClass> _test10;

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
using Datetime = System.DateTime;
using Microsoft.Extensions.Logging;

namespace SourceGenerator.Demo
{
    public partial class UserClass2
    {
        private readonly ILogger<UserClass2> _logger;

        public UserClass2(ILogger<UserClass2> a0, UserClass a1, UserClass a2, UserClass a3, IOptions<UserClass> a4, UserClass3 a5) : this(a5)
        {
            this._logger = a0;
            this._test = a1;
            this._test2 = a2;
            this._test3 = a3;
            this._test10 = a4;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source1);

        Assert.Equal(expected, actual[3]);
    }


    [Fact]
    public void Test_OnlyLogger()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    [Logger]
    [Args]
    public partial class UserClass
    {
        private UserClass() 
        {
        }
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;
using Microsoft.Extensions.Logging;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        private readonly ILogger<UserClass> _logger;

        public UserClass(ILogger<UserClass> a0) : this()
        {
            this._logger = a0;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual[3]);
    }
}