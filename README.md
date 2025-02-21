# ds5678.Modding.TLD.SourceGenerator

A source generator to help with modding The Long Dark.

## Installation

```xml
<ItemGroup>
	<PackageReference Include="ds5678.Modding.TLD.SourceGenerator" Version="1.0.0">
		<PrivateAssets>all</PrivateAssets>
		<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
</ItemGroup>
```

Of course, referencing the assemblies package will still be required. For example, you might also need this:

```xml
<ItemGroup>
	<PackageReference Include="STBlade.Modding.TLD.Il2CppAssemblies.Windows" Version="2.36.0" />
</ItemGroup>
```

## Usage

```xml
<PropertyGroup>
	<!--This is the version of the mod.-->
	<Version>1.0.0</Version>

	<!--This is who created the mod.-->
	<Authors>ds5678</Authors>
</PropertyGroup>
```

```cs
[TheLongDarkMods.ModClass]
internal partial class MyMod
{
}
```
