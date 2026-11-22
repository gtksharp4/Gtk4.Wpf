namespace System.Windows.Controls;

// Store-only: the GTK text widgets are sized by the layout pass, not by a line count.
public partial class TextBox
{
	/// <summary>Identifies the MaxLines property (default Int32.MaxValue; must be at least 1).</summary>
	public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
		nameof(MaxLines), typeof(int), typeof(TextBox),
		new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure),
		v => v is int i && i >= 1);

	/// <summary>Identifies the MinLines property (default 1; must be at least 1).</summary>
	public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register(
		nameof(MinLines), typeof(int), typeof(TextBox),
		new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure),
		v => v is int i && i >= 1);

	/// <summary>Gets or sets the maximum number of visible lines.</summary>
	public int MaxLines
	{
		get => (int)GetValue(MaxLinesProperty)!;
		set => SetValue(MaxLinesProperty, value);
	}

	/// <summary>Gets or sets the minimum number of visible lines.</summary>
	public int MinLines
	{
		get => (int)GetValue(MinLinesProperty)!;
		set => SetValue(MinLinesProperty, value);
	}
}
