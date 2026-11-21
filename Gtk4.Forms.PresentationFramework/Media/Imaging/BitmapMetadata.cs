using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>
/// Image metadata addressed by query paths (WPF <c>BitmapMetadata</c>). PNG <c>/Text/key</c> entries are
/// persisted by the PNG codec; every other path, and the policy properties, are kept in memory only.
/// </summary>
public partial class BitmapMetadata : ImageMetadata
{
	static readonly string[] _formats = { "png", "jpg", "gif", "tiff", "ifd", "wmphoto", "bmp" };

	readonly Dictionary<string, object> _queries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
	readonly string _format;

	/// <summary>Creates empty metadata for a container (<c>png</c>, <c>jpg</c>, <c>gif</c>, <c>tiff</c>,
	/// <c>ifd</c>, <c>wmphoto</c> or <c>bmp</c>).</summary>
	/// <exception cref="ArgumentNullException"><paramref name="containerFormat"/> is null.</exception>
	/// <exception cref="ArgumentException">The container is not known.</exception>
	public BitmapMetadata(string containerFormat)
	{
		if (containerFormat == null)
			throw new ArgumentNullException(nameof(containerFormat));
		if (Array.IndexOf(_formats, containerFormat) < 0)
			throw new ArgumentException($"Unknown metadata container '{containerFormat}'.", nameof(containerFormat));
		_format = containerFormat;
	}

	/// <summary>The container format name.</summary>
	public string Format => _format;

	/// <summary>The query path of this metadata block within its container.</summary>
	public string Location => "/";

	/// <summary>Whether the metadata is frozen (decoder metadata is).</summary>
	public bool IsReadOnly => IsFrozen;

	/// <summary>Whether entries cannot be added or removed (same as <see cref="IsReadOnly"/>).</summary>
	public bool IsFixedSize => IsFrozen;

	/// <summary>Returns the value at <paramref name="query"/>, or <see langword="null"/> when absent.</summary>
	public object? GetQuery(string query)
	{
		Check(query);
		return _queries.TryGetValue(query, out var value) ? value : null;
	}

	/// <summary>Whether a value exists at <paramref name="query"/>.</summary>
	public bool ContainsQuery(string query)
	{
		Check(query);
		return _queries.ContainsKey(query);
	}

	/// <summary>Sets the value at <paramref name="query"/>.</summary>
	/// <exception cref="InvalidOperationException">The metadata is frozen.</exception>
	public void SetQuery(string query, object value)
	{
		Check(query);
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		ThrowIfFrozen();
		_queries[query] = value;
	}

	/// <summary>Removes the value at <paramref name="query"/>.</summary>
	/// <exception cref="InvalidOperationException">The metadata is frozen.</exception>
	public void RemoveQuery(string query)
	{
		Check(query);
		ThrowIfFrozen();
		_queries.Remove(query);
	}

	/// <summary>Creates an unfrozen copy.</summary>
	public new BitmapMetadata Clone() => (BitmapMetadata)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapMetadata(_format);

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		var clone = (BitmapMetadata)destination;
		foreach (var kv in _queries)
			clone._queries[kv.Key] = kv.Value;
		CopyPolicy(clone);
	}

	internal IEnumerable<KeyValuePair<string, object>> Entries => _queries;

	internal bool SameEntriesAs(BitmapMetadata other)
	{
		if (_queries.Count != other._queries.Count)
			return false;
		foreach (var kv in _queries)
		{
			if (!other._queries.TryGetValue(kv.Key, out var v) || !Equals(v, kv.Value))
				return false;
		}
		return true;
	}

	static void Check(string query)
	{
		if (query == null)
			throw new ArgumentNullException(nameof(query));
		if (query.Length == 0 || query[0] != '/')
			throw new ArgumentException("A metadata query must start with '/'.", nameof(query));
	}
}
