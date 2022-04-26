using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute : Attribute
    {
        public const string Name = "Controller";

        public string Init { get; set; }
    }
}