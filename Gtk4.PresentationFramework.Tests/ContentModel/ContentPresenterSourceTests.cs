using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>A ContentPresenter inside a ControlTemplate follows its templated parent (WPF ContentSource).</summary>
public class ContentPresenterSourceTests : ContentModelTestBase
{
	static ControlTemplate PresenterTemplate(string? source = null) => new ControlTemplate
	{
		Factory = () =>
		{
			var p = new ContentPresenter();
			if (source != null) p.ContentSource = source;
			return p;
		},
	};

	[Fact]
	public void A_templated_ContentPresenter_shows_the_parent_Content() => Run(() =>
	{
		var cc = new ContentControl { Template = PresenterTemplate(), Content = "Hello" };
		Assert.Equal("Hello", Assert.Single(Texts(cc)));
		cc.Content = new TextBlock { Text = "later" };
		Assert.Equal("later", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void A_templated_ContentPresenter_follows_ContentTemplate_and_StringFormat() => Run(() =>
	{
		var cc = new ContentControl { Template = PresenterTemplate(), Content = new Place { Name = "A" } };
		Assert.Equal("Place:A", Assert.Single(Texts(cc)));
		cc.ContentTemplate = Template("t:");
		Assert.Equal("t:A", Assert.Single(Texts(cc)));
		cc.ContentTemplate = null;
		cc.Content = 5;
		cc.ContentStringFormat = "v{0}";
		Assert.Equal("v5", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void An_empty_ContentSource_disables_the_aliasing() => Run(() =>
	{
		var cc = new ContentControl { Template = PresenterTemplate(string.Empty), Content = "Hello" };
		Assert.Empty(Texts(cc));
	});

	[Fact]
	public void ContentSource_Header_aliases_the_Header_properties() => Run(() =>
	{
		var h = new HeaderedContentControl
		{
			Template = PresenterTemplate("Header"), Header = new Place { Name = "H" }, HeaderTemplate = Template("h:"),
			Content = "body",
		};
		Assert.Equal("h:H", Assert.Single(Texts(h)));
	});

	[Fact]
	public void Removing_the_Template_restores_the_own_content() => Run(() =>
	{
		var cc = new ContentControl { Content = "Hello" };
		_ = cc.Widget;
		cc.Template = PresenterTemplate(string.Empty);
		Assert.Empty(Texts(cc));
		cc.Template = null;
		Assert.Equal("Hello", Assert.Single(Texts(cc)));
	});
}
