using System.Collections.Generic;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Provides the set of XML-namespace→CLR-namespace mappings declared by referenced assemblies
	/// via <c>[assembly: XmlnsDefinition(xmlnsUri, clrNamespace)]</c>.
	/// </summary>
	/// <remarks>
	/// The production implementation (<see cref="AssemblyXmlnsDefinitionSource"/>) reads the
	/// attribute from already-loaded reference assemblies via reflection.  Unit tests supply a
	/// lightweight fake without touching the file system.
	/// </remarks>
	internal interface IXmlnsDefinitionSource
	{
		/// <summary>
		/// Returns all <c>(xmlnsUri, clrNamespace)</c> pairs from every referenced assembly.
		/// </summary>
		IReadOnlyList<XmlnsDefinitionEntry> GetEntries();
	}

	/// <summary>
	/// One <c>[assembly: XmlnsDefinition(xmlnsUri, clrNamespace)]</c> entry.
	/// </summary>
	internal readonly struct XmlnsDefinitionEntry
	{
		/// <summary>XML namespace URI (e.g. <c>http://schemas.microsoft.com/winfx/2006/xaml/presentation</c>).</summary>
		internal string XmlNamespace { get; }

		/// <summary>CLR namespace whose public types are mapped to <see cref="XmlNamespace"/>.</summary>
		internal string ClrNamespace { get; }

		/// <summary>Creates an entry.</summary>
		internal XmlnsDefinitionEntry(string xmlNamespace, string clrNamespace)
		{
			XmlNamespace = xmlNamespace;
			ClrNamespace = clrNamespace;
		}
	}

	/// <summary>
	/// An <see cref="IXmlnsDefinitionSource"/> that always returns zero entries.
	/// Used when no reference assemblies are available (e.g. during unit tests that do not
	/// inject a custom source, so the fallback hard-coded map is authoritative).
	/// </summary>
	internal sealed class EmptyXmlnsDefinitionSource : IXmlnsDefinitionSource
	{
		/// <summary>Singleton — no state.</summary>
		internal static readonly EmptyXmlnsDefinitionSource Instance = new EmptyXmlnsDefinitionSource();

		IReadOnlyList<XmlnsDefinitionEntry> IXmlnsDefinitionSource.GetEntries() =>
			System.Array.Empty<XmlnsDefinitionEntry>();
	}
}
