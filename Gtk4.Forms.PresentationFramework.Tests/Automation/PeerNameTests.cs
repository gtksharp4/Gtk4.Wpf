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
	[InlineData("trailing_", "trailing")]
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
	public void ToolTip_is_the_last_name_fallback() => Run(() =>
	{
		var button = new Button();
		button.ToolTip = "tip";
		Assert.Equal("tip", NameOf(button));
		button.Content = "Label";
		Assert.Equal("Label", NameOf(button));
	});
}
