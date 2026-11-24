using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Automation;

/// <summary>The plain-text names peers report, as WPF derives them from content.</summary>
public class PeerNameTests : WpfGtkTestBase
{
	static string NameOf(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element)!.GetName();

	[Theory]
	[InlineData("_OK", "OK")]
	[InlineData("Save _As", "Save As")]
	[InlineData("A__B", "A_B")]
	[InlineData("plain", "plain")]
	[InlineData("trailing_", "trailing_")]
	[InlineData("a_b_c", "ab_c")]
	public void Access_key_markers_are_removed_from_string_content(string content, string name) => Run(() =>
	{
		Assert.Equal(name, NameOf(new Button { Content = content }));
		Assert.Equal(name, NameOf(new Label { Content = content }));
	});

	[Fact]
	public void Non_string_content_gives_its_plain_text() => Run(() =>
	{
		Assert.Equal("Go", NameOf(new Button { Content = new TextBlock { Text = "Go" } }));
		var inner = new Button { Content = new TextBlock { Text = "Deep" } };
		Assert.Equal("Deep", NameOf(new CheckBox { Content = inner }));
		Assert.Equal("Wrapped", NameOf(new Label { Content = new TextBlock { Text = "Wrapped" } }));
	});

	[Fact]
	public void ListBoxItem_items_are_named_after_their_content() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add(new ListBoxItem { Content = "Apple" });
		list.Items.Add(new ListBoxItem { Content = new TextBlock { Text = "Pear" } });
		list.Items.Add("plain");
		var window = new Window { Content = list };
		_ = list.Widget;
		var items = UIElementAutomationPeer.CreatePeerForElement(list)!.GetChildren()!;
		Assert.Equal(new[] { "Apple", "Pear", "plain" }, new[] { items[0].GetName(), items[1].GetName(),
			items[2].GetName() });
		var explicitName = (ListBoxItem)list.Items[0]!;
		AutomationProperties.SetName(explicitName, "Fruit");
		Assert.Equal("Fruit", items[0].GetName());
	});

	[Fact]
	public void Items_keep_underscores_and_the_menu_item_header_loses_its_marker() => Run(() =>
	{
		Assert.Equal("file_name.txt", NameOf(new ListBoxItem { Content = "file_name.txt" }));
		Assert.Equal("a_b", NameOf(new ComboBoxItem { Content = "a_b" }));
		Assert.Equal("a_b", NameOf(new TreeViewItem { Header = "a_b" }));
		Assert.Equal("Tab", NameOf(new TabItem { Header = "_Tab" }));
		Assert.Equal("Group", NameOf(new GroupBox { Header = "_Group" }));
		Assert.Equal("_Exp", NameOf(new Expander { Header = "_Exp" }));
		Assert.Equal("Open", NameOf(new MenuItem { Header = "_Open" }));
	});

	[Fact]
	public void ToolTip_is_the_last_name_fallback() => Run(() =>
	{
		var button = new Button();
		button.ToolTip = "tip";
		Assert.Equal("tip", NameOf(button));
		button.Content = "Label";
		Assert.Equal("Label", NameOf(button));
	});
}
