using System;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Text-input control. Single-line backed by <see cref="Gtk.Entry"/>;
/// multiline (<see cref="AcceptsReturn"/>=<c>true</c>) backed by
/// <see cref="Gtk.TextView"/> in a <see cref="Gtk.ScrolledWindow"/>.
/// Two-way <see cref="Text"/> binding works in both modes.
/// Selection model lives in <c>TextBox.Selection.cs</c>.
/// </summary>
public partial class TextBox : Control
{
	// ── TextChanged event ────────────────────────────────────────────────────

	/// <summary>Raised when <see cref="Text"/> changes.</summary>
	public event EventHandler<TextChangedEventArgs>? TextChanged;

	// ── SelectionChanged event ───────────────────────────────────────────────

	/// <summary>Raised when the selection changes.</summary>
	public event EventHandler? SelectionChanged;

	// ── Text DP ──────────────────────────────────────────────────────────────

	/// <summary>The text in the box.</summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(TextBox),
			new PropertyMetadata(string.Empty, OnTextDpChanged));

	static void OnTextDpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tb = (TextBox)d;
		var text = (string?)e.NewValue ?? string.Empty;

		// Clamp to MaxLength when Text is set longer than the limit.
		var maxLen = tb.MaxLength;
		if (maxLen > 0 && text.Length > maxLen)
		{
			// Guard _updating to avoid re-entrance through the GTK→DP sync path.
			if (!tb._updating)
			{
				tb._updating = true;
				try { tb.SetValue(TextProperty, text.Substring(0, maxLen)); }
				finally { tb._updating = false; }
			}
			return;
		}

		tb.OnTextChanged(text);
	}

	/// <summary>Gets or sets the displayed text.</summary>
	public string Text
	{
		get => (string?)GetValue(TextProperty) ?? string.Empty;
		set => SetValue(TextProperty, value);
	}

	// ── AcceptsReturn DP ─────────────────────────────────────────────────────

	/// <summary>
	/// When <c>true</c> the Enter key inserts a newline; the backing widget is
	/// <see cref="Gtk.TextView"/> in a <see cref="Gtk.ScrolledWindow"/>.
	/// Read once at widget-creation time — changes after the widget is realised
	/// have no effect in the current round.
	/// </summary>
	public static readonly DependencyProperty AcceptsReturnProperty =
		DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(TextBox),
			new PropertyMetadata(false));

	/// <summary>Gets or sets whether Enter inserts a newline (multiline mode).</summary>
	public bool AcceptsReturn
	{
		get => (bool)(GetValue(AcceptsReturnProperty) ?? false);
		set => SetValue(AcceptsReturnProperty, value);
	}

	// ── TextWrapping DP ──────────────────────────────────────────────────────

	/// <summary>Text wrapping behaviour (applies in multiline mode).</summary>
	public static readonly DependencyProperty TextWrappingProperty =
		DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(TextBox),
			new PropertyMetadata(TextWrapping.NoWrap));

	/// <summary>Gets or sets text wrapping.</summary>
	public TextWrapping TextWrapping
	{
		get => (TextWrapping)(GetValue(TextWrappingProperty) ?? TextWrapping.NoWrap);
		set => SetValue(TextWrappingProperty, value);
	}

	// ── MaxLength DP ─────────────────────────────────────────────────────────

	/// <summary>Maximum number of characters (0 = unlimited).</summary>
	public static readonly DependencyProperty MaxLengthProperty =
		DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(TextBox),
			new PropertyMetadata(0, OnMaxLengthChanged));

	static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tb = (TextBox)d;
		var maxLen = (int)(e.NewValue ?? 0);
		tb.ApplyMaxLength(maxLen);

		// Truncate Text when MaxLength shrinks below current length.
		if (maxLen > 0 && tb.Text.Length > maxLen)
			tb.SetValue(TextProperty, tb.Text.Substring(0, maxLen));
	}

	/// <summary>Gets or sets the maximum allowed character count (0 = no limit).</summary>
	public int MaxLength
	{
		get => (int)(GetValue(MaxLengthProperty) ?? 0);
		set => SetValue(MaxLengthProperty, value);
	}

	// ── IsReadOnly DP ────────────────────────────────────────────────────────

	/// <summary>When <c>true</c> the user cannot edit the text.</summary>
	public static readonly DependencyProperty IsReadOnlyProperty =
		DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(TextBox),
			new PropertyMetadata(false, OnIsReadOnlyChanged));

	static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBox)d).ApplyIsReadOnly((bool)(e.NewValue ?? false));

	/// <summary>Gets or sets whether the text box is read-only.</summary>
	public bool IsReadOnly
	{
		get => (bool)(GetValue(IsReadOnlyProperty) ?? false);
		set => SetValue(IsReadOnlyProperty, value);
	}
}

/// <summary>Text-wrapping mode.</summary>
public enum TextWrapping
{
	/// <summary>No wrapping.</summary>
	NoWrap = 0,
	/// <summary>Wraps at word boundaries.</summary>
	Wrap   = 1,
	/// <summary>Wraps at any character.</summary>
	WrapWithOverflow = 2,
}

/// <summary>Event data for <see cref="TextBox.TextChanged"/>.</summary>
public sealed class TextChangedEventArgs : EventArgs
{
	/// <summary>The text box whose text changed.</summary>
	public TextBox Source { get; }
	internal TextChangedEventArgs(TextBox source) => Source = source;
}