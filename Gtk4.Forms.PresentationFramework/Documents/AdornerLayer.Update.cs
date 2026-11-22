namespace System.Windows.Documents;

public sealed partial class AdornerLayer
{
	/// <summary>Invalidates the layout of every adorner in the layer.</summary>
	public void Update()
	{
		foreach (var list in _adorners.Values)
			Invalidate(list);
	}

	/// <summary>Invalidates the layout of the adorners bound to <paramref name="element"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	public void Update(UIElement element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (_adorners.TryGetValue(element, out var list))
			Invalidate(list);
	}

	static void Invalidate(System.Collections.Generic.List<Adorner> adorners)
	{
		foreach (var adorner in adorners)
			adorner.InvalidateMeasure();
	}
}
