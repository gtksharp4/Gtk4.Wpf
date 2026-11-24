namespace System.Windows.Controls;

public abstract partial class Control
{
	// The CSS node that paints each native control's face. The theme paints these with background-image and
	// box-shadow, which cover a plain background-color, so an explicit brush also clears them.
	// Inner nodes take a provider on the child widget (GTK 4 providers do not reach child nodes).
	// Check/radio buttons paint the indicator ('check'/'radio'): WPF binds Background/BorderBrush there.
	static string? ChildFace(Gtk.Widget widget) => widget switch
	{
		Gtk.CheckButton _ => "check",
		Gtk.DropDown _ => "button",
		_ => null,
	};

	static bool IsButtonFace(Gtk.Widget widget) => widget is Gtk.Button || widget is Gtk.ToggleButton;

	static bool IsThemedFace(Gtk.Widget widget) =>
		IsButtonFace(widget) || widget is Gtk.Entry || widget is Gtk.PasswordEntry || widget is Gtk.TextView;

	bool FaceIsRoot => ChildFace(Widget) == null;

	string FaceDeclarations()
	{
		string background = ElementCssStyle.Background(this);
		string border = ElementCssStyle.Border(this);
		if (background.Length > 0 && (IsThemedFace(Widget) || ChildFace(Widget) != null))
			background = ClearTheme(background);
		return ElementCssStyle.Join(background, border);
	}

	// Solid: colour plus no theme image. Gradient: the image alone, with the theme colour made transparent.
	static string ClearTheme(string background) => background.StartsWith("background-image", StringComparison.Ordinal)
		? "background-color: transparent; " + background + " box-shadow: none;"
		: background + " background-image: none; box-shadow: none;";

	string FaceRule()
	{
		var selector = ChildFace(Widget);
		if (selector == null) return string.Empty;
		string declarations = FaceDeclarations();
		if (declarations.Length == 0) return string.Empty;
		return selector + " { " + declarations + " }";
	}
}
