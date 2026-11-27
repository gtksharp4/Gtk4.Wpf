namespace System.Windows.Documents;

/// <summary>
/// An inline that anchors block content to its position in the flow. Mirrors
/// <c>System.Windows.Documents.Floater</c>; see <see cref="AnchoredBlock"/> for the rendering gap.
/// </summary>
public class Floater : AnchoredBlock
{
	/// <summary>Identifies the HorizontalAlignment property (default Stretch).</summary>
	public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register(
		nameof(HorizontalAlignment), typeof(HorizontalAlignment), typeof(Floater),
		new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

	/// <summary>Identifies the Width property (default NaN; NaN or 0 to 1000000).</summary>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
		nameof(Width), typeof(double), typeof(Floater), new FrameworkPropertyMetadata(double.NaN),
		v => v is double d && (double.IsNaN(d) || (d >= 0 && d <= 1000000)));

	/// <summary>Creates an empty floater.</summary>
	public Floater() : this(null, null) { }

	/// <summary>Creates a floater holding <paramref name="childBlock"/>.</summary>
	public Floater(Block? childBlock) : this(childBlock, null) { }

	/// <summary>Creates a floater holding <paramref name="childBlock"/> at <paramref name="insertionPosition"/>.</summary>
	public Floater(Block? childBlock, TextPointer? insertionPosition) : base(childBlock, insertionPosition) { }

	/// <summary>Gets or sets the horizontal alignment.</summary>
	public HorizontalAlignment HorizontalAlignment
	{
		get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty)!;
		set => SetValue(HorizontalAlignmentProperty, value);
	}

	/// <summary>Gets or sets the width (NaN is auto).</summary>
	public double Width
	{
		get => (double)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}
}
