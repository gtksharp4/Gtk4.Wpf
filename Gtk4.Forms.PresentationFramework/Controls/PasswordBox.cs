using System;
using System.Security;

namespace System.Windows.Controls;

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
	static PasswordBox()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordBox),
			new PropertyMetadata(typeof(PasswordBox)));
	}

	// ── Password (CLR, not DP — matches WPF) ─────────────────────────────────

	string _password = string.Empty;
	bool _updating;

	/// <summary>Gets or sets the password text.
	/// Clamped to <see cref="MaxLength"/> (when &gt; 0) on set.</summary>
	public string Password
	{
		get => _password;
		set
		{
			var incoming = value ?? string.Empty;

			// Clamp to MaxLength.
			var maxLen = MaxLength;
			if (maxLen > 0 && incoming.Length > maxLen)
				incoming = incoming.Substring(0, maxLen);

			if (_password == incoming) return;
			_password = incoming;

			if (_entry != null && !_updating)
			{
				_updating = true;
				try { _entry.Text = _password; }
				finally { _updating = false; }
			}

			RaisePasswordChanged();
		}
	}

	// ── PasswordChar DP ──────────────────────────────────────────────────────

	/// <summary>
	/// Character used to mask each character in the password.
	/// <para>
	/// <b>GTK 4 limitation:</b> <see cref="Gtk.PasswordEntry"/> does not expose a
	/// per-character masking API; this property is stored and round-trips faithfully
	/// but has no visual effect on the backing widget.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty PasswordCharProperty =
		DependencyProperty.Register(
			nameof(PasswordChar),
			typeof(char),
			typeof(PasswordBox),
			new PropertyMetadata('●'));

	/// <summary>Gets or sets the masking character.
	/// Round-trips faithfully; has no visual effect on the GTK 4 backing widget —
	/// see <see cref="PasswordCharProperty"/> for details.</summary>
	public char PasswordChar
	{
		get => (char?)GetValue(PasswordCharProperty) ?? '●';
		set => SetValue(PasswordCharProperty, value);
	}

	// ── MaxLength DP ─────────────────────────────────────────────────────────

	/// <summary>Maximum number of characters (0 = unlimited).
	/// <para>
	/// <b>GTK 4 note:</b> <see cref="Gtk.PasswordEntry"/> has no native MaxLength
	/// API; the limit is enforced in managed code only (clamped on <see cref="Password"/> set).
	/// </para>
	/// </summary>
	public static readonly DependencyProperty MaxLengthProperty =
		DependencyProperty.Register(
			nameof(MaxLength),
			typeof(int),
			typeof(PasswordBox),
			new PropertyMetadata(0, OnMaxLengthChanged));

	static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var pb = (PasswordBox)d;
		var maxLen = (int)(e.NewValue ?? 0);
		// Truncate the current password if it exceeds the new limit.
		if (maxLen > 0 && pb._password.Length > maxLen)
			pb.Password = pb._password.Substring(0, maxLen);
	}

	/// <summary>Gets or sets the maximum allowed character count (0 = unlimited).</summary>
	public int MaxLength
	{
		get => (int)(GetValue(MaxLengthProperty) ?? 0);
		set => SetValue(MaxLengthProperty, value);
	}

	// ── PasswordChanged event ────────────────────────────────────────────────

	/// <summary>Raised when <see cref="Password"/> changes (programmatically or via user input).</summary>
	public event RoutedEventHandler? PasswordChanged;

	void RaisePasswordChanged()
		=> PasswordChanged?.Invoke(this, new RoutedEventArgs());

	// ── SecurePassword ───────────────────────────────────────────────────────

	/// <summary>
	/// Returns a <see cref="SecureString"/> copy of the current <see cref="Password"/>.
	/// The caller is responsible for disposing the returned instance.
	/// </summary>
	public SecureString SecurePassword
	{
		get
		{
			var ss = new SecureString();
			foreach (char c in _password)
				ss.AppendChar(c);
			ss.MakeReadOnly();
			return ss;
		}
	}

	// ── Clear ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Clears the password. Raises <see cref="PasswordChanged"/> only if it was non-empty
	/// (setting the password to its current value is a no-op).
	/// </summary>
	public void Clear() => Password = string.Empty;

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.PasswordEntry? _entry;

	/// <inheritdoc/>
	protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
		new System.Windows.Automation.Peers.PasswordBoxAutomationPeer(this);

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
		string raw;
		try
		{
			raw = _entry!.Text ?? string.Empty;
		}
		finally { _updating = false; }

		// Apply MaxLength clamp from GTK input.
		var maxLen = MaxLength;
		if (maxLen > 0 && raw.Length > maxLen)
			raw = raw.Substring(0, maxLen);

		if (_password == raw) return;
		_password = raw;

		// Sync clamp back to entry if we truncated.
		if (_entry!.Text != _password)
		{
			_updating = true;
			try { _entry.Text = _password; }
			finally { _updating = false; }
		}

		RaisePasswordChanged();
	}
}
