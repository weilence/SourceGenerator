using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ArgsAttribute : Attribute
    {
        public const string Name = "Args";
    }
}