using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// The GTK side shared by FlowDocumentScrollViewer and FlowDocumentReader: a scrolled window around one Pango
/// label. The document's Background becomes CSS on the label (which fills the viewport); its FontStyle and
/// FontWeight reach the markup through <see cref="FlowDocument.BuildViewMarkup"/>.
/// </summary>
sealed class FlowDocumentPresenter
{
	readonly Gtk.Label _label;
	readonly ElementCssStyle _css;
	bool _tabsSet;

	internal FlowDocumentPresenter()
	{
		_label = new Gtk.Label(string.Empty)
		{
			Xalign = 0f,
			Yalign = 0f,
			Wrap = true,
			Hexpand = true,
			Vexpand = true
		};
		Window = new Gtk.ScrolledWindow();
		Window.Child = _label;
		_css = new ElementCssStyle(_label);
	}

	internal Gtk.ScrolledWindow Window { get; }

	internal Gtk.Label Label => _label;

	internal void Show(FlowDocument? document)
	{
		if (document == null)
		{
			_label.UseMarkup = false;
			_label.Text = string.Empty;
			_css.Update(string.Empty);
			SetTabStops(null);
			return;
		}
		_label.UseMarkup = true;
		_label.Markup = document.BuildViewMarkup();
		_css.Update(BrushCss.Background(document.Background));
		SetTabStops(document.BuildTabStops());
	}

	// A label has one tab array, so table columns share it; see FlowDocument.BuildTabStops.
	void SetTabStops(int[]? stops)
	{
		if (stops == null)
		{
			if (_tabsSet) _label.Tabs = null!;
			_tabsSet = false;
			return;
		}
		var tabs = new Pango.TabArray(stops.Length, true);
		for (int i = 0; i < stops.Length; i++) tabs.SetTab(i, Pango.TabAlign.Left, stops[i]);
		_label.Tabs = tabs;
		_tabsSet = true;
	}

	internal void SetScrollBars(ScrollBarVisibility horizontal, ScrollBarVisibility vertical)
	{
		Window.HscrollbarPolicy = ScrollViewer.ToGtkPolicy(horizontal);
		Window.VscrollbarPolicy = ScrollViewer.ToGtkPolicy(vertical);
	}
}
