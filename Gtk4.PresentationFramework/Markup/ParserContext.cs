using System;
using System.Xml;

namespace System.Windows.Markup;

/// <summary>
/// Information for <see cref="XamlReader.Load(System.IO.Stream, ParserContext)"/>: the base URI that relative
/// URIs in the loaded XAML resolve against, plus the xml:lang and xml:space defaults. Mirrors
/// <c>System.Windows.Markup.ParserContext</c> for the members listed here.
/// </summary>
/// <remarks>
/// Gap: <c>XamlTypeMapper</c>, <c>XmlnsDictionary</c> and <c>NamespaceMap</c> are not provided.
/// </remarks>
public class ParserContext : IUriContext
{
	/// <summary>Creates an empty context.</summary>
	public ParserContext() { }

	/// <summary>Creates a context from an XML parser context (its base URI, xml:lang and xml:space).</summary>
	public ParserContext(XmlParserContext xmlParserContext)
	{
		if (xmlParserContext == null) throw new ArgumentNullException(nameof(xmlParserContext));
		if (!string.IsNullOrEmpty(xmlParserContext.BaseURI))
			BaseUri = new Uri(xmlParserContext.BaseURI, UriKind.RelativeOrAbsolute);
		XmlLang = xmlParserContext.XmlLang;
		XmlSpace = xmlParserContext.XmlSpace switch
		{
			System.Xml.XmlSpace.Preserve => "preserve",
			System.Xml.XmlSpace.Default => "default",
			_ => string.Empty,
		};
	}

	/// <summary>The base URI that relative URIs in the XAML resolve against.</summary>
	public Uri? BaseUri { get; set; }

	/// <summary>The default xml:lang (empty when none).</summary>
	public string XmlLang { get; set; } = string.Empty;

	/// <summary>The default xml:space: "default", "preserve", or empty (a string, as in WPF).</summary>
	public string XmlSpace { get; set; } = string.Empty;

	/// <summary>Builds an <see cref="XmlParserContext"/> from the current values.</summary>
	public static XmlParserContext ToXmlParserContext(ParserContext parserContext)
	{
		if (parserContext == null) throw new ArgumentNullException(nameof(parserContext));
		var space = parserContext.XmlSpace switch
		{
			"preserve" => System.Xml.XmlSpace.Preserve,
			"default" => System.Xml.XmlSpace.Default,
			_ => System.Xml.XmlSpace.None,
		};
		return new XmlParserContext(null, null, parserContext.XmlLang, space)
		{
			BaseURI = parserContext.BaseUri?.OriginalString
		};
	}

	/// <summary>Converts to the equivalent <see cref="XmlParserContext"/>.</summary>
	public static implicit operator XmlParserContext(ParserContext parserContext) => ToXmlParserContext(parserContext);
}
