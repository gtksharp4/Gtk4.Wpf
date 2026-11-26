using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ContentElementContextMenuTests : WpfGtkTestBase
{
	[Fact]
	public void FlowDocument_and_TextElement_store_a_ContextMenu() => Run(() =>
	{
		var menu = new ContextMenu();
		var doc = new FlowDocument { ContextMenu = menu };
		Assert.Same(menu, doc.ContextMenu);
		Assert.Same(menu, doc.GetValue(FrameworkContentElement.ContextMenuProperty));
		var run = new Run("x");
		Assert.Null(run.ContextMenu);
		run.ContextMenu = menu;
		Assert.Same(menu, FrameworkElement.GetContextMenu(run));
	});

	[Fact]
	public void The_property_element_syntax_assigns_it_from_xaml() => Run(() =>
	{
		const string xaml = "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
			+ "<FlowDocument.ContextMenu><ContextMenu><MenuItem Header=\"Add\"/></ContextMenu></FlowDocument.ContextMenu>"
			+ "<Paragraph>Hi</Paragraph></FlowDocument>";
		var doc = (FlowDocument)System.Windows.Markup.XamlReader.Parse(xaml);
		Assert.NotNull(doc.ContextMenu);
		Assert.Equal(1, doc.ContextMenu!.Items.Count);
	});
}
