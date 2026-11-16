using System;

namespace System.Windows.Controls
{
	/// <summary>
	/// A text-input control that obscures the entered text.
	/// Backed by <see cref="Gtk.PasswordEntry"/> (GTK 4 native).
	/// <para>
	/// Unlike most WPF DPs, <see cref="Password"/> is a plain CLR property (not a
	/// <see cref="DependencyProperty"/>) — matching the real WPF <c>PasswordBox</c>.
	/// </para>
	/// </summary>
	public class PasswordBox : Control
	{
		// ── Password (CLR, not DP — matches WPF) ─────────────────────────────────

		string _password = string.Empty;
		bool _updating;

		/// <summary>Gets or sets the password text.</summary>
		public string Password
		{
			get => _password;
			set
			{
				if (_password == value) return;
				_password = value ?? string.Empty;
				if (_entry != null && !_updating)
				{
					_updating = true;
					try { _entry.Text = _password; }
					finally { _updating = false; }
				}
				PasswordChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		// ── PasswordChar DP ──────────────────────────────────────────────────────

		/// <summary>
		/// Character used to mask each character in the password.
		/// GTK4 <see cref="Gtk.PasswordEntry"/> does not expose per-char masking;
		/// this property is stored and round-trips faithfully but has no visual effect.
		/// </summary>
		public static readonly DependencyProperty PasswordCharProperty =
			DependencyProperty.Register(
				nameof(PasswordChar),
				typeof(char),
				typeof(PasswordBox),
				new PropertyMetadata('●'));

		/// <summary>Gets or sets the masking character (cosmetic; not applied by GTK4).</summary>
		public char PasswordChar
		{
			get => (char?)GetValue(PasswordCharProperty) ?? '●';
			set => SetValue(PasswordCharProperty, value);
		}

		// ── PasswordChanged event ────────────────────────────────────────────────

		/// <summary>Raised when <see cref="Password"/> changes.</summary>
		public event EventHandler? PasswordChanged;

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.PasswordEntry? _entry;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_entry = new Gtk.PasswordEntry();
			_entry.ShowPeekIcon = true;
			_entry.Text = _password;
			_entry.Changed += OnGtkChanged;
			return _entry;
		}

		void OnGtkChanged(object? sender, EventArgs e)
		{
			if (_updating) return;
			_updating = true;
			try
			{
				_password = _entry!.Text ?? string.Empty;
			}
			finally { _updating = false; }
			PasswordChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
