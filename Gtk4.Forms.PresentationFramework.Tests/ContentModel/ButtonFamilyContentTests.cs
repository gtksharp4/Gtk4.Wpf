using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.ContentModel;

/// <summary>Button, RepeatButton, ToggleButton, CheckBox and RadioButton host non-string content.</summary>
public class ButtonFamilyContentTests : ContentModelTestBase
{
	public static readonly TheoryData<string> Kinds = new() { "Button", "RepeatButton", "ToggleButton", "CheckBox", "RadioButton" };

	static ContentControl Make(string kind) => kind switch
	{
		"Button" => new Button(),
		"RepeatButton" => new RepeatButton(),
		"ToggleButton" => new ToggleButton(),
		"CheckBox" => new CheckBox(),
		_ => new RadioButton(),
	};

	[Theory]
	[MemberData(nameof(Kinds))]
	public void A_TextBlock_child_is_shown_inside_the_GTK_button(string kind) => Run(() =>
	{
		var c = Make(kind);
		var text = new TextBlock { Text = "child" };
		c.Content = text;
		Assert.True(IsInside(c.Widget, text.Widget));
		Assert.Equal("child", Assert.Single(Texts(c)));
		Assert.Same(c, text.LogicalParent);
	});

	[Theory]
	[MemberData(nameof(Kinds))]
	public void A_StackPanel_child_is_shown_with_all_its_texts(string kind) => Run(() =>
	{
		var c = Make(kind);
		var panel = new StackPanel();
		panel.Children.Add(new TextBlock { Text = "one" });
		panel.Children.Add(new TextBlock { Text = "two" });
		c.Content = panel;
		Assert.Equal(new[] { "one", "two" }, Texts(c));
	});

	[Theory]
	[MemberData(nameof(Kinds))]
	public void Content_can_switch_between_element_string_and_template(string kind) => Run(() =>
	{
		var c = Make(kind);
		var text = new TextBlock { Text = "el" };
		c.Content = text;
		c.Content = "plain";
		Assert.Equal("plain", Assert.Single(Texts(c)));
		Assert.Null(text.LogicalParent);
		c.ContentTemplate = Template("t:");
		c.Content = new Place { Name = "N" };
		Assert.Equal("t:N", Assert.Single(Texts(c)));
		c.ContentTemplate = null;
		c.Content = "again";
		Assert.Equal("again", Assert.Single(Texts(c)));
	});

	[Theory]
	[MemberData(nameof(Kinds))]
	public void String_content_keeps_the_access_key_mnemonic(string kind) => Run(() =>
	{
		var c = Make(kind);
		c.Content = "_Open";
		var label = Descendants(c.Widget).OfType<Gtk.Label>().Single();
		Assert.Equal("Open", label.Text);
		Assert.True(label.UseUnderline);
	});

	[Fact]
	public void Click_still_fires_for_a_button_with_element_content() => Run(() =>
	{
		var b = new Button { Content = new TextBlock { Text = "x" } };
		_ = b.Widget;
		int clicks = 0;
		b.Click += (_, _) => clicks++;
		GLib.Signal.Emit((Gtk.Button)b.Widget, "clicked");
		Assert.Equal(1, clicks);
	});

	[Fact]
	public void A_CheckBox_with_element_content_still_toggles_and_clicks() => Run(() =>
	{
		var c = new CheckBox { Content = new TextBlock { Text = "x" } };
		int clicks = 0;
		c.Click += (_, _) => clicks++;
		((Gtk.CheckButton)c.Widget).Active = true;
		Assert.True(c.IsChecked);
		Assert.Equal(1, clicks);
	});

	[Fact]
	public void Non_string_content_without_a_template_shows_its_text() => Run(() =>
	{
		var b = new Button { Content = new Place { Name = "Q" } };
		Assert.Equal("Place:Q", Assert.Single(Texts(b)));
	});
}
