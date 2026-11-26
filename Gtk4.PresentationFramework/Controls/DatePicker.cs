using System;

namespace System.Windows.Controls;

/// <summary>
/// A date-picker control: a text entry showing the selected date next to a
/// calendar-popover button.
/// <para>
/// GTK4 backing: a horizontal <see cref="Gtk.Box"/> containing a
/// <see cref="Gtk.Entry"/> (editable text) and a <see cref="Gtk.MenuButton"/>
/// whose popover holds a <see cref="Gtk.Calendar"/>.
/// </para>
/// Two-way: <see cref="SelectedDate"/> ↔ GTK calendar selection;
/// <see cref="Text"/> reflects the formatted date string.
/// </summary>
public class DatePicker : Control
{
	static readonly string _format = "yyyy-MM-dd";

	// ── SelectedDate DP ──────────────────────────────────────────────────────

	/// <summary>The selected date, or <c>null</c> when none is selected.</summary>
	public static readonly DependencyProperty SelectedDateProperty =
		DependencyProperty.Register(
			nameof(SelectedDate),
			typeof(DateTime?),
			typeof(DatePicker),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

	static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var dp = (DatePicker)d;
		dp.ApplySelectedDate((DateTime?)e.NewValue);
		// Raise event for DP-originated changes (widget not required).
		if (!dp._updating)
			dp.SelectedDateChanged?.Invoke(dp, EventArgs.Empty);
	}

	/// <summary>Gets or sets the selected date.</summary>
	public DateTime? SelectedDate
	{
		get => (DateTime?)GetValue(SelectedDateProperty);
		set => SetValue(SelectedDateProperty, value);
	}

	// ── Text DP ──────────────────────────────────────────────────────────────

	/// <summary>Text representation of the selected date (yyyy-MM-dd).</summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(DatePicker),
			new PropertyMetadata(string.Empty));

	/// <summary>Gets the formatted date text.</summary>
	public string Text
	{
		get => (string?)GetValue(TextProperty) ?? string.Empty;
		set => SetValue(TextProperty, value);
	}

	// ── SelectedDateChanged event ─────────────────────────────────────────────

	/// <summary>Raised when <see cref="SelectedDate"/> changes.</summary>
	public event EventHandler? SelectedDateChanged;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Box? _box;
	Gtk.Entry? _entry;
	Gtk.Calendar? _cal;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Horizontal, 4);

		_entry = new Gtk.Entry();
		_entry.PlaceholderText = _format;
		_entry.Hexpand = true;
		_entry.Changed += OnEntryChanged;

		_cal = new Gtk.Calendar();
		_cal.DaySelected += OnCalDaySelected;

		var popover = new Gtk.Popover();
		popover.Child = _cal;

		var btn = new Gtk.MenuButton();
		btn.Popover = popover;
		btn.IconName = "x-office-calendar-symbolic";

		_box.Append(_entry);
		_box.Append(btn);

		ApplySelectedDate(SelectedDate);
		return _box;
	}

	void ApplySelectedDate(DateTime? date)
	{
		if (_updating) return;
		_updating = true;
		try
		{
			var text = date.HasValue ? date.Value.ToString(_format) : string.Empty;
			SetValue(TextProperty, text);
			if (_entry != null) _entry.Text = text;
			if (_cal != null && date.HasValue)
				_cal.SelectDay(GLib.DateTime.NewUtc(
					date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0.0));
		}
		finally { _updating = false; }
	}

	void OnCalDaySelected(object? sender, EventArgs e)
	{
		if (_updating) return;
		_updating = true;
		try
		{
			var newDate = new DateTime(_cal!.Year, _cal.Month + 1, _cal.Day);
			var text = newDate.ToString(_format);
			SetValue(SelectedDateProperty, (object?)newDate);
			SetValue(TextProperty, text);
			if (_entry != null) _entry.Text = text;
		}
		finally { _updating = false; }
		SelectedDateChanged?.Invoke(this, EventArgs.Empty);
	}

	void OnEntryChanged(object? sender, EventArgs e)
	{
		if (_updating) return;
		var text = _entry?.Text ?? string.Empty;
		if (DateTime.TryParse(text, out var dt))
		{
			_updating = true;
			try
			{
				SetValue(SelectedDateProperty, (object?)dt);
				SetValue(TextProperty, text);
				if (_cal != null)
					_cal.SelectDay(GLib.DateTime.NewUtc(dt.Year, dt.Month, dt.Day, 0, 0, 0.0));
			}
			finally { _updating = false; }
			SelectedDateChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}