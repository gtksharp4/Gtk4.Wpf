using System.Windows.Annotations;

namespace System.Windows.Controls;

public sealed partial class StickyNoteControl
{
	Gtk.Box? _box;
	Gtk.TextView? _text;
	Gtk.Popover? _popover;
	bool _loading;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Vertical, 4) { WidthRequest = 220 };
		_box.Append(new Gtk.Label(Author) { Xalign = 0f });
		if (StickyNoteType == StickyNoteType.Text)
		{
			_text = new Gtk.TextView { WrapMode = Gtk.WrapMode.Word, HeightRequest = 100 };
			_loading = true;
			_text.Buffer.Text = StickyNoteData.ReadText(_annotation);
			_loading = false;
			_text.Buffer.Changed += (_, _) =>
			{
				if (!_loading) StickyNoteData.WriteText(_annotation, _text.Buffer.Text ?? string.Empty);
			};
			_box.Append(_text);
		}
		return _box;
	}

	/// <summary>The text shown in the note's text view (test seam).</summary>
	internal string NoteText
	{
		get
		{
			_ = Widget;
			return _text?.Buffer.Text ?? string.Empty;
		}
		set
		{
			_ = Widget;
			if (_text != null) _text.Buffer.Text = value;
		}
	}

	internal Gtk.Popover? GtkPopover => _popover;

	/// <summary>Shows the note as a popover on the viewer's widget, once that widget is mapped.</summary>
	internal void Present(UIElement host)
	{
		if (StickyNoteType != StickyNoteType.Text) return;
		var parent = host.Widget;
		_popover = new Gtk.Popover { Child = Widget, Autohide = false };
		_popover.Parent = parent;
		if (parent.IsMapped) SyncExpanded();
		else parent.Mapped += OnHostMapped;
	}

	void OnHostMapped(object? sender, EventArgs e)
	{
		if (_popover?.Parent is { } parent) parent.Mapped -= OnHostMapped;
		SyncExpanded();
	}

	void SyncExpanded()
	{
		if (_popover?.Parent is not { IsMapped: true }) return;
		if (IsExpanded) _popover.Popup();
		else _popover.Popdown();
	}

	/// <summary>Hides the note and releases its popover (the annotation was deleted or the service disabled).</summary>
	internal void Dismiss()
	{
		if (_popover == null) return;
		if (_popover.Parent is { } parent) parent.Mapped -= OnHostMapped;
		_popover.Popdown();
		_popover.Child = null!;
		_popover.Unparent();
		_popover = null;
	}
}
