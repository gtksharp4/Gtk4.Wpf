namespace System.Windows.Controls;

/// <summary>
/// Partial extension of <see cref="Control"/> that adds layout/alignment DPs:
/// <see cref="Padding"/>, <see cref="HorizontalContentAlignment"/> and
/// <see cref="VerticalContentAlignment"/>.  These carry style-layer values set by
/// default styles in <see cref="Themes.Generic"/> and may be overridden by explicit
/// setters or local assignments.
/// </summary>
public abstract partial class Control
{
	// ── Padding ───────────────────────────────────────────────────────────────

	/// <summary>Identifies the <see cref="Padding"/> dependency property.</summary>
	public static readonly DependencyProperty PaddingProperty =
		DependencyProperty.Register(
			nameof(Padding),
			typeof(Thickness),
			typeof(Control),
			new PropertyMetadata(new Thickness(0)));

	/// <summary>Inner padding between the border and the content area.</summary>
	public Thickness Padding
	{
		get => (Thickness?)GetValue(PaddingProperty) ?? default;
		set => SetValue(PaddingProperty, value);
	}

	// ── HorizontalContentAlignment ────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="HorizontalContentAlignment"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty HorizontalContentAlignmentProperty =
		DependencyProperty.Register(
			nameof(HorizontalContentAlignment),
			typeof(HorizontalAlignment),
			typeof(Control),
			new PropertyMetadata(HorizontalAlignment.Left));

	/// <summary>Horizontal alignment of the content within the control.</summary>
	public HorizontalAlignment HorizontalContentAlignment
	{
		get => (HorizontalAlignment?)GetValue(HorizontalContentAlignmentProperty) ?? HorizontalAlignment.Left;
		set => SetValue(HorizontalContentAlignmentProperty, value);
	}

	// ── VerticalContentAlignment ──────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="VerticalContentAlignment"/> dependency property.
	/// </summary>
	public static readonly DependencyProperty VerticalContentAlignmentProperty =
		DependencyProperty.Register(
			nameof(VerticalContentAlignment),
			typeof(VerticalAlignment),
			typeof(Control),
			new PropertyMetadata(VerticalAlignment.Top));

	/// <summary>Vertical alignment of the content within the control.</summary>
	public VerticalAlignment VerticalContentAlignment
	{
		get => (VerticalAlignment?)GetValue(VerticalContentAlignmentProperty) ?? VerticalAlignment.Top;
		set => SetValue(VerticalContentAlignmentProperty, value);
	}
}