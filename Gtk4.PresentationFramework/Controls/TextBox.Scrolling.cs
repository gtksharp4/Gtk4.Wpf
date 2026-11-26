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

	Gtk.TextMark? _scrollMark;
	bool _scrollToStart;
	uint _scrollRepeat;

	bool IsMultiline => AcceptsReturn;

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
		if (text.Length == 0) return;
		// Over MaxLength the DP clamp must run, so take the normal (rewriting) path.
		if (_textView == null || (MaxLength > 0 && Text.Length + text.Length > MaxLength))
		{
			Text += text;
			return;
		}
		// Insert at the end so the caret and undo history survive; _updating skips the buffer rewrite.
		_updating = true;
		try
		{
			var end = _textView.Buffer.EndIter;
			_textView.Buffer.Insert(ref end, text);
			SetValue(TextProperty, _textView.Buffer.Text);
		}
		finally { _updating = false; }
	}

	/// <summary>Scrolls to the start of the content. No-op for the single-line backing.</summary>
	public void ScrollToHome() => ScrollTo(toStart: true);

	/// <summary>Scrolls to the end of the content. No-op for the single-line backing.</summary>
	public void ScrollToEnd() => ScrollTo(toStart: false);

	// A mark, not an iter: GTK applies a mark scroll after line heights are validated. GTK can still
	// apply it against heights estimated before the whole buffer is laid out, so it is repeated once the
	// main loop is idle (validation runs at a higher priority).
	void ScrollTo(bool toStart)
	{
		if (_textView == null) return;
		_scrollToStart = toStart;
		ScrollToMark();
		if (_scrollRepeat != 0) return;
		_scrollRepeat = GLib.Idle.Add(GLib.Priority.DefaultIdle, () =>
		{
			_scrollRepeat = 0;
			ScrollToMark();
			return false;
		});
	}

	void ScrollToMark()
	{
		if (_textView == null) return;
		var buffer = _textView.Buffer;
		var iter = _scrollToStart ? buffer.StartIter : buffer.EndIter;
		if (_scrollMark == null)
			_scrollMark = buffer.CreateMark(null, iter, false);
		else
			buffer.MoveMark(_scrollMark, iter);
		_textView.ScrollToMark(_scrollMark, 0.0, true, 0.0, _scrollToStart ? 0.0 : 1.0);
	}
}
