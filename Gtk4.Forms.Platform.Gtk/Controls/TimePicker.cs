using System;
using Xamarin.Forms.Platform.GTK.Extensions;
using System.Linq;

namespace Xamarin.Forms.Platform.GTK.Controls;

public class TimeEventArgs : EventArgs
{
	TimeSpan _time;

	public TimeSpan Time
	{
		get
		{
			return _time;
		}
	}

	public TimeEventArgs(TimeSpan time)
	{
		_time = time;
	}
}

public class TimePickerWindow : Gtk.Window
{
	Gtk.Box _timeBox;
	Gtk.Label _labelHour;
	Gtk.SpinButton _txtHour;
	Gtk.Label _labelMin;
	Gtk.SpinButton _txtMin;
	Gtk.Label _labelSec;
	Gtk.SpinButton _txtSec;

	public TimePickerWindow()
		: base()
	{
		BuildTimePickerWindow();
		// GTK4: device grabs (gtk_grab_add / gdk_seat_grab) are removed.
		// See the equivalent note in DatePickerWindow.

		RefreshTime();
	}

	public TimeSpan CurrentTime
	{
		get
		{
			return new TimeSpan((int)_txtHour.Value, (int)_txtMin.Value, (int)_txtSec.Value);
		}

		set
		{
			_txtHour.Value = value.Hours;
			_txtMin.Value = value.Minutes;
			_txtSec.Value = value.Seconds;
		}
	}

	public delegate void TimeEventHandler(object sender, TimeEventArgs args);

	public event TimeEventHandler OnTimeChanged;

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

	protected virtual void OnButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
	{
		Close();
	}

	void BuildTimePickerWindow()
	{
		Title = "TimePicker";
		// GTK4: TypeHint (GdkWindowTypeHint), WindowPosition and SkipPager/TaskbarHint
		// are all removed.  An undecorated non-resizable window is the nearest equivalent.
		BorderWidth = 1;
		Resizable = false;
		Decorated = false;
		DestroyWithParent = true;

		_timeBox = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
		_timeBox.Spacing = 6;
		_timeBox.BorderWidth = 3;

		_labelHour = new Gtk.Label();
		_labelHour.LabelProp = "H:";
		_timeBox.Add(_labelHour);

		Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(_timeBox[_labelHour]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;

		_txtHour = new Gtk.SpinButton(0D, 24D, 1D);
		_txtHour.CanFocus = true;
		_txtHour.Adjustment.PageIncrement = 1D;
		_txtHour.ClimbRate = 1D;
		_txtHour.Numeric = true;
		_timeBox.Add(_txtHour);

		Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(_timeBox[_txtHour]));
		w3.Position = 1;
		w3.Expand = false;
		w3.Fill = false;

		_labelMin = new Gtk.Label();
		_labelMin.LabelProp = "M:";
		_timeBox.Add(_labelMin);
		Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(_timeBox[_labelMin]));

		w4.Position = 2;
		w4.Expand = false;
		w4.Fill = false;

		_txtMin = new Gtk.SpinButton(0D, 60D, 1D);
		_txtMin.CanFocus = true;
		_txtMin.Adjustment.PageIncrement = 10D;
		_txtMin.ClimbRate = 1D;
		_txtMin.Numeric = true;
		_timeBox.Add(_txtMin);

		Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(_timeBox[_txtMin]));
		w5.Position = 3;
		w5.Expand = false;
		w5.Fill = false;

		_labelSec = new Gtk.Label();
		_labelSec.LabelProp = "S:";
		_timeBox.Add(_labelSec);
		Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(_timeBox[_labelSec]));
		w6.Position = 4;
		w6.Expand = false;
		w6.Fill = false;

		_txtSec = new Gtk.SpinButton(0D, 60D, 1D);
		_txtSec.CanFocus = true;
		_txtSec.Adjustment.PageIncrement = 10D;
		_txtSec.ClimbRate = 1D;
		_txtSec.Numeric = true;
		_timeBox.Add(_txtSec);

		Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(_timeBox[_txtSec]));
		w7.Position = 5;
		w7.Expand = false;
		w7.Fill = false;

		Add(_timeBox);

		if ((Child != null))
		{
			Child.ShowAll();
		}

		Show();

		ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnButtonPressEvent);
		_txtHour.ValueChanged += new EventHandler(OnTxtHourValueChanged);
		_txtHour.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtHourButtonPressEvent);
		_txtMin.ValueChanged += new EventHandler(OnTxtMinValueChanged);
		_txtMin.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtMinButtonPressEvent);
		_txtSec.ValueChanged += new EventHandler(OnTxtSecValueChanged);
		_txtSec.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtSecButtonPressEvent);
	}

	// GTK3 Gtk.Window gained a Close() method; this dismisses the popup, not the window.
	// internal so TimePicker.ClosePicker can dismiss it - see the comment there.
	internal new void Close()
	{
		// GTK4: no grab to release; just destroy.
		Destroy();
	}

	void RefreshTime()
	{
		OnTimeChanged?.Invoke(this, new TimeEventArgs(CurrentTime));
	}

	protected virtual void OnTxtHourValueChanged(object sender, EventArgs e)
	{
		if (_txtHour.Value == 24)
			_txtHour.Value = 0;

		RefreshTime();
	}

	protected virtual void OnTxtMinValueChanged(object sender, EventArgs e)
	{
		if (_txtMin.Value == 60)
			_txtMin.Value = 0;

		RefreshTime();
	}

	protected virtual void OnTxtSecValueChanged(object sender, EventArgs e)
	{
		if (_txtSec.Value == 60)
			_txtSec.Value = 0;

		RefreshTime();
	}

	protected virtual void OnTxtHourButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
	{
		args.RetVal = true;
	}

	protected virtual void OnTxtMinButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
	{
		args.RetVal = true;
	}

	protected virtual void OnTxtSecButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
	{
		args.RetVal = true;
	}
}

public class TimePicker : GtkFormsContainer
{
	const string DefaultTimeFormat = @"hh\:mm\:ss";

	CustomComboBox _comboBox;
	Gdk.Color _color;
	TimeSpan _currentTime;
	string _timeFormat;

	public event EventHandler TimeChanged;
	public event EventHandler GotFocus;
	public event EventHandler LostFocus;

	public TimePicker()
	{
		BuildTimePicker();

		CurrentTime = new TimeSpan(DateTime.Now.Ticks);

		TextColor = _comboBox.Entry.GetDefaultTextColor(Gtk.StateFlags.Normal);

		_comboBox.Entry.Changed += new EventHandler(OnTxtTimeChanged);
		_comboBox.PopupButton.Clicked += new EventHandler(OnBtnShowTimePickerClicked);
	}

	public TimeSpan CurrentTime
	{
		get
		{
			return _currentTime;
		}
		set
		{
			_currentTime = value;
			UpdateEntryText();
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

	public string TimeFormat
	{
		get
		{
			return _timeFormat;
		}
		set
		{
			_timeFormat = value;
			UpdateEntryText();
		}
	}

	public void SetBackgroundColor(Gdk.Color color)
	{
		_comboBox.SetBackgroundColor(color);
	}

	public void OpenPicker()
	{
		ShowTimePickerWindow();
	}

	public void ClosePicker()
	{
		var windows = Gtk.Window.ListToplevels();
		var window = windows.FirstOrDefault(w => w.GetType() == typeof(TimePickerWindow))
			as TimePickerWindow;

		// Close(), not Remove() - see the identical note in DatePicker.ClosePicker.
		if (window != null)
		{
			window.Close();
		}
	}

	protected virtual void OnTxtTimeChanged(object sender, EventArgs e)
	{
		_comboBox.Entry.SetTextColor(TextColor);

		TimeChanged?.Invoke(this, e);
	}

	protected virtual void OnBtnShowTimePickerClicked(object sender, EventArgs e)
	{
		ShowTimePickerWindow();
	}

	void ShowTimePickerWindow()
	{
		// GTK4: gtk_window_get_origin and gtk_window_move are removed; the window manager
		// controls toplevel positions.  The picker appears at the default position.
		// TODO: position relative to the entry using Gtk.Popover once a parent widget
		// reference is available to set as the popover's relative-to.
		var picker = new TimePickerWindow();
		picker.CurrentTime = CurrentTime;
		picker.OnTimeChanged += OnPopupTimeChanged;
		picker.CloseRequest += new Gtk.CloseRequestHandler(OnPickerCloseRequest);
		GotFocus?.Invoke(this, EventArgs.Empty);
	}

	void OnPopupTimeChanged(object sender, TimeEventArgs args)
	{
		CurrentTime = args.Time;
	}

	void BuildTimePicker()
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
		_comboBox.Entry.Text = string.IsNullOrEmpty(_timeFormat)
			? _currentTime.ToString(DefaultTimeFormat)
			: DateTime.Today.Date.Add(_currentTime).ToString(_timeFormat);
	}

	void OnPickerCloseRequest(object sender, Gtk.CloseRequestArgs e)
	{
		// GTK4: Destroyed event is removed; CloseRequest replaces it for cleanup.
		// The old OnPickerClosed called Remove(window), which was always wrong for a
		// toplevel - the renderer already ignores that; we just raise LostFocus here.
		LostFocus?.Invoke(this, EventArgs.Empty);
	}
}