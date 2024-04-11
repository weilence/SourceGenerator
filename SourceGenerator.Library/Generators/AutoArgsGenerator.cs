using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoArgsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
            {
                postInitializationContext.AddSource("ArgsAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class ArgsAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("IgnoreAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Field)]
                        public class IgnoreAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
                postInitializationContext.AddSource("LoggerAttribute.cs", SourceText.From("""
                    using System;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class LoggerAttribute : Attribute
                        {
                        }
                    }
                    """, Encoding.UTF8));
            });

            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                "SourceGenerator.Common.ArgsAttribute",
                static (_, _) => true,
                Transform
            );

            context.RegisterSourceOutput(pipeline, Output);
        }

        public static GeneratedModel<AutoArgsModel> Transform(GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
            var model = new GeneratedModel<AutoArgsModel>();
            if (!ReportUtils.CheckPartial(model, classDeclarationSyntax))
            {
                return model;
            }

            var usings = SyntaxUtils.GetUsings(classDeclarationSyntax);
            var fields = new List<Field>();

            var namedTypeSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var hasLogger = namedTypeSymbol.GetAttributes().Any(m =>
                m.AttributeClass!.ToString() == "SourceGenerator.Common.LoggerAttribute");
            if (hasLogger)
            {
                usings.Add("using Microsoft.Extensions.Logging;");
                fields.Add(new Field()
                {
                    Type = "ILogger<" + namedTypeSymbol.Name + ">",
                    Name = "_logger",
                });
            }

            var fieldSymbols = namedTypeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Private && m.IsReadOnly && !m.IsStatic &&
                            !m.IsConst && !m.GetAttributes().Any(n =>
                                n.AttributeClass!.ToString() == "SourceGenerator.Common.IgnoreAttribute")
                )
                .ToList();

            foreach (var fieldSymbol in fieldSymbols)
            {
                var syntaxNode = fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                var initializer = syntaxNode!.DescendantNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

                if (initializer != null)
                {
                    continue;
                }

                fields.Add(new Field() { Name = fieldSymbol.Name, Type = fieldSymbol.Type.Name });
            }

            var methodSymbol =
                namedTypeSymbol.Constructors.FirstOrDefault(m => m.DeclaredAccessibility == Accessibility.Private);
            if (methodSymbol != null)
            {
                foreach (var parameterSymbol in methodSymbol.Parameters)
                {
                    var type = parameterSymbol.Type.Name;

                    var field = fields.FirstOrDefault(m => m.Type == type);
                    if (field != null)
                    {
                        field.InBase = true;
                    }
                    else
                    {
                        fields.Add(new Field()
                        {
                            Name = parameterSymbol.Name, Type = type, Ignore = true, InBase = true,
                        });
                    }
                }
            }

            if (fields.Count == 0)
            {
                return model;
            }

            model.Data = new AutoArgsModel()
            {
                Usings = usings,
                Namespace = SyntaxUtils.GetNamespaceName(classDeclarationSyntax),
                Class = SyntaxUtils.GetName(classDeclarationSyntax),
                Fields = fields,
            };
            return model;
        }

        public static void Output(SourceProductionContext sourceOutputContext, GeneratedModel<AutoArgsModel> model)
        {
            if (model.HasError)
            {
                foreach (var diagnostic in model.Diagnostics)
                {
                    sourceOutputContext.ReportDiagnostic(diagnostic);
                }

                return;
            }

            if (model.Data == null)
            {
                return;
            }

            var data = model.Data;
            sourceOutputContext.AddSource($"{data.Namespace}.{data.Class}.g.cs",
                new AutoArgs(data).TransformText());
        }
    }
}