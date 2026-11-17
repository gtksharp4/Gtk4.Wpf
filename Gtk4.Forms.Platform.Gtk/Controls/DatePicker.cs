using System;
using Gtk4.Forms.Platform.GTK.Extensions;
using System.Linq;
using Gtk;

namespace Gtk4.Forms.Platform.GTK.Controls;

public class DateEventArgs : EventArgs
{
	DateTime _date;

	public DateTime Date
	{
		get
		{
			return _date;
		}
	}

	public DateEventArgs(DateTime date)
	{
		_date = date;
	}
}

public partial class DatePickerWindow : Window
{
	Box _datebox;
	RangeCalendar _calendar;

	public DatePickerWindow()
		: base()
	{
		BuildDatePickerWindow();
		// GTK4: device grabs (gtk_grab_add / gdk_seat_grab) are removed.
		// A plain undecorated window without a grab is the idiomatic approach
		// for a lightweight inline picker; a Gtk.Popover would be the full GTK4
		// idiom but requires a relative-to widget, which the renderer does not
		// supply here.  The window dismisses itself on button-press outside the
		// calendar, which replicates the previous close-on-click behaviour.
		SelectedDate = DateTime.Now;
	}

	public DateTime SelectedDate
	{
		get
		{
			return GLibDateToSystem(_calendar.Date);
		}

		set
		{
			_calendar.Date = SystemDateToGLib(value);
		}
	}

	public DateTime MinimumDate
	{
		get
		{
			return _calendar.MinimumDate;
		}

		set
		{
			_calendar.MinimumDate = value;
		}
	}

	public DateTime MaximumDate
	{
		get
		{
			return _calendar.MaximumDate;
		}

		set
		{
			_calendar.MaximumDate = value;
		}
	}

	public delegate void DateEventHandler(object sender, DateEventArgs args);

	public event DateEventHandler OnDateTimeChanged;

	protected override bool OnDrawn(Cairo.Context cr)
	{
		base.OnDrawn(cr);

		int winWidth = Width;
		int winHeight = Height;

		// GTK3 removed Gdk.GC drawing (Style.ForegroundGC + GdkWindow.DrawRectangle).
		// Stroke the 1px frame with Cairo instead, using the theme's foreground colour.
		// GTK4: StyleContext.GetColor(StateFlags) is gone; Color property replaces it.
		var color = StyleContext.Color;
		cr.SetSourceRGBA(color.Red, color.Green, color.Blue, color.Alpha);
		cr.LineWidth = 1;
		cr.Rectangle(0.5, 0.5, winWidth - 1, winHeight - 1);
		cr.Stroke();

		return false;
	}

	protected virtual void OnButtonPressEvent(object o, ButtonPressEventArgs args)
	{
		Close();
	}

	protected virtual void OnCalendarButtonPressEvent(object o, ButtonPressEventArgs args)
	{
		args.RetVal = true;
	}

	protected virtual void OnCalendarDaySelected(object sender, EventArgs e)
	{
		OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
	}

	// GTK4: GtkCalendar removed the day-selected-double-click signal.
	// TODO: consider using a GtkGestureClick with n-press=2 on the calendar
	// to restore close-on-double-click behaviour once the gesture API is wired.

	void BuildDatePickerWindow()
	{
		Title = "DatePicker";
		// GTK4: TypeHint (GdkWindowTypeHint), WindowPosition and SkipPager/TaskbarHint
		// are all removed.  An undecorated non-resizable window is the nearest equivalent.
		BorderWidth = 1;
		Resizable = false;
		Decorated = false;
		DestroyWithParent = true;

		_datebox = new Box(Gtk.Orientation.Vertical, 0);
		_datebox.Spacing = 6;
		_datebox.BorderWidth = 3;

		_calendar = new RangeCalendar();
		_calendar.CanFocus = true;
		// GTK4: CalendarDisplayOptions is gone; individual bool properties replace it.
		_calendar.ShowHeading = true;
		_datebox.Add(_calendar);
		Box.BoxChild dateBoxChild = ((Box.BoxChild)(_datebox[_calendar]));
		dateBoxChild.Position = 0;

		Add(_datebox);

		if ((Child != null))
		{
			Child.ShowAll();
		}

		Show();

		ButtonPressEvent += new ButtonPressEventHandler(OnButtonPressEvent);
		_calendar.ButtonPressEvent += new ButtonPressEventHandler(OnCalendarButtonPressEvent);
		_calendar.DaySelected += new EventHandler(OnCalendarDaySelected);
		// DaySelectedDoubleClick: removed in GTK4 (see TODO above).
	}

