using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 45 follow-up: current-item write-back, ObjectDataProvider overloads, ItemsSource switch.</summary>
public class Round45FollowUpTests : WpfGtkTestBase
{
	public sealed class Person
	{
		public string Name { get; set; } = "";
	}

	[Fact]
	public void Write_back_through_an_implicit_current_item_path_sets_the_current_item()
	{
		var people = new ObservableCollection<Person> { new Person { Name = "a" }, new Person { Name = "b" } };
		CollectionViewSource.GetDefaultView(people)!.MoveCurrentToPosition(1);
		Assert.True(BindingEngine_WriteBack(people, "Name", "z"));
		Assert.Equal("a", people[0].Name);
		Assert.Equal("z", people[1].Name);
	}

	[Fact]
	public void A_TwoWay_binding_to_a_collection_source_writes_the_current_item() => Run(() =>
	{
		var people = new ObservableCollection<Person> { new Person { Name = "a" }, new Person { Name = "b" } };
		var tb = new WpfTextBox();
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name") { Source = people, Mode = BindingMode.TwoWay });
		Assert.Equal("a", tb.Text);
		tb.Text = "edited";
		tb.GetBindingExpression(WpfTextBox.TextProperty)!.UpdateSource();
		Assert.Equal("edited", people[0].Name);
	});

	static bool BindingEngine_WriteBack(object source, string path, object value) =>
		System.Windows.Data.BindingEngine.WriteBackPath(source, path, value);

	[Fact]
	public void ObjectDataProvider_picks_the_overload_whose_parameter_types_fit()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Math), MethodName = "Max" };
		odp.MethodParameters.Add(1.5);
		odp.MethodParameters.Add(2.5);
		odp.InitialLoad();
		Assert.Equal(2.5, odp.Data);
	}

	public sealed class Holder
	{
		public List<object?> Pushed { get; } = new List<object?>();
		public object? Selected
		{
			get => Pushed.Count > 0 ? Pushed[Pushed.Count - 1] : null;
			set => Pushed.Add(value);
		}
	}

	[Fact]
	public void Switching_ItemsSource_never_pushes_the_old_views_current_item() => Run(() =>
	{
		var list1 = new ObservableCollection<string> { "a", "b" };
		var list2 = new ObservableCollection<string> { "x", "y" };
		var holder = new Holder();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = list1 };
		lb.SetBinding(ListBox.SelectedItemProperty,
			new Binding("Selected") { Source = holder, Mode = BindingMode.TwoWay });
		lb.SelectedItem = "b";
		holder.Pushed.Clear();
		lb.ItemsSource = list2;
		Assert.DoesNotContain("b", holder.Pushed);
		Assert.Equal("x", lb.SelectedItem);
	});
}
