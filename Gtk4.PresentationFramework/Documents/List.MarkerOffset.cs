namespace System.Windows.Documents;

public partial class List
{
	/// <summary>Identifies the MarkerOffset property (default NaN: automatic).</summary>
	public static readonly DependencyProperty MarkerOffsetProperty = DependencyProperty.Register(
		nameof(MarkerOffset), typeof(double), typeof(List), new FrameworkPropertyMetadata(double.NaN),
		v => v is double d && (double.IsNaN(d) || (d >= -1000000 && d <= 1000000)));

	/// <summary>Gets or sets the distance between the marker and the list item content (stored only).</summary>
	public double MarkerOffset
	{
		get => (double)GetValue(MarkerOffsetProperty)!;
		set => SetValue(MarkerOffsetProperty, value);
	}
}
