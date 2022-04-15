using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;

namespace SourceGenerator.Library
{
    public class FieldAttributeReceiver : ISyntaxReceiver
    {
        private readonly List<string> _names;

        public FieldAttributeReceiver(List<string> names)
        {
            this._names = names;
        }

        public HashSet<ClassDeclarationSyntax> AttributeSyntaxList { get; } = new HashSet<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                _names.Contains(identifierName.Identifier.ValueText))
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