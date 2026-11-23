using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class FixedPageShapeTests : WpfGtkTestBase
{
	[Fact]
	public void FixedPage_derives_from_FrameworkElement_not_Panel() => Run(() =>
	{
		Assert.False(typeof(Panel).IsAssignableFrom(typeof(FixedPage)));
		Assert.Same(typeof(FrameworkElement), typeof(FixedPage).BaseType);
	});

	[Fact]
	public void FixedPage_Background_is_the_Panel_property_with_a_second_owner() => Run(() =>
	{
		Assert.Same(Panel.BackgroundProperty, FixedPage.BackgroundProperty);
		var red = new SolidColorBrush(Colors.Red);
		var page = new FixedPage { Background = red };
		Assert.Same(red, page.Background);
		Assert.Same(red, page.GetValue(Panel.BackgroundProperty));
	});

	[Fact]
	public void FixedPage_Children_is_a_content_property_with_visual_and_logical_children() => Run(() =>
	{
		var page = new FixedPage();
		var button = new Button();
		((IAddChild)page).AddChild(button);
		Assert.Single(page.Children);
		Assert.Same(page, LogicalTreeHelper.GetParent(button));
		Assert.Same(page, VisualTreeHelper.GetParent(button));
		Assert.Equal(1, VisualTreeHelper.GetChildrenCount(page));
		Assert.Same(button, LogicalTreeHelper.GetChildren(page).Cast<object>().Single());
		page.Children.Remove(button);
		Assert.Null(LogicalTreeHelper.GetParent(button));
		Assert.Equal(0, VisualTreeHelper.GetChildrenCount(page));
		var attr = (ContentPropertyAttribute)typeof(FixedPage)
			.GetCustomAttributes(typeof(ContentPropertyAttribute), true)
			.Single();
		Assert.Equal("Children", attr.Name);
	});
	[Fact]
	public void A_DocumentViewer_hosts_the_pages_and_places_their_children_on_GTK() => Run(() =>
	{
		var page = new FixedPage { Width = 200, Height = 100, Background = Brushes.White };
		var button = new Button { Content = "x" };
		var origin = new Button { Content = "x" };
		FixedPage.SetLeft(button, 10);
		page.Children.Add(button);
		page.Children.Add(origin);
		var content = new PageContent();
		content.Child = page;
		var document = new FixedDocument();
		document.Pages.Add(content);
		var window = new Window { Content = new DocumentViewer { Document = document }, Width = 400, Height = 300 };
		window.Show();
		Assert.True(GtkPump.Until(() => button.Widget.IsMapped && button.Widget.Width > 0));
		Assert.Equal(200, page.Widget.WidthRequest);
		Assert.Equal(100, page.Widget.HeightRequest);
		var fixedPage = (Gtk.Fixed)page.Widget;
		fixedPage.GetChildPosition(button.Widget, out var x, out var y);
		fixedPage.GetChildPosition(origin.Widget, out var x0, out var y0);
		// GTK adds a widget-specific offset to both, so the placement is measured against an unplaced sibling.
		Assert.Equal(10, x - x0);
		Assert.Equal(0, y - y0);
		window.Close();
	});

	[Fact]
	public void PageContent_Child_can_be_set_once_and_paginates() => Run(() =>
	{
		var first = new FixedPage { Width = 10, Height = 20 };
		var second = new FixedPage { Width = 30, Height = 40 };
		var content = new PageContent();
		content.Child = first;
		Assert.Same(first, content.Child);
		Assert.Same(content, LogicalTreeHelper.GetParent(first));
		Assert.Throws<System.InvalidOperationException>(() => content.Child = second);
		Assert.Same(first, content.Child);
		Assert.Same(content, LogicalTreeHelper.GetParent(first));
		Assert.Null(LogicalTreeHelper.GetParent(second));
		var document = new FixedDocument();
		document.Pages.Add(content);
		Assert.Same(first, document.DocumentPaginator.GetPage(0).Visual);
	});

	[Fact]
	public void PageContent_Child_rejects_null_and_a_second_AddChild() => Run(() =>
	{
		var content = new PageContent();
		Assert.Throws<System.ArgumentNullException>(() => content.Child = null);
		var page = new FixedPage();
		((IAddChild)content).AddChild(page);
		Assert.Same(page, content.Child);
		Assert.Throws<System.InvalidOperationException>(() => ((IAddChild)content).AddChild(new FixedPage()));
		Assert.Throws<System.InvalidOperationException>(() => content.Child = new FixedPage());
		Assert.Same(page, content.GetPageRoot(false));
	});

	[Fact]
	public void PageContent_Child_can_be_set_while_Source_is_non_null() => Run(() =>
	{
		var content = new PageContent { Source = new System.Uri("a.fpage", System.UriKind.Relative) };
		var page = new FixedPage();
		content.Child = page;
		Assert.Same(page, content.Child);
		var other = new PageContent { Source = new System.Uri("b.fpage", System.UriKind.Relative) };
		((IAddChild)other).AddChild(new FixedPage());
		Assert.NotNull(other.Child);
	});
}
