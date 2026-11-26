using System;
using System.IO;

namespace System.Windows.Input;

/// <summary>
/// Represents a mouse cursor shape.
/// <para>
/// Standard cursors are obtained from <see cref="Cursors"/>. The overloads that
/// accept a file path or <see cref="Stream"/> are accepted for API compatibility
/// but are <b>not supported</b> in this GTK 4 implementation: custom <c>.cur</c>/
/// <c>.ani</c> files have no GTK equivalent and the cursor will fall back to
/// <c>default</c>.
/// </para>
/// </summary>
public sealed class Cursor : IDisposable
{
	/// <summary>
	/// The GTK / CSS cursor name used by <c>Gtk.Widget.CursorFromName</c>.
	/// </summary>
	internal string GdkName { get; }

	/// <summary>
	/// The WPF cursor type that created this instance, or <c>null</c> for
	/// cursors created from a file/stream (unsupported).
	/// </summary>
	internal CursorType? CursorType { get; }

	/// <summary>Creates a cursor for a standard <see cref="Input.CursorType"/>.</summary>
	internal Cursor(CursorType cursorType)
	{
		CursorType = cursorType;
		GdkName = CursorMap.ToGdkName(cursorType);
	}

	/// <summary>
	/// Creates a cursor from a <c>.cur</c> file path.
	/// <b>Not supported in this GTK 4 implementation</b>; cursor falls back to <c>default</c>.
	/// </summary>
	/// <param name="cursorFile">Path to a <c>.cur</c> or <c>.ani</c> file.</param>
	public Cursor(string cursorFile)
	{
		_ = cursorFile; // accepted but unused
		GdkName = "default";
	}

	/// <summary>
	/// Creates a cursor from a stream containing cursor data.
	/// <b>Not supported in this GTK 4 implementation</b>; cursor falls back to <c>default</c>.
	/// </summary>
	/// <param name="cursorStream">Stream with <c>.cur</c> or <c>.ani</c> data.</param>
	public Cursor(Stream cursorStream)
	{
		_ = cursorStream; // accepted but unused
		GdkName = "default";
	}

	/// <summary>Creates a cursor from a file; same fallback as <see cref="Cursor(string)"/>.</summary>
	public Cursor(string cursorFile, bool scaleWithDpi) : this(cursorFile) { }

	/// <summary>Creates a cursor from a stream; same fallback as <see cref="Cursor(Stream)"/>.</summary>
	public Cursor(Stream cursorStream, bool scaleWithDpi) : this(cursorStream) { }

	/// <summary>Releases the cursor. GTK cursors are looked up by name, so there is nothing to free.</summary>
	public void Dispose() { }

	/// <inheritdoc/>
	public override string ToString() => GdkName;
}
