using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SourceGenerator.Library.Models;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoOptionsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static postInitializationContext =>
            {
                postInitializationContext.AddSource("OptionsAttribute.cs", SourceText.From("""
                    using System;
                    using System.Collections.Generic;

                    namespace SourceGenerator.Common
                    {
                        [AttributeUsage(AttributeTargets.Class)]
                        public class OptionsAttribute : Attribute
                        {
                            public string Path { get; set; } = "";
                        }
                    }
                    """, Encoding.UTF8));
            });

            var projectDirProvider = context.AnalyzerConfigOptionsProvider.Select((context, cancellationToken) =>
            {
                if (!context.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir))
                {
                    return "";
                }

                return projectDir;
            });

            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                "SourceGenerator.Common.OptionsAttribute",
                static (syntaxNode, cancellationToken) => true,
                static (context, cancellationToken) =>
                {
                    var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
                    var classInfoName = SyntaxUtils.GetName(classDeclarationSyntax);

                    var model = new GeneratedModel<AutoOptionsModel>();

                    if (!SyntaxUtils.HasModifier(classDeclarationSyntax, SyntaxKind.PartialKeyword))
                    {
                        model.Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.SGL001,
                            classDeclarationSyntax.GetLocation(),
                            classInfoName));
                        return model;
                    }

                    string path = null;
                    foreach (var keyValuePair in context.Attributes.FirstOrDefault().NamedArguments)
                    {
                        if (keyValuePair.Key == "Path")
                        {
                            path = keyValuePair.Value.Value.ToString();
                        }
                    }

                    var data = new AutoOptionsModel()
                    {
                        Namespace = context.TargetSymbol.ContainingNamespace.ToString(),
                        Class = new ClassInfo()
                        {
                            Name = context.TargetSymbol.Name,
                        },
                        Path = path,
                    };

                    model.Data = data;
                    return model;
                }
            );

            var combinedPipeline = pipeline.Combine(projectDirProvider)
                .Where((tuple => tuple.Right != "" && tuple.Left.Data?.Path != null))
                .Select((tuple, token) =>
                {
                    tuple.Left.Data.Path = Path.Combine(tuple.Right, tuple.Left.Data.Path);
                    return tuple.Left;
                });

            context.RegisterSourceOutput(combinedPipeline, (sourceOutputContext, model) =>
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

                var appSettingsFile = File.ReadAllText(model.Data.Path);
                var rootElement = JsonDocument.Parse(appSettingsFile).RootElement;
                var classInfo = ParseJson(rootElement);
                var data = model.Data;
                classInfo.Name = data.Class.Name;
                data.Class = classInfo;

                var code = Write(data);
                sourceOutputContext.AddSource(data.Class.Name + ".g.cs", code);
            });
        }

        public record AutoOptionsModel
        {
            public string Namespace { get; set; }

            public ClassInfo Class { get; set; }

            public string Path { get; set; }
        }

        public record ClassInfo
        {
            public string Name { get; set; }

            public ValueArray<PropertyInfo> Properties { get; set; }
        }

        public record PropertyInfo
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public string Value { get; set; }

            public ClassInfo Class { get; set; }
        }

        public static string Write(AutoOptionsModel model)
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            writer.WriteLine("// Auto-generated code");
            writer.WriteLine("namespace " + model.Namespace);
            writer.WriteLine("{");
            writer.Indent++;
            RenderClass(writer, model.Class);
            writer.Indent--;
            writer.WriteLine("}");

            return sw.ToString();
        }

        private static void RenderClass(IndentedTextWriter writer, ClassInfo modelClass)
        {
            writer.WriteLine($"public partial class {modelClass.Name}");
            writer.WriteLine("{");
            writer.Indent++;
            foreach (var property in modelClass.Properties)
            {
                writer.WriteLine($"public {property.Type} {property.Name} {{ get; set; }}");
            }

            writer.Indent--;
            writer.WriteLine("}");

            foreach (var property in modelClass.Properties)
            {
                if (property.Class != null)
                {
                    writer.WriteLineNoTabs("");
                    RenderClass(writer, property.Class);
                }
            }
        }

        public static ClassInfo ParseJson(JsonElement jsonElement)
        {
            var classInfo = new ClassInfo()
            {
                Properties = new List<PropertyInfo>()
            };

            foreach (var jsonProperty in jsonElement.EnumerateObject())
            {
                var property = ParseProperty(jsonProperty);
                if (property == null)
                {
                    continue;
                }

                classInfo.Properties.Add(property);
            }

            return classInfo;
        }

        public static PropertyInfo ParseProperty(JsonProperty property)
        {
            var propertyName = StringUtils.ToPascalCase(property.Name);
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Number:
                    var value = property.Value.ToString();
                    return new PropertyInfo()
                    {
                        Type = value.Contains(".") ? "decimal" : "int",
                        Name = propertyName,
                        Value = value,
                    };
                case JsonValueKind.True:
                    return new PropertyInfo()
                    {
                        Type = "bool",
                        Name = propertyName,
                        Value = "true",
                    };
                case JsonValueKind.False:
                    return new PropertyInfo()
                    {
                        Type = "bool",
                        Name = propertyName,
                        Value = "false",
                    };
                case JsonValueKind.String:
                    return new PropertyInfo()
                    {
                        Type = "string",
                        Name = propertyName,
                        Value = property.Value.GetString(),
                    };
                case JsonValueKind.Object:
                    var classInfo = ParseJson(property.Value);
                    classInfo.Name = propertyName;
                    return new PropertyInfo()
                    {
                        Type = propertyName,
                        Name = propertyName,
                        Value = propertyName,
                        Class = classInfo,
                    };
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                case JsonValueKind.Array:
                    return null;
                default:
                    throw new ArgumentException($"Unknown value kind: {property.Value.ValueKind}");
            }
        }
    }
}