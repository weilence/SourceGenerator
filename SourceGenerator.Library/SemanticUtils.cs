using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library
{
    public class SemanticUtils
    {
        public static Dictionary<string, string> GetAttributeValue(SemanticModel model, AttributeSyntax syntax)
        {
            var result = new Dictionary<string, string>();
            var arguments = syntax?.ArgumentList?.Arguments;
            if (arguments == null)
            {
                return result;
            }

            foreach (var argumentSyntax in arguments)
            {
                var nameEqualSyntax = argumentSyntax.NameEquals;
                if (nameEqualSyntax == null)
                {
                    continue;
                }

                var constantValue = model.GetConstantValue(argumentSyntax.Expression);
                if (constantValue.Value != null)
                {
                    result[SyntaxUtils.GetName(nameEqualSyntax)] = constantValue.Value.ToString();
                }
            }

            return result;
        }
    }
}