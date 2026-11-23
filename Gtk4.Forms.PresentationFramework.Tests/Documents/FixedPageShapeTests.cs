using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
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
	public void A_DocumentViewer_hosts_the_pages_and_their_children_on_GTK() => Run(() =>
	{
		var page = new FixedPage { Width = 200, Height = 100, Background = Brushes.White };
		var button = new Button { Content = "x" };
		FixedPage.SetLeft(button, 10);
		page.Children.Add(button);
		var content = new PageContent();
		content.Child = page;
		var document = new FixedDocument();
		document.Pages.Add(content);
		var viewer = new DocumentViewer { Document = document };
		Assert.NotNull(viewer.Widget);
		Assert.NotNull(page.Widget.Parent);
		Assert.NotNull(button.Widget.Parent);
	});

	[Fact]
	public void PageContent_Child_setter_replaces_the_logical_child_and_paginates() => Run(() =>
	{
		var first = new FixedPage { Width = 10, Height = 20 };
		var second = new FixedPage { Width = 30, Height = 40 };
		var content = new PageContent();
		content.Child = first;
		Assert.Same(first, content.Child);
		Assert.Same(content, LogicalTreeHelper.GetParent(first));
		content.Child = second;
		Assert.Same(second, content.Child);
		Assert.Same(content, LogicalTreeHelper.GetParent(second));
		Assert.Null(LogicalTreeHelper.GetParent(first));
		var document = new FixedDocument();
		document.Pages.Add(content);
		Assert.Same(second, document.DocumentPaginator.GetPage(0).Visual);
		content.Child = null;
		Assert.Null(content.Child);
		Assert.Null(LogicalTreeHelper.GetParent(second));
	});

	[Fact]
	public void PageContent_Child_setter_works_alongside_IAddChild_and_Source() => Run(() =>
	{
		var content = new PageContent();
		((IAddChild)content).AddChild(new FixedPage());
		var replacement = new FixedPage();
		content.Child = replacement;
		Assert.Same(replacement, content.GetPageRoot(false));
		var withSource = new PageContent { Source = new System.Uri("a.fpage", System.UriKind.Relative) };
		Assert.Throws<System.InvalidOperationException>(() => withSource.Child = new FixedPage());
	});
}
