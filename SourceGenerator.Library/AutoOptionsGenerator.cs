using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoOptionsGenerator : ISourceGenerator
    {
        public static Dictionary<string, string> namePath = new Dictionary<string, string>();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new FieldAttributeReceiver(new List<string> { nameof(OptionsAttribute), OptionsAttribute.Name }));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (FieldAttributeReceiver)context.SyntaxReceiver;
            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            foreach (var classDeclarationSyntax in syntaxList)
            {
                var classInfoName = SyntaxUtils.GetName(classDeclarationSyntax);

                if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir",
                        out var projectDir))
                {
                    if (!namePath.ContainsKey(classInfoName))
                    {
                        return;
                    }

                    projectDir = namePath[classInfoName];
                }
                else
                {
                    namePath[classInfoName] = projectDir;
                }

                if (!SyntaxUtils.HasModifier(classDeclarationSyntax, SyntaxKind.PartialKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SGL001,
                        classDeclarationSyntax.GetLocation(),
                        classInfoName));
                    continue;
                }

                var attributeSyntax =
                    SyntaxUtils.GetAttribute(classDeclarationSyntax, name => receiver.Names.Contains(name));
                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var attributeValue = SemanticUtils.GetAttributeValue(semanticModel, attributeSyntax);
                if (!attributeValue.TryGetValue(nameof(OptionsAttribute.Path), out var path))
                {
                    continue;
                }

                path = Path.Combine(projectDir, path);

                if (!File.Exists(path))
                {
                    return;
                }

                var appSettingsFile = File.ReadAllText(path);

                var rootElement = JsonDocument.Parse(appSettingsFile).RootElement;
                var classInfo = JsonUtils.ParseJson(rootElement);

                var baseNamespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
                var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
                classInfo.Name = classInfoName;
                var appSettings = new OptionsModel()
                {
                    Namespace = namespaceName,
                    Class = classInfo,
                };
                context.AddSource(classInfo.Name + ".g.cs", RenderUtils.Render("Options", appSettings));
            }
        }
    }
}