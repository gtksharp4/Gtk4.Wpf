using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Real-widget tests for object / markup-extension Setter values. Appearance assertions target the CSS
/// the per-widget provider carries, as in the Appearance suite.
/// </summary>
public class StyleTemplateSetterGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Style_Template_setter_object_reaches_Button_and_builds_template_tree() => Run(() =>
	{
		var root = new Border();
		var template = new ControlTemplate { TargetType = typeof(Button), Factory = () => root };
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter(Control.TemplateProperty, template));

		var button = new Button { Style = style };

		Assert.Same(template, button.Template);
		Assert.Same(button, root.TemplatedParent);
		Assert.IsType<Gtk.Button>(button.Widget);
	});

	[Fact]
	public void Style_DynamicResource_Background_emits_css_and_updates_on_replace() => Run(() =>
	{
		var button = new Button();
		button.Resources["bg"] = Brushes.Red;
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension("bg")));
		button.Style = style;
		_ = button.Widget;
		Assert.Contains("background-color: rgba(255,0,0,1);", button.AppearanceCss);

		button.Resources["bg"] = Brushes.Blue;
		Assert.Contains("background-color: rgba(0,0,255,1);", button.AppearanceCss);

		button.Style = null;
		Assert.Equal(string.Empty, button.AppearanceCss);
	});
}
