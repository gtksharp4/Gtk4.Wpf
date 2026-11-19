using System;

namespace System.Windows.Controls;

public partial class ComboBox
{
	/// <summary>Default <see cref="MaxDropDownHeight"/>: WPF uses primary screen height / 3; 1080 / 3 here.</summary>
	internal const double DefaultMaxDropDownHeight = 360.0;

	/// <summary>Identifies <see cref="IsDropDownOpen"/>.</summary>
	public static readonly DependencyProperty IsDropDownOpenProperty =
		DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(ComboBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnIsDropDownOpenChanged));

	/// <summary>
	/// Identifies <see cref="MaxDropDownHeight"/>. Store-only: GtkDropDown sizes its popover itself.
	/// </summary>
	public static readonly DependencyProperty MaxDropDownHeightProperty =
		DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(ComboBox),
			new FrameworkPropertyMetadata(DefaultMaxDropDownHeight));

	/// <summary>Raised when the drop-down opens.</summary>
	public event EventHandler? DropDownOpened;

	/// <summary>Raised when the drop-down closes.</summary>
	public event EventHandler? DropDownClosed;

	/// <summary>Gets or sets whether the drop-down is open.</summary>
	public bool IsDropDownOpen
	{
		get => (bool)GetValue(IsDropDownOpenProperty)!;
		set => SetValue(IsDropDownOpenProperty, value);
	}

	/// <summary>Gets or sets the maximum drop-down height (store-only on GTK).</summary>
	public double MaxDropDownHeight
	{
		get => (double)GetValue(MaxDropDownHeightProperty)!;
		set => SetValue(MaxDropDownHeightProperty, value);
	}

	/// <summary>Raises <see cref="DropDownOpened"/>.</summary>
	protected virtual void OnDropDownOpened(EventArgs e) => DropDownOpened?.Invoke(this, e);

	/// <summary>Raises <see cref="DropDownClosed"/>.</summary>
	protected virtual void OnDropDownClosed(EventArgs e) => DropDownClosed?.Invoke(this, e);

	static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		var open = (bool)e.NewValue!;
		cb.PushDropDownState(open);
		if (open) cb.OnDropDownOpened(EventArgs.Empty);
		else cb.OnDropDownClosed(EventArgs.Empty);
	}

	// ── GTK popover sync ─────────────────────────────────────────────────────

	Gtk.Popover? _popover;
	bool _syncingPopover;

	/// <summary>
	/// The DropDown's internal popover is found by walking its children. If it is not
	/// reachable, closing from code is store-only.
	/// </summary>
	void HookPopover()
	{
		for (var child = _dropDown.FirstChild; child != null; child = child.NextSibling)
		{
			if (child is Gtk.Popover p)
			{
				_popover = p;
				p.AddNotification("visible", OnPopoverVisibleChanged);
				return;
			}
		}
	}

	void PushDropDownState(bool open)
	{
		if (_dropDown == null || _syncingPopover) return;
		_syncingPopover = true;
		try
		{
			if (open) _popover?.Popup();
			else _popover?.Popdown();
		}
		finally { _syncingPopover = false; }
	}

	void OnPopoverVisibleChanged(object? sender, GLib.NotifyArgs e)
	{
		if (_syncingPopover || _popover == null) return;
		var visible = _popover.Visible;
		if (visible != IsDropDownOpen)
			SetValue(IsDropDownOpenProperty, visible);
	}
}
