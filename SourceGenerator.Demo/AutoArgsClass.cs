using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Logger]
[Service(Type = typeof(IAutoArgsClass))]
public partial class AutoArgsClass : IAutoArgsClass
{
    private readonly AutoPropertyClass _autoPropertyClass;
    private readonly AutoServiceClass _autoServiceClass;

    private AutoArgsClass(AutoServiceClass autoServiceClass)
    {
        _autoServiceClass = autoServiceClass;
    }
}

public interface IAutoArgsClass
{
}