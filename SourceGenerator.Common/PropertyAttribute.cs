using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PropertyAttribute : Attribute
    {
        public const string Name = "Property";
    }
}