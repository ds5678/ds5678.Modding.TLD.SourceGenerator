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
	private static readonly LocalizableString MessageFormat = "The ModClassAttribute should only be used on partial classes";
	private static readonly LocalizableString Description = "MarkerAttribute is used in an unsupported location.";
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
		var symbol = context.Symbol;

		// Check if the symbol has any attributes
		foreach (var attribute in symbol.GetAttributes())
		{
			// Check if the attribute is the marker Attribute
			if ((attribute.AttributeClass?.Name) != ModAnnotationGenerator.AttributeName || attribute.AttributeClass.ContainingNamespace.ToDisplayString() != ModAnnotationGenerator.AttributeNamespace)
			{
				continue;
			}

			Location? location = attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
			if (location is null)
			{
				continue;
			}

			INamedTypeSymbol type = (INamedTypeSymbol)symbol;
			if (type.DeclaringSyntaxReferences.All(s => s.GetSyntax() is ClassDeclarationSyntax c && c.IsPartial()))
			{
				continue;
			}

			// Report a diagnostic if the marker Attribute is found
			var diagnostic = Diagnostic.Create(Rule, location);
			context.ReportDiagnostic(diagnostic);
		}
	}
}