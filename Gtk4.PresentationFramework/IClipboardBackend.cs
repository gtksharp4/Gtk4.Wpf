namespace System.Windows;

/// <summary>
/// Injectable clipboard backend. The default is <see cref="GtkClipboardBackend"/>;
/// tests inject <see cref="InMemoryClipboardBackend"/> to avoid any display/GTK dependency.
/// </summary>
/// <remarks>
/// <b>GTK 4 async clipboard:</b> <c>Gdk.Clipboard</c> exposes only async read APIs.
/// The synchronous WPF surface is backed by an in-memory mirror that <c>SetText</c> and
/// <c>SetDataObject</c> always write, so <c>GetText</c>/<c>GetDataObject</c> return
/// immediately from that mirror. The GTK clipboard is a best-effort side-channel: data is
/// pushed to it on set, but pulling it back (from another process) is not supported through
/// this synchronous API.
/// </remarks>
internal interface IClipboardBackend
{
	/// <summary>Clears all data.</summary>
	void Clear();

	/// <summary>Returns whether text is present.</summary>
	bool ContainsText();

	/// <summary>Returns the text, or <see langword="null"/> when absent.</summary>
	string? GetText();

	/// <summary>Stores text.</summary>
	void SetText(string text);

	/// <summary>Returns whether a named format is present.</summary>
	bool ContainsData(string format);

	/// <summary>Returns the stored <see cref="IDataObject"/>, or <see langword="null"/>.</summary>
	IDataObject? GetDataObject();

	/// <summary>Stores the data object. Implementations should also push text to the native clipboard.</summary>
	void SetDataObject(IDataObject data);

	/// <summary>Returns the data stored for <paramref name="format"/>, including content another process offers, or null.</summary>
	object? GetData(string format);
}
