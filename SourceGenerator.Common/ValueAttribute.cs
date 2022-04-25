using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ValueAttribute : Attribute
    {
        public const string Name = "Value";
    }
}