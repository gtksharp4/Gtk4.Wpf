using System.Windows.Shapes;

namespace System.Windows.Media;

public partial class VisualBrush
{
	// True when an element inside the source paints this very brush (the owner paints inside its own source),
	// however it got there: a local value, a Style or binding result, or nested in a DrawingBrush or Pen.
	bool PaintedInsideSource() => _watched != null && Holds(_watched);

	bool Holds(DependencyObject element)
	{
		var values = element.GetLocalValueEnumerator();
		while (values.MoveNext())
			if (Reaches(values.Current.Value)) return true;
		if (element is Shape shape && (Reaches(shape.Fill) || Reaches(shape.Stroke))) return true;
		if (element is Controls.Panel panel && Reaches(panel.Background)) return true;
		if (element is Controls.Border border && Reaches(border.Background)) return true;
		int count = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < count; i++)
			if (Holds(VisualTreeHelper.GetChild(element, i))) return true;
		return false;
	}

	bool Reaches(object? value) => value switch
	{
		VisualBrush brush => ReferenceEquals(brush, this),
		DrawingBrush { Drawing: { } drawing } => Reaches(drawing),
		DrawingGroup group => ReachesAny(group),
		GeometryDrawing g => Reaches(g.Brush) || Reaches(g.Pen),
		Pen pen => Reaches(pen.Brush),
		_ => false,
	};

	bool ReachesAny(DrawingGroup group)
	{
		foreach (var child in group.Children)
			if (Reaches(child)) return true;
		return false;
	}

	// A storm-breaker check that found the source unchanged may have run before GTK drew the change: look again
	// once, after the frame, so a real change still gets through.
	void RecheckLater()
	{
		if (_rechecking) return;
		_rechecking = true;
		GLib.Timeout.Add(EchoMilliseconds, new GLib.TimeoutHandler(() =>
		{
			_rechecking = false;
			if (!IsFrozen && SourceDiffers())
			{
				_identical = 0;
				OnChanged();
			}
			return false;
		}));
	}
}
