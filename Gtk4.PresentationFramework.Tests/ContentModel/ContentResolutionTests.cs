using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>WPF ContentPresenter content resolution on a ContentControl: element, template, selector, implicit.</summary>
public class ContentResolutionTests : ContentModelTestBase
{
	sealed class PickSelector : DataTemplateSelector
	{
		public int Calls;
		public override DataTemplate? SelectTemplate(object? item, DependencyObject? container)
		{
			Calls++;
			return Template("sel:");
		}
	}

	[Fact]
	public void A_UIElement_content_is_hosted_as_the_child_itself() => Run(() =>
	{
		var text = new TextBlock { Text = "inner" };
		var cc = new ContentControl { Content = text };
		Assert.True(IsInside(cc.Widget, text.Widget));
		Assert.Same(cc, text.LogicalParent);
		Assert.Equal("inner", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void A_Label_hosting_a_ContentControl_shows_its_content_not_the_type_name() => Run(() =>
	{
		var cc = new ContentControl { Content = "Hello" };
		var label = new Label { Content = cc };
		Assert.True(IsInside(label.Widget, cc.Widget));
		Assert.Contains("Hello", Texts(label));
		Assert.DoesNotContain(Texts(label), t => t.Contains("System.Windows"));
	});

	[Fact]
	public void An_explicit_template_is_applied_with_the_content_as_DataContext() => Run(() =>
	{
		var cc = new ContentControl { Content = new Place { Name = "Oslo" }, ContentTemplate = Template("t:") };
		Assert.Equal("t:Oslo", Assert.Single(Texts(cc)));
		var root = (FrameworkElement)cc.HostedElement!;
		Assert.IsType<Place>(root.DataContext);
	});

	[Fact]
	public void The_selector_is_consulted_only_when_the_template_is_null() => Run(() =>
	{
		var selector = new PickSelector();
		var cc = new ContentControl { Content = new Place { Name = "A" }, ContentTemplateSelector = selector };
		Assert.Equal("sel:A", Assert.Single(Texts(cc)));
		cc.ContentTemplate = Template("t:");
		Assert.Equal("t:A", Assert.Single(Texts(cc)));
		var calls = selector.Calls;
		cc.Content = new Place { Name = "B" };
		Assert.Equal(calls, selector.Calls);
	});

	[Fact]
	public void An_implicit_DataTemplate_is_found_on_an_ancestor_element() => Run(() =>
	{
		var panel = new StackPanel();
		panel.Resources[new DataTemplateKey(typeof(Place))] = Template("imp:");
		var cc = new ContentControl();
		panel.Children.Add(cc);
		cc.Content = new Place { Name = "X" };
		Assert.Equal("imp:X", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void An_implicit_DataTemplate_is_found_via_a_base_type_and_the_nearest_scope_wins() => Run(() =>
	{
		var outer = new StackPanel();
		outer.Resources[new DataTemplateKey(typeof(Place))] = Template("outer:");
		var inner = new StackPanel();
		outer.Children.Add(inner);
		var cc = new ContentControl();
		inner.Children.Add(cc);
		cc.Content = new SpecialPlace { Name = "S" };
		Assert.Equal("outer:S", Assert.Single(Texts(cc)));

		inner.Resources[new DataTemplateKey(typeof(Place))] = Template("inner:");
		cc.Content = new SpecialPlace { Name = "T" };
		Assert.Equal("inner:T", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void An_implicit_DataTemplate_is_found_in_Window_resources_even_when_attached_late() => Run(() =>
	{
		var window = new Window();
		window.Resources[new DataTemplateKey(typeof(Place))] = Template("win:");
		var cc = new ContentControl { Content = new Place { Name = "W" } };
		_ = cc.Widget;
		window.Content = cc;
		Assert.Equal("win:W", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void A_keyed_DataTemplate_is_never_chosen_implicitly() => Run(() =>
	{
		var panel = new StackPanel();
		var keyed = Template("keyed:");
		keyed.DataType = typeof(Place);
		panel.Resources["k"] = keyed;
		var cc = new ContentControl();
		panel.Children.Add(cc);
		cc.Content = new Place { Name = "Z" };
		Assert.Equal("Place:Z", Assert.Single(Texts(cc)));
	});

	[Fact]
	public void DataTemplate_exposes_its_DataTemplateKey() => Run(() =>
	{
		var t = new DataTemplate { DataType = typeof(Place) };
		Assert.Equal(new DataTemplateKey(typeof(Place)), t.DataTemplateKey);
		Assert.Null(new DataTemplate().DataTemplateKey);
	});

	[Fact]
	public void String_content_falls_back_to_ToString_the_format_and_a_TypeConverter() => Run(() =>
	{
		var cc = new ContentControl { Content = new Place { Name = "P" } };
		Assert.Equal("Place:P", Assert.Single(Texts(cc)));
		cc.Content = 42;
		cc.ContentStringFormat = "n={0:D4}";
		Assert.Equal("n=0042", Assert.Single(Texts(cc)));
		cc.ContentStringFormat = null;
		cc.Content = System.Windows.Media.Colors.Red;
		Assert.Single(Texts(cc));
	});

	[Fact]
	public void Content_swapping_element_string_null_does_not_leak_the_logical_parent() => Run(() =>
	{
		var a = new TextBlock { Text = "a" };
		var cc = new ContentControl { Content = a };
		_ = cc.Widget;
		cc.Content = "text";
		Assert.Null(a.LogicalParent);
		Assert.Equal("text", Assert.Single(Texts(cc)));
		cc.Content = a;
		Assert.Same(cc, a.LogicalParent);
		Assert.Equal("a", Assert.Single(Texts(cc)));
		cc.Content = null;
		Assert.Null(a.LogicalParent);
		Assert.Empty(Texts(cc));
	});

	[Fact]
	public void An_Inline_content_is_shown_inside_a_TextBlock() => Run(() =>
	{
		var link = new System.Windows.Documents.Hyperlink();
		link.Inlines.Add(new System.Windows.Documents.Run("Go"));
		var cc = new ContentControl { Content = link };
		_ = cc.Widget;
		var tb = Assert.IsType<TextBlock>(cc.HostedElement);
		Assert.Single(tb.Inlines);
		Assert.Equal("Go", Assert.Single(Texts(cc)));
		cc.Content = null;
		Assert.Empty(tb.Inlines);
	});
}
