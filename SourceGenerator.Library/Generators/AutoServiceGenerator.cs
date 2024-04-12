using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoServiceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
            {
                postInitializationContext.AddSource("ServiceAttribute.cs", SourceText.From("""
                    using System;
                    using Microsoft.Extensions.DependencyInjection;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class ServiceAttribute : Attribute
                        {
                            public Type[] Types { get; set; }
                            
                            public ServiceLifetime Lifetime { get; set; }
                            
                            public ServiceAttribute(params Type[] types)
                            {
                                Types = types;
                            }
                        }
                    }
                    """, Encoding.UTF8));
            });

            var pipeline1 = context.SyntaxProvider.ForAttributeWithMetadataName(
                "SourceGenerator.Common.ServiceAttribute",
                static (syntaxNode, cancellationToken) => true,
                static (context, cancellationToken) =>
                {
                    var model = new GeneratedModel<AutoServiceItem>();
                    var data = new AutoServiceItem()
                    {
                        Class = context.TargetSymbol.ToString(), Types = new List<string>(),
                    };

                    var attributeData = context.Attributes.FirstOrDefault();
                    foreach (var attributeDataConstructorArgument in attributeData.ConstructorArguments)
                    {
                        foreach (var value in attributeDataConstructorArgument.Values)
                        {
                            data.Types.Add(value.ToCSharpString());
                        }
                    }

                    foreach (var argumentSyntax in attributeData.NamedArguments)
                    {
                        data.Lifetime = argumentSyntax.Value.ToCSharpString();
                    }

                    model.Data = data;
                    return model;
                }
            );

            var errorProvider = pipeline1.Where(model => model.HasError);
            context.RegisterSourceOutput(errorProvider, static (sourceOutputContext, models) =>
            {
                foreach (var diagnostic in models.Diagnostics)
                {
                    sourceOutputContext.ReportDiagnostic(diagnostic);
                }
            });

            var successProvider = pipeline1.Where(model => !model.HasError && model.Data != null)
                .Select((m, _) => m.Data)
                .Collect();
            context.RegisterSourceOutput(successProvider,
                static (sourceOutputContext, data) =>
                {
                    var code = Write(data);
                    sourceOutputContext.AddSource("AutoServiceExtension.Class.g.cs", code);
                });

            var pipeline2 = context.SyntaxProvider.ForAttributeWithMetadataName(
                "SourceGenerator.Common.ServiceAttribute",
                static (_, _) => true,
                AutoArgsGenerator.Transform
            );

            context.RegisterSourceOutput(pipeline2, AutoArgsGenerator.Output);
        }

        public record AutoServiceItem
        {
            public string Class { get; set; }

            public ValueArray<string> Types { get; set; }

            public string Lifetime { get; set; }
        }

        private static string Write(IEnumerable<AutoServiceItem> model)
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);
            writer.WriteLine("""
                             // Auto-generated code
                             using System;

                             namespace Microsoft.Extensions.DependencyInjection
                             {
                                 public static class AutoServiceExtension
                                 {
                                     public static IServiceCollection AddAutoServices(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
                                     {
                             """);
            writer.Indent += 3;

            foreach (var @class in model)
            {
                var lifetime = "lifetime";
                if (@class.Lifetime != null)
                {
                    lifetime = @class.Lifetime;
                }

                if (@class.Types.Count > 0)
                {
                    foreach (var type in @class.Types)
                    {
                        writer.WriteLine($"AddService(services, {type}, typeof({@class.Class}), {lifetime});");
                    }
                }
                else
                {
                    writer.WriteLine(
                        $"AddService(services, typeof({@class.Class}), typeof({@class.Class}), {lifetime});");
                }
            }

            writer.Indent -= 3;
            writer.WriteLine("""
                                         return services;
                                     }
                             
                                     private static void AddService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
                                     {
                                         switch (lifetime)
                                         {
                                             case ServiceLifetime.Singleton:
                                                 services.AddSingleton(serviceType, implementationType);
                                                 break;
                                             case ServiceLifetime.Scoped:
                                                 services.AddScoped(serviceType, implementationType);
                                                 break;
                                             case ServiceLifetime.Transient:
                                                 services.AddTransient(serviceType, implementationType);
                                                 break;
                                             default:
                                                 throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
                                         }
                                     }
                                 }
                             }
                             """);


            return sw.ToString();
        }
    }
}