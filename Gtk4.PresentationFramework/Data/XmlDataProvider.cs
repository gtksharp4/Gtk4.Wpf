using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace System.Windows.Data;

/// <summary>
/// Loads XML from a <see cref="Source"/> URI, an inline <see cref="Document"/>,
/// or inline XML set via <see cref="XmlSerializer"/>, then evaluates
/// <see cref="XPath"/> to produce the list of matching nodes as
/// <see cref="DataSourceProvider.Data"/>.
/// </summary>
public class XmlDataProvider : DataSourceProvider
{
	Uri? _source;
	XmlDocument? _document;
	string? _inlineXml;
	string? _xPath;

	// ── Configuration ────────────────────────────────────────────────────────

	/// <summary>URI of the XML file to load.  Mutually exclusive with <see cref="Document"/>.</summary>
	public Uri? Source
	{
		get => _source;
		set { _source = value; _document = null; }
	}

	/// <summary>
	/// An already-loaded <see cref="XmlDocument"/>.
	/// Mutually exclusive with <see cref="Source"/>.
	/// </summary>
	public XmlDocument? Document
	{
		get => _document;
		set { _document = value; _source = null; }
	}

	/// <summary>
	/// Inline XML string set programmatically (mirrors XAML inline XML content).
	/// When set, it overrides <see cref="Source"/> and <see cref="Document"/>.
	/// </summary>
	public string? XmlSerializer
	{
		get => _inlineXml;
		set { _inlineXml = value; _source = null; _document = null; }
	}

	/// <summary>
	/// XPath expression evaluated against the loaded document.
	/// When empty the document element is returned as a single-item list.
	/// </summary>
	public string? XPath
	{
		get => _xPath;
		set => _xPath = value;
	}

	// ── DataSourceProvider ───────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override void QueryData()
	{
		XmlDocument doc;

		if (!string.IsNullOrEmpty(_inlineXml))
		{
			doc = new XmlDocument();
			doc.XmlResolver = null; // defence-in-depth: no XXE
			doc.LoadXml(_inlineXml!);
		}
		else if (_document != null)
		{
			doc = _document;
		}
		else if (_source != null)
		{
			doc = new XmlDocument();
			doc.XmlResolver = null; // defence-in-depth: no XXE
			try { doc.Load(_source.ToString()); }
			catch (Exception ex) { OnQueryFinished(ex); return; }
		}
		else
		{
			OnQueryFinished(null);
			return;
		}

		if (string.IsNullOrEmpty(_xPath))
		{
			// No XPath — return document element wrapped in a list.
			var list = new List<XmlNode>();
			if (doc.DocumentElement != null) list.Add(doc.DocumentElement);
			OnQueryFinished(list);
			return;
		}

		try
		{
			var nav = doc.CreateNavigator()!;
			var iter = nav.Select(_xPath!);
			var nodes = new List<XmlNode>();
			while (iter.MoveNext())
			{
				if (iter.Current is IHasXmlNode hxn)
					nodes.Add(hxn.GetNode());
			}
			OnQueryFinished(nodes);
		}
		catch (Exception ex)
		{
			OnQueryFinished(ex);
		}
	}
}
