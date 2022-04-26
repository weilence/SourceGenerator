using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public const string Name = "Service";

        public string Init { get; set; }
        
        public Type Type { get; set; }
    }
}