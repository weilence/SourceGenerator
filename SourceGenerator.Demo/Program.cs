using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Demo;

class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddOptions<AppSettings>().BindConfiguration("");
        services.AddAutoServices();
        var appSettings = new AppSettings();
        appSettings.Test = "test";
        appSettings.Test3 = "test";
    }
}