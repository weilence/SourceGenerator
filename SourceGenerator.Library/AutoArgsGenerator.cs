using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoArgsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new FieldAttributeReceiver(new List<string>
                {
                    nameof(ArgsAttribute), ArgsAttribute.Name, nameof(ServiceAttribute),
                    ServiceAttribute.Name
                }));
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
                if (!SyntaxUtils.HasModifier(classDeclarationSyntax, SyntaxKind.PartialKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SGL001,
                        classDeclarationSyntax.GetLocation(),
                        SyntaxUtils.GetName(classDeclarationSyntax)));
                    continue;
                }

                var namespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

                var fieldDeclarationList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
                if (fieldDeclarationList.Count == 0)
                {
                    continue;
                }

                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var attributeSyntax =
                    SyntaxUtils.GetAttribute(classDeclarationSyntax, name => receiver.Names.Contains(name));

                var classAttributeValue = SemanticUtils.GetAttributeValue(semanticModel, attributeSyntax);
                var hasOptions = false;

                var fields = new List<Field>();
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    if (SyntaxUtils.HasModifier(fieldDeclaration, SyntaxKind.StaticKeyword, SyntaxKind.ConstKeyword))
                    {
                        continue;
                    }

                    if (!SyntaxUtils.HasModifiers(fieldDeclaration, SyntaxKind.PrivateKeyword,
                            SyntaxKind.ReadOnlyKeyword))
                    {
                        continue;
                    }

                    var fieldAttribute =
                        SyntaxUtils.GetAttribute(fieldDeclaration, name => receiver.Names.Contains(name));

                    if (fieldAttribute == null && attributeSyntax == null)
                    {
                        continue;
                    }

                    if (SyntaxUtils.HasAttribute(fieldDeclaration,
                            name => new[] { IgnoreAttribute.Name, nameof(IgnoreAttribute) }.Contains(name)))
                    {
                        continue;
                    }

                    var type = fieldDeclaration.Declaration.Type.ToString();
                    var typeInfo = semanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type);
                    var @namespace = typeInfo.Type?.ContainingNamespace.ToString();
                    if (@namespace == "System" || @namespace == "System.Collections.Generic")
                    {
                        continue;
                    }

                    var isValue = SyntaxUtils.HasAttribute(fieldDeclaration,
                        name => name == ValueAttribute.Name || name == nameof(ValueAttribute));
                    if (isValue)
                    {
                        hasOptions = true;
                    }

                    foreach (var declarationVariable in fieldDeclaration.Declaration.Variables)
                    {
                        var name = SyntaxUtils.GetName(declarationVariable);
                        fields.Add(new Field()
                        {
                            Name = name,
                            Type = type,
                            IsOptions = isValue,
                        });
                    }
                }

                var constructorDeclarationSyntax =
                    classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(m =>
                        SyntaxUtils.HasModifier(
                            m, SyntaxKind.PrivateKeyword));

                if (constructorDeclarationSyntax != null)
                {
                    foreach (var parameterSyntax in constructorDeclarationSyntax.ParameterList.Parameters)
                    {
                        var name = SyntaxUtils.GetName(parameterSyntax);
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
                                Name = name,
                                Type = type,
                                Ignore = true,
                                InBase = true,
                            });
                        }
                    }
                }

                if (fields.Count == 0)
                {
                    continue;
                }

                var usings = SyntaxUtils.GetUsings(classDeclarationSyntax);
                if (hasOptions && !usings.Contains("using Microsoft.Extensions.Options;"))
                {
                    usings.Add("using Microsoft.Extensions.Options;");
                }

                var model = new AutoArgsModel()
                {
                    Usings = usings,
                    Namespace = SyntaxUtils.GetName(namespaceDeclarationSyntax),
                    Class = SyntaxUtils.GetName(classDeclarationSyntax),
                    Fields = fields,
                    HasBase = constructorDeclarationSyntax != null,
                };

                context.AddSource($"{model.Class}.g.cs", RenderUtils.Render("AutoArgs", model));
            }
        }
    }
}