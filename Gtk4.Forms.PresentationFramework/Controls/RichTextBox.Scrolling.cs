namespace System.Windows.Controls;

public partial class RichTextBox
{
	/// <summary>Horizontal scroll-bar visibility (the <see cref="ScrollViewer"/> property). Default: Hidden.</summary>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(RichTextBox),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollBarVisibilityChanged));

	/// <summary>Vertical scroll-bar visibility (the <see cref="ScrollViewer"/> property). Default: Hidden.</summary>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(RichTextBox),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollBarVisibilityChanged));

	static void OnScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((RichTextBox)d).ApplyScrollPolicy();

	/// <summary>Gets or sets the horizontal scroll-bar visibility.</summary>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get => (ScrollBarVisibility)(GetValue(HorizontalScrollBarVisibilityProperty) ?? ScrollBarVisibility.Hidden);
		set => SetValue(HorizontalScrollBarVisibilityProperty, value);
	}

	/// <summary>Gets or sets the vertical scroll-bar visibility.</summary>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get => (ScrollBarVisibility)(GetValue(VerticalScrollBarVisibilityProperty) ?? ScrollBarVisibility.Hidden);
		set => SetValue(VerticalScrollBarVisibilityProperty, value);
	}

	void ApplyScrollPolicy()
	{
		if (_scrolledWindow == null) return;
		_scrolledWindow.HscrollbarPolicy = ScrollViewer.ToGtkPolicy(HorizontalScrollBarVisibility);
		_scrolledWindow.VscrollbarPolicy = ScrollViewer.ToGtkPolicy(VerticalScrollBarVisibility);
	}
}
