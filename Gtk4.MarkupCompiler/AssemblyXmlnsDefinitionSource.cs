using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Reads <c>[assembly: XmlnsDefinition(xmlnsUri, clrNamespace)]</c> attributes from
	/// referenced assemblies by loading each one from its raw bytes (no file lock held after
	/// construction) and reflecting on its custom attributes.
	/// </summary>
	/// <remarks>
	/// Only assemblies that carry an attribute whose type name ends with
	/// <c>XmlnsDefinitionAttribute</c> are probed — this handles the case where the attribute
	/// type comes from a different identity than the one in scope (e.g. the host MSBuild already
	/// loaded a different version).  The properties <c>XmlNamespace</c> and <c>ClrNamespace</c>
	/// are read by name so no hard type dependency on the WindowsBase assembly is required here.
	/// </remarks>
	internal sealed class AssemblyXmlnsDefinitionSource : IXmlnsDefinitionSource
	{
		readonly List<XmlnsDefinitionEntry> _entries = new List<XmlnsDefinitionEntry>();

		/// <summary>
		/// Loads each path from bytes and harvests <c>XmlnsDefinitionAttribute</c> entries.
		/// Paths that do not exist or cannot be loaded are silently skipped.
		/// </summary>
		internal AssemblyXmlnsDefinitionSource(IReadOnlyList<string> referencePaths)
		{
			if (referencePaths == null || referencePaths.Count == 0)
				return;

			foreach (string path in referencePaths)
			{
				if (!File.Exists(path)) continue;
				try
				{
					byte[] bytes = File.ReadAllBytes(path);
					Assembly asm = Assembly.Load(bytes);
					HarvestEntries(asm);
				}
				catch { /* skip unloadable assemblies */ }
			}
		}

		void HarvestEntries(Assembly asm)
		{
			IEnumerable<CustomAttributeData> attrs;
			try { attrs = asm.CustomAttributes; }
			catch { return; }

			foreach (CustomAttributeData attr in attrs)
			{
				string typeName = attr.AttributeType?.Name ?? string.Empty;
				if (!typeName.Equals("XmlnsDefinitionAttribute", StringComparison.Ordinal))
					continue;

				if (attr.ConstructorArguments.Count < 2) continue;

				string xmlNs = attr.ConstructorArguments[0].Value as string;
				string clrNs = attr.ConstructorArguments[1].Value as string;

				if (!string.IsNullOrEmpty(xmlNs) && !string.IsNullOrEmpty(clrNs))
					_entries.Add(new XmlnsDefinitionEntry(xmlNs, clrNs));
			}
		}

		/// <inheritdoc />
		IReadOnlyList<XmlnsDefinitionEntry> IXmlnsDefinitionSource.GetEntries() => _entries;
	}
}
