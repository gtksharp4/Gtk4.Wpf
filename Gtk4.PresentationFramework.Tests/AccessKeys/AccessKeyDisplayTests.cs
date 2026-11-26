using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AccessKeys;

/// <summary>GTK label text and key registration for the controls whose WPF template recognizes access keys.</summary>
public class AccessKeyDisplayTests : AccessKeyTestBase
{
	const uint VoidSymbol = 0xffffff;

	public static readonly TheoryData<string> Texts = new() { "_Open", "__Open", "Open_", "A__B" };


	[Theory]
	[InlineData("_Open", "Open", 'O')]
	[InlineData("__Open", "_Open", '\0')]
	[InlineData("Open_", "Open_", '\0')]
	[InlineData("A__B", "A_B", '\0')]
	[InlineData("a_b_c", "ab_c", 'b')]
	[InlineData("x_", "x_", '\0')]
	public void A_GTK_label_shows_the_WPF_display_text_and_mnemonic(string wpf, string shown, char key) => Run(() =>
	{
		var label = new Gtk.Label(string.Empty);
		label.Text = AccessKeyParser.ToGtkMnemonic(wpf);
		label.UseUnderline = true;
		Assert.Equal(shown, label.Text);
		// "No mnemonic" is GDK_KEY_VoidSymbol on GTK 4.22 and 0 on older GTK 4.
		if (key == (char)0) Assert.True(label.MnemonicKeyval is VoidSymbol or 0);
		else Assert.Equal(key, label.MnemonicKeyval);
	});

	[Theory]
	[MemberData(nameof(Texts))]
	public void Button_family_sets_mnemonic_text_with_UseUnderline(string text) => Run(() =>
	{
		string expected = AccessKeyParser.ToGtkMnemonic(text);
		foreach (var button in new ButtonBase[]
			{ new Button { Content = text }, new RepeatButton { Content = text }, new ToggleButton { Content = text } })
		{
			var gtk = (Gtk.Button)button.Widget;
			Assert.True(gtk.UseUnderline);
			Assert.Equal(expected, gtk.Label);
			Assert.Equal(AccessKeyParser.Parse(text).DisplayText, ((Gtk.Label)gtk.Child!).Text);
		}
		var check = (Gtk.CheckButton)new CheckBox { Content = text }.Widget;
		Assert.True(check.UseUnderline);
		Assert.Equal(expected, check.Label);
		var radio = (Gtk.CheckButton)new RadioButton { Content = text }.Widget;
		Assert.True(radio.UseUnderline);
		Assert.Equal(expected, radio.Label);
	});

	[Theory]
	[MemberData(nameof(Texts))]
	public void Label_menu_item_group_expander_and_tab_set_mnemonic_text(string text) => Run(() =>
	{
		string expected = AccessKeyParser.ToGtkMnemonic(text);
		string shown = AccessKeyParser.Parse(text).DisplayText;
		var label = new Label { Content = text };
		_ = label.Widget;
		var inner = (Gtk.Label)label.AccessibleTarget;
		Assert.Equal(shown, inner.Text);
		Assert.True(inner.UseUnderline);

		var item = new MenuItem { Header = text };
		var itemButton = (Gtk.Button)item.Widget;
		Assert.True(itemButton.UseUnderline);
		Assert.Equal(expected, itemButton.Label);

		var group = new GroupBox { Header = text };
		var frameLabel = (Gtk.Label)((Gtk.Frame)group.Widget).LabelWidget!;
		Assert.Equal(shown, frameLabel.Text);
		Assert.True(frameLabel.UseUnderline);

		var expander = (Gtk.Expander)new Expander { Header = text }.Widget;
		Assert.True(expander.UseUnderline);
		Assert.Equal(expected, expander.Label);

		var tabLabel = new TabItem { Header = text }.CreateTabLabel();
		Assert.Equal(shown, tabLabel.Text);
		Assert.True(tabLabel.UseUnderline);
	});

	[Fact]
	public void Controls_without_recognition_keep_underscores_literal() => Run(() =>
	{
		var block = new TextBlock { Text = "a_b" };
		Assert.Equal("a_b", ((Gtk.Label)block.Widget).Text);
		var list = new ListBox();
		list.Items.Add(new ListBoxItem { Content = "a_b" });
		var window = Show(list);
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "B"));
		Assert.Equal("a_b", ((ListBoxItem)list.Items[0]!).Content);
		var content = new ContentControl();
		_ = content.Widget;
		content.Content = "a_b";
		Assert.Equal("a_b", ((Gtk.Label)content.Widget.FirstChild!).Text);
		window.Close();
	});

	[Fact]
	public void Changing_content_re_registers_and_clearing_it_unregisters() => Run(() =>
	{
		var button = new Button { Content = "_A" };
		var window = Show(button);
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "a"));
		button.Content = "_B";
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "A"));
		Assert.True(AccessKeyManager.IsKeyRegistered(window, "B"));
		button.Content = "B";
		Assert.False(AccessKeyManager.IsKeyRegistered(window, "B"));
		window.Close();
	});

	[Theory]
	[InlineData("_Open", 'O')]
	[InlineData("__Open", '\0')]
	[InlineData("Open_", '\0')]
	[InlineData("A__B", '\0')]
	[InlineData("a_b_c", 'b')]
	[InlineData("", '\0')]
	public void AccessText_exposes_the_key_and_the_GTK_label(string text, char key) => Run(() =>
	{
		var access = new AccessText { Text = text };
		Assert.Equal(key, access.AccessKey);
		var gtk = (Gtk.Label)access.Widget;
		Assert.Equal(AccessKeyParser.Parse(text).DisplayText, gtk.Text);
		Assert.True(gtk.UseUnderline);
	});

	[Fact]
	public void AccessText_registers_itself_and_re_registers_when_Text_changes() => Run(() =>
	{
		var access = new AccessText { Text = "_Open" };
		var window = Show(access);
		Assert.True(AccessKeyManager.HasRegistrationIn(window, "O"));
		access.Text = "_Save";
		Assert.False(AccessKeyManager.HasRegistrationIn(window, "O"));
		Assert.True(AccessKeyManager.HasRegistrationIn(window, "S"));
		Assert.Equal(string.Empty, new AccessText().Text);
		Assert.Equal(TextWrapping.NoWrap, access.TextWrapping);
		Assert.Equal(TextTrimming.None, access.TextTrimming);
		window.Close();
	});

	[Fact]
	public void ContentPresenter_hosts_an_AccessText_only_when_recognizing_and_underscored() => Run(() =>
	{
		var recognizing = new ContentPresenter { Content = "_File", RecognizesAccessKey = true };
		var gtk = (Gtk.Label)recognizing.Widget.FirstChild!;
		Assert.Equal("File", gtk.Text);
		Assert.Equal('F', (char)gtk.MnemonicKeyval);
		var literal = new ContentPresenter { Content = "_File" };
		Assert.Equal("_File", ((Gtk.Label)literal.Widget.FirstChild!).Text);
	});
}
