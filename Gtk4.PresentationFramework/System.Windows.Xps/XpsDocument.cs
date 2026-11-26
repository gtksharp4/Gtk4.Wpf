using System.IO;
using System.Windows.Documents;

namespace System.Windows.Xps.Packaging;

/// <summary>
/// An XPS document: an Open Packaging Conventions ZIP file read and written with
/// <see cref="System.IO.Compression.ZipArchive"/>. Mirrors WPF <c>XpsDocument</c> for the path-based members.
/// </summary>
/// <remarks>
/// Part URIs use the private <c>xpspkg://</c> scheme instead of WPF's <c>pack://</c>. Relationship parts, fonts and
/// images are read on demand; digital signatures, thumbnails and core properties are not supported.
/// </remarks>
public class XpsDocument : IDisposable
{
	XpsPackage? _package;
	FixedDocumentSequence? _sequence;

	/// <summary>Opens (or, for write access, creates) the package at <paramref name="path"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
	/// <exception cref="FileNotFoundException">Read access to a file that does not exist.</exception>
	/// <exception cref="InvalidDataException">The file is not a ZIP package.</exception>
	public XpsDocument(string path, FileAccess packageAccess) => _package = XpsPackage.OpenFile(path, packageAccess);

	/// <summary>The full path of the package file, as a URI.</summary>
	public Uri Uri
	{
		get
		{
			ThrowIfClosed();
			return new Uri(System.IO.Path.GetFullPath(_package!.Path));
		}
	}

	internal XpsPackage Package
	{
		get
		{
			ThrowIfClosed();
			return _package!;
		}
	}

	/// <summary>
	/// Reads the first fixed document sequence of the package; its documents and pages load on demand. Returns null
	/// when the package has none.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The document is closed.</exception>
	public FixedDocumentSequence? GetFixedDocumentSequence()
	{
		ThrowIfClosed();
		if (_sequence != null)
			return _sequence;
		string? part = _package!.FixedSequencePart();
		if (part == null || !_package.PartExists(part))
			return null;
		_sequence = XpsPartReader.ReadSequence(_package, part);
		return _sequence;
	}

	/// <summary>Creates a writer that replaces the content of <paramref name="xpsDocument"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="xpsDocument"/> is null.</exception>
	public static XpsDocumentWriter CreateXpsDocumentWriter(XpsDocument xpsDocument)
	{
		if (xpsDocument == null)
			throw new ArgumentNullException(nameof(xpsDocument));
		return new XpsDocumentWriter(xpsDocument);
	}

	/// <summary>Closes the package and releases the file.</summary>
	public void Close() => Dispose();

	/// <inheritdoc/>
	public void Dispose()
	{
		_package?.Dispose();
		_package = null;
		_sequence = null;
		GC.SuppressFinalize(this);
	}

	void ThrowIfClosed()
	{
		if (_package == null)
			throw new ObjectDisposedException(nameof(XpsDocument));
	}
}
