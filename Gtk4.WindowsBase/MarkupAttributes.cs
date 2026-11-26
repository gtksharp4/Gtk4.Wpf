using System;

namespace System.Windows.Markup;

/// <summary>
/// Maps a CLR namespace in an assembly to an XML namespace used in XAML markup.
/// Applied at the assembly level; multiple attributes can appear on one assembly.
/// </summary>
/// <remarks>
/// Reimplemented here because <c>System.Xaml</c> (which carries the real
/// <c>XmlnsDefinitionAttribute</c>) is Windows-Desktop-only and absent on Linux/macOS.
/// The markup compiler resolves the presentation xmlns to our <c>System.Windows.*</c>
/// types by scanning this attribute on referenced assemblies.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class XmlnsDefinitionAttribute : Attribute
{
	/// <summary>The XML namespace (e.g. <c>http://schemas.microsoft.com/winfx/2006/xaml/presentation</c>).</summary>
	public string XmlNamespace { get; }

	/// <summary>The CLR namespace whose public types are mapped to <see cref="XmlNamespace"/>.</summary>
	public string ClrNamespace { get; }

	/// <summary>Creates the attribute.</summary>
	public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
	{
		XmlNamespace = xmlNamespace ?? throw new ArgumentNullException(nameof(xmlNamespace));
		ClrNamespace = clrNamespace ?? throw new ArgumentNullException(nameof(clrNamespace));
	}
}

/// <summary>
/// Identifies the property of a type that is the XAML content property; that is, the
/// property set when the XAML processor encounters inner content rather than a property
/// element.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ContentPropertyAttribute : Attribute
{
	/// <summary>Name of the content property.</summary>
	public string? Name { get; }

	/// <summary>Creates the attribute without a name (effectively disables content-property inheritance).</summary>
	public ContentPropertyAttribute() { }

	/// <summary>Creates the attribute with the given property name.</summary>
	public ContentPropertyAttribute(string name) => Name = name;
}