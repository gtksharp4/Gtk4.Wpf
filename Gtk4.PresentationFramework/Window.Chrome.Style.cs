namespace System.Windows;

public partial class Window
{
	// ── ResizeMode ────────────────────────────────────────────────────────────

	/// <summary>Gets or sets how the user can resize the window.</summary>
	public static readonly DependencyProperty ResizeModeProperty =
		DependencyProperty.Register(
			nameof(ResizeMode),
			typeof(ResizeMode),
			typeof(Window),
			new PropertyMetadata(ResizeMode.CanResize, OnResizeModeChanged));

	static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var w = (Window)d;
		if (w._window != null)
			ApplyResizeMode(w._window, w._headerBar, (ResizeMode)e.NewValue!);
	}

	/// <summary>
	/// Gets or sets whether and how the user can resize the window.
	/// </summary>
	/// <remarks>
	/// GTK4 limitation: GTK4 has no per-button title-bar visibility API.
	/// <see cref="ResizeMode.NoResize"/> hides all window-manager buttons via
	/// <c>HeaderBar.ShowTitleButtons = false</c> (min + max + close together).
	/// <see cref="ResizeMode.CanMinimize"/> maps to non-resizable with buttons shown
	/// (maximize button is visible but ineffective — GTK4 honours <c>Resizable</c>
	/// in its own decoration and prevents user resizing, but the button remains).
	/// <see cref="ResizeMode.CanResizeWithGrip"/> maps identically to
	/// <see cref="ResizeMode.CanResize"/>; GTK4 manages resize grips natively.
	/// </remarks>
	public ResizeMode ResizeMode
	{
		get => (ResizeMode)(GetValue(ResizeModeProperty) ?? ResizeMode.CanResize);
		set => SetValue(ResizeModeProperty, value);
	}

	static void ApplyResizeMode(Gtk.Window win, Gtk.HeaderBar? hb, ResizeMode mode)
	{
		switch (mode)
		{
			case ResizeMode.NoResize:
				win.Resizable = false;
				if (hb != null) hb.ShowTitleButtons = false;
				break;
			case ResizeMode.CanMinimize:
				win.Resizable = false;
				if (hb != null) hb.ShowTitleButtons = true;
				break;
			case ResizeMode.CanResizeWithGrip:
			case ResizeMode.CanResize:
			default:
				win.Resizable = true;
				if (hb != null) hb.ShowTitleButtons = true;
				break;
		}
	}

	// ── WindowStyle ───────────────────────────────────────────────────────────

	/// <summary>Gets or sets the border and title-bar style of the window.</summary>
	public static readonly DependencyProperty WindowStyleProperty =
		DependencyProperty.Register(
			nameof(WindowStyle),
			typeof(WindowStyle),
			typeof(Window),
			new PropertyMetadata(WindowStyle.SingleBorderWindow, OnWindowStyleChanged));

	static void OnWindowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var w = (Window)d;
		w.ApplyShellChrome();
	}

	/// <summary>
	/// Gets or sets the border and title-bar appearance of the window.
	/// </summary>
	/// <remarks>
	/// GTK4 limitation: only <see cref="WindowStyle.None"/> (Decorated = false) is
	/// distinguishable. <see cref="WindowStyle.SingleBorderWindow"/>,
	/// <see cref="WindowStyle.ThreeDBorderWindow"/> and <see cref="WindowStyle.ToolWindow"/>
	/// all map to <c>Gtk.Window.Decorated = true</c> — GTK4 uses native platform decoration
	/// only and exposes no border-variant or reduced-size title-bar API.
	/// </remarks>
	public WindowStyle WindowStyle
	{
		get => (WindowStyle)(GetValue(WindowStyleProperty) ?? WindowStyle.SingleBorderWindow);
		set => SetValue(WindowStyleProperty, value);
	}
}
