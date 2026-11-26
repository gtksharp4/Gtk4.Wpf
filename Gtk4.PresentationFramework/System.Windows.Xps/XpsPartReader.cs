using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>Reads the fixed document sequence, fixed document and fixed page parts of an XPS package.</summary>
static class XpsPartReader
{
	static readonly XNamespace Ns = XpsNames.Markup;

	/// <summary>Reads the sequence part; each reference keeps its relative source and the part URI as base.</summary>
	internal static FixedDocumentSequence ReadSequence(XpsPackage package, string partName)
	{
		var root = Root(package, partName, "FixedDocumentSequence");
		var uri = package.PartUri(partName);
		var sequence = new FixedDocumentSequence();
		((IUriContext)sequence).BaseUri = uri;
		foreach (var e in root.Elements(Ns + "DocumentReference"))
		{
			var reference = new DocumentReference { Source = Source(e) };
			((IUriContext)reference).BaseUri = uri;
			sequence.References.Add(reference);
		}
		return sequence;
	}

	/// <summary>Reads the document part; pages stay unloaded until <c>PageContent.GetPageRoot</c>.</summary>
	internal static FixedDocument ReadDocument(XpsPackage package, string partName)
	{
		var root = Root(package, partName, "FixedDocument");
		var uri = package.PartUri(partName);
		var document = new FixedDocument();
		((IUriContext)document).BaseUri = uri;
		foreach (var e in root.Elements(Ns + "PageContent"))
		{
			var page = new PageContent { Source = Source(e) };
			((IUriContext)page).BaseUri = uri;
			if (e.Attribute("Width") != null)
				page.Width = XpsValues.Double((string?)e.Attribute("Width"));
			if (e.Attribute("Height") != null)
				page.Height = XpsValues.Double((string?)e.Attribute("Height"));
			document.Pages.Add(page);
		}
		return document;
	}

	internal static FixedPage ReadPage(XpsPackage package, string partName) =>
		new XpsPageReader(package, partName).Read();

	static Uri Source(XElement e) => new Uri((string?)e.Attribute("Source")
		?? throw new InvalidOperationException("The element has no Source attribute."), UriKind.RelativeOrAbsolute);

	static XElement Root(XpsPackage package, string partName, string expected)
	{
		var root = package.ReadXml(partName).Root!;
		if (root.Name != Ns + expected)
			throw new InvalidOperationException(
				"The part " + partName + " is not a " + expected + " (found " + root.Name + ").");
		return root;
	}

	/// <summary>The part names (page XML and its relationship targets) a page copy needs.</summary>
	internal static string[] RelatedParts(XpsPackage package, string pagePart) =>
		package.Relationships(pagePart).Select(r => r.Target).Distinct().ToArray();
}
