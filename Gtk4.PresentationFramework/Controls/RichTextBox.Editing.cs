using System;

namespace System.Windows.Controls;

/// <summary>
/// Editing surface for <see cref="RichTextBox"/>: clipboard, undo/redo, select-all, scrolling,
/// input options and change events. These map the WPF <c>TextBoxBase</c> editing API onto the
/// backing <see cref="Gtk.TextView"/>/<see cref="Gtk.TextBuffer"/>.
/// </summary>
public partial class RichTextBox
{
	// ── Change events ──────────────────────────────────────────────────────────

	/// <summary>Raised whenever the content changes (user edit or programmatic edit).</summary>
	public event EventHandler? TextChanged;

	internal void RaiseTextChanged() => TextChanged?.Invoke(this, EventArgs.Empty);
	void RaiseSelectionChanged() => RaiseSelectionChangedEvent();

	// Wired to Gtk.TextBuffer.MarkSet in CreateWidget; fires only for the caret/selection marks.
	void OnBufferMarkSet(object? sender, Gtk.MarkSetArgs args)
	{
		if (_suppressMarkSet) return;
		var name = args.Mark?.Name;
		if (name == "insert" || name == "selection_bound")
			RaiseSelectionChanged();
	}

	// ── IsUndoEnabled ──────────────────────────────────────────────────────────

	/// <summary>Whether the undo stack is active (WPF default: true).</summary>
	public static readonly DependencyProperty IsUndoEnabledProperty =
		DependencyProperty.Register(nameof(IsUndoEnabled), typeof(bool), typeof(RichTextBox),
			new PropertyMetadata(true, OnIsUndoEnabledChanged));

	static void OnIsUndoEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var rtb = (RichTextBox)d;
		if (rtb._textView != null)
			rtb._textView.Buffer.EnableUndo = (bool)(e.NewValue ?? true);
	}

	/// <summary>Gets or sets whether undo/redo is enabled.</summary>
	public bool IsUndoEnabled
	{
		get => (bool)(GetValue(IsUndoEnabledProperty) ?? true);
		set => SetValue(IsUndoEnabledProperty, value);
	}

	// ── AcceptsTab / AcceptsReturn ──────────────────────────────────────────────

	/// <summary>Whether a Tab key inserts a tab (true) or moves focus (false). WPF default: false.</summary>
	public static readonly DependencyProperty AcceptsTabProperty =
		DependencyProperty.Register(nameof(AcceptsTab), typeof(bool), typeof(RichTextBox),
			new PropertyMetadata(false, OnAcceptsTabChanged));

	static void OnAcceptsTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var rtb = (RichTextBox)d;
		if (rtb._textView != null)
			rtb._textView.AcceptsTab = (bool)(e.NewValue ?? false);
	}

	/// <summary>Gets or sets whether Tab is inserted into the text.</summary>
	public bool AcceptsTab
	{
		get => (bool)(GetValue(AcceptsTabProperty) ?? false);
		set => SetValue(AcceptsTabProperty, value);
	}

	/// <summary>
	/// Whether Enter inserts a new line. The backing multi-line <see cref="Gtk.TextView"/> always
	/// accepts Return, so this is stored for WPF source compatibility.
	/// </summary>
	public static readonly DependencyProperty AcceptsReturnProperty =
		DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(RichTextBox),
			new PropertyMetadata(true));

	/// <summary>Gets or sets whether Enter inserts a new line (always effectively true).</summary>
	public bool AcceptsReturn
	{
		get => (bool)(GetValue(AcceptsReturnProperty) ?? true);
		set => SetValue(AcceptsReturnProperty, value);
	}

	// ── Undo / Redo ────────────────────────────────────────────────────────────

	/// <summary>True when there is an action to undo.</summary>
	public bool CanUndo => _textView != null && _textView.Buffer.CanUndo;

	/// <summary>True when there is an action to redo.</summary>
	public bool CanRedo => _textView != null && _textView.Buffer.CanRedo;

	/// <summary>Undoes the most recent edit.</summary>
	public void Undo()
	{
		if (_textView != null && _textView.Buffer.CanUndo)
			_textView.Buffer.Undo();
	}

	/// <summary>Redoes the most recently undone edit.</summary>
	public void Redo()
	{
		if (_textView != null && _textView.Buffer.CanRedo)
			_textView.Buffer.Redo();
	}

	// ── Clipboard ──────────────────────────────────────────────────────────────

	/// <summary>Copies the current selection to the clipboard.</summary>
	public void Copy() => EmitViewAction("copy-clipboard");

	/// <summary>Cuts the current selection to the clipboard.</summary>
	public void Cut() => EmitViewAction("cut-clipboard");

	/// <summary>Pastes the clipboard contents at the caret.</summary>
	public void Paste() => EmitViewAction("paste-clipboard");

	// GTK exposes copy/cut/paste as action signals on the view, not callable methods.
	void EmitViewAction(string signal)
	{
		if (_textView == null) return;
		try { GLib.Signal.Emit(_textView, signal); }
		catch { /* no display / not realised: best-effort */ }
	}

	// ── Selection / scrolling ──────────────────────────────────────────────────

	/// <summary>Selects the entire contents.</summary>
	public void SelectAll()
	{
		if (_textView == null) return;
		var buf = _textView.Buffer;
		buf.SelectRange(buf.StartIter, buf.EndIter);
		RaiseSelectionChanged();
	}

	/// <summary>Scrolls to the start of the content.</summary>
	public void ScrollToHome() => ScrollTo(toStart: true);

	/// <summary>Scrolls to the end of the content.</summary>
	public void ScrollToEnd() => ScrollTo(toStart: false);

	void ScrollTo(bool toStart)
	{
		if (_textView == null) return;
		var iter = toStart ? _textView.Buffer.StartIter : _textView.Buffer.EndIter;
		_textView.ScrollToIter(iter, 0.0, false, 0.0, 0.0);
	}
}
