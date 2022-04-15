namespace SourceGenerator.Demo;

class Program
{
    static void Main(string[] args)
    {
        var x = new UserClass();
        Console.WriteLine(x.Test);
        Console.WriteLine(x.Test2);
        Console.WriteLine(AppSettings.Test3);
    }
}