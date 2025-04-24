using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace SourceGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MarkerAttributeAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "TLD_MODS_001";
	private static readonly LocalizableString Title = "Mod class not partial";
	private static readonly LocalizableString MessageFormat = "Classes with the ModClassAttribute should be partial";
	private static readonly LocalizableString Description = "Classes with the ModClassAttribute need to have another partial declaration source generated, so it's invalid for classes with this attribute to not be marked as partial.";
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
	}

	private void AnalyzeSymbol(SymbolAnalysisContext context)
	{
		// Get the symbol for the analyzed type or method
		INamedTypeSymbol symbol = (INamedTypeSymbol)context.Symbol;

		if (symbol.DeclaringSyntaxReferences.All(static s => s.GetSyntax() is not ClassDeclarationSyntax c || c.IsPartial()))
		{
			return;
		}

		// Check if the symbol has any attributes
		foreach (AttributeData attribute in symbol.GetAttributes())
		{
			// Check if the attribute is the marker Attribute
			if (attribute.AttributeClass?.Name is not ModAnnotationGenerator.AttributeName || attribute.AttributeClass.ContainingNamespace.ToDisplayString() != ModAnnotationGenerator.AttributeNamespace)
			{
				continue;
			}

			if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
			{
				continue;
			}

			if (attributeSyntax.Parent is not AttributeListSyntax attributeList || attributeList.Parent is not ClassDeclarationSyntax classDeclaration)
			{
				continue;
			}

			// Report a diagnostic if the marker Attribute is found
			context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
		}
	}
}
