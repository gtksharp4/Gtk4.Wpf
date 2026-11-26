using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>A column definition of a <see cref="Table"/>. Stored only: see <see cref="Table"/>.</summary>
public class TableColumn : FrameworkContentElement
{
	/// <summary>Identifies the Width property (default Auto).</summary>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
		nameof(Width), typeof(GridLength), typeof(TableColumn), new FrameworkPropertyMetadata(GridLength.Auto));

	/// <summary>Identifies the Background property (default null).</summary>
	public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
		nameof(Background), typeof(Brush), typeof(TableColumn), new FrameworkPropertyMetadata(null));

	/// <summary>Gets or sets the column width.</summary>
	public GridLength Width
	{
		get => (GridLength)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}

	/// <summary>Gets or sets the column background.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}
}
