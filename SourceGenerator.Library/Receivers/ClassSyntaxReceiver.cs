using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library.Receivers
{
    public class ClassSyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<string> AttributeNames;

        public ClassSyntaxReceiver(List<string> attributeNames)
        {
            AttributeNames = attributeNames;
        }

        public HashSet<ClassDeclarationSyntax> AttributeSyntaxList { get; } = new HashSet<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                AttributeNames.Contains(identifierName.Identifier.ValueText))
            {
                var syntax = cds.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (syntax == null) return;
                AttributeSyntaxList.Add(syntax);
            }
        }
    }
}