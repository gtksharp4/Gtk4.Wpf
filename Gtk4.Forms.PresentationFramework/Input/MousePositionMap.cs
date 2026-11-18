namespace System.Windows.Input;

/// <summary>
/// Translates a pointer position between element coordinate spaces using the GTK 4-native
/// <c>gtk_widget_compute_point</c> (<see cref="Gtk.Widget.ComputePoint"/>).
/// </summary>
internal static class MousePositionMap
{
	/// <summary>
	/// Translates <paramref name="position"/> (relative to <paramref name="source"/>) into the
	/// coordinate space of <paramref name="relativeTo"/>. Returns the position unchanged when
	/// either side is missing, is not a <see cref="UIElement"/>, has no realized widget, or GTK
	/// cannot relate the two widgets (no common ancestor / not yet laid out).
	/// </summary>
	internal static Point Translate(UIElement? source, IInputElement? relativeTo, Point position)
	{
		if (source == null || relativeTo is not UIElement target || ReferenceEquals(source, target))
			return position;
		if (!source.HasBackingWidget || !target.HasBackingWidget)
			return position;

		var from = new Graphene.Point();
		from.X = (float)position.X;
		from.Y = (float)position.Y;
		return source.Widget.ComputePoint(target.Widget, from, out var to)
			? new Point(to.X, to.Y)
			: position;
	}
}
