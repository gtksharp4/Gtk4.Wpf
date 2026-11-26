namespace System.Windows;

/// <summary>
/// WPF-compatible stand-in for <c>System.Windows.ThemeInfoAttribute</c>. The attribute is
/// emitted verbatim by the default WPF project template's <c>AssemblyInfo.cs</c>:
/// <c>[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]</c>.
/// The Gtk4.Wpf backend does not use OS theme dictionaries, so the values are carried for
/// source compatibility only and have no runtime effect.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ThemeInfoAttribute : Attribute
{
	/// <summary>
	/// Creates the attribute with the theme and generic dictionary locations.
	/// </summary>
	/// <param name="themeDictionaryLocation">Where theme-specific dictionaries live.</param>
	/// <param name="genericDictionaryLocation">Where the generic dictionary lives.</param>
	public ThemeInfoAttribute(
		ResourceDictionaryLocation themeDictionaryLocation,
		ResourceDictionaryLocation genericDictionaryLocation)
	{
		ThemeDictionaryLocation = themeDictionaryLocation;
		GenericDictionaryLocation = genericDictionaryLocation;
	}

	/// <summary>Location of theme-specific resource dictionaries.</summary>
	public ResourceDictionaryLocation ThemeDictionaryLocation { get; }

	/// <summary>Location of the generic resource dictionary.</summary>
	public ResourceDictionaryLocation GenericDictionaryLocation { get; }
}