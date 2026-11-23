using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;

namespace System.Windows.Xps;

/// <summary>
/// Writes fixed documents into an <see cref="XpsDocument"/>, replacing its content. Pages that came from an XPS
/// package are copied part for part (with their fonts and images); pages built in memory are serialised when they
/// consist of Canvas and Glyphs elements.
/// </summary>
/// <remarks>
/// Gap: an in-memory page that holds anything else (for example a Path, which has no geometry serialiser) throws
/// <see cref="NotSupportedException"/>. Printing and the <c>Visual</c>/<c>DocumentPaginator</c> overloads are not
/// supported.
/// </remarks>
public sealed class XpsDocumentWriter
{
	readonly XpsDocument _document;

	internal XpsDocumentWriter(XpsDocument document) => _document = document;

	/// <summary>Replaces the package content with <paramref name="fixedDocumentSequence"/>.</summary>
	/// <exception cref="ArgumentNullException">The argument is null.</exception>
	/// <exception cref="InvalidOperationException">The package was opened read-only.</exception>
	/// <exception cref="NotSupportedException">An in-memory page cannot be serialised.</exception>
	public void Write(FixedDocumentSequence fixedDocumentSequence)
	{
		if (fixedDocumentSequence == null)
			throw new ArgumentNullException(nameof(fixedDocumentSequence));
		var builder = new XpsSequenceWriter();
		foreach (var reference in fixedDocumentSequence.References)
			builder.AddDocument(reference.GetDocument(false));
		builder.Save(_document.Package);
	}

	/// <summary>Replaces the package content with a sequence of the single <paramref name="fixedDocument"/>.</summary>
	/// <exception cref="ArgumentNullException">The argument is null.</exception>
	public void Write(FixedDocument fixedDocument)
	{
		if (fixedDocument == null)
			throw new ArgumentNullException(nameof(fixedDocument));
		var builder = new XpsSequenceWriter();
		builder.AddDocument(fixedDocument);
		builder.Save(_document.Package);
	}

	/// <summary>Replaces the package content with a one-page document holding <paramref name="fixedPage"/>.</summary>
	/// <exception cref="ArgumentNullException">The argument is null.</exception>
	public void Write(FixedPage fixedPage)
	{
		if (fixedPage == null)
			throw new ArgumentNullException(nameof(fixedPage));
		var content = new PageContent();
		((IAddChild)content).AddChild(fixedPage);
		var document = new FixedDocument();
		document.Pages.Add(content);
		Write(document);
	}
}
