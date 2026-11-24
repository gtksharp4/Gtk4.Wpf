namespace System.Windows.Documents;

public abstract partial class Inline
{
	/// <summary>Identifies the <see cref="BaselineAlignment"/> property (default Baseline).</summary>
	public static readonly DependencyProperty BaselineAlignmentProperty = DependencyProperty.Register(
		nameof(BaselineAlignment), typeof(BaselineAlignment), typeof(Inline),
		new FrameworkPropertyMetadata(BaselineAlignment.Baseline));

	/// <summary>
	/// Gets or sets the baseline alignment. Only <see cref="BaselineAlignment.Baseline"/> changes the layout of a
	/// hosted <see cref="InlineUIContainer"/>; the other values are stored.
	/// </summary>
	public BaselineAlignment BaselineAlignment
	{
		get => (BaselineAlignment)GetValue(BaselineAlignmentProperty)!;
		set => SetValue(BaselineAlignmentProperty, value);
	}
}
