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
public class ParserContext
{
	readonly XmlParserContext? _xmlContext;

	/// <summary>Creates an empty context.</summary>
	public ParserContext() { }

	/// <summary>Creates a context from an XML parser context (its base URI, xml:lang and xml:space).</summary>
	public ParserContext(XmlParserContext xmlParserContext)
	{
		_xmlContext = xmlParserContext ?? throw new ArgumentNullException(nameof(xmlParserContext));
		if (!string.IsNullOrEmpty(xmlParserContext.BaseURI))
			BaseUri = new Uri(xmlParserContext.BaseURI, UriKind.RelativeOrAbsolute);
		XmlLang = xmlParserContext.XmlLang;
		XmlSpace = xmlParserContext.XmlSpace;
	}

	/// <summary>The base URI that relative URIs in the XAML resolve against.</summary>
	public Uri? BaseUri { get; set; }

	/// <summary>The default xml:lang (empty when none).</summary>
	public string XmlLang { get; set; } = string.Empty;

	/// <summary>The default xml:space.</summary>
	public System.Xml.XmlSpace XmlSpace { get; set; }

	/// <summary>Converts to the equivalent <see cref="XmlParserContext"/>.</summary>
	public static implicit operator XmlParserContext(ParserContext parserContext)
	{
		if (parserContext == null) throw new ArgumentNullException(nameof(parserContext));
		return parserContext._xmlContext ?? new XmlParserContext(null, null, parserContext.XmlLang,
			parserContext.XmlSpace) { BaseURI = parserContext.BaseUri?.OriginalString };
	}
}
