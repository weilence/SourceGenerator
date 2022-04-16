using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library
{
    public class ClassAttributeReceiver : ISyntaxReceiver
    {
        public readonly List<string> Names;

        public ClassAttributeReceiver(List<string> names)
        {
            this.Names = names;
        }

        public HashSet<ClassDeclarationSyntax> AttributeSyntaxList { get; } = new HashSet<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                Names.Contains(identifierName.Identifier.ValueText))
            {
                var syntax = cds.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (syntax == null) return;
                AttributeSyntaxList.Add(syntax);
            }
        }
    }
}