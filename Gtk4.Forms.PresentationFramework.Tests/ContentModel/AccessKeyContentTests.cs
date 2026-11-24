using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Gtk4.Forms.PresentationFramework.Tests.AccessKeys;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.ContentModel;

/// <summary>Content changes keep the access key in step through the shared hosting path.</summary>
public class AccessKeyContentTests : AccessKeyTestBase
{
	public static readonly TheoryData<string> Kinds =
		new() { "Button", "RepeatButton", "ToggleButton", "CheckBox", "RadioButton" };

	static ButtonBase Make(string kind, object? content) => kind switch
	{
		"Button" => new Button { Content = content },
		"RepeatButton" => new RepeatButton { Content = content },
		"ToggleButton" => new ToggleButton { Content = content },
		"CheckBox" => new CheckBox { Content = content },
		_ => new RadioButton { Content = content },
	};

	[Theory]
	[MemberData(nameof(Kinds))]
	public void Content_changing_from_a_mnemonic_string_to_an_element_unregisters_the_key(string kind) => Run(() =>
	{
		var button = Make(kind, "_Q");
		var window = Show(button);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "Q"));
		button.Content = new TextBlock { Text = "element" };
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "Q"));
		button.Content = "_R";
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "R"));
		window.Close();
	});

	[Theory]
	[MemberData(nameof(Kinds))]
	public void Content_changing_from_A_to_B_moves_the_mnemonic(string kind) => Run(() =>
	{
		var button = Make(kind, "_A");
		var window = Show(button);
		button.Content = "_B";
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "A"));
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "B"));
		window.Close();
	});

	[Fact]
	public void Content_set_before_the_widget_exists_registers_the_key_immediately() => Run(() =>
	{
		var button = new Button();
		button.Content = "_Z";
		var window = Show(button);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "Z"));
		window.Close();
	});

	[Fact]
	public void Label_content_changing_to_an_element_unregisters_the_key() => Run(() =>
	{
		var box = new TextBox();
		var label = new Label { Content = "_L", Target = box };
		var panel = new StackPanel();
		panel.Children.Add(label);
		panel.Children.Add(box);
		var window = Show(panel);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "L"));
		label.Content = new TextBlock { Text = "element" };
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "L"));
		label.Content = "_M";
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "M"));
		window.Close();
	});
}
