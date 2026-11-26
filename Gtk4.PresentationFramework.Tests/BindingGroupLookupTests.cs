using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class BindingGroupLookupTests : WpfGtkTestBase
{
	public class Person
	{
		public string Name { get; set; } = "";
	}

	static Binding Explicit(string path, string groupName = "") => new Binding(path)
	{
		Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
		BindingGroupName = groupName,
	};

	[Fact]
	public void A_named_binding_walks_past_an_unnamed_inner_group_to_the_matching_ancestor() => Run(() =>
	{
		var person = new Person();
		var outer = new StackPanel { BindingGroup = new BindingGroup { Name = "outer" }, DataContext = person };
		var inner = new StackPanel { BindingGroup = new BindingGroup() };
		var box = new TextBox();
		outer.Children.Add(inner);
		inner.Children.Add(box);
		box.SetBinding(TextBox.TextProperty, Explicit("Name", "outer"));
		Assert.Single(outer.BindingGroup!.BindingExpressions);
		Assert.Empty(inner.BindingGroup!.BindingExpressions);
	});

	[Fact]
	public void An_unnamed_binding_joins_only_when_it_reads_the_group_owners_DataContext() => Run(() =>
	{
		var panel = new StackPanel { BindingGroup = new BindingGroup(), DataContext = new Person() };
		var viaContext = new TextBox();
		var viaSource = new TextBox();
		var viaElement = new TextBox { Name = "x" };
		var otherContext = new TextBox { DataContext = new Person() };
		panel.Children.Add(viaContext);
		panel.Children.Add(viaSource);
		panel.Children.Add(viaElement);
		panel.Children.Add(otherContext);
		var withSource = Explicit("Name");
		withSource.Source = new Person();
		var withElement = Explicit("Text");
		withElement.ElementName = "x";
		viaContext.SetBinding(TextBox.TextProperty, Explicit("Name"));
		viaSource.SetBinding(TextBox.TextProperty, withSource);
		viaElement.SetBinding(TextBox.TextProperty, withElement);
		otherContext.SetBinding(TextBox.TextProperty, Explicit("Name"));
		var expressions = panel.BindingGroup!.BindingExpressions;
		Assert.Single(expressions);
		Assert.Same(viaContext, expressions[0].TargetElement);
	});
}
