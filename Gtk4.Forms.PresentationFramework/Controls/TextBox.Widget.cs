using System;

namespace System.Windows.Controls;

/// <summary>
/// GTK widget creation and synchronisation for <see cref="TextBox"/>.
/// Single-line: <see cref="Gtk.Entry"/>; multiline: <see cref="Gtk.TextView"/>
/// inside a <see cref="Gtk.ScrolledWindow"/>.
/// </summary>
public partial class TextBox
{
	Gtk.Entry?         _entry;
	Gtk.TextView?      _textView;
	Gtk.ScrolledWindow? _scrolled;
	bool _updating;

	/// <summary>
	/// For multiline mode the focus target is the inner <see cref="Gtk.TextView"/>,
	/// not the <see cref="Gtk.ScrolledWindow"/> wrapper; focus and key controllers
	/// must attach there so GTK routes events correctly.
	/// </summary>
	internal override Gtk.Widget FocusWidget =>
		_textView != null ? (Gtk.Widget)_textView : Widget;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		if (IsMultiline)
			return CreateMultilineWidget();
		return CreateSingleLineWidget();
	}

	Gtk.Widget CreateSingleLineWidget()
	{
		_entry = new Gtk.Entry { Text = Text };
		ApplyMaxLength(MaxLength);
		ApplyIsReadOnly(IsReadOnly);
		ApplyTextAlignment();
		_entry.Changed += OnEntryChanged;
		return _entry;
	}

	Gtk.Widget CreateMultilineWidget()
	{
		_textView = new Gtk.TextView();
		ApplyWrapMode();
		ApplyIsReadOnly(IsReadOnly);
		ApplyAcceptsTab(AcceptsTab);
		ApplyTextAlignment();
		_textView.Buffer.Text = Text;
		_textView.Buffer.Changed += OnBufferChanged;

		_scrolled = new Gtk.ScrolledWindow();
		ApplyScrollPolicy();
		_scrolled.Child = _textView;
		return _scrolled;
	}

	// ── GTK→DP sync ──────────────────────────────────────────────────────────

	void OnEntryChanged(object? sender, EventArgs e)
	{
		if (_updating || _entry == null) return;
		_updating = true;
		try { SetValue(TextProperty, _entry.Text); }
		finally { _updating = false; }
	}

	void OnBufferChanged(object? sender, EventArgs e)
	{
		if (_updating || _textView == null) return;
		_updating = true;
		try { SetValue(TextProperty, _textView.Buffer.Text); }
		finally { _updating = false; }
	}

	// ── DP→GTK sync ──────────────────────────────────────────────────────────

	void OnTextChanged(string text)
	{
		// Keep the managed selection model consistent with the new text length.
		ClampSelectionToTextLength();

		// Always fire TextChanged (pure model concern, widget optional).
		TextChanged?.Invoke(this, new TextChangedEventArgs(this));

		// Sync to the backing GTK widget when available.
		if (!IsWidgetCreated || _updating) return;
		_updating = true;
		try
		{
			if (_entry != null && _entry.Text != text)
				_entry.Text = text;
			else if (_textView != null && _textView.Buffer.Text != text)
				_textView.Buffer.Text = text;
		}
		finally { _updating = false; }
	}

	void ApplyMaxLength(int maxLen)
	{
		if (_entry != null)
			_entry.MaxLength = maxLen;
		// Gtk.TextView has no built-in MaxLength; enforced on buffer change if needed.
	}

	void ApplyIsReadOnly(bool ro)
	{
		if (_entry != null)
			_entry.IsEditable = !ro;
		if (_textView != null)
			_textView.Editable = !ro;
	}

	void ApplyWrapMode()
	{
		if (_textView == null) return;
		_textView.WrapMode = TextWrappingMap.ToTextView(TextWrapping);
	}

	/// <summary>
	/// Maps <see cref="AcceptsTab"/> to the backing widget.
	/// Only <see cref="Gtk.TextView"/> (multiline) exposes <c>AcceptsTab</c>;
	/// <see cref="Gtk.Entry"/> (single-line) has no GTK equivalent — the flag is
	/// stored in the DP but has no widget-level effect in single-line mode.
	/// </summary>
	void ApplyAcceptsTab(bool accepts)
	{
		if (_textView != null)
			_textView.AcceptsTab = accepts;
	}

	// ── Selection sync ───────────────────────────────────────────────────────

	void SyncSelectionToWidget()
	{
		if (!IsWidgetCreated) return;

		if (_entry != null)
		{
			_entry.SelectRegion(_selectionStart, _selectionStart + _selectionLength);
		}
		else if (_textView != null)
		{
			var buf   = _textView.Buffer;
			var start = buf.GetIterAtOffset(_selectionStart);
			var end   = buf.GetIterAtOffset(_selectionStart + _selectionLength);
			buf.SelectRange(start, end);
		}
	}
}