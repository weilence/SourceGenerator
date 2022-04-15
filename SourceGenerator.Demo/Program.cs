namespace SourceGenerator.Demo;

class Program
{
    static void Main(string[] args)
    {
        var autoPropertyClass = new AutoPropertyClass();
        Console.WriteLine(autoPropertyClass.Test);
        Console.WriteLine(autoPropertyClass.Test2);
        Console.WriteLine(AppSettings.Test3);
        var autoArgsClass = new AutoArgsClass("test", "test2");
    }
}