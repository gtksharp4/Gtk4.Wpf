using System;

namespace System.Windows.Controls;

/// <summary>
/// A calendar control backed by <see cref="Gtk.Calendar"/> (GTK 4 native).
/// Two-way: <see cref="SelectedDate"/> ↔ GTK selection; <see cref="DisplayDate"/>
/// scrolls the view without changing selection.
/// </summary>
public class Calendar : Control
{
	// ── SelectedDate DP ──────────────────────────────────────────────────────

	/// <summary>The currently selected date, or <c>null</c> when none is selected.</summary>
	public static readonly DependencyProperty SelectedDateProperty =
		DependencyProperty.Register(
			nameof(SelectedDate),
			typeof(DateTime?),
			typeof(Calendar),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

	static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cal = (Calendar)d;
		cal.ApplySelectedDate((DateTime?)e.NewValue);
		// Raise event for any DP-originated change (widget not required).
		if (!cal._updating)
			cal.SelectedDatesChanged?.Invoke(cal, EventArgs.Empty);
	}

	/// <summary>Gets or sets the selected date.</summary>
	public DateTime? SelectedDate
	{
		get => (DateTime?)GetValue(SelectedDateProperty);
		set => SetValue(SelectedDateProperty, value);
	}

	// ── DisplayDate DP ───────────────────────────────────────────────────────

	/// <summary>The month/year shown in the calendar view.</summary>
	public static readonly DependencyProperty DisplayDateProperty =
		DependencyProperty.Register(
			nameof(DisplayDate),
			typeof(DateTime),
			typeof(Calendar),
			new FrameworkPropertyMetadata(DateTime.Today,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayDateChanged));

	static void OnDisplayDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Calendar)d).ApplyDisplayDate((DateTime)e.NewValue!);
	}

	/// <summary>Gets or sets the displayed month.</summary>
	public DateTime DisplayDate
	{
		get => (DateTime?)GetValue(DisplayDateProperty) ?? DateTime.Today;
		set => SetValue(DisplayDateProperty, value);
	}

	// ── SelectedDatesChanged event ────────────────────────────────────────────

	/// <summary>Raised when <see cref="SelectedDate"/> changes.</summary>
	public event EventHandler? SelectedDatesChanged;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Calendar? _cal;
	bool _updating;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_cal = new Gtk.Calendar();
		ApplyDisplayDate(DisplayDate);
		ApplySelectedDate(SelectedDate);
		_cal.DaySelected += OnGtkDaySelected;
		return _cal;
	}

	void ApplySelectedDate(DateTime? date)
	{
		if (_cal == null || _updating) return;
		_updating = true;
		try
		{
			if (date.HasValue)
				// GLib.DateTime month is 1-based; GTK Calendar Month prop is 0-based.
				_cal.SelectDay(GLib.DateTime.NewUtc(
					date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0.0));
			else
				_cal.SelectDay(null);
		}
		finally { _updating = false; }
	}

	void ApplyDisplayDate(DateTime date)
	{
		if (_cal == null) return;
		_cal.Year = date.Year;
		_cal.Month = date.Month - 1; // GTK Calendar Month is 0-based
	}

	void OnGtkDaySelected(object? sender, EventArgs e)
	{
		if (_updating) return;
		_updating = true;
		try
		{
			// Calendar.Year, Calendar.Month (0-based), Calendar.Day
			var newDate = new DateTime(_cal!.Year, _cal.Month + 1, _cal.Day);
			SetValue(SelectedDateProperty, (object?)newDate);
		}
		finally { _updating = false; }
		SelectedDatesChanged?.Invoke(this, EventArgs.Empty);
	}
}