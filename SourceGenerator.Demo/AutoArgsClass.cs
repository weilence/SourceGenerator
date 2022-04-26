using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Service(Type = typeof(IAutoArgsClass))]
public partial class AutoArgsClass : IAutoArgsClass
{
    private readonly AutoPropertyClass _autoPropertyClass;
}

public interface IAutoArgsClass
{
}