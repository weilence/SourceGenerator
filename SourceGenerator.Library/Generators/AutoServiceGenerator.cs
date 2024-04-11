using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

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
                    sourceOutputContext.AddSource("AutoServiceExtension.Class.g.cs",
                        new AutoService(data).TransformText());
                });

            var pipeline2 = context.SyntaxProvider.ForAttributeWithMetadataName(
                "SourceGenerator.Common.ServiceAttribute",
                static (_, _) => true,
                AutoArgsGenerator.Transform
            );

            context.RegisterSourceOutput(pipeline2, AutoArgsGenerator.Output);
        }
    }
}