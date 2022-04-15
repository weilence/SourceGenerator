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
            var syntaxReceiver = (AutoPropertyReceiver)context.SyntaxReceiver;
            var attributeSyntaxList = syntaxReceiver.AttributeSyntaxList;

            if (attributeSyntaxList.Count == 0)
            {
                return;
            }

            var classList = new List<string>();
            foreach (var attributeSyntax in attributeSyntaxList)
            {
                var semanticModel = context.Compilation.GetSemanticModel(attributeSyntax.SyntaxTree);
                var typeSymbol = semanticModel.GetTypeInfo(attributeSyntax).Type;
                if (typeSymbol?.ToString() != typeof(PropertyAttribute).FullName)
                {
                    continue;
                }

                var classDeclarationSyntax = attributeSyntax.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (classDeclarationSyntax == null ||
                    !classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    continue;
                }

                var namespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

                if (classList.Contains(classDeclarationSyntax.Identifier.ValueText))
                {
                    continue;
                }

                var fieldDeclarationList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
                if (fieldDeclarationList.Count == 0)
                {
                    continue;
                }

                var model = new AutoPropertyModel()
                {
                    Namespace = namespaceDeclarationSyntax.Name.ToString(),
                    Class = classDeclarationSyntax.Identifier.ValueText,
                    Fields = new List<Field>()
                };
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    var type = fieldDeclaration.Declaration.Type.ToString();

                    var variableDeclaratorSyntax = fieldDeclaration.Declaration.Variables.FirstOrDefault();
                    var name = variableDeclaratorSyntax.Identifier.ValueText;
                    model.Fields.Add(new Field()
                    {
                        Name = name,
                        Type = type
                    });
                }

                var autoProperty = new AutoProperty(model);

                context.AddSource($"{model.Class}.g.cs", autoProperty.TransformText());
                classList.Add(classDeclarationSyntax.Identifier.ValueText);
            }
        }

        private class AutoPropertyReceiver : ISyntaxReceiver
        {
            public List<AttributeSyntax> AttributeSyntaxList { get; } = new List<AttributeSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                    (
                        identifierName.Identifier.ValueText == PropertyAttribute.Name ||
                        identifierName.Identifier.ValueText == nameof(PropertyAttribute))
                   )
                {
                    AttributeSyntaxList.Add(cds);
                }
            }
        }
    }
}