using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Library.Models;

namespace SourceGenerator.Library.Utils
{
    public class ReportUtils
    {
        /// <summary>
        /// class must be partial
        /// </summary>
        /// <param name="model"></param>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns></returns>
        public static bool CheckPartial<T>(GeneratedModel<T> model, ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (SyntaxUtils.HasModifier(classDeclarationSyntax, SyntaxKind.PartialKeyword))
            {
                return true;
            }

            model.Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.SGL001,
                classDeclarationSyntax.GetLocation(),
                SyntaxUtils.GetName(classDeclarationSyntax)));

            return false;
        }
    }
}