using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreAttribute : Attribute
    {
        public const string Name = "Ignore";
    }
}