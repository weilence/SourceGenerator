using Microsoft.Extensions.Options;
using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Logger]
[Service(typeof(IAutoArgsClass))]
public partial class AutoArgsClass : IAutoArgsClass
{
    private readonly AutoServiceClass _autoServiceClass;
    [Ignore] private readonly AppSettings _appSettings;

    private AutoArgsClass(IOptions<AppSettings> appSettings)
    {
        this._appSettings = appSettings.Value;
    }
}

public interface IAutoArgsClass
{
}