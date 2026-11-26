using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string DictionaryType = "System.Windows.ResourceDictionary";

		// An unresolvable Source must not throw at startup, so generated code loads through the non-throwing path.
		const string TryLoad = "global::System.Windows.Markup.GtkFormsXamlRegistry.TryLoadDictionary";

		bool IsDictionaryRoot(XElement root) => ResolveType(root) == DictionaryType;

		// <ResourceDictionary Source="X.xaml"> as the whole resource value loads that dictionary.
		void EmitDictionarySource(IndentedWriter w, XElement dictionary, string ownerVar)
		{
			string source = (string)dictionary.Attribute("Source");
			if (source != null) w.Line($"{TryLoad}({ownerVar}.Resources, \"{EscapeString(source)}\");");
		}

		// <ResourceDictionary.MergedDictionaries>: each Source dictionary is loaded when the markup runs, so
		// parse-time {StaticResource} lookups see its keys.
		void EmitMergedDictionaries(IndentedWriter w, XElement merged, string ownerVar)
		{
			foreach (XElement child in merged.Elements())
			{
				string source = (string)child.Attribute("Source");
				if (child.Name.LocalName != "ResourceDictionary" || source == null)
				{
					NoteDropped(child, "merged dictionary " + child.Name.LocalName + " without Source");
					continue;
				}
				w.Line($"{{ var __d = new {DictionaryType}(); if ({TryLoad}(__d, \"{EscapeString(source)}\")) " +
					$"{ownerVar}.Resources.MergedDictionaries.Add(__d); }}");
			}
		}

		// A ResourceDictionary file: its items are built into a scratch owner, then moved into the dictionary.
		void EmitDictionaryRoot(IndentedWriter w, XElement root, XNamespace xNs, ref int counter)
		{
			w.Line("var __owner = new global::System.Windows.Controls.Border();");
			EmitDictionarySource(w, root, "__owner");
			EmitResourceItems(w, root.Elements(), xNs, "__owner", ref counter);
			w.Line("this.CopyEntriesFrom(__owner.Resources);");
		}
	}
}