	// GTK3 Gtk.Window gained a Close() method; this dismisses the popup, not the window.
	// internal so DatePicker.ClosePicker can dismiss it - see the comment there.
	internal new void Close()
	{
		// GTK4: no grab to release; just destroy.
		Destroy();
	}

	void NotifyDateChanged()
	{
		OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
	}

	// GTK4: Calendar.Date is GLib.DateTime; convert to/from System.DateTime.
	// GLib months are 1-12, matching System.DateTime - no offset needed.
	static DateTime GLibDateToSystem(GLib.DateTime d)
	{
		if (d == null)
			return DateTime.Now;
		return new DateTime(d.Year, d.Month, d.DayOfMonth);
	}

	static GLib.DateTime SystemDateToGLib(DateTime d)
	{
		return new GLib.DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
	}

	class RangeCalendar : Calendar
	{
		DateTime _minimumDate;
		DateTime _maximumDate;

		public RangeCalendar()
		{
			_minimumDate = new DateTime(1900, 1, 1);
			_maximumDate = new DateTime(2199, 1, 1);
		}

		public DateTime MinimumDate
		{
			get
			{
				return _minimumDate;
			}

			set
			{
				if (MaximumDate < value)
				{
					throw new InvalidOperationException($"{nameof(MinimumDate)} must be lower than {nameof(MaximumDate)}");
				}

				_minimumDate = value;
			}
		}

		public DateTime MaximumDate
		{
			get
			{
				return _maximumDate;
			}

			set
			{
				if (MinimumDate > value)
				{
					throw new InvalidOperationException($"{nameof(MaximumDate)} must be greater than {nameof(MinimumDate)}");
				}

				_maximumDate = value;
			}
		}

		protected override void OnDaySelected()
		{
			var current = DatePickerWindow.GLibDateToSystem(Date);

			if (current < MinimumDate)
			{
				Date = DatePickerWindow.SystemDateToGLib(MinimumDate);
			}

			if (current > MaximumDate)
			{
				Date = DatePickerWindow.SystemDateToGLib(MaximumDate);
			}
		}
	}
}

public partial class DatePicker : EventBox
{
	CustomComboBox _comboBox;
	Gdk.Color _color;
	DateTime _currentDate;
	DateTime _minDate;
	DateTime _maxDate;
	string _dateFormat;

	public event EventHandler DateChanged;
	public event EventHandler GotFocus;
	public event EventHandler LostFocus;

	public DatePicker()
	{
		BuildDatePicker();

		CurrentDate = DateTime.Now;

		TextColor = _comboBox.Entry.GetDefaultTextColor(Gtk.StateFlags.Normal);

		// GTK4: Entry.CanDefault is removed (default-activation is handled differently).
		_comboBox.Entry.CanFocus = false;
		_comboBox.Entry.IsEditable = false;
		_comboBox.Entry.SetStateFlags(StateFlags.Normal, true);
		// GTK4: Entry.FocusGrabbed is removed; FocusInEvent (compat) replaces it.
		_comboBox.Entry.FocusInEvent += new FocusInEventHandler((o, e) => OnEntryFocused(o, EventArgs.Empty));
		_comboBox.PopupButton.Clicked += new EventHandler(OnBtnShowCalendarClicked);
	}

