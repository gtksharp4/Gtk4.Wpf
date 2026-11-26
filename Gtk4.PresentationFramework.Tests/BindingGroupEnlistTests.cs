using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class BindingGroupEnlistTests : WpfGtkTestBase
{
	public class Person
	{
		public string Name { get; set; } = "";
	}

	sealed class NeverRule : ValidationRule
	{
		public override ValidationResult Validate(object? value, CultureInfo cultureInfo) =>
			ValidationResult.ValidResult;
	}

	static Binding Explicit(string path) => new Binding(path)
	{
		Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
	};

	[Fact]
	public void A_TextBox_under_a_group_enlists_and_exposes_its_proposed_value() => Run(() =>
	{
		var person = new Person { Name = "a" };
		var panel = new StackPanel { BindingGroup = new BindingGroup(), DataContext = person };
		var box = new TextBox();
		panel.Children.Add(box);
		box.SetBinding(TextBox.TextProperty, Explicit("Name"));

		var group = panel.BindingGroup!;
		Assert.Single(group.BindingExpressions);
		box.Text = "proposed";
		Assert.Equal("proposed", group.GetValue(person, "Name"));
		Assert.Equal("a", person.Name);
	});

	[Fact]
	public void Bindings_made_before_the_group_or_the_parenting_are_enlisted_later() => Run(() =>
	{
		var panel = new StackPanel { DataContext = new Person() };
		var box = new TextBox();
		box.SetBinding(TextBox.TextProperty, Explicit("Name"));
		panel.Children.Add(box);
		var group = new BindingGroup();
		Assert.Empty(group.BindingExpressions);
		panel.BindingGroup = group;
		Assert.Single(group.BindingExpressions);

		var second = new TextBox();
		second.SetBinding(TextBox.TextProperty, Explicit("Name"));
		panel.Children.Add(second);
		Assert.Equal(2, group.BindingExpressions.Count);
	});

	[Fact]
	public void A_binding_with_another_group_name_stays_out() => Run(() =>
	{
		var panel = new StackPanel { BindingGroup = new BindingGroup { Name = "g" }, DataContext = new Person() };
		var box = new TextBox();
		panel.Children.Add(box);
		var other = Explicit("Name");
		other.BindingGroupName = "other";
		box.SetBinding(TextBox.TextProperty, other);
		Assert.Empty(panel.BindingGroup!.BindingExpressions);
		var mine = new TextBox();
		panel.Children.Add(mine);
		var named = Explicit("Name");
		named.BindingGroupName = "g";
		mine.SetBinding(TextBox.TextProperty, named);
		Assert.Single(panel.BindingGroup.BindingExpressions);
	});

	[Fact]
	public void ItemBindingGroup_gives_each_container_a_distinct_copy_with_the_rules() => Run(() =>
	{
		var source = new ObservableCollection<Person> { new Person(), new Person() };
		var rule = new NeverRule();
		var group = new BindingGroup { Name = "items" };
		group.ValidationRules.Add(rule);
		var ic = new ItemsControl
		{
			ItemsSource = source,
			ItemBindingGroup = group,
		};
		_ = ic.Widget;
		var first = (FrameworkElement)ic.ContainerAt(0)!;
		var second = (FrameworkElement)ic.ContainerAt(1)!;
		Assert.NotNull(first.BindingGroup);
		Assert.NotNull(second.BindingGroup);
		Assert.NotSame(first.BindingGroup, second.BindingGroup);
		Assert.NotSame(ic.ItemBindingGroup, first.BindingGroup);
		Assert.Equal("items", first.BindingGroup!.Name);
		Assert.Same(rule, Assert.Single(first.BindingGroup.ValidationRules));
	});
}
