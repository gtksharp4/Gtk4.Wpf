using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Emits each keyed resource element (and merged dictionaries) into <paramref name="ownerVar"/>.Resources.
		void EmitResourceItems(
			IndentedWriter w,
			System.Collections.Generic.IEnumerable<XElement> items,
			XNamespace xNs,
			string ownerVar,
			ref int counter)
		{
			foreach (XElement item in items)
			{
				if (item.Name.LocalName == "ResourceDictionary.MergedDictionaries")
				{
					EmitMergedDictionaries(w, item, ownerVar);
					continue;
				}
				string key = (string)item.Attribute(xNs + "Key");
				if (key != null && (ContainsStaticResource(item) || IsUnshared(item, xNs))
					&& TryEmitDeferredResource(w, item, xNs, ownerVar, key, ref counter))
					continue;
				EmitResourceItem(w, item, xNs, ownerVar, key, ref counter);
			}
		}

		void EmitResourceItem(
			IndentedWriter w, XElement item, XNamespace xNs, string ownerVar, string key, ref int counter)
		{
			{
				if (item.Name.LocalName == "Style"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs)
				{
					EmitStyleResource(w, item, xNs, ownerVar, key, ref counter);
				}
				else if (item.Name.LocalName == "ControlTemplate"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs
					&& key != null)
				{
					string tmplVar = $"__ct{counter++}";
					EmitControlTemplateInline(w, item, xNs, tmplVar, ref counter);
					w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {tmplVar};");
				}
				else if (item.Name.LocalName == "DataTemplate"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs
					&& key != null)
				{
					string tmplVar = $"__dt{counter++}";
					EmitDataTemplateInline(w, item, xNs, tmplVar, ref counter);
					w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {tmplVar};");
				}
				else if (item.Name.LocalName == "HierarchicalDataTemplate"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs
					&& key != null)
				{
					string tmplVar = $"__dt{counter++}";
					EmitHierarchicalTemplateInline(w, item, xNs, tmplVar, ownerVar, ref counter);
					w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {tmplVar};");
				}
				else if (item.Name.LocalName == "Storyboard"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs
					&& key != null)
				{
					EmitStoryboardResource(w, item, xNs, ownerVar, key, ref counter);
				}
				else if (item.Name.LocalName == "AlternationConverter"
					&& item.Name.NamespaceName == WpfXmlnsMap.PresentationNs
					&& key != null && AlternationItemsEmittable(item))
				{
					EmitAlternationConverter(w, item, ownerVar, key, ref counter);
				}
				else if (key != null && ResourceObjectType(item) is string objType)
				{
					EmitResourceObject(w, item, xNs, ownerVar, key, objType, ref counter);
				}
				else if (key != null && ObjectResourceType(item, xNs) is string objFq)
				{
					EmitResourceObject(w, item, xNs, ownerVar, key, objFq, ref counter);
				}
				else if (key != null)
				{
					// Simple keyed value — emit as string for now; type-converter is later.
					string escapedKey = EscapeString(key);
					string escapedVal = EscapeString(item.Value.Trim());
					w.Line($"{ownerVar}.Resources[\"{escapedKey}\"] = \"{escapedVal}\";");
				}
			}
		}
	}
}
