namespace System.Windows;

/// <summary>
/// Pure <see cref="Visibility"/> to GTK 4 widget-state mapping. Collapsed hides the widget so GTK
/// containers skip it in measure/allocate; Hidden keeps it mapped (and so keeps its allocation) but
/// suppresses rendering and input, as WPF's Hidden does.
/// </summary>
internal static class VisibilityGtkMap
{
	/// <summary>GTK state implied by a <see cref="Visibility"/> value.</summary>
	internal readonly struct Flags
	{
		internal Flags(bool widgetVisible, bool suppressOpacity, bool allowTarget)
		{
			WidgetVisible = widgetVisible;
			SuppressOpacity = suppressOpacity;
			AllowTarget = allowTarget;
		}

		/// <summary><c>Gtk.Widget.Visible</c>.</summary>
		internal bool WidgetVisible { get; }

		/// <summary>Force <c>Gtk.Widget.Opacity</c> to 0 (the Opacity DP is restored when false).</summary>
		internal bool SuppressOpacity { get; }

		/// <summary>Whether <c>Gtk.Widget.CanTarget</c> / <c>CanFocus</c> may be true.</summary>
		internal bool AllowTarget { get; }
	}

	internal static Flags Map(Visibility visibility) => visibility switch
	{
		Visibility.Collapsed => new Flags(false, false, false),
		Visibility.Hidden => new Flags(true, true, false),
		_ => new Flags(true, false, true),
	};

	internal static double EffectiveOpacity(Visibility visibility, double opacity) =>
		Map(visibility).SuppressOpacity ? 0.0 : opacity;

	internal static bool CanTarget(Visibility visibility, bool isHitTestVisible) =>
		Map(visibility).AllowTarget && isHitTestVisible;

	internal static bool CanFocus(Visibility visibility, bool tabFocusable) =>
		Map(visibility).AllowTarget && tabFocusable;
}
