using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace System.Windows.Markup;

/// <summary>
/// Serializes an object graph to XAML that <see cref="XamlReader"/> loads back: dependency-property local
/// values that differ from the default (bindings and {DynamicResource} in attribute form), simple CLR properties
/// that differ from a new instance, the content
/// property and <c>Resources</c>. Styles, triggers, templates and event handlers are not serialized.
/// </summary>
public static class XamlWriter
{
	/// <summary>Returns the XAML for <paramref name="obj"/>.</summary>
	public static string Save(object obj) => new XamlObjectWriter().Write(obj).ToString();

	/// <summary>Writes the XAML for <paramref name="obj"/> to <paramref name="stream"/> as UTF-8.</summary>
	public static void Save(object obj, Stream stream)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		var settings = new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true,
			Encoding = new UTF8Encoding(false),
		};
		using XmlWriter writer = XmlWriter.Create(stream, settings);
		Save(obj, writer);
	}

	/// <summary>Writes the XAML for <paramref name="obj"/> to <paramref name="writer"/>.</summary>
	public static void Save(object obj, TextWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		writer.Write(Save(obj));
		writer.Flush();
	}

	/// <summary>Writes the XAML for <paramref name="obj"/> to <paramref name="writer"/>.</summary>
	public static void Save(object obj, XmlWriter writer)
	{
		if (writer == null) throw new ArgumentNullException(nameof(writer));
		new XamlObjectWriter().Write(obj).WriteTo(writer);
		writer.Flush();
	}
}
