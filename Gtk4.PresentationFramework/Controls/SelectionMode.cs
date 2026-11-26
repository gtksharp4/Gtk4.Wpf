namespace System.Windows.Controls;

/// <summary>
/// Specifies the selection behaviour of a <see cref="ListBox"/>.
/// </summary>
/// <remarks>
/// GTK 4 does not distinguish between <see cref="Multiple"/> and
/// <see cref="Extended"/> at the input level — both map to
/// <see cref="Gtk.SelectionMode.Multiple"/>. The WPF keyboard-modifier
/// semantics (Shift=range, Ctrl=toggle for Extended; every click toggles
/// for Multiple) are not reproduced.
/// </remarks>
public enum SelectionMode
{
	/// <summary>At most one item may be selected at a time.</summary>
	Single,

	/// <summary>
	/// Each click toggles the clicked item independently.
	/// Maps to <see cref="Gtk.SelectionMode.Multiple"/>.
	/// </summary>
	Multiple,

	/// <summary>
	/// Range and toggle selection via Shift/Ctrl (WPF). In this layer,
	/// treated identically to <see cref="Multiple"/> (GTK limitation).
	/// Maps to <see cref="Gtk.SelectionMode.Multiple"/>.
	/// </summary>
	Extended,
}
