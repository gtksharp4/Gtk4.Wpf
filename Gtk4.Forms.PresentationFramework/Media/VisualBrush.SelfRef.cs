namespace System.Windows.Media;

public partial class VisualBrush
{
	// True when an element inside the source holds this very brush (the owner paints inside its own source).
	bool PaintedInsideSource() => _watched != null && Holds(_watched);

	bool Holds(DependencyObject element)
	{
		var values = element.GetLocalValueEnumerator();
		while (values.MoveNext())
			if (ReferenceEquals(values.Current.Value, this)) return true;
		int count = VisualTreeHelper.GetChildrenCount(element);
		for (int i = 0; i < count; i++)
			if (Holds(VisualTreeHelper.GetChild(element, i))) return true;
		return false;
	}
}
