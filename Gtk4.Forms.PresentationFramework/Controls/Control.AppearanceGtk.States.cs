namespace System.Windows.Controls;

public abstract partial class Control
{
	// WPF's Aero2 ButtonChrome triggers override a local Background on Button, RepeatButton and ToggleButton:
	// IsMouseOver, IsPressed (':active'), IsChecked (':checked') and IsEnabled=false. GTK expresses all four as
	// pseudo-classes. Gap: the Aero2 hover/pressed fade animations and the focus rectangle are not reproduced.
	string StateRules()
	{
		if (!IsButtonFace(Widget) || ElementCssStyle.Background(this).Length == 0) return string.Empty;
		return ElementCssStyle.Join(
			State(":hover", "#BEE6FD", "#3C7FB1"),
			State(":active", "#C4E5F6", "#2C628B"),
			State(":checked", "#BCDDEE", "#245A83"),
			State(":disabled", "#F4F4F4", "#ADB2B5", "color: #838383;"));
	}

	static string State(string pseudo, string background, string border, string extra = "") =>
		$"&{pseudo} {{ background-color: {background}; background-image: none; box-shadow: none; " +
		$"border-color: {border}; {extra}}}";
}
