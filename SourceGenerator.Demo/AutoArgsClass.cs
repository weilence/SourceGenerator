using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Service(Init = nameof(Init))]
public partial class AutoArgsClass
{
    private readonly string _test = "test";

    private readonly string _test2 = "test2";

    private readonly AutoPropertyClass _autoPropertyClass;

    private string _test4;

    public void Init()
    {
        _test4 = "_test3";
    }
}