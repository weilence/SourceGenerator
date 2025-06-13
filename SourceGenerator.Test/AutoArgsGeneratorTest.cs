using System.Linq;
using SourceGenerator.Library.Generators;
using Xunit;

namespace SourceGenerator.Test;

public class AutoArgsGeneratorTest : BaseTest
{
    [Fact]
    public void Test_Basic()
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

        public UserClass2(ILogger<UserClass2> logger, UserClass test, UserClass test2, UserClass test3, IOptions<UserClass> test10)
        {
            this._logger = logger;
            this._test = test;
            this._test2 = test2;
            this._test3 = test3;
            this._test10 = test10;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source1);

        Assert.Equal(expected, actual.Last());
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

        public UserClass(ILogger<UserClass> logger)
        {
            this._logger = logger;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_KeyedServices()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    public interface IOtherService
    {
    }

    [Args]
    public partial class UserClass
    {
        private readonly IMyService _myService;

        [Key(""special"")]
        private readonly IOtherService _otherService;

        [Key("""")]
        private readonly string _keyedString;
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(IMyService myService, [FromKeyedServices(""special"")] IOtherService otherService, [FromKeyedServices("""")] string keyedString)
        {
            this._myService = myService;
            this._otherService = otherService;
            this._keyedString = keyedString;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_KeyedServicesWithLogger()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    [Args, Logger]
    public partial class UserClass
    {
        private readonly IMyService _myService;

        [Key(""cache"")]
        private readonly IMyService _cachedService;
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        private readonly ILogger<UserClass> _logger;

        public UserClass(ILogger<UserClass> logger, IMyService myService, [FromKeyedServices(""cache"")] IMyService cachedService)
        {
            this._logger = logger;
            this._myService = myService;
            this._cachedService = cachedService;
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_PostConstruct()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    [Args]
    public partial class UserClass
    {
        private readonly IMyService _myService;
        private readonly string _name;

        [PostConstruct]
        private void Initialize()
        {
            // Initialization logic here
        }
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(IMyService myService, string name)
        {
            this._myService = myService;
            this._name = name;

            this.Initialize();
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_PostConstructWithLogger()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    [Args, Logger]
    public partial class UserClass
    {
        private readonly IMyService _myService;

        [PostConstruct]
        private void Setup()
        {
            // Setup logic with logger available
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

        public UserClass(ILogger<UserClass> logger, IMyService myService)
        {
            this._logger = logger;
            this._myService = myService;

            this.Setup();
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_PostConstructWithKeyedServices()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    [Args]
    public partial class UserClass
    {
        private readonly IMyService _myService;

        [Key(""cache"")]
        private readonly IMyService _cachedService;

        [PostConstruct]
        private void InitializeServices()
        {
            // Initialize services
        }
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;
using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(IMyService myService, [FromKeyedServices(""cache"")] IMyService cachedService)
        {
            this._myService = myService;
            this._cachedService = cachedService;

            this.InitializeServices();
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_PostConstructWithParameters()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    public interface IConfiguration
    {
    }

    [Args]
    public partial class UserClass
    {
        private readonly IMyService _myService;

        [PostConstruct]
        private void Initialize(IConfiguration config, string connectionString)
        {
            // Initialization logic with parameters
        }
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(IMyService myService, IConfiguration config, string connectionString)
        {
            this._myService = myService;

            this.Initialize(config, connectionString);
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }

    [Fact]
    public void Test_PostConstructWithExistingParameters()
    {
        var source = @"
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public interface IMyService
    {
    }

    [Args]
    public partial class UserClass
    {
        private readonly IMyService _myService;
        private readonly string _connectionString;

        [PostConstruct]
        private void Initialize(IMyService service, string connectionString)
        {
            // Uses existing injected dependencies
        }
    }
}
";
        var expected = @"// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(IMyService myService, string connectionString)
        {
            this._myService = myService;
            this._connectionString = connectionString;

            this.Initialize(myService, connectionString);
        }
    }
}
";

        var actual = Run<AutoArgsGenerator>(source);

        Assert.Equal(expected, actual.Last());
    }
}