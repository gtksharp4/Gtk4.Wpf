namespace System.Windows.Controls;

public abstract partial class Control
{
	// The CSS node that paints each native control's face. The theme paints these with background-image and
	// box-shadow, which cover a plain background-color, so an explicit brush also clears them.
	// Inner nodes take a provider on the child widget (GTK 4 providers do not reach child nodes).
	// Check/radio buttons paint the indicator: WPF binds Background/BorderBrush there. GTK 4 names that node
	// "radio" once a Group is set and "check" otherwise, so the rule names both.
	// A multiline TextBox paints its TextView, an editable ComboBox its entry and its dropdown button.
	// Gap: the scrolled window around a multiline TextBox keeps the theme frame.
	(Gtk.Widget Target, string Selector)[] FaceNodes() => Widget switch
	{
		Gtk.CheckButton c when c.FirstChild != null => new[] { (c.FirstChild, "check, radio") },
		Gtk.DropDown d when d.FirstChild != null => new[] { (d.FirstChild, "button") },
		Gtk.ScrolledWindow s when s.Child is Gtk.TextView t => new (Gtk.Widget, string)[] { (t, "textview, text") },
		Gtk.Box b when b.FirstChild is Gtk.Entry e && b.LastChild is Gtk.DropDown d && d.FirstChild != null =>
			new (Gtk.Widget, string)[] { (e, "entry"), (d.FirstChild, "button") },
		_ => Array.Empty<(Gtk.Widget, string)>(),
	};

	static bool IsButtonFace(Gtk.Widget widget) => widget is Gtk.Button || widget is Gtk.ToggleButton;

	static bool IsThemedFace(Gtk.Widget widget) =>
		IsButtonFace(widget) || widget is Gtk.Entry || widget is Gtk.PasswordEntry || widget is Gtk.TextView;

	bool FaceIsRoot => FaceNodes().Length == 0;

	string FaceDeclarations()
	{
		string background = ElementCssStyle.Background(this);
		string border = ElementCssStyle.Border(this);
		if (background.Length > 0 && (IsThemedFace(Widget) || !FaceIsRoot))
			background = ClearTheme(background);
		return ElementCssStyle.Join(background, border);
	}

	// Solid: colour plus no theme image. Gradient: the image alone, with the theme colour made transparent.
	static string ClearTheme(string background) => background.StartsWith("background-image", StringComparison.Ordinal)
		? "background-color: transparent; " + background + " box-shadow: none;"
		: background + " background-image: none; box-shadow: none;";

	string FaceRule(int index)
	{
		var nodes = FaceNodes();
		if (index >= nodes.Length) return string.Empty;
		string declarations = FaceDeclarations();
		if (declarations.Length == 0) return string.Empty;
		return nodes[index].Selector + " { " + declarations + " }";
	}

	Gtk.Widget? FaceTarget(int index)
	{
		var nodes = FaceNodes();
		return index < nodes.Length ? nodes[index].Target : null;
	}
}
