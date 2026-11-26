using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A ContextMenu shared through a resource or style follows the element that opens it.</summary>
public class ContextMenuSharedTests : WpfGtkTestBase
{
	[Fact]
	public void A_shared_ContextMenu_is_anchored_to_the_element_that_opens_it() => Run(() =>
	{
		var menu = new ContextMenu();
		menu.Items.Add(new MenuItem { Header = "x" });
		var first = new Button { ContextMenu = menu };
		var second = new Button { ContextMenu = menu };
		var panel = new StackPanel();
		panel.Children.Add(first);
		panel.Children.Add(second);
		var win = new WpfWindow { Content = panel };
		win.Show();
		GtkPump.For(100);

		menu.Open(first.Widget);
		Assert.Equal(first.Widget.Handle, menu.GtkPopover!.Parent!.Handle);
		menu.Open(second.Widget);
		Assert.Equal(second.Widget.Handle, menu.GtkPopover.Parent!.Handle);
		menu.Open(second.Widget);
		Assert.Equal(second.Widget.Handle, menu.GtkPopover.Parent!.Handle);
		menu.IsOpen = false;
		win.Close();
	});
}
