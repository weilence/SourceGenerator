using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library.Receivers
{
    public class AttributeSyntaxReceiver : ISyntaxReceiver
    {
        private readonly List<string> _attributeNames;
        public HashSet<AttributeSyntax> AttributeSyntaxList { get; } = new HashSet<AttributeSyntax>();

        public AttributeSyntaxReceiver(List<string> attributeNames)
        {
            _attributeNames = attributeNames;
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                _attributeNames.Contains(identifierName.Identifier.ValueText))
            {
                AttributeSyntaxList.Add(cds);
            }
        }
    }
}