using System.Windows;
using System.Windows.Media;
using System.Windows.Themes;
using Xunit;
using WpfBorder = System.Windows.Controls.Border;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfContentPresenter = System.Windows.Controls.ContentPresenter;
using WpfControl = System.Windows.Controls.Control;
using WpfControlTemplate = System.Windows.Controls.ControlTemplate;
using WpfLabel = System.Windows.Controls.Label;
using WpfRadioButton = System.Windows.Controls.RadioButton;
using WpfSetter = System.Windows.Setter;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for Round-12 track B: Generic theme-dictionary registration,
/// default ControlTemplate, SystemColors, and Control alignment/padding DPs.
/// </summary>
public class DefaultStyleGenericTests
{
	// ── Generic registration ──────────────────────────────────────────────────

	[Fact]
	public void Generic_registers_Button_defaultStyle()
	{
		Generic.Reset();
		Assert.NotNull(ThemeManager.FindDefaultStyle(typeof(WpfButton)));
	}

	[Fact]
	public void Generic_registers_CheckBox_defaultStyle()
	{
		Generic.Reset();
		Assert.NotNull(ThemeManager.FindDefaultStyle(typeof(WpfCheckBox)));
	}

	[Fact]
	public void Generic_registers_RadioButton_defaultStyle()
	{
		Generic.Reset();
		Assert.NotNull(ThemeManager.FindDefaultStyle(typeof(WpfRadioButton)));
	}

	[Fact]
	public void Generic_registers_Label_defaultStyle()
	{
		Generic.Reset();
		Assert.NotNull(ThemeManager.FindDefaultStyle(typeof(WpfLabel)));
	}

	[Fact]
	public void Generic_registers_Border_defaultStyle()
	{
		Generic.Reset();
		Assert.NotNull(ThemeManager.FindDefaultStyle(typeof(WpfBorder)));
	}

	// ── Default ControlTemplate ───────────────────────────────────────────────

	/// <summary>
	/// The Button default style has NO managed ControlTemplate — rendering is
	/// delegated to the native Gtk.Button widget.  Applying managed templates over
	/// native-widget controls is a later-round feature; the template-factory
	/// machinery remains but is not seeded in the default style.
	/// </summary>
	[Fact]
	public void Button_defaultStyle_has_no_ControlTemplate()
	{
		Generic.Reset();

		var style = ThemeManager.FindDefaultStyle(typeof(WpfButton));
		Assert.NotNull(style);

		bool hasTemplate = false;
		foreach (var setter in style!.Setters)
		{
			if (setter is WpfSetter s && s.Property == WpfControl.TemplateProperty)
				hasTemplate = true;
		}
		Assert.False(hasTemplate,
			"Button default style must not include a ControlTemplate setter (native rendering).");
	}

	/// <summary>
	/// The ControlTemplate factory machinery is intact: creating one manually produces
	/// a Border → ContentPresenter tree.
	/// </summary>
	[Fact]
	public void ControlTemplate_factory_machinery_intact()
	{
		var template = new WpfControlTemplate(typeof(WpfButton))
		{
			Factory = () =>
			{
				var border = new WpfBorder { BorderThickness = new Thickness(1) };
				border.Child = new WpfContentPresenter();
				return border;
			}
		};
		Assert.True(template.HasContent);
		var root = template.CreateVisualTree();
		Assert.IsType<WpfBorder>(root);
		Assert.IsType<WpfContentPresenter>(((WpfBorder)root!).Child);
	}

	// ── Control.PaddingProperty ───────────────────────────────────────────────

	[Fact]
	public void Button_defaultStyle_has_Padding_setter()
	{
		Generic.Reset();

		var style = ThemeManager.FindDefaultStyle(typeof(WpfButton));
		Assert.NotNull(style);

		bool hasPadding = false;
		foreach (var setter in style!.Setters)
		{
			if (setter is WpfSetter s && s.Property == WpfControl.PaddingProperty)
				hasPadding = true;
		}
		Assert.True(hasPadding);
	}

	// ── SystemColors ──────────────────────────────────────────────────────────

	/// <summary>SystemColors.ControlBrush is non-null and is a SolidColorBrush.</summary>
	[Fact]
	public void SystemColors_ControlBrush_isNonNull()
	{
		Assert.NotNull(SystemColors.ControlBrush);
		Assert.IsType<SolidColorBrush>(SystemColors.ControlBrush);
	}

	[Fact]
	public void SystemColors_WindowBrush_isNonNull()
	{
		Assert.NotNull(SystemColors.WindowBrush);
	}

	[Fact]
	public void SystemColors_HighlightBrush_isNonNull()
	{
		Assert.NotNull(SystemColors.HighlightBrush);
	}

	[Fact]
	public void SystemColors_ControlTextBrush_isNonNull()
	{
		Assert.NotNull(SystemColors.ControlTextBrush);
	}
}