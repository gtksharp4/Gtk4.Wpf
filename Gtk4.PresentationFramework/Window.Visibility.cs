using System;

namespace System.Windows;

/// <summary>
/// Window show state as <see cref="UIElement.Visibility"/>, as in WPF: the default is
/// <see cref="Visibility.Collapsed"/>, <see cref="Show"/> sets Visible, <see cref="Hide"/> sets Hidden,
/// and setting the property shows or hides the window without closing it. A shown window is the
/// root of <see cref="UIElement.IsVisible"/> for its content.
/// </summary>
public partial class Window
{
	// The presentation-source analogue: true from Present() until hidden or closed.
	bool _shown;

	const string ClosedMessage =
		"Cannot set Visibility or call Show, ShowDialog, or WindowInteropHelper.EnsureHandle "
		+ "after a Window has closed.";

	void VerifyNotClosed()
	{
		if (_closed) throw new InvalidOperationException(ClosedMessage);
	}

	readonly Application? _application;

	/// <summary>
	/// Registers the window with <see cref="Application.Current"/> (its <see cref="Application.Windows"/>
	/// list, and <see cref="Application.MainWindow"/> if unset) when one exists on this thread.
	/// </summary>
	public Window()
	{
		_application = Application.Current;
		_application?.RegisterWindow(this);
		InitDefaultButtons();
	}

	static Window()
	{
		VisibilityProperty.OverrideMetadata(typeof(Window),
			new PropertyMetadata(Visibility.Collapsed, null, CoerceWindowVisibility));
	}

	// WPF coerces before storing, so a rejected Visible leaves the stored value untouched.
	static object? CoerceWindowVisibility(DependencyObject d, object? value)
	{
		if (value is Visibility visibility && visibility == Visibility.Visible)
			((Window)d).VerifyNotClosed();
		return value;
	}

	/// <summary>Shows the window (sets <see cref="UIElement.Visibility"/> to Visible).</summary>
	public void Show()
	{
		VerifyNotClosed();
		if (Visibility == Visibility.Visible)
			ShowCore();
		else
			Visibility = Visibility.Visible;
	}

	/// <summary>Hides the window without closing it (sets <see cref="UIElement.Visibility"/> to Hidden).</summary>
	public void Hide() => Visibility = Visibility.Hidden;

	internal override void OnVisibilityChangedCore(Visibility visibility)
	{
		if (visibility == Visibility.Visible)
		{
			ShowCore();
		}
		else if (_window != null)
		{
			_window.Hide();
			SetShown(false);
		}
	}

	// A top-level's show state is not content suppression: its widget mapping is always Visible.
	internal override Visibility WidgetVisibility => Visibility.Visible;

	// A top-level's GTK visibility is the show state, not Opacity/CanTarget suppression.
	internal override void ApplyVisibilityToWidget(Gtk.Widget widget) { }

	internal override bool ComputeIsVisible() => Visibility == Visibility.Visible && _shown;

	void SetShown(bool shown)
	{
		if (_shown == shown) return;
		_shown = shown;
		RecomputeIsVisible();
	}
}
