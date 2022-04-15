using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgsAttribute : Attribute
    {
        public const string Name = "Args";
    }
}