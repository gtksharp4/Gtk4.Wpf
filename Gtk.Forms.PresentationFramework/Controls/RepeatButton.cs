using System;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives
{
	/// <summary>
	/// A button that fires <see cref="Click"/> repeatedly while it is held down.
	/// Backed by a <see cref="Gtk.Button"/> with a <see cref="System.Threading.Timer"/>
	/// for the repeat cadence.
	/// <para>
	/// <see cref="Delay"/> ms after the press the repeat starts; subsequent firings
	/// occur every <see cref="Interval"/> ms.
	/// </para>
	/// </summary>
	public class RepeatButton : ContentControl, ICommandSource, IDisposable
	{
		// ── Delay DP ─────────────────────────────────────────────────────────────

		/// <summary>Milliseconds before the first repeat fires after the initial press.</summary>
		public static readonly DependencyProperty DelayProperty =
			DependencyProperty.Register(
				nameof(Delay),
				typeof(int),
				typeof(RepeatButton),
				new PropertyMetadata(500));

		/// <summary>Gets or sets the initial repeat delay (ms).</summary>
		public int Delay
		{
			get => (int?)GetValue(DelayProperty) ?? 500;
			set => SetValue(DelayProperty, value);
		}

		// ── Interval DP ──────────────────────────────────────────────────────────

		/// <summary>Milliseconds between subsequent repeated firings.</summary>
		public static readonly DependencyProperty IntervalProperty =
			DependencyProperty.Register(
				nameof(Interval),
				typeof(int),
				typeof(RepeatButton),
				new PropertyMetadata(33));

		/// <summary>Gets or sets the repeat interval (ms).</summary>
		public int Interval
		{
			get => (int?)GetValue(IntervalProperty) ?? 33;
			set => SetValue(IntervalProperty, value);
		}

		// ── Click event ──────────────────────────────────────────────────────────

		/// <summary>Raised on each repeat while the button is held.</summary>
		public event RoutedEventHandler? Click;

		// ── ICommandSource ───────────────────────────────────────────────────────

		/// <summary>Command executed on each click repetition.</summary>
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RepeatButton),
				new PropertyMetadata(null));

		/// <inheritdoc/>
		public ICommand? Command
		{
			get => (ICommand?)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <inheritdoc/>
		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <summary>Parameter passed to <see cref="Command"/> on execution.</summary>
		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(RepeatButton),
				new PropertyMetadata(null));

		/// <inheritdoc/>
		public UIElement? CommandTarget => null;

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Button? _button;
		System.Threading.Timer? _timer;
		bool _pressed;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_button = new Gtk.Button();
			_button.Label = (Content as string) ?? string.Empty;

			var pressCtrl = new Gtk.GestureClick();
			pressCtrl.Button = 1; // primary
			pressCtrl.Pressed += OnGtkPressed;
			pressCtrl.Released += OnGtkReleased;
			_button.AddController(pressCtrl);

			return _button;
		}

		/// <inheritdoc/>
		protected override Gtk.Box ContentContainer =>
			throw new InvalidOperationException("RepeatButton manages its own content via Label.");

		/// <inheritdoc/>
		protected override Gtk.Label ContentLabel =>
			throw new InvalidOperationException("RepeatButton manages its own content via Label.");

		void OnGtkPressed(object o, Gtk.PressedArgs e)
		{
			// Dispose any stale timer from a previous press that was never released.
			_timer?.Dispose();
			_timer = null;

			_pressed = true;
			FireClick(); // immediate first fire
			_timer = new System.Threading.Timer(_ =>
			{
				if (!_pressed) return;
				GLib.Idle.Add(() => { if (_pressed) FireClick(); return false; });
			}, null, Delay, Interval);
		}

		void OnGtkReleased(object o, Gtk.ReleasedArgs e)
		{
			_pressed = false;
			_timer?.Dispose();
			_timer = null;
		}

		void FireClick()
		{
			var args = new RoutedEventArgs { Source = this };
			Click?.Invoke(this, args);
			var cmd = Command;
			var param = CommandParameter;
			if (cmd != null && cmd.CanExecute(param))
				cmd.Execute(param);
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == ContentProperty && _button != null && e.NewValue is string s)
				_button.Label = s;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			// Stop repeat firing immediately; guard the idle callbacks.
			_pressed = false;
			_timer?.Dispose();
			_timer = null;
		}

		// ── Test helpers (InternalsVisibleTo) ────────────────────────────────────

		/// <summary>Drives the press state machine without a GTK gesture object (for unit tests).</summary>
		internal void SimulatePressForTest()
		{
			_timer?.Dispose();
			_timer = null;
			_pressed = true;
			FireClick();
			_timer = new System.Threading.Timer(_ =>
			{
				if (!_pressed) return;
				GLib.Idle.Add(() => { if (_pressed) FireClick(); return false; });
			}, null, Delay, Interval);
		}

		/// <summary>Drives the release state machine without a GTK gesture object (for unit tests).</summary>
		internal void SimulateReleaseForTest()
		{
			_pressed = false;
			_timer?.Dispose();
			_timer = null;
		}
	}
}
