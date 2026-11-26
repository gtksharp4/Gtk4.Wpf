namespace System.Windows.Controls;

public partial class WrapPanel
{
	/// <summary>Identifies <see cref="ItemWidth"/>. Default NaN: each child keeps its own width.</summary>
	public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
		nameof(ItemWidth), typeof(double), typeof(WrapPanel),
		new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidItemSize);

	/// <summary>Identifies <see cref="ItemHeight"/>. Default NaN: each child keeps its own height.</summary>
	public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
		nameof(ItemHeight), typeof(double), typeof(WrapPanel),
		new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidItemSize);

	/// <summary>Gets or sets the width every child is given, or NaN for each child's own width.</summary>
	public double ItemWidth
	{
		get => (double)GetValue(ItemWidthProperty)!;
		set => SetValue(ItemWidthProperty, value);
	}

	/// <summary>Gets or sets the height every child is given, or NaN for each child's own height.</summary>
	public double ItemHeight
	{
		get => (double)GetValue(ItemHeightProperty)!;
		set => SetValue(ItemHeightProperty, value);
	}

	static bool IsValidItemSize(object? value) =>
		value is double d && (double.IsNaN(d) || (d >= 0.0 && !double.IsInfinity(d)));

	Size ItemConstraint() => new Size(
		double.IsNaN(ItemWidth) ? double.PositiveInfinity : ItemWidth,
		double.IsNaN(ItemHeight) ? double.PositiveInfinity : ItemHeight);

	double ItemWidthOf(UIElement child) => double.IsNaN(ItemWidth) ? child.DesiredSize.Width : ItemWidth;

	double ItemHeightOf(UIElement child) => double.IsNaN(ItemHeight) ? child.DesiredSize.Height : ItemHeight;
}
