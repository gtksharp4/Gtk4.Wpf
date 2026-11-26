using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string GridType = "System.Windows.Controls.Grid";
		const string ViewSourceType = "System.Windows.Data.CollectionViewSource";

		// Collection property elements whose items are plain element objects: the item element name, or null.
		static string CollectionItemName(string ownerFq, string prop)
		{
			if (ownerFq == GridType && prop == "RowDefinitions") return "RowDefinition";
			if (ownerFq == GridType && prop == "ColumnDefinitions") return "ColumnDefinition";
			if (ownerFq == ViewSourceType && prop == "GroupDescriptions") return "PropertyGroupDescription";
			return null;
		}

		// <Grid.RowDefinitions>, <CollectionViewSource.GroupDescriptions>: each item is built and added.
		bool TryEmitItemCollection(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string ownerFq, string prop,
			ref int counter)
		{
			string item = CollectionItemName(ownerFq, prop);
			if (item == null) return false;
			foreach (XElement child in pe.Elements())
			{
				string fq = ResolveType(child);
				if (child.Name.LocalName != item || fq == null)
				{
					NoteDropped(child, "element " + child.Name.LocalName + " in " + pe.Name.LocalName);
					continue;
				}
				string v = $"__e{counter++}";
				EmitElement(w, child, xNs, v, fq, ref counter);
				w.Line($"{parentVar}.{prop}.Add({v});");
			}
			return true;
		}

		// A property element the generator cannot emit is reported (as a message, not a warning).
		void NoteDropped(XObject at, string what)
		{
			string line = at is System.Xml.IXmlLineInfo li && li.HasLineInfo() ? $" (line {li.LineNumber})" : "";
			Note($"Skipped {what}{line}: not supported by the markup compiler.");
		}

		void NoteDroppedPropertyElement(XElement pe)
		{
			bool content = pe.Elements().Any()
				|| pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value));
			if (content) NoteDropped(pe, "property element " + pe.Name.LocalName);
		}

		// HierarchicalDataTemplate: a DataTemplate plus the child-items binding and the child template.
		void EmitHierarchicalTemplateInline(
			IndentedWriter w, XElement el, XNamespace xNs, string tmplVar, string receiver, ref int counter)
		{
			EmitDataTemplateInline(w, el, xNs, tmplVar, ref counter, "System.Windows.HierarchicalDataTemplate");
			string items = (string)el.Attribute("ItemsSource");
			if (items != null && IsBindingExpression(items, out string bindingInit))
				w.Line($"{tmplVar}.ItemsSource = {bindingInit};");
			string child = (string)el.Attribute("ItemTemplate");
			if (child != null && IsStaticResourceValue(child, out string key))
				w.Line($"{tmplVar}.ItemTemplate = (System.Windows.Controls.DataTemplate)" +
					$"{StaticResourceLookup(receiver, EscapeString(key))};");
		}

		// <AlternationConverter> with text-valued items such as <SolidColorBrush>Red</SolidColorBrush>.
		bool AlternationItemsEmittable(XElement item)
			=> item.Elements().All(e => !e.HasElements && e.Attributes().All(a => a.IsNamespaceDeclaration)
				&& ResolveType(e) is string fq && AlternationValueType(fq) != null);

		static string AlternationValueType(string fq)
			=> fq == "System.Windows.Media.SolidColorBrush" ? "System.Windows.Media.Brush"
				: ConverterBackedStructs.Contains(fq) ? fq : null;

		void EmitAlternationConverter(
			IndentedWriter w, XElement item, string ownerVar, string key, ref int counter)
		{
			string v = $"__e{counter++}";
			w.Line($"var {v} = new System.Windows.Controls.AlternationConverter();");
			foreach (XElement e in item.Elements())
			{
				string t = AlternationValueType(ResolveType(e));
				string text = EscapeString(e.Value.Trim());
				w.Line($"{v}.Values.Add({Converter}.ConvertByType(typeof(global::{t}), \"{text}\"));");
			}
			w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {v};");
		}
	}
}
