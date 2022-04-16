using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class ArgsIgnoreAttribute : Attribute
    {
        public const string Name = "ArgsIgnore";
    }
}