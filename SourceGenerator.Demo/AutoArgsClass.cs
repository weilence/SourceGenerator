using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Logger]
[Service(typeof(IAutoArgsClass))]
public partial class AutoArgsClass : IAutoArgsClass
{
    private readonly AutoServiceClass _autoServiceClass;

    private AutoArgsClass(AutoServiceClass autoServiceClass)
    {
    }
}

public interface IAutoArgsClass
{
}