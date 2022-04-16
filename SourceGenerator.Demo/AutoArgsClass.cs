using SourceGenerator.Common;

namespace SourceGenerator.Demo;

public partial class AutoArgsClass
{
    [Args] private string _test = "test";

    [Args] private string _test2 = "test2";
    
    [Args] private AutoPropertyClass _autoPropertyClass;
    
}