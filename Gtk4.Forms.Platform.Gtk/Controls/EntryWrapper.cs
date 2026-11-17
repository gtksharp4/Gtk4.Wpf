using Xamarin.Forms.Platform.GTK.Extensions;
using Gtk;
using Pango;

namespace Xamarin.Forms.Platform.GTK.Controls;

// Wraps a Gtk.Entry so the renderers keep a stable surface.
//
// GTK4 rewrite: the GTK3 version stacked a Gtk.Label over the Gtk.Entry in a single Grid cell
// and toggled each one's Visible from the entry's Changed/FocusOut signals to fake placeholder
// text. Under GTK4 that scheme deadlocks: setting a child's Visible from inside the entry's
// Changed signal re-enters GTK's measure/allocate pass on two widgets stacked in the same
// homogeneous Grid cell - together with the deferred SetSizeRequest this wrapper used to queue -
// and the relayout never reaches a fixed point, so the whole GTK thread wedges
// (PropertyMappingTests.EntryMapsTextPlaceholderAndPassword / SearchBarMapsTextAndPlaceholder).
//
// GTK4's Gtk.Entry has native placeholder text (gtk_entry_set_placeholder_text): the entry draws
// the placeholder itself while empty, with no overlay widget and no signal-driven visibility
// toggling. So the wrapper is now just the entry, and PlaceholderText maps straight to it.
public class EntryWrapper : EventBox
{
	private Gtk.Entry _entry;

	public EntryWrapper()
	{
		_entry = new Gtk.Entry();

		SetBackgroundColor(_entry.GetDefaultBaseColor(Gtk.StateFlags.Normal));

		Add(_entry);
	}

	public Gtk.Entry Entry => _entry;

	public string PlaceholderText
	{
		get
		{
			return _entry.PlaceholderText ?? string.Empty;
		}
		set
		{
			_entry.PlaceholderText = value ?? string.Empty;
		}
	}

	public void SetBackgroundColor(Gdk.Color color)
	{
		StyleExtensions.SetBackgroundColor(this, color);
		_entry.SetBaseColor(color);
	}

	public void SetTextColor(Gdk.Color color)
	{
		_entry.SetTextColor(color);
	}

	public void SetPlaceholderTextColor(Gdk.Color color)
	{
		// GTK4 draws the native placeholder in the theme's dimmed colour; there is no dedicated
		// API for it, so push it as CSS on the entry's internal placeholder node.
		_entry.SetStyleProperty("color", color.ToCss(), "text > placeholder");
	}

	public void SetAlignment(float aligmentValue)
	{
		_entry.Alignment = aligmentValue;
	}

	public void SetFont(FontDescription fontDescription)
	{
		_entry.SetFont(fontDescription);
	}

	public void SetMaxLength(int maxLength)
	{
		_entry.MaxLength = maxLength;
	}

	protected override bool OnGrabFocus()
	{
		return _entry != null && _entry.GrabFocus();
	}
}