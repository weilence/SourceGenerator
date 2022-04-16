## Introduction
This project's purpose is to provide repeat code generated from [Source Generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)

## Install
Nuget: [SourceGenerator.Library](https://www.nuget.org/packages/SourceGenerator.Library/)

## How To Use
When install or update `SourceGenerator.Library`, you need close and reopen IDE for intelligent.

### AppSettings Generator
This generator will generate appsettings.json `top level key` to `AppSettings.[Key]`.

Only simple types are supported.

Example:
```json
{
  "Test": "1",
  "Test2": 2,
  "Test3": true,
  "Test4": {},
  "Test5": [],
  "Test6": null
}
```
Generated:
```c#
// Auto-generated code

namespace compilation
{
    public class AppSettings
    {
        public const string Test = "1";
        public const int Test2 = 2;
        public const bool Test3 = True;
    }
}
```
Test4 is `object`, Test5 is `array`, Test6 is `null`, so it will be ignored.

### AutoArgs Generator
This generator will auto generate constructor by field and need install [SourceGenerator.Common](https://www.nuget.org/packages/SourceGenerator.Common/)

You can use `ArgsAttribute` in `class` or `field`. When used in class, you can use `ArgsIgnoreAttribute` to ignore field.

**Target class must be public partial.**

Only `private` `non-static` `non-const` field is supported.

Example:
```c#
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Args]
        private string _test;

        [Args]
        private string _test2, _test3;

        private const string test4;
    }

    [Args]
    public partial class UserClass2
    {
        private string _test;

        private string _test2, _test3;

        private const string test4;

        public string test5;
    }
}
```
Generated:
```c#
// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public UserClass(string a0, string a1, string a2)
        {
            this._test = a0;
            this._test2 = a1;
            this._test3 = a2;
        }
    }
}
```
and
```c#
// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass2
    {
        public UserClass2(string a0, string a1, string a2)
        {
            this._test = a0;
            this._test2 = a1;
            this._test3 = a2;
        }
    }
}
```

### AutoProperty Generator
This generator will auto generate property by field and need install [SourceGenerator.Common](https://www.nuget.org/packages/SourceGenerator.Common/)

You can use `PropertyAttribute` in `field`.

**Target class must be public partial.**

Example:
```c#
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        [Property]
        private string _test;

        [Property]
        private string _test2, _test3;

        private const string test4;
    }
}
```
Generated:
```c#
// Auto-generated code
using SourceGenerator.Common;

namespace SourceGenerator.Demo
{
    public partial class UserClass
    {
        public string Test { get => _test; set => _test = value; }
        public string Test2 { get => _test2; set => _test2 = value; }
        public string Test3 { get => _test3; set => _test3 = value; }
    }
}
```