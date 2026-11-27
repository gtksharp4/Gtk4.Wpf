using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace System.Windows.Markup;

/// <summary>
/// Builds an object graph from loose XAML at runtime, by reflection (no <c>x:Class</c>, no events,
/// no <c>x:TypeArguments</c>). A <see cref="ParserContext"/> supplies the base URI given to each
/// <see cref="IUriContext"/>.
/// </summary>
public class XamlReader
{
	/// <summary>Loads the object tree described by the XAML in <paramref name="stream"/>.</summary>
	/// <exception cref="XamlParseException">The XAML is malformed or cannot be built.</exception>
	public static object Load(Stream stream)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		using var reader = XmlReader.Create(stream, Settings);
		return Load(reader);
	}

	/// <summary>Loads the object tree described by the XAML in <paramref name="reader"/>.</summary>
	/// <exception cref="XamlParseException">The XAML is malformed or cannot be built.</exception>
	public static object Load(XmlReader reader) => Load(reader, null);

	/// <summary>Loads the XAML in <paramref name="stream"/>, with <paramref name="parserContext"/>.BaseUri as the base URI.</summary>
	/// <exception cref="XamlParseException">The XAML is malformed or cannot be built.</exception>
	public static object Load(Stream stream, ParserContext parserContext)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		parserContext ??= new ParserContext();
		using var reader = XmlReader.Create(stream, Settings, (XmlParserContext)parserContext);
		return Load(reader, parserContext.BaseUri);
	}

	static object Load(XmlReader reader, Uri? baseUri)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		XDocument document;
		try
		{
			document = XDocument.Load(reader, LoadOptions.SetLineInfo);
		}
		catch (XmlException ex)
		{
			throw new XamlParseException(ex.Message, ex.LineNumber, ex.LinePosition, ex);
		}
		return new XamlObjectBuilder { BaseUri = baseUri }.Build(document.Root!);
	}

	/// <summary>Loads the object tree described by the XAML text <paramref name="xamlText"/>.</summary>
	/// <exception cref="XamlParseException">The XAML is malformed or cannot be built.</exception>
	public static object Parse(string xamlText)
	{
		if (xamlText == null) throw new ArgumentNullException(nameof(xamlText));
		using var text = new StringReader(xamlText);
		using var reader = XmlReader.Create(text, Settings);
		return Load(reader);
	}

	static XmlReaderSettings Settings => new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
}
