using System;
using System.ComponentModel;
using Gtk4.Forms.Platform.GTK.Controls;
using Gtk4.Forms.Platform.GTK.Extensions;

namespace Gtk4.Forms.Platform.GTK.Renderers;

public class SearchBarRenderer : ViewRenderer<SearchBar, SearchEntry>
{
	// Same re-entrant set_Text spin the EntryRenderer guards against: SearchEntry.SearchText goes
	// through Gtk.Entry.Text, whose delete-then-insert raises Changed with a momentary "". Writing
	// that back into SearchBar.Text re-dispatches UpdateText and re-enters set_Text on the inner
	// GtkText mid-edit, which busy-loops under GTK4. Suppress the write-back while we drive it.
	private bool _settingText;

	protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
	{
		if (e.NewElement != null)
		{
			if (Control == null)
			{
				// Custom control using a Gtk.Entry with some ImageButtons.
				SetNativeControl(new SearchEntry());

				Control.SearchTextChanged += SearchTextChanged;
				Control.SearchButtonClicked += SearchButtonClicked;
				Control.Entry.Focused += SearchFocused;
				Control.Entry.FocusOutEvent += SearchFocusOutEvent;
			}

			UpdateText();
			UpdateTextColor();
			UpdateFont();
			UpdateAlignment();
			UpdatePlaceholder();
			UpdateCancelButtonColor();
		}

		base.OnElementChanged(e);
	}

	protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		base.OnElementPropertyChanged(sender, e);

		if (e.PropertyName == SearchBar.TextProperty.PropertyName ||
		    e.PropertyName == SearchBar.TextTransformProperty.PropertyName)
			UpdateText();
		else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
			UpdateTextColor();
		else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
			UpdateAlignment();
		else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
			UpdateFont();
		else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
			UpdateFont();
		else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
			UpdateFont();
		else if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName)
			UpdatePlaceholder();
		else if (e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
			UpdatePlaceholder();
		else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
			UpdateCancelButtonColor();
	}

	protected override void UpdateBackgroundColor()
	{
		base.UpdateBackgroundColor();

		if (!Element.BackgroundColor.IsDefaultOrTransparent())
		{
			Control.SetBackgroundColor(Element.BackgroundColor.ToGtkColor());
		}
	}

	protected override void Dispose(bool disposing)
	{
		// BEFORE base.Dispose, not after. ViewRenderer.Dispose runs Control.Destroy() and then
		// Control = null unconditionally, so with the old ordering the guard below was always
		// false and none of these detaches ever ran. Destroying a focused SearchBar synthesises
		// a focus-out, which then re-entered SearchFocusOutEvent mid-teardown and raised
		// Unfocused into application code. Every sibling renderer detaches first.
		if (disposing)
		{
			if (Control != null)
			{
				Control.SearchTextChanged -= SearchTextChanged;
				Control.SearchButtonClicked -= SearchButtonClicked;
				Control.Entry.Focused -= SearchFocused;
				Control.Entry.FocusOutEvent -= SearchFocusOutEvent;
			}
		}

		base.Dispose(disposing);
	}

	void UpdateText()
	{
		var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
		if (Control.SearchText == text)
			return;

		_settingText = true;
		try
		{
			Control.SearchText = text;
		}
		finally
		{
			_settingText = false;
		}
	}

	private void UpdatePlaceholder()
	{
		Control.PlaceholderText = Element.Placeholder ?? string.Empty;
		Control.SetPlaceholderTextColor(Element.PlaceholderColor.ToGtkColor());
	}

	private void UpdateTextColor()
	{
		Control.SetTextColor(Element.TextColor.ToGtkColor());
	}

	private void UpdateCancelButtonColor()
	{
		if (!Element.CancelButtonColor.IsDefaultOrTransparent())
		{
			Control.SetCancelButtonColor(Element.CancelButtonColor.ToGtkColor());
		}
	}

	private void UpdateFont()
	{
		Pango.FontDescription fontDescription = Helpers.FontDescriptionHelper.CreateFontDescription(
			Element.FontSize, Element.FontFamily, Element.FontAttributes);
		Control.SetFont(fontDescription);
	}

	private void UpdateAlignment()
	{
		Control.SetAlignment(Element.HorizontalTextAlignment.ToNativeValue());
	}

	private void SearchTextChanged(object sender, EventArgs e)
	{
		// Ignore the intermediate Changed our own UpdateText raises; writing the momentary ""
		// back would re-enter set_Text and spin - see _settingText.
		if (_settingText)
			return;

		ElementController.SetValueFromRenderer(SearchBar.TextProperty, Control.SearchText);
	}

	private void SearchButtonClicked(object sender, EventArgs e)
	{
		Element.OnSearchButtonPressed();
	}

	private void SearchFocused(object o, Gtk.FocusedArgs args)
	{
		ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
	}

	private void SearchFocusOutEvent(object o, Gtk.FocusOutEventArgs args)
	{
		ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
	}
}