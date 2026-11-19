using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Themes;

/// <summary>
/// Seeds the <see cref="ThemeManager"/> with default <see cref="Style"/>s and
/// <see cref="Controls.ControlTemplate"/>s for the built-in controls — the code
/// equivalent of WPF's <c>generic.xaml</c> theme dictionary.
/// <para>
/// Loading from an actual <c>generic.xaml</c> file requires build-time compiler
/// support that is planned for a later round; styles are registered in code here.
/// </para>
/// Call <see cref="Register"/> once at startup (e.g. from <see cref="Application"/> ctor).
/// </summary>
public static partial class Generic
{
	static bool _registered;

	/// <summary>
	/// Populates the <see cref="ThemeManager"/> with all built-in default styles.
	/// Idempotent — safe to call multiple times.
	/// </summary>
	public static void Register()
	{
		if (_registered) return;
		_registered = true;

		RegisterButton();
		RegisterCheckBox();
		RegisterRadioButton();
		RegisterLabel();
		RegisterTextBox();
		RegisterBorder();
		RegisterToggleButton();
		RegisterRepeatButton();
		RegisterPasswordBox();
	}

	/// <summary>
	/// Resets the registration flag and re-registers all default styles.
	/// Intended for unit tests that need a clean ThemeManager state.
	/// </summary>
	internal static void Reset()
	{
		_registered = false;
		Register();
	}

	// ── Button ────────────────────────────────────────────────────────────────

	static void RegisterButton()
	{
		var style = new Style(typeof(Button));
		style.Setters.Add(ControlText());
		style.Setters.Add(new Setter
		{
			Property = Control.PaddingProperty,
			Value = new Thickness(6, 2, 6, 2)
		});
		style.Setters.Add(new Setter
		{
			Property = Control.HorizontalContentAlignmentProperty,
			Value = HorizontalAlignment.Center
		});
		style.Setters.Add(new Setter
		{
			Property = Control.VerticalContentAlignmentProperty,
			Value = VerticalAlignment.Center
		});
		// Note: a managed ControlTemplate that replaces native-widget rendering is a
		// later-round feature. The template factory machinery is kept intact but is NOT
		// seeded in the Button default style — the native Gtk.Button is the renderer.
		ThemeManager.RegisterDefaultStyle(typeof(Button), style);
	}

	// ── CheckBox ──────────────────────────────────────────────────────────────

	static void RegisterCheckBox()
	{
		var style = new Style(typeof(CheckBox));
		style.Setters.Add(ControlText());
		style.Setters.Add(new Setter
		{
			Property = Control.PaddingProperty,
			Value = new Thickness(2)
		});
		style.Setters.Add(new Setter
		{
			Property = Control.VerticalContentAlignmentProperty,
			Value = VerticalAlignment.Center
		});
		ThemeManager.RegisterDefaultStyle(typeof(CheckBox), style);
	}

	// ── RadioButton ───────────────────────────────────────────────────────────

	static void RegisterRadioButton()
	{
		var style = new Style(typeof(RadioButton));
		style.Setters.Add(ControlText());
		style.Setters.Add(new Setter
		{
			Property = Control.PaddingProperty,
			Value = new Thickness(2)
		});
		style.Setters.Add(new Setter
		{
			Property = Control.VerticalContentAlignmentProperty,
			Value = VerticalAlignment.Center
		});
		ThemeManager.RegisterDefaultStyle(typeof(RadioButton), style);
	}

	// ── Label ─────────────────────────────────────────────────────────────────

	static void RegisterLabel()
	{
		var style = new Style(typeof(Label));
		style.Setters.Add(ControlText());
		style.Setters.Add(new Setter
		{
			Property = Control.PaddingProperty,
			Value = new Thickness(5, 0, 0, 0)
		});
		style.Setters.Add(new Setter
		{
			Property = Control.HorizontalContentAlignmentProperty,
			Value = HorizontalAlignment.Left
		});
		style.Setters.Add(new Setter
		{
			Property = Control.VerticalContentAlignmentProperty,
			Value = VerticalAlignment.Top
		});
		ThemeManager.RegisterDefaultStyle(typeof(Label), style);
	}

	// ── TextBox ───────────────────────────────────────────────────────────────

	static void RegisterTextBox()
	{
		var style = new Style(typeof(TextBox));
		style.Setters.Add(new Setter
		{
			Property = Control.ForegroundProperty,
			Value = SystemColors.WindowTextBrush
		});
		ThemeManager.RegisterDefaultStyle(typeof(TextBox), style);
	}

	// WPF themes colour these controls with ControlTextBrush, so Window.Foreground does not reach them.
	static Setter ControlText() => new Setter
	{
		Property = Control.ForegroundProperty,
		Value = SystemColors.ControlTextBrush
	};

	// ── Border ────────────────────────────────────────────────────────────────

	static void RegisterBorder()
	{
		var style = new Style(typeof(Border));
		style.Setters.Add(new Setter
		{
			Property = Border.BorderThicknessProperty,
			Value = new Thickness(0)
		});
		style.Setters.Add(new Setter
		{
			Property = Border.PaddingProperty,
			Value = new Thickness(0)
		});
		ThemeManager.RegisterDefaultStyle(typeof(Border), style);
	}
}