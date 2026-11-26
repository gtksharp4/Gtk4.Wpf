using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PageTemplateOwnerTests : WpfGtkTestBase
{
	[Fact]
	public void Page_shares_the_Control_Template_property() => Run(() =>
	{
		Assert.Same(Control.TemplateProperty, Page.TemplateProperty);
		var page = new Page();
		var template = new ControlTemplate();
		page.Template = template;
		Assert.Same(template, page.Template);
		page.Template = null;
		Assert.Null(page.Template);
	});
}
