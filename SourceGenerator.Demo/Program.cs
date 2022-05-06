using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Demo;

class Program
{
    static void Main(string[] args)
    {
        var autoPropertyClass = new AutoPropertyClass();
        Console.WriteLine(autoPropertyClass.Test);
        Console.WriteLine(autoPropertyClass.Test2);
        var services = new ServiceCollection();
        services.AddOptions<AppSettings>().BindConfiguration("");
        services.AddAutoServices();
        var appSettings = new AppSettings();
        appSettings.Test = "test";
        appSettings.Test3 = "test";
    }
}