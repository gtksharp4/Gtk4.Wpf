namespace System.Windows.Controls.Primitives;

/// <summary>
/// Maps WPF <see cref="PlacementMode"/> values to <see cref="Gtk.PositionType"/>.
/// <para>
/// GTK 4 popover supports four cardinal positions (Bottom, Top, Left, Right).
/// WPF modes that have no direct equivalent fall back as documented per value.
/// </para>
/// </summary>
internal static class PopupPlacementMap
{
	/// <summary>
	/// Returns the <see cref="Gtk.PositionType"/> that best approximates
	/// <paramref name="mode"/> for a <see cref="Gtk.Popover"/>.
	/// <list type="table">
	///   <listheader><term>WPF mode</term><description>GTK position / notes</description></listheader>
	///   <item><term>Bottom</term><description>Bottom — exact match.</description></item>
	///   <item><term>Top</term><description>Top — exact match.</description></item>
	///   <item><term>Left</term><description>Left — exact match.</description></item>
	///   <item><term>Right</term><description>Right — exact match.</description></item>
	///   <item><term>Center</term><description>Bottom — GTK has no centered mode; falls back to Bottom.</description></item>
	///   <item><term>Relative / RelativePoint</term><description>Bottom — no direct GTK equivalent; use <c>Gtk.Popover.SetOffset</c> for fine-tuning.</description></item>
	///   <item><term>Absolute / AbsolutePoint</term><description>Bottom — GTK popover anchors to a widget, not a screen point; falls back to Bottom.</description></item>
	///   <item><term>Mouse / MousePoint</term><description>Bottom — GTK handles mouse-relative via <c>PointingTo</c>; falls back to Bottom.</description></item>
	///   <item><term>Custom</term><description>Bottom — application-defined; falls back to Bottom.</description></item>
	/// </list>
	/// </summary>
	/// <param name="mode">WPF placement mode.</param>
	/// <returns>The closest <see cref="Gtk.PositionType"/> equivalent.</returns>
	internal static Gtk.PositionType ToGtkPosition(PlacementMode mode) => mode switch
	{
		PlacementMode.Bottom        => Gtk.PositionType.Bottom,
		PlacementMode.Top           => Gtk.PositionType.Top,
		PlacementMode.Left          => Gtk.PositionType.Left,
		PlacementMode.Right         => Gtk.PositionType.Right,
		// All other modes have no exact GTK equivalent — Bottom is the sensible default.
		_                           => Gtk.PositionType.Bottom,
	};
}
