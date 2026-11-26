namespace System.Windows;

/// <summary>
/// Pure in-memory clipboard backend used by default when no GTK display is available
/// (headless tests, CI). Also used as the in-process mirror by <see cref="GtkClipboardBackend"/>.
/// </summary>
internal sealed class InMemoryClipboardBackend : IClipboardBackend
{
	IDataObject? _data;

	/// <inheritdoc/>
	public void Clear() => _data = null;

	/// <inheritdoc/>
	public bool ContainsText() =>
		_data != null &&
		(_data.GetDataPresent(DataFormats.UnicodeText) || _data.GetDataPresent(DataFormats.Text));

	/// <inheritdoc/>
	public string? GetText()
	{
		if (_data == null)
			return null;
		return _data.GetData(DataFormats.UnicodeText) as string
			?? _data.GetData(DataFormats.Text) as string;
	}

	/// <inheritdoc/>
	public void SetText(string text)
	{
		var obj = new DataObject();
		obj.SetData(DataFormats.UnicodeText, text);
		_data = obj;
	}

	/// <inheritdoc/>
	public bool ContainsData(string format) => _data?.GetDataPresent(format) == true;

	/// <inheritdoc/>
	public object? GetData(string format) => _data?.GetData(format);

	/// <inheritdoc/>
	public IDataObject? GetDataObject() => _data;

	/// <inheritdoc/>
	public void SetDataObject(IDataObject data) => _data = data;
}
