namespace System.Windows;

/// <summary>
/// Default <see cref="IClipboardBackend"/>: wraps <c>Gdk.Clipboard</c> from the default display.
/// Falls back to the <see cref="InMemoryClipboardBackend"/> mirror when no display is available
/// (headless tests, CI environments without a window system).
/// </summary>
/// <remarks>
/// <b>GTK 4 async clipboard:</b> <c>Gdk.Clipboard</c> provides only async read APIs
/// (<c>ReadTextAsync</c> / <c>ReadTextFinish</c>). The synchronous WPF surface is backed by an
/// in-memory mirror: every <c>SetText</c>/<c>SetDataObject</c> call writes the in-memory store
/// first, so <c>GetText</c>/<c>GetDataObject</c> return synchronously from that mirror.
/// <para>
/// The GTK clipboard is used as a push-only side channel for text: <c>SetText</c> also pushes
/// to <c>Gdk.Clipboard</c> so other applications see the text. Cross-process reads (pulling text
/// that another app put on the clipboard) are NOT supported through this synchronous API.
/// </para>
/// <para>
/// <b>Degrade:</b> when <c>Gdk.Display.Default</c> returns <see langword="null"/> (no display),
/// all operations silently delegate to the in-memory store and nothing throws.
/// </para>
/// </remarks>
internal sealed class GtkClipboardBackend : IClipboardBackend
{
	readonly InMemoryClipboardBackend _mirror = new();

	// ── Display / native clipboard ────────────────────────────────────────────

	/// <summary>Returns the native GDK clipboard, or null when no display is available.</summary>
	static Gdk.Clipboard? NativeClipboard()
	{
		try
		{
			var display = Gdk.Display.Default;
			return display?.Clipboard;
		}
		catch
		{
			// GLib/GDK can throw on first access when GTK is not initialised.
			return null;
		}
	}

	// ── IClipboardBackend ─────────────────────────────────────────────────────

	/// <inheritdoc/>
	public void Clear()
	{
		_mirror.Clear();
		try
		{
			NativeClipboard()?.SetContent(null);
		}
		catch { /* best effort */ }
	}

	/// <inheritdoc/>
	public bool ContainsText() => _mirror.ContainsText();

	/// <inheritdoc/>
	public string? GetText() => _mirror.GetText();

	/// <inheritdoc/>
	public void SetText(string text)
	{
		_mirror.SetText(text);
		try
		{
			NativeClipboard()?.Text = text;
		}
		catch { /* best effort */ }
	}

	/// <inheritdoc/>
	public bool ContainsData(string format) => _mirror.ContainsData(format);

	/// <inheritdoc/>
	public IDataObject? GetDataObject() => _mirror.GetDataObject();

	/// <inheritdoc/>
	public void SetDataObject(IDataObject data)
	{
		_mirror.SetDataObject(data);

		// Best-effort: push any text to the native clipboard so other apps see it.
		string? text = data.GetData(DataFormats.UnicodeText) as string
			?? data.GetData(DataFormats.Text) as string;
		if (text != null)
		{
			try
			{
				NativeClipboard()?.Text = text;
			}
			catch { /* best effort */ }
		}
	}
}
