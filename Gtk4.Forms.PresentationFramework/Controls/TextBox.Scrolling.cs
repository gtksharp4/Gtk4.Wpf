using System;

namespace System.Windows.Controls;

public partial class TextBox
{
	/// <summary>Horizontal scroll-bar visibility (the <see cref="ScrollViewer"/> property). Default: Hidden.</summary>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(TextBox),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollBarVisibilityChanged));

	/// <summary>Vertical scroll-bar visibility (the <see cref="ScrollViewer"/> property). Default: Hidden.</summary>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(TextBox),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnScrollBarVisibilityChanged));

	static void OnScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBox)d).ApplyScrollPolicy();

	/// <summary>
	/// Gets or sets the horizontal scroll-bar visibility. Applied to the multiline backing only;
	/// a single-line <see cref="Gtk.Entry"/> has no scroll bars, so the value is stored.
	/// </summary>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get => (ScrollBarVisibility)(GetValue(HorizontalScrollBarVisibilityProperty) ?? ScrollBarVisibility.Hidden);
		set => SetValue(HorizontalScrollBarVisibilityProperty, value);
	}

	/// <summary>
	/// Gets or sets the vertical scroll-bar visibility. Applied to the multiline backing only;
	/// a single-line <see cref="Gtk.Entry"/> has no scroll bars, so the value is stored.
	/// </summary>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get => (ScrollBarVisibility)(GetValue(VerticalScrollBarVisibilityProperty) ?? ScrollBarVisibility.Hidden);
		set => SetValue(VerticalScrollBarVisibilityProperty, value);
	}

	bool IsMultiline => AcceptsReturn || TextWrapping != TextWrapping.NoWrap;

	static void OnTextWrappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBox)d).ApplyWrapMode();

	void ApplyScrollPolicy()
	{
		if (_scrolled == null) return;
		_scrolled.HscrollbarPolicy = ScrollViewer.ToGtkPolicy(HorizontalScrollBarVisibility);
		_scrolled.VscrollbarPolicy = ScrollViewer.ToGtkPolicy(VerticalScrollBarVisibility);
	}

	/// <summary>Appends <paramref name="text"/> to the end of <see cref="Text"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
	public void AppendText(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		if (text.Length > 0) Text += text;
	}

	/// <summary>Scrolls to the start of the content. No-op for the single-line backing.</summary>
	public void ScrollToHome() => ScrollTo(toStart: true);

	/// <summary>Scrolls to the end of the content. No-op for the single-line backing.</summary>
	public void ScrollToEnd() => ScrollTo(toStart: false);

	void ScrollTo(bool toStart)
	{
		if (_textView == null) return;
		var buffer = _textView.Buffer;
		_textView.ScrollToIter(toStart ? buffer.StartIter : buffer.EndIter, 0.0, false, 0.0, 0.0);
	}
}
