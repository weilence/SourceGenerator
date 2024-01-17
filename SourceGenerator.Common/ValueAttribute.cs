using System;

namespace SourceGenerator.Common
{
    [Obsolete]
    [AttributeUsage(AttributeTargets.Field)]
    public class ValueAttribute : Attribute
    {
        public const string Name = "Value";
    }
}