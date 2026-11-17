namespace System.Windows;

/// <summary>
/// Specifies the location of a resource dictionary (WPF-compatible). Consumed by
/// <see cref="ThemeInfoAttribute"/>, which the default WPF <c>AssemblyInfo.cs</c> applies.
/// </summary>
public enum ResourceDictionaryLocation
{
	/// <summary>No theme dictionary exists in this assembly.</summary>
	None = 0,
	/// <summary>The dictionary is located in the assembly that defines the theme.</summary>
	SourceAssembly = 1,
	/// <summary>The dictionary is located in a satellite assembly alongside this one.</summary>
	ExternalAssembly = 2,
}