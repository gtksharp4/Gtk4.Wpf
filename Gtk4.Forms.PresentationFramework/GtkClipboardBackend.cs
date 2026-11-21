using System.Runtime.InteropServices;

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
internal sealed partial class GtkClipboardBackend : IClipboardBackend
{
	readonly InMemoryClipboardBackend _mirror = new();

	// GTK 4's macOS (Quartz) GdkClipboard SIGSEGVs on SetContent / Text-setter writes in this
	// GtkSharp build (the content-provider/formats path asserts then crashes). A native crash is
	// not catchable by try/catch, so the only safe option is to NOT call it: on macOS the native
	// clipboard side-channel is disabled and everything runs off the in-memory mirror (reads are
	// unaffected on every platform — they already come from the mirror). Cross-app clipboard push
	// therefore does not work on macOS; within-process copy/paste still does.
	static readonly bool _nativeEnabled = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	// ── Display / native clipboard ────────────────────────────────────────────

	/// <summary>
	/// Returns the native GDK clipboard, or null when no display is available or the native
	/// clipboard is disabled on this platform (macOS — see <see cref="_nativeEnabled"/>).
	/// </summary>
	static Gdk.Clipboard? NativeClipboard()
	{
		if (!_nativeEnabled)
			return null;
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
	public bool ContainsData(string format) => _mirror.ContainsData(format) || ForeignHas(format);

	/// <inheritdoc/>
	public IDataObject? GetDataObject() => _mirror.GetDataObject();

	/// <inheritdoc/>
	public void SetDataObject(IDataObject data)
	{
		_mirror.SetDataObject(data);
		PushMedia(data);

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
