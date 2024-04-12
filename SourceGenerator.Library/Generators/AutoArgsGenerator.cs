using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;
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

            var namedTypeSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var usings = SyntaxUtils.GetUsings(context.TargetNode.SyntaxTree);
            var fields = new List<Field>();
            var hasLogger = namedTypeSymbol.GetAttributes().Any(m =>
                m.AttributeClass!.ToString() == "SourceGenerator.Common.LoggerAttribute");
            if (hasLogger)
            {
                usings.Add("Microsoft.Extensions.Logging");
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
                var syntaxNodes = fieldSymbol.DeclaringSyntaxReferences
                    .Select(m => m.GetSyntax())
                    .OfType<VariableDeclaratorSyntax>()
                    .ToList();
                if (syntaxNodes.Any(m => m.Initializer != null))
                {
                    continue;
                }

                if (syntaxNodes.FirstOrDefault()?.Parent is not VariableDeclarationSyntax variableDeclarationSyntax)
                {
                    continue;
                }

                fields.Add(new Field()
                    { Name = fieldSymbol.Name, Type = variableDeclarationSyntax.Type.ToString() });
            }

            var hasBase = false;
            var methodSymbol =
                namedTypeSymbol.Constructors.FirstOrDefault(m =>
                    m.DeclaredAccessibility == Accessibility.Private && !m.IsStatic);
            if (methodSymbol != null)
            {
                hasBase = true;

                foreach (var parameterSymbol in methodSymbol.Parameters)
                {
                    var parameterSyntax =
                        parameterSymbol.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax() as ParameterSyntax;
                    if (parameterSyntax?.Type == null)
                    {
                        continue;
                    }

                    var type = parameterSyntax.Type.ToString();
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
                HasBase = hasBase,
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
            var code = Write(data);
            sourceOutputContext.AddSource($"{data.Namespace}.{data.Class}.g.cs", code);
        }

        public record AutoArgsModel : ClassModel
        {
            public bool HasBase { get; set; }
            public bool HasLogger => Fields.Any(m => m.Name == "_logger");
        }

        public static string Write(AutoArgsModel model)
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);
            writer.WriteLine("// Auto-generated code");
            foreach (var item in model.Usings)
            {
                writer.WriteLine(string.IsNullOrEmpty(item.Alias)
                    ? $"using {item.Name};"
                    : $"using {item.Alias} {item.Name};");
            }

            writer.WriteLineNoTabs("");
            writer.WriteLine($"namespace {model.Namespace}");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"public partial class {model.Class}");
            writer.WriteLine("{");
            writer.Indent++;
            if (model.HasLogger)
            {
                writer.WriteLine($"private readonly ILogger<{model.Class}> _logger;");
                writer.WriteLineNoTabs("");
            }

            writer.WriteLine($"public {model.Class}({WriteParameters(model)}){WriteInitializer(model)}");
            writer.WriteLine("{");
            writer.Indent++;
            for (var i = 0; i < model.Fields.Count; i++)
            {
                var field = model.Fields[i];
                if (field.Ignore)
                {
                    continue;
                }

                writer.WriteLine($"this.{field.Name} = a{i};");
            }

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");

            return sw.ToString();
        }

        private static string WriteParameters(AutoArgsModel model)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < model.Fields.Count; i++)
            {
                var field = model.Fields[i];
                sb.Append(field.Type);
                sb.Append(" a");
                sb.Append(i);

                if (i < model.Fields.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        private static string WriteInitializer(AutoArgsModel model)
        {
            if (!model.HasBase)
            {
                return "";
            }

            var sb = new StringBuilder();
            sb.Append(" : this(");
            for (var i = 0; i < model.Fields.Count; i++)
            {
                var field = model.Fields[i];
                if (!field.InBase) continue;
                sb.Append("a");
                sb.Append(i);
                if (i < model.Fields.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }
    }
}