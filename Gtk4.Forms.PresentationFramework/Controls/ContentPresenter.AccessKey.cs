namespace System.Windows.Controls;

public partial class ContentPresenter
{
	/// <summary>
	/// Identifies the <see cref="RecognizesAccessKey"/> dependency property (default false).
	/// </summary>
	public static readonly DependencyProperty RecognizesAccessKeyProperty =
		DependencyProperty.Register(
			"RecognizesAccessKey",
			typeof(bool),
			typeof(ContentPresenter),
			new FrameworkPropertyMetadata(false, (d, _) => ((ContentPresenter)d).RefreshContent()));

	/// <summary>
	/// When true, string content containing an underscore is hosted in an <see cref="AccessText"/>: "_File" shows
	/// "File" with an access key and "__" shows a literal underscore. When false, underscores stay literal.
	/// </summary>
	public bool RecognizesAccessKey
	{
		get => (bool)GetValue(RecognizesAccessKeyProperty)!;
		set => SetValue(RecognizesAccessKeyProperty, value);
	}
}
