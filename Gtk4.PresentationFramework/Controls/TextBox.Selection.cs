using System;

namespace System.Windows.Controls;

/// <summary>
/// Selection model and input options for <see cref="TextBox"/>: SelectionStart,
/// SelectionLength, SelectedText, CaretIndex, Select, SelectAll, Clear, AcceptsTab.
/// </summary>
public partial class TextBox
{
	int _selectionStart;
	int _selectionLength;

	// ── AcceptsTab DP ────────────────────────────────────────────────────────

	/// <summary>
	/// Whether pressing Tab inserts a tab character instead of moving focus.
	/// Applies only in multiline mode (<see cref="AcceptsReturn"/>=<c>true</c>),
	/// where the backing widget is <see cref="Gtk.TextView"/> which exposes
	/// <c>AcceptsTab</c>. In single-line mode (<see cref="Gtk.Entry"/>) GTK has
	/// no equivalent; the flag is stored but has no GTK effect.
	/// WPF default: <c>false</c>.
	/// </summary>
	public static readonly DependencyProperty AcceptsTabProperty =
		DependencyProperty.Register(nameof(AcceptsTab), typeof(bool), typeof(TextBox),
			new PropertyMetadata(false, OnAcceptsTabChanged));

	static void OnAcceptsTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBox)d).ApplyAcceptsTab((bool)(e.NewValue ?? false));

	/// <summary>Gets or sets whether Tab inserts a tab character (multiline only).</summary>
	public bool AcceptsTab
	{
		get => (bool)(GetValue(AcceptsTabProperty) ?? false);
		set => SetValue(AcceptsTabProperty, value);
	}

	// ── Internal helpers used by TextBox.Widget.cs ───────────────────────────

	/// <summary>
	/// Clamps selection to the current text length after the text changes.
	/// Called from <c>OnTextChanged</c> so the managed model stays consistent.
	/// </summary>
	internal void ClampSelectionToTextLength()
	{
		var len = Text.Length;
		_selectionStart  = Math.Max(0, Math.Min(_selectionStart, len));
		_selectionLength = Math.Max(0, Math.Min(_selectionLength, len - _selectionStart));
	}

	/// <summary>Gets or sets the character index of the selection start.</summary>
	public int SelectionStart
	{
		get => _selectionStart;
		set
		{
			value = Math.Max(0, Math.Min(value, Text.Length));
			if (_selectionStart == value) return;
			_selectionStart = value;
			// Re-clamp SelectionLength so start+length never exceeds text end.
			_selectionLength = Math.Max(0, Math.Min(_selectionLength, Text.Length - _selectionStart));
			SyncSelectionToWidget();
			RaiseSelectionChangedEvent();
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
			RaiseSelectionChangedEvent();
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
			// Caret lands at end of inserted text, selection collapses (WPF behaviour).
			var insertionPoint = _selectionStart + value.Length;
			SetValue(TextProperty, before + value + after);
			_selectionStart  = insertionPoint;
			_selectionLength = 0;
			SyncSelectionToWidget();
			RaiseSelectionChangedEvent();
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
		RaiseSelectionChangedEvent();
	}

	/// <summary>Selects all text.</summary>
	public void SelectAll()
	{
		_selectionStart  = 0;
		_selectionLength = Text.Length;
		SyncSelectionToWidget();
		RaiseSelectionChangedEvent();
	}

	/// <summary>Clears the text.</summary>
	public void Clear()
	{
		SetValue(TextProperty, string.Empty);
		_selectionStart  = 0;
		_selectionLength = 0;
	}
}