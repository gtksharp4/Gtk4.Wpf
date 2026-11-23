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
	/// When true, string content is shown with an access key: GTK <c>Label.UseUnderline</c>, so "_File" shows
	/// "File" with a mnemonic and "__" shows a literal underscore. When false, underscores stay literal.
	/// </summary>
	public bool RecognizesAccessKey
	{
		get => (bool)GetValue(RecognizesAccessKeyProperty)!;
		set => SetValue(RecognizesAccessKeyProperty, value);
	}
}
