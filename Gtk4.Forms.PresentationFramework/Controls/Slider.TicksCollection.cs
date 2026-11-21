using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace System.Windows.Controls;

public partial class Slider
{
	/// <summary>Identifies <see cref="Ticks"/>. Default: <c>null</c>.</summary>
	public static readonly DependencyProperty TicksProperty =
		DependencyProperty.Register(nameof(Ticks), typeof(DoubleCollection), typeof(Slider),
			new PropertyMetadata(null, OnTickChanged));

	/// <summary>Gets or sets explicit tick positions; when non-empty they replace <see cref="TickFrequency"/>.</summary>
	public DoubleCollection? Ticks
	{
		get => (DoubleCollection?)GetValue(TicksProperty);
		set => SetValue(TicksProperty, value);
	}

	// Explicit ticks win; positions outside the range are dropped.
	internal IEnumerable<double> TickValues()
	{
		if (Ticks is { Count: > 0 } ticks)
			return ticks.Where(t => t >= Minimum && t <= Maximum).ToArray();
		return SliderSnap.TickPositions(Minimum, Maximum, TickFrequency);
	}
}
