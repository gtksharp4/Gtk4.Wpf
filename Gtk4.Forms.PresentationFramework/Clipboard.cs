namespace System.Windows;

/// <summary>
/// Provides static methods to access the system clipboard, mirroring the WPF API.
/// All operations are synchronous. The actual storage is delegated to <see cref="Backend"/>;
/// tests substitute an <see cref="InMemoryClipboardBackend"/> to avoid any display/GTK dependency.
/// </summary>
/// <remarks>
/// <b>GTK 4 async clipboard:</b> GTK 4's <c>Gdk.Clipboard</c> exposes only async read APIs.
/// This class therefore maintains an in-process in-memory mirror of everything written via
/// <see cref="SetText"/> and <see cref="SetDataObject"/>. <see cref="GetText"/> and
/// <see cref="GetDataObject"/> read from that mirror synchronously.
/// <para>
/// <b>Cross-process reads are NOT supported:</b> text written by another application to the
/// system clipboard is not visible through these APIs; a future async API would be needed.
/// </para>
/// <para>
/// <b>Headless degrade:</b> when no GTK display is available (CI, unit tests) the default
/// <see cref="GtkClipboardBackend"/> silently falls back to the in-memory store; nothing throws.
/// </para>
/// </remarks>
public static partial class Clipboard
{
	/// <summary>
	/// Injectable clipboard backend. Defaults to <see cref="GtkClipboardBackend"/>;
	/// replace with <see cref="InMemoryClipboardBackend"/> (or any <see cref="IClipboardBackend"/>)
	/// in unit tests.
	/// </summary>
	internal static IClipboardBackend Backend { get; set; } = new GtkClipboardBackend();

	// ── Text ──────────────────────────────────────────────────────────────────

	/// <summary>Stores the specified text on the clipboard.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
	public static void SetText(string text)
	{
		ArgumentNullException.ThrowIfNull(text);
		Backend.SetText(text);
	}

	/// <summary>
	/// Returns the text currently on the clipboard, or <see cref="string.Empty"/> when absent.
	/// </summary>
	public static string GetText() => Backend.GetText() ?? string.Empty;

	/// <summary>Returns whether the clipboard contains text.</summary>
	public static bool ContainsText() => Backend.ContainsText();

	// ── Data object ──────────────────────────────────────────────────────────

	/// <summary>Stores the data object on the clipboard.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
	public static void SetDataObject(IDataObject data)
	{
		ArgumentNullException.ThrowIfNull(data);
		Backend.SetDataObject(data);
	}

	/// <summary>
	/// Stores the data object on the clipboard, optionally making it available after
	/// the application exits (<paramref name="copy"/> = <see langword="true"/> triggers a
	/// best-effort <c>Gdk.Clipboard.StoreAsync</c>; in this implementation the in-memory
	/// mirror is unaffected either way).
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
	public static void SetDataObject(IDataObject data, bool copy) => SetDataObject(data);

	/// <summary>
	/// Returns the current clipboard content as an <see cref="IDataObject"/>,
	/// or <see langword="null"/> when empty.
	/// </summary>
	public static IDataObject? GetDataObject() => Backend.GetDataObject();

	// ── Presence ─────────────────────────────────────────────────────────────

	/// <summary>Returns whether the clipboard contains data in the specified format.</summary>
	public static bool ContainsData(string format)
	{
		ArgumentNullException.ThrowIfNull(format);
		return Backend.ContainsData(format);
	}

	// ── Clear ─────────────────────────────────────────────────────────────────

	/// <summary>Removes all data from the clipboard.</summary>
	public static void Clear() => Backend.Clear();
}
