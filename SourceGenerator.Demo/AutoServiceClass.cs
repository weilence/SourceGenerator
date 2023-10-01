using Microsoft.Extensions.DependencyInjection;
using SourceGenerator.Common;

namespace SourceGenerator.Demo;

[Service(Lifetime = ServiceLifetime.Scoped)]
public partial class AutoServiceClass
{
}