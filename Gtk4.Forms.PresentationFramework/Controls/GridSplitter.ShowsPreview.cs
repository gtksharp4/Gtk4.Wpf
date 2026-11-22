namespace System.Windows.Controls;

public partial class GridSplitter
{
	/// <summary>Identifies <see cref="ShowsPreview"/>. Default false.</summary>
	public static readonly DependencyProperty ShowsPreviewProperty = DependencyProperty.Register(
		nameof(ShowsPreview), typeof(bool), typeof(GridSplitter), new FrameworkPropertyMetadata(false));

	/// <summary>
	/// Gets or sets whether the resize is previewed and applied on release. Stored only: the splitter always
	/// resizes live.
	/// </summary>
	public bool ShowsPreview
	{
		get => (bool)GetValue(ShowsPreviewProperty)!;
		set => SetValue(ShowsPreviewProperty, value);
	}
}
