using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>InlineUIContainer is store-only; MenuItem.Items accepts Separator as a section break.</summary>
public class InlineContainerAndMenuSeparatorTests : WpfGtkTestBase
{
	[Fact]
	public void InlineUIContainer_stores_its_child()
	{
		var button = new Button();
		Assert.Same(button, new InlineUIContainer(button).Child);
		Assert.Null(new InlineUIContainer().Child);
		Assert.IsAssignableFrom<Inline>(new InlineUIContainer());
	}

	[Fact]
	public void TextBlock_with_an_InlineUIContainer_realizes_without_throwing() => Run(() =>
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "a " });
		tb.Inlines.Add(new InlineUIContainer(new Button()));
		tb.Inlines.Add(new Span());
		((Span)tb.Inlines[2]).Inlines.Add(new InlineUIContainer(new TextBox()));
		Assert.NotNull(tb.Widget);
	});

	[Fact]
	public void MenuItem_Items_holds_items_and_separators() => Run(() =>
	{
		var file = new MenuItem { Header = "F" };
		file.Items.Add(new MenuItem { Header = "A" });
		file.Items.Add(new Separator());
		file.Items.Add(new MenuItem { Header = "B" });
		file.Items.Add("store-only");
		Assert.Equal(4, file.Items.Count);

		var popover = ((Gtk.MenuButton)file.Widget).Popover!;
		var box = (Gtk.Box)popover.Child!;
		var children = new System.Collections.Generic.List<Gtk.Widget>();
		for (var c = box.FirstChild; c != null; c = c.NextSibling) children.Add(c);
		Assert.Equal(3, children.Count);
		Assert.IsType<Gtk.Separator>(children[1]);
		Assert.Equal(2, file.Items.OfType<MenuItem>().Count());
	});

	[Fact]
	public void MenuItem_with_only_a_separator_builds_a_leaf_widget() => Run(() =>
	{
		var file = new MenuItem { Header = "F" };
		file.Items.Add(new Separator());
		Assert.IsType<Gtk.Button>(file.Widget);
	});

	[Fact]
	public void ToggleButton_OnToggle_is_protected_not_internal()
	{
		var m = typeof(System.Windows.Controls.Primitives.ToggleButton).GetMethod("OnToggle",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
		Assert.True(m.IsFamily);
		Assert.False(m.IsFamilyOrAssembly);
		var r = typeof(RadioButton).GetMethod("OnToggle",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly)!;
		Assert.True(r.IsFamily);
	}
}
