using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoPropertyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AutoPropertyReceiver());
        }

        private string GetCamelCase(string name)
        {
            return char.ToUpper(name[1]) + name.Substring(2);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (AutoPropertyReceiver)context.SyntaxReceiver;
            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            foreach (var classDeclarationSyntax in syntaxList)
            {
                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                // var typeSymbol = semanticModel.GetTypeInfo(attributeSyntax).Type;
                // if (typeSymbol?.ToString() != typeof(PropertyAttribute).FullName)
                // {
                //     continue;
                // }

                var namespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

                var fieldDeclarationList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
                if (fieldDeclarationList.Count == 0)
                {
                    continue;
                }

                var model = new AutoPropertyModel()
                {
                    Namespace = SyntaxUtils.GetName(namespaceDeclarationSyntax),
                    Class = SyntaxUtils.GetName(classDeclarationSyntax),
                    Fields = new List<Field>()
                };
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    var type = fieldDeclaration.Declaration.Type.ToString();

                    foreach (var declarationVariable in fieldDeclaration.Declaration.Variables)
                    {
                        var name = SyntaxUtils.GetName(declarationVariable);
                        model.Fields.Add(new Field()
                        {
                            Name = name,
                            Type = type
                        });
                    }
                }

                var autoProperty = new AutoProperty(model);

                context.AddSource($"{model.Class}.g.cs", autoProperty.TransformText());
            }
        }

        private class AutoPropertyReceiver : ISyntaxReceiver
        {
            public HashSet<ClassDeclarationSyntax> AttributeSyntaxList { get; } = new HashSet<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                    (
                        identifierName.Identifier.ValueText == PropertyAttribute.Name ||
                        identifierName.Identifier.ValueText == nameof(PropertyAttribute))
                   )
                {
                    var syntax = cds.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                    if (syntax == null) return;
                    if (SyntaxUtils.HasModifier(syntax, SyntaxKind.PartialKeyword))
                    {
                        AttributeSyntaxList.Add(syntax);
                    }
                }
            }
        }
    }
}