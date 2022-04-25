using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsAttribute : Attribute
    {
        public const string Name = "Options";
        public string Path { get; set; }
    }
}