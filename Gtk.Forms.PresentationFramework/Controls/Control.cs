namespace System.Windows.Controls;

/// <summary>
/// Base class for WPF controls. Adds foreground/background brush support (Phase-1)
/// and font properties (Phase-1); in Phase 0 it is a thin pass-through to
/// <see cref="FrameworkElement"/>.
/// </summary>
public abstract partial class Control : FrameworkElement
{
	/// <summary>
	/// Applies the theme-default style at construction time so a normally-constructed
	/// control receives its default-style setters without any manual call.
	/// Order: explicit Style &gt; implicit TargetType style &gt; theme default style.
	/// </summary>
	protected Control()
	{
		ApplyDefaultStyle();
	}
}