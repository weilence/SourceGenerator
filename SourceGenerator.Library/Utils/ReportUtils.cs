using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library.Utils
{
    public class ReportUtils
    {
        /// <summary>
        /// class must be partial
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns></returns>
        public static bool CheckPartial(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (SyntaxUtils.HasModifier(classDeclarationSyntax, SyntaxKind.PartialKeyword))
            {
                return true;
            }

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SGL001,
                classDeclarationSyntax.GetLocation(),
                SyntaxUtils.GetName(classDeclarationSyntax)));
            return false;
        }
    }
}