using System;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class ArgsAttribute : Attribute
    {
        public const string Name = "Args";

        public string Init { get; set; }

        public bool IsOptions { get; set; }
    }
}