using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string DictionaryType = "System.Windows.ResourceDictionary";

		bool IsDictionaryRoot(XElement root) => ResolveType(root) == DictionaryType;

		static string SourceExpression(string source)
			=> $"new System.Uri(\"{EscapeString(source)}\", System.UriKind.RelativeOrAbsolute)";

		// <ResourceDictionary Source="X.xaml"> as the whole resource value loads that dictionary.
		void EmitDictionarySource(IndentedWriter w, XElement dictionary, string ownerVar)
		{
			string source = (string)dictionary.Attribute("Source");
			if (source != null) w.Line($"{ownerVar}.Resources.Source = {SourceExpression(source)};");
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
				w.Line($"{ownerVar}.Resources.MergedDictionaries.Add(" +
					$"new {DictionaryType} {{ Source = {SourceExpression(source)} }});");
			}
		}

		// A ResourceDictionary file: its items are built into a scratch owner, then moved into the dictionary.
		void EmitDictionaryRoot(IndentedWriter w, XElement root, XNamespace xNs, ref int counter)
		{
			w.Line("var __owner = new global::System.Windows.Controls.Border();");
			EmitDictionarySource(w, root, "__owner");
			EmitResourceItems(w, root.Elements(), xNs, "__owner", ref counter);
			w.Line("foreach (var __p in __owner.Resources) this[__p.Key] = __p.Value;");
			w.Line("foreach (var __m in __owner.Resources.MergedDictionaries) this.MergedDictionaries.Add(__m);");
		}
	}
}
