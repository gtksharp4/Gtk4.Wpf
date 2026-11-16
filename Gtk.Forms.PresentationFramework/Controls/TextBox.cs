using System;
using System.Windows.Data;

namespace System.Windows.Controls
{
	/// <summary>
	/// Text-input control. Single-line backed by <see cref="Gtk.Entry"/>;
	/// multiline (<see cref="AcceptsReturn"/>=<c>true</c>) backed by
	/// <see cref="Gtk.TextView"/> in a <see cref="Gtk.ScrolledWindow"/>.
	/// Two-way <see cref="Text"/> binding works in both modes.
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
			((TextBox)d).OnTextChanged((string?)e.NewValue ?? string.Empty);
		}

		/// <summary>Gets or sets the displayed text.</summary>
		public string Text
		{
			get => (string?)GetValue(TextProperty) ?? string.Empty;
			set => SetValue(TextProperty, value);
		}

		// ── AcceptsReturn DP ─────────────────────────────────────────────────────

		/// <summary>
		/// When <c>true</c> the Enter key inserts a newline and the backing
		/// widget switches to <see cref="Gtk.TextView"/>.
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
			=> ((TextBox)d).ApplyMaxLength((int)(e.NewValue ?? 0));

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

		// ── Selection ────────────────────────────────────────────────────────────

		int _selectionStart;
		int _selectionLength;

		/// <summary>Gets or sets the character index of the selection start.</summary>
		public int SelectionStart
		{
			get => _selectionStart;
			set
			{
				value = Math.Max(0, Math.Min(value, Text.Length));
				if (_selectionStart == value) return;
				_selectionStart = value;
				SyncSelectionToWidget();
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>Gets or sets the selection length in characters.</summary>
		public int SelectionLength
		{
			get => _selectionLength;
			set
			{
				value = Math.Max(0, Math.Min(value, Text.Length - _selectionStart));
				if (_selectionLength == value) return;
				_selectionLength = value;
				SyncSelectionToWidget();
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>Gets or sets the currently selected text.</summary>
		public string SelectedText
		{
			get
			{
				if (_selectionLength == 0) return string.Empty;
				return Text.Substring(
					Math.Min(_selectionStart, Text.Length),
					Math.Min(_selectionLength, Text.Length - _selectionStart));
			}
			set
			{
				if (value == null) value = string.Empty;
				var current = Text;
				var before = current.Substring(0, _selectionStart);
				var after  = current.Substring(_selectionStart + _selectionLength);
				SetValue(TextProperty, before + value + after);
				_selectionLength = value.Length;
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		// ── CaretIndex ───────────────────────────────────────────────────────────

		/// <summary>Gets or sets the caret position (character index).</summary>
		public int CaretIndex
		{
			get => _selectionStart;
			set
			{
				value = Math.Max(0, Math.Min(value, Text.Length));
				if (_selectionStart == value) return;
				_selectionStart  = value;
				_selectionLength = 0;
				SyncSelectionToWidget();
			}
		}

		// ── Methods ──────────────────────────────────────────────────────────────

		/// <summary>Selects the range from <paramref name="start"/> for <paramref name="length"/> characters.</summary>
		public void Select(int start, int length)
		{
			start  = Math.Max(0, Math.Min(start, Text.Length));
			length = Math.Max(0, Math.Min(length, Text.Length - start));
			_selectionStart  = start;
			_selectionLength = length;
			SyncSelectionToWidget();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>Selects all text.</summary>
		public void SelectAll()
		{
			_selectionStart  = 0;
			_selectionLength = Text.Length;
			SyncSelectionToWidget();
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>Clears the text.</summary>
		public void Clear()
		{
			SetValue(TextProperty, string.Empty);
			_selectionStart  = 0;
			_selectionLength = 0;
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
}
