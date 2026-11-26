using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace System.Windows.Xps.Packaging;

/// <summary>
/// An XPS (Open Packaging Conventions) ZIP package. Part names are absolute with forward slashes
/// (<c>/Documents/1/FixedDocument.fdoc</c>), matched case-insensitively. Parts are addressed by URIs of the form
/// <c>xpspkg://{id}/{part}</c>; WPF uses <c>pack://</c> URIs for the same purpose.
/// </summary>
sealed partial class XpsPackage : IDisposable
{
	const string Scheme = "xpspkg";
	static readonly object Gate = new object();
	static readonly Dictionary<string, XpsPackage> Open = new Dictionary<string, XpsPackage>();

	readonly Stream _stream;
	ZipArchive? _zip;
	Dictionary<string, ZipArchiveEntry> _entries = new Dictionary<string, ZipArchiveEntry>();

	XpsPackage(string path, FileAccess access)
	{
		Path = path;
		CanWrite = access != FileAccess.Read;
		_stream = CanWrite
			? new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read)
			: new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		try
		{
			_zip = new ZipArchive(_stream, CanWrite ? ZipArchiveMode.Update : ZipArchiveMode.Read);
		}
		catch
		{
			_stream.Dispose();
			throw;
		}
		Id = Guid.NewGuid().ToString("N");
		IndexEntries();
		lock (Gate)
			Open[Id] = this;
	}

	internal string Id { get; }

	internal string Path { get; }

	internal bool CanWrite { get; }

	internal bool IsDisposed => _zip == null;

	internal static XpsPackage OpenFile(string path, FileAccess access)
	{
		if (path == null)
			throw new ArgumentNullException(nameof(path));
		return new XpsPackage(path, access);
	}

	/// <summary>Resolves a part URI back to its open package and part name.</summary>
	internal static bool TryResolve(Uri? uri, out XpsPackage? package, out string partName)
	{
		package = null;
		partName = string.Empty;
		if (uri == null || !uri.IsAbsoluteUri || uri.Scheme != Scheme)
			return false;
		lock (Gate)
			Open.TryGetValue(uri.Host, out package);
		partName = NormalizePart(Uri.UnescapeDataString(uri.AbsolutePath));
		return package != null && !package.IsDisposed;
	}

	internal Uri PartUri(string partName) => new Uri(Scheme + "://" + Id + NormalizePart(partName));

	internal static string NormalizePart(string name)
	{
		var stack = new List<string>();
		foreach (var segment in name.Replace('\\', '/').Split('/'))
		{
			if (segment.Length == 0 || segment == ".")
				continue;
			if (segment == "..")
			{
				if (stack.Count > 0)
					stack.RemoveAt(stack.Count - 1);
			}
			else
				stack.Add(segment);
		}
		return "/" + string.Join("/", stack);
	}

	/// <summary>Resolves <paramref name="target"/> (absolute or relative) against the part that refers to it.</summary>
	internal static string ResolveTarget(string basePart, string target)
	{
		if (target.StartsWith("/", StringComparison.Ordinal))
			return NormalizePart(Uri.UnescapeDataString(target));
		int slash = basePart.LastIndexOf('/');
		return NormalizePart(basePart.Substring(0, slash + 1) + Uri.UnescapeDataString(target));
	}

	void IndexEntries()
	{
		_entries = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
		foreach (var entry in _zip!.Entries)
			_entries["/" + entry.FullName.Replace('\\', '/')] = entry;
	}

	internal bool PartExists(string partName) => _entries.ContainsKey(NormalizePart(partName));

	internal byte[] ReadPart(string partName)
	{
		if (_zip == null)
			throw new ObjectDisposedException(nameof(XpsPackage));
		if (!_entries.TryGetValue(NormalizePart(partName), out var entry))
			throw new FileNotFoundException("The package has no part " + partName + ".", partName);
		using (var s = entry.Open())
		using (var ms = new MemoryStream())
		{
			s.CopyTo(ms);
			return ms.ToArray();
		}
	}

	internal XDocument ReadXml(string partName)
	{
		using (var ms = new MemoryStream(ReadPart(partName)))
			return XDocument.Load(ms);
	}

	/// <summary>The part of the first fixed document sequence, from the package relationships.</summary>
	internal string? FixedSequencePart() =>
		Relationships("/").Where(r => r.Type == XpsNames.FixedRepresentation).Select(r => r.Target).FirstOrDefault();

	public void Dispose()
	{
		lock (Gate)
			Open.Remove(Id);
		_zip?.Dispose();
		_zip = null;
		_stream.Dispose();
	}
}
