using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Receivers;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoOptionsGenerator : ISourceGenerator
    {
        public static Dictionary<string, string> namePath = new Dictionary<string, string>();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new ClassSyntaxReceiver(new List<string>
                {
                    nameof(OptionsAttribute), OptionsAttribute.Name
                }));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ClassSyntaxReceiver)context.SyntaxReceiver;
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
                    SyntaxUtils.GetAttribute(classDeclarationSyntax, name => receiver.AttributeNames.Contains(name));
                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var attributeValue = SemanticUtils.GetAttributeValue(semanticModel, attributeSyntax);
                if (!attributeValue.TryGetValue(nameof(OptionsAttribute.Path), out var path))
                {
                    continue;
                }

                path = Path.Combine(projectDir, path);

#pragma warning disable RS1035
                if (!File.Exists(path))
#pragma warning restore RS1035
                {
                    return;
                }

#pragma warning disable RS1035
                var appSettingsFile = File.ReadAllText(path);
#pragma warning restore RS1035

                var rootElement = JsonDocument.Parse(appSettingsFile).RootElement;
                var classInfo = JsonUtils.ParseJson(rootElement);

                var baseNamespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
                var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
                classInfo.Name = classInfoName;
                var model = new AutoOptionsModel()
                {
                    Namespace = namespaceName, Class = classInfo,
                };

                context.AddSource(classInfo.Name + ".g.cs", new AutoOptions(model).TransformText());
            }
        }
    }
}