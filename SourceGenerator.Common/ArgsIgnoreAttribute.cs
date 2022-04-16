using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgsIgnoreAttribute : Attribute
    {
        public const string Name = "ArgsIgnore";
    }
}