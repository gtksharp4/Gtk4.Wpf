using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class FrameworkTemplateFindNameTests : WpfGtkTestBase
{
	static ControlTemplate GridTemplate() => new ControlTemplate
	{
		Factory = () =>
		{
			var grid = new Grid { Name = "grid" };
			grid.Children.Add(new TextBlock { Name = "label", Text = "t" });
			return grid;
		},
	};

	[Fact]
	public void Finds_a_named_element_of_a_ControlTemplate() => Run(() =>
	{
		var template = GridTemplate();
		var button = new Button { Template = template };
		var grid = Assert.IsType<Grid>(template.FindName("grid", button));
		Assert.Same(button, grid.TemplatedParent);
		Assert.IsType<TextBlock>(template.FindName("label", button));
	});

	[Fact]
	public void Unknown_name_returns_null() => Run(() =>
	{
		var template = GridTemplate();
		var button = new Button { Template = template };
		Assert.Null(template.FindName("nope", button));
	});

	[Fact]
	public void Null_templatedParent_throws() => Run(() =>
	{
		Assert.Throws<ArgumentNullException>(() => GridTemplate().FindName("grid", null!));
	});

	[Fact]
	public void The_wrong_templatedParent_throws() => Run(() =>
	{
		var template = GridTemplate();
		var other = new Button { Template = GridTemplate() };
		var plain = new Button();
		Assert.Throws<InvalidOperationException>(() => template.FindName("grid", other));
		Assert.Throws<InvalidOperationException>(() => template.FindName("grid", plain));
	});

	[Fact]
	public void Finds_a_DataTemplate_element_through_its_ContentPresenter() => Run(() =>
	{
		var template = new DataTemplate
		{
			ItemFactory = item => new TextBlock { Name = "textBlock", Text = (string)item! },
		};
		var presenter = new ContentPresenter { Content = "hello", ContentTemplate = template };
		_ = presenter.Widget;
		var text = Assert.IsType<TextBlock>(template.FindName("textBlock", presenter));
		Assert.Equal("hello", text.Text);
	});

	[Fact]
	public void The_walk_stops_at_a_nested_template() => Run(() =>
	{
		var inner = new ControlTemplate { Factory = () => new TextBlock { Name = "deep" } };
		var outer = new ControlTemplate { Factory = () => new Button { Name = "nested", Template = inner } };
		var host = new Button { Template = outer };
		Assert.IsType<Button>(outer.FindName("nested", host));
		Assert.Null(outer.FindName("deep", host));
	});

	[Fact]
	public void GetTemplateChild_uses_the_same_lookup() => Run(() =>
	{
		var control = new ProbeButton { Template = GridTemplate() };
		Assert.IsType<Grid>(control.Probe("grid"));
		Assert.Null(new ProbeButton().Probe("grid"));
	});

	sealed class ProbeButton : Button
	{
		public DependencyObject? Probe(string name) => GetTemplateChild(name);
	}
}
