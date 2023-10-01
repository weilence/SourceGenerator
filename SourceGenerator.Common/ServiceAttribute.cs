using System;
using Microsoft.Extensions.DependencyInjection;

namespace SourceGenerator.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public const string Name = "Service";
        
        public Type Type { get; set; }

        public ServiceLifetime Lifetime { get; set; }
    }
}