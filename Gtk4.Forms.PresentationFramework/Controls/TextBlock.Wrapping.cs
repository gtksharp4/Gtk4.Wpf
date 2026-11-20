namespace System.Windows.Controls;

public partial class TextBlock
{
	/// <summary>How text wraps. Default: <see cref="TextWrapping.NoWrap"/>.</summary>
	public static readonly DependencyProperty TextWrappingProperty =
		DependencyProperty.Register(
			nameof(TextWrapping),
			typeof(TextWrapping),
			typeof(TextBlock),
			new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnTextWrappingChanged));

	static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> (d as TextBlock)?.ApplyTextWrapping();

	/// <summary>
	/// Gets or sets text wrapping. <see cref="TextWrapping.Wrap"/> breaks inside a word when it does not
	/// fit; <see cref="TextWrapping.WrapWithOverflow"/> only breaks between words.
	/// </summary>
	public TextWrapping TextWrapping
	{
		get => (TextWrapping)(GetValue(TextWrappingProperty) ?? TextWrapping.NoWrap);
		set => SetValue(TextWrappingProperty, value);
	}

	void ApplyTextWrapping()
	{
		if (_label == null) return;
		var (wrap, mode) = TextWrappingMap.ToLabel(TextWrapping);
		_label.Wrap = wrap;
		_label.WrapMode = mode;
	}
}
