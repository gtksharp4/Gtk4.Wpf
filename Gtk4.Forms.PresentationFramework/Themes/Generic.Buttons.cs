using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Themes;

public static partial class Generic
{
	// ── ToggleButton / RepeatButton / PasswordBox ───────────────────────────────────

	static void RegisterToggleButton()
	{
		var style = new Style(typeof(ToggleButton));
		style.Setters.Add(ControlText());
		ThemeManager.RegisterDefaultStyle(typeof(ToggleButton), style);
	}

	static void RegisterRepeatButton()
	{
		var style = new Style(typeof(RepeatButton));
		style.Setters.Add(ControlText());
		ThemeManager.RegisterDefaultStyle(typeof(RepeatButton), style);
	}

	static void RegisterPasswordBox()
	{
		var style = new Style(typeof(PasswordBox));
		style.Setters.Add(new Setter
		{
			Property = Control.ForegroundProperty,
			Value = SystemColors.WindowTextBrush
		});
		ThemeManager.RegisterDefaultStyle(typeof(PasswordBox), style);
	}
}