	public DateTime CurrentDate
	{
		get
		{
			return _currentDate;
		}
		set
		{
			if (_currentDate == value)
				return;

			_currentDate = value;
			UpdateEntryText();

			// Raised here rather than only from the calendar popup, which is what the sibling
			// Controls.TimePicker effectively does (its CurrentTime setter retexts the entry,
			// whose Changed handler raises TimeChanged). Two things were wrong without it:
			// DatePickerRenderer.OnDateChanged - the whole native-to-element direction - never
			// fired for any change that did not come from the popup, and OnPopupDateChanged's
			// two clamping branches returned before their explicit Invoke, so picking a date
			// outside Min/MaxDate moved the control and left the element stale.
			//
			// The guard above is what keeps this from echoing: the renderer's UpdateDate sets
			// this property from the element, so the round trip back through
			// SetValueFromRenderer must settle, and an unchanged value now stops at the door.
			DateChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public DateTime MinDate
	{
		get
		{
			return _minDate;
		}
		set
		{
			_minDate = value;
		}
	}

	public DateTime MaxDate
	{
		get
		{
			return _maxDate;
		}
		set
		{
			_maxDate = value;
		}
	}

	public Gdk.Color TextColor
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			_comboBox.Color = _color;
		}
	}

	public string DateFormat
	{
		get
		{
			return _dateFormat;
		}
		set
		{
			_dateFormat = value;
			UpdateEntryText();
		}
	}

	public void SetBackgroundColor(Gdk.Color color)
	{
		_comboBox.SetBackgroundColor(color);
	}

	public void OpenPicker()
	{
		ShowPickerWindow();
	}

	public void ClosePicker()
	{
		var windows = Gtk.Window.ListToplevels();
		var window = windows.FirstOrDefault(w => w.GetType() == typeof(DatePickerWindow))
			as DatePickerWindow;

		// Close(), not Remove(). DatePicker is a Gtk.EventBox, so Remove(window) was
		// gtk_container_remove against a TOPLEVEL that was never its child: GTK logged a
		// critical and did nothing, so the popup stayed on screen and kept its grab while
		// ClosePicker still reported success to Forms' Unfocus(). Close() destroys the window.
		if (window != null)
		{
			window.Close();
		}
	}

	void ShowPickerWindow()
	{
		// GTK4: gtk_window_get_origin and gtk_window_move are removed; the window manager
		// controls toplevel positions.  The picker appears at the default position.
		// TODO: position relative to the entry using Gtk.Popover once a parent widget
		// reference is available to set as the popover's relative-to.
		var picker = new DatePickerWindow();
		picker.SelectedDate = CurrentDate;
		picker.MinimumDate = _minDate;
		picker.MaximumDate = _maxDate;
		picker.OnDateTimeChanged += OnPopupDateChanged;
		picker.CloseRequest += new Gtk.CloseRequestHandler(OnPickerCloseRequest);
	}

	void OnPopupDateChanged(object sender, DateEventArgs args)
	{
		var date = args.Date;

		if (date < MinDate)
		{
			CurrentDate = MinDate;
			return;
		}

		if (date > MaxDate)
		{
			CurrentDate = MaxDate;
			return;
		}

		// The setter raises DateChanged, including for the two clamped paths above.
		CurrentDate = args.Date;
	}

	void BuildDatePicker()
	{
		_comboBox = new CustomComboBox();
		Add(_comboBox);

		if ((Child != null))
		{
			Child.ShowAll();
		}

		Show();
	}

	void UpdateEntryText()
	{
		_comboBox.Entry.Text = _currentDate.ToString(string.IsNullOrEmpty(_dateFormat) ? "D" : _dateFormat);
	}

	void OnBtnShowCalendarClicked(object sender, EventArgs e)
	{
		ShowPickerWindow();
	}

	void OnEntryFocused(object sender, EventArgs e)
	{
		ShowPickerWindow();
		GotFocus?.Invoke(this, EventArgs.Empty);
	}

	void OnPickerCloseRequest(object sender, Gtk.CloseRequestArgs e)
	{
		// GTK4: Destroyed event is removed; CloseRequest replaces it for cleanup.
		// The old OnPickerClosed called Remove(window), which was always wrong for a
		// toplevel - the renderer already ignores that; we just raise LostFocus here.
		LostFocus?.Invoke(this, EventArgs.Empty);
	}
}