using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoggerAttribute : Attribute
    {
        public const string Name = "Logger";
    }
}