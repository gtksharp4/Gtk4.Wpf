using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows.Markup;

namespace System.Windows.Themes;

static partial class FluentTheme
{
	/// <summary>
	/// The top-level <c>x:Key</c>s of the embedded files that the layer could not build (a type, member or
	/// template part it lacks), per file. Those entries are left out; every other entry loads.
	/// </summary>
	internal static readonly Dictionary<string, List<string>> Skipped = new Dictionary<string, List<string>>();

	static ResourceDictionary Load(string file)
	{
		using Stream stream = typeof(FluentTheme).Assembly.GetManifestResourceStream(Prefix + file)
			?? throw new IOException("Missing embedded Fluent resource '" + file + "'.");
		using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
		XDocument document = XDocument.Load(reader, LoadOptions.SetLineInfo);
		var skipped = new List<string>();
		DropUnresolvableKeys(document.Root!, skipped);
		var fonts = TakeFontFamilies(document.Root!);
		ResolveSystemColorAliases(document.Root!);
		while (true)
		{
			try
			{
				var dictionary = (ResourceDictionary)new XamlObjectBuilder().Build(document.Root!);
				foreach (var font in fonts) dictionary[font.Key] = font.Value;
				lock (Skipped) Skipped[file] = skipped;
				return dictionary;
			}
			catch (XamlParseException ex) when (Drop(document.Root!, ex, skipped))
			{
			}
		}
	}

	// Removes the top-level entry containing the failing line; false (rethrow) when there is none.
	static bool Drop(XElement root, XamlParseException ex, List<string> skipped)
	{
		int line = ex.LineNumber;
		if (line <= 0) return false;
		XElement? entry = root.Elements().LastOrDefault(e => ((IXmlLineInfo)e).LineNumber <= line);
		if (entry == null) return false;
		XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
		skipped.Add(((string?)entry.Attribute(x + "Key") ?? entry.Name.LocalName) + " => " + (ex.InnerException?.Message ?? ex.Message));
		entry.Remove();
		return true;
	}

	// An x:Key that is a markup extension (x:Static SystemParameters.XKey, ToolBar.ButtonStyleKey, ...) fails
	// without a position when the layer lacks the member, so those entries are tested on their own first.
	static void DropUnresolvableKeys(XElement root, List<string> skipped)
	{
		XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
		var verdict = new Dictionary<string, bool>();
		foreach (XElement entry in root.Elements().ToList())
		{
			string? key = (string?)entry.Attribute(x + "Key");
			if (key == null || !key.StartsWith("{", StringComparison.Ordinal)) continue;
			if (!verdict.TryGetValue(key, out bool ok))
			{
				var probe = new XElement(root.Name, root.Attributes(),
					new XElement(root.Name.Namespace + "Border", new XAttribute(x + "Key", key)));
				try
				{
					new XamlObjectBuilder().Build(probe);
					ok = true;
				}
				catch (XamlParseException)
				{
				}
				verdict[key] = ok;
			}
			if (ok) continue;
			skipped.Add(key);
			entry.Remove();
		}
	}

	// <FontFamily x:Key="K">name, name</FontFamily> is text content, which the XAML builder does not take for
	// FontFamily: those entries are built here.
	static List<KeyValuePair<object, FontFamily>> TakeFontFamilies(XElement root)
	{
		XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
		var result = new List<KeyValuePair<object, FontFamily>>();
		foreach (XElement entry in root.Elements().Where(e => e.Name.LocalName == "FontFamily" && !e.HasElements).ToList())
		{
			string? key = (string?)entry.Attribute(x + "Key");
			if (key == null) continue;
			result.Add(new KeyValuePair<object, FontFamily>(key, new FontFamily(entry.Value.Trim())));
			entry.Remove();
		}
		return result;
	}

	// <DynamicResource x:Key="K" ResourceKey="{x:Static SystemColors.XKey}" /> aliases a system colour (the accent
	// family). The XAML builder cannot create a DynamicResource element, so the alias becomes the colour itself.
	static void ResolveSystemColorAliases(XElement root)
	{
		XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
		foreach (XElement entry in root.Elements().Where(e => e.Name.LocalName == "DynamicResource").ToList())
		{
			string? reference = (string?)entry.Attribute("ResourceKey");
			const string prefix = "{x:Static SystemColors.";
			if (reference == null || !reference.StartsWith(prefix, StringComparison.Ordinal) || !reference.EndsWith("Key}"))
				continue;
			string name = reference.Substring(prefix.Length, reference.Length - prefix.Length - "Key}".Length);
			if (typeof(SystemColors).GetProperty(name)?.GetValue(null) is not Color color) continue;
			entry.ReplaceWith(new XElement(root.Name.Namespace + "Color", entry.Attribute(x + "Key"), color.ToString()));
		}
	}
}
