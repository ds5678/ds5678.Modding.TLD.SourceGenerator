using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator;

internal static class RoslynExtensions
{
	public static bool IsPartial(this ClassDeclarationSyntax c)
	{
		return c.Modifiers.Any(SyntaxKind.PartialKeyword);
	}
}
