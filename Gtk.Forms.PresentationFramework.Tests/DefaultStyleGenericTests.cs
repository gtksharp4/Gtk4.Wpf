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

namespace Gtk.Forms.PresentationFramework.Tests
{
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
		/// A Button registered in Generic has a non-null Template from the default
		/// style.  The template factory builds a visual tree: Border → ContentPresenter.
		/// </summary>
		[Fact]
		public void Button_defaultStyle_has_ControlTemplate()
		{
			Generic.Reset();

			var style = ThemeManager.FindDefaultStyle(typeof(WpfButton));
			Assert.NotNull(style);

			WpfControlTemplate? template = null;
			foreach (var setter in style!.Setters)
			{
				if (setter is WpfSetter s && s.Property == WpfControl.TemplateProperty)
					template = s.Value as WpfControlTemplate;
			}
			Assert.NotNull(template);
			Assert.True(template!.HasContent);

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
}
