using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Implements the WPF data-transfer container. Stores arbitrary objects keyed by
/// a format string. Auto-conversion between <see cref="DataFormats.Text"/> and
/// <see cref="DataFormats.UnicodeText"/> is supported; arbitrary object serialization
/// is NOT supported (data is returned as-is; no cross-process marshalling).
/// </summary>
public class DataObject : IDataObject
{
	// ── Storage ───────────────────────────────────────────────────────────────

	readonly Dictionary<string, object> _store =
		new(StringComparer.OrdinalIgnoreCase);

	// ── Constructors ──────────────────────────────────────────────────────────

	/// <summary>Initialises an empty data object.</summary>
	public DataObject() { }

	/// <summary>Initialises a data object containing the supplied data.</summary>
	public DataObject(object data) => SetData(data);

	/// <summary>Initialises a data object containing data stored under the specified format.</summary>
	public DataObject(string format, object data) => SetData(format, data);

	/// <summary>Initialises a data object containing data stored under the specified type.</summary>
	public DataObject(Type format, object data) => SetData(format, data);

	// ── IDataObject — GetData ─────────────────────────────────────────────────

	/// <inheritdoc/>
	public object? GetData(string format) => GetData(format, autoConvert: true);

	/// <inheritdoc/>
	public object? GetData(Type format) => GetData(TypeToFormat(format), autoConvert: true);

	/// <inheritdoc/>
	public object? GetData(string format, bool autoConvert)
	{
		if (_store.TryGetValue(format, out var value))
			return value;

		if (autoConvert)
		{
			string? alternate = GetTextAlternate(format);
			if (alternate != null && _store.TryGetValue(alternate, out value))
				return value;
		}

		return null;
	}

	// ── IDataObject — GetDataPresent ──────────────────────────────────────────

	/// <inheritdoc/>
	public bool GetDataPresent(string format) => GetDataPresent(format, autoConvert: true);

	/// <inheritdoc/>
	public bool GetDataPresent(Type format) => GetDataPresent(TypeToFormat(format), autoConvert: true);

	/// <inheritdoc/>
	public bool GetDataPresent(string format, bool autoConvert)
	{
		if (_store.ContainsKey(format))
			return true;

		if (autoConvert)
		{
			string? alternate = GetTextAlternate(format);
			return alternate != null && _store.ContainsKey(alternate);
		}

		return false;
	}

	// ── IDataObject — GetFormats ──────────────────────────────────────────────

	/// <inheritdoc/>
	public string[] GetFormats() => GetFormats(autoConvert: true);

	/// <inheritdoc/>
	public string[] GetFormats(bool autoConvert)
	{
		if (!autoConvert)
			return [.. _store.Keys];

		var formats = new HashSet<string>(_store.Keys, StringComparer.OrdinalIgnoreCase);

		// Expose the text alternate when the other side is present.
		if (_store.ContainsKey(DataFormats.Text) && !_store.ContainsKey(DataFormats.UnicodeText))
			formats.Add(DataFormats.UnicodeText);
		else if (_store.ContainsKey(DataFormats.UnicodeText) && !_store.ContainsKey(DataFormats.Text))
			formats.Add(DataFormats.Text);

		return [.. formats];
	}

	// ── IDataObject — SetData ─────────────────────────────────────────────────

	/// <inheritdoc/>
	public void SetData(object data)
	{
		ArgumentNullException.ThrowIfNull(data);
		SetData(data.GetType().FullName ?? data.GetType().Name, data, autoConvert: true);
	}

	/// <inheritdoc/>
	public void SetData(string format, object data) => SetData(format, data, autoConvert: true);

	/// <inheritdoc/>
	public void SetData(Type format, object data) => SetData(TypeToFormat(format), data, autoConvert: true);

	/// <inheritdoc/>
	public void SetData(string format, object data, bool autoConvert)
	{
		ArgumentNullException.ThrowIfNull(format);
		ArgumentNullException.ThrowIfNull(data);
		_store[format] = data;
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Returns the text-format alternate for auto-conversion, or <see langword="null"/>
	/// when no auto-conversion exists for <paramref name="format"/>.
	/// </summary>
	static string? GetTextAlternate(string format)
	{
		if (string.Equals(format, DataFormats.Text, StringComparison.OrdinalIgnoreCase))
			return DataFormats.UnicodeText;
		if (string.Equals(format, DataFormats.UnicodeText, StringComparison.OrdinalIgnoreCase))
			return DataFormats.Text;
		return null;
	}

	/// <summary>Derives the format name from a <see cref="Type"/>.</summary>
	static string TypeToFormat(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return type.FullName ?? type.Name;
	}
}
