namespace System.Windows.Controls;

public static partial class ToolTipService
{
	// ── Placement DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetPlacement"/>/<see cref="SetPlacement"/> attached property.
	/// Default: <see cref="PlacementMode.Mouse"/>.
	/// Store-only: GTK positions tooltips automatically; this value is not forwarded to GTK.
	/// </summary>
	public static readonly DependencyProperty PlacementProperty =
		DependencyProperty.RegisterAttached(
			"Placement",
			typeof(PlacementMode),
			typeof(ToolTipService),
			new PropertyMetadata(PlacementMode.Mouse));

	/// <summary>Gets the placement mode hint for the tooltip on <paramref name="element"/>.</summary>
	public static PlacementMode GetPlacement(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (PlacementMode)(element.GetValue(PlacementProperty) ?? PlacementMode.Mouse);
	}

	/// <summary>Sets the placement mode hint for the tooltip on <paramref name="element"/>.</summary>
	public static void SetPlacement(DependencyObject element, PlacementMode value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(PlacementProperty, value);
	}

	// ── PlacementTarget DP ───────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetPlacementTarget"/>/<see cref="SetPlacementTarget"/>
	/// attached property. Default: <see langword="null"/>.
	/// Store-only: GTK anchors tooltips to the host widget automatically.
	/// </summary>
	public static readonly DependencyProperty PlacementTargetProperty =
		DependencyProperty.RegisterAttached(
			"PlacementTarget",
			typeof(UIElement),
			typeof(ToolTipService),
			new PropertyMetadata(null));

	/// <summary>Gets the placement target for the tooltip on <paramref name="element"/>.</summary>
	public static UIElement? GetPlacementTarget(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (UIElement?)element.GetValue(PlacementTargetProperty);
	}

	/// <summary>Sets the placement target for the tooltip on <paramref name="element"/>.</summary>
	public static void SetPlacementTarget(DependencyObject element, UIElement? value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(PlacementTargetProperty, value);
	}

	// ── HorizontalOffset DP ──────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetHorizontalOffset"/>/<see cref="SetHorizontalOffset"/>
	/// attached property. Default: 0.0.
	/// Store-only: GTK positions tooltips automatically; this value is not forwarded.
	/// </summary>
	public static readonly DependencyProperty HorizontalOffsetProperty =
		DependencyProperty.RegisterAttached(
			"HorizontalOffset",
			typeof(double),
			typeof(ToolTipService),
			new PropertyMetadata(0.0));

	/// <summary>Gets the horizontal offset for the tooltip on <paramref name="element"/>.</summary>
	public static double GetHorizontalOffset(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (double)(element.GetValue(HorizontalOffsetProperty) ?? 0.0);
	}

	/// <summary>Sets the horizontal offset for the tooltip on <paramref name="element"/>.</summary>
	public static void SetHorizontalOffset(DependencyObject element, double value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(HorizontalOffsetProperty, value);
	}

	// ── VerticalOffset DP ────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetVerticalOffset"/>/<see cref="SetVerticalOffset"/>
	/// attached property. Default: 0.0.
	/// Store-only: GTK positions tooltips automatically; this value is not forwarded.
	/// </summary>
	public static readonly DependencyProperty VerticalOffsetProperty =
		DependencyProperty.RegisterAttached(
			"VerticalOffset",
			typeof(double),
			typeof(ToolTipService),
			new PropertyMetadata(0.0));

	/// <summary>Gets the vertical offset for the tooltip on <paramref name="element"/>.</summary>
	public static double GetVerticalOffset(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (double)(element.GetValue(VerticalOffsetProperty) ?? 0.0);
	}

	/// <summary>Sets the vertical offset for the tooltip on <paramref name="element"/>.</summary>
	public static void SetVerticalOffset(DependencyObject element, double value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(VerticalOffsetProperty, value);
	}
}
