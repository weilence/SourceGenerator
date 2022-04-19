using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Args(Init = nameof(Init))]
public partial class AutoArgsClass
{
    [Args] private string _test = "test";

    [Args] private string _test2 = "test2";
    
    [Args] private AutoPropertyClass _autoPropertyClass;

    public void Init()
    {
        _test2 = "_test3";
    }
}