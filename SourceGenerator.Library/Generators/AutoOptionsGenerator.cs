using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                            public string Path { get; set; }
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
                var classInfo = JsonUtils.ParseJson(rootElement);
                var data = model.Data;
                classInfo.Name = data.Class.Name;
                data.Class = classInfo;
                sourceOutputContext.AddSource(data.Class.Name + ".g.cs", new AutoOptions(data).TransformText());
            });
        }
    }
}