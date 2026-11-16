using System;

namespace System.Windows.Controls
{
	/// <summary>
	/// A two-state (or three-state) check control backed by <see cref="Gtk.CheckButton"/>.
	/// Two-way: GTK toggle writes back into <see cref="IsChecked"/>.
	/// </summary>
	public class CheckBox : ContentControl
	{
		// ── Checked / Unchecked events ───────────────────────────────────────────

		/// <summary>Raised when <see cref="IsChecked"/> becomes <c>true</c>.</summary>
		public event EventHandler? Checked;

		/// <summary>Raised when <see cref="IsChecked"/> becomes <c>false</c>.</summary>
		public event EventHandler? Unchecked;

		// ── IsChecked DP ─────────────────────────────────────────────────────────

		/// <summary>Three-valued check state; <c>null</c> = indeterminate.</summary>
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register(
				nameof(IsChecked),
				typeof(bool?),
				typeof(CheckBox),
				new PropertyMetadata(false, OnIsCheckedChanged));

		static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cb = (CheckBox)d;
			if (cb._check != null)
			{
				var active = (e.NewValue as bool?) == true;
				if (cb._check.Active != active)
					cb._check.Active = active;
			}
			if ((e.NewValue as bool?) == true)
				cb.Checked?.Invoke(cb, EventArgs.Empty);
			else
				cb.Unchecked?.Invoke(cb, EventArgs.Empty);
		}

		/// <summary>Gets or sets the check state.</summary>
		public bool? IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.CheckButton _check = null!;
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_check = new Gtk.CheckButton { Active = IsChecked == true };
			_check.Toggled += OnGtkToggled;
			return _check;
		}

		// CheckBox doesn't use the ContentControl box/label; override to avoid NotImplementedException.
		/// <inheritdoc/>
		protected override Gtk.Box ContentContainer =>
			throw new InvalidOperationException("CheckBox manages its own widget.");

		/// <inheritdoc/>
		protected override Gtk.Label ContentLabel =>
			throw new InvalidOperationException("CheckBox manages its own widget.");

		void OnGtkToggled(object? sender, EventArgs e)
		{
			if (_updating) return;
			_updating = true;
			try { SetValue(IsCheckedProperty, _check.Active ? (bool?)true : false); }
			finally { _updating = false; }
		}
	}
}
