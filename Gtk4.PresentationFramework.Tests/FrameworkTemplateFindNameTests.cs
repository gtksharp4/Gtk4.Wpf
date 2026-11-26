using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

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

	[Fact]
	public void GetTemplateChild_has_the_WPF_signature()
	{
		var method = typeof(FrameworkElement).GetMethod(
			"GetTemplateChild", BindingFlags.Instance | BindingFlags.NonPublic)!;
		Assert.NotNull(method);
		Assert.True(method.IsFamilyOrAssembly);
		Assert.False(method.IsVirtual);
	}

	[Fact]
	public void The_walk_does_not_enter_a_DataTemplate_nested_in_a_ContentPresenter() => Run(() =>
	{
		var dataTemplate = new DataTemplate { ItemFactory = _ => new TextBlock { Name = "inner" } };
		ContentPresenter? presenter = null;
		var outer = new ControlTemplate
		{
			Factory = () =>
			{
				var grid = new Grid { Name = "root" };
				presenter = new ContentPresenter { Name = "presenter", Content = "x", ContentTemplate = dataTemplate };
				grid.Children.Add(presenter);
				return grid;
			},
		};
		var host = new ContentControl { Template = outer };
		_ = host.Widget;
		_ = presenter!.Widget;
		Assert.NotNull(outer.FindName("presenter", host));
		Assert.Null(outer.FindName("inner", host));
		Assert.IsType<TextBlock>(dataTemplate.FindName("inner", presenter!));
	});

	[Fact]
	public void FindInTemplate_skips_elements_of_another_templatedParent() => Run(() =>
	{
		var host = new Button();
		var grid = new Grid { Name = "root" };
		var deep = new TextBlock { Name = "deep" };
		grid.Children.Add(deep);
		FrameworkElement.StampTemplatedParentTree(grid, host);
		deep.TemplatedParent = new Button();
		Assert.Null(FrameworkElement.FindInTemplate(grid, "deep", host));
		Assert.Same(grid, FrameworkElement.FindInTemplate(grid, "root", host));
	});
}
