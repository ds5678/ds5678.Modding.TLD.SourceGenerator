using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SGF;

namespace SourceGenerator;

[IncrementalGenerator]
public sealed class ModAnnotationGenerator : IncrementalGenerator
{
	public const string AttributeNamespace = "TheLongDarkMods";
	public const string AttributeName = "ModClassAttribute";
	const string AttributeFullName = AttributeNamespace + "." + AttributeName;

	public ModAnnotationGenerator() : base(nameof(ModAnnotationGenerator))
	{
	}

	public override void OnInitialize(SgfInitializationContext context)
	{
		context.RegisterPostInitializationOutput(AddAttribute);
		IncrementalValuesProvider<(string Namespace, string Name)> classProvider = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeFullName, (syntaxNode, cancellationToken) =>
		{
			return syntaxNode is ClassDeclarationSyntax c && c.IsPartial() && c.Parent is BaseNamespaceDeclarationSyntax;
		},
		(context, cancellationToken) =>
		{
			ClassDeclarationSyntax @class = (ClassDeclarationSyntax)context.TargetNode;
			BaseNamespaceDeclarationSyntax @namespace = (BaseNamespaceDeclarationSyntax)@class.Parent!;

			return (@namespace.Name.ToString(), @class.Identifier.Text);
		});

		IncrementalValueProvider<(string Version, string Author)> versionProvider = context.AnalyzerConfigOptionsProvider.Select((provider, ct) =>
		{
			provider.GlobalOptions.TryGetValue("build_property.Version", out string? version);
			if (string.IsNullOrEmpty(version))
			{
				version = "1.0.0";
			}

			provider.GlobalOptions.TryGetValue("build_property.Authors", out string? author);
			if (string.IsNullOrEmpty(author))
			{
				author = "Unknown";
			}

			return (version!, author!);
		});

		var assemblyNameProvider = context.CompilationProvider.Select((c, ct) => c.AssemblyName);

		IncrementalValuesProvider<(string Namespace, string Name, string Version, string Author, string? AssemblyName)> combinedProvider = classProvider
			.Combine(versionProvider)
			.Combine(assemblyNameProvider)
			.Select((p, ct) => (p.Left.Left.Namespace, p.Left.Left.Name, p.Left.Right.Version, p.Left.Right.Author, p.Right));

		context.RegisterSourceOutput(combinedProvider, AnnotateModClass);
	}

	private static void AddAttribute(IncrementalGeneratorPostInitializationContext context)
	{
		const string AttributeText = $$"""
			namespace {{AttributeNamespace}}
			{
				//[global::Microsoft.CodeAnalysis.Embedded]
				[global::System.AttributeUsage(global::System.AttributeTargets.Class)]
				internal sealed class {{AttributeName}} : global::System.Attribute
				{
					public {{AttributeName}}()
					{
					}
				}
			}
			""";
		context.AddSource($"{AttributeName}.g.cs", AttributeText);

		//context.AddEmbeddedAttributeDefinition(); //To do: this api isn't available yet
		//https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#put-microsoftcodeanalysisembeddedattribute-on-generated-marker-types
	}

	private static void AnnotateModClass(SgfSourceProductionContext context, (string Namespace, string Name, string Version, string Author, string? AssemblyName) data)
	{
		string text = $$"""
			// This registers the mod with MelonLoader.
			[assembly: global::MelonLoader.MelonInfo(typeof(global::{{data.Namespace}}.{{data.Name}}), {{SymbolDisplay.FormatLiteral(data.AssemblyName ?? data.Name, true)}}, {{SymbolDisplay.FormatLiteral(data.Version, true)}}, {{SymbolDisplay.FormatLiteral(data.Author, true)}})]
			
			// This tells MelonLoader that the mod is only for The Long Dark.
			[assembly: global::MelonLoader.MelonGame("Hinterland", "TheLongDark")]

			namespace {{data.Namespace}}
			{
				partial class {{data.Name}} : global::MelonLoader.MelonMod
				{
				}
			}
			""";
		context.AddSource($"{data.Name}.g.cs", text);
	}
}
