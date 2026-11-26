using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>An owner-qualified PathString "(Owner.Name)" whose owner is the source type, at every consumer.</summary>
public class OwnerQualifiedPathConsumerTests : WpfGtkTestBase
{
	sealed class Person : INotifyPropertyChanged
	{
		string? _name;
		bool _flag;
		public string? Name
		{
			get => _name;
			set { _name = value; Raise(nameof(Name)); }
		}
		public bool Flag
		{
			get => _flag;
			set { _flag = value; Raise(nameof(Flag)); }
		}
		public List<string> Kids { get; } = new List<string> { "k" };
		public event PropertyChangedEventHandler? PropertyChanged;
		void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("OwnerOut", typeof(string), typeof(TEl), new PropertyMetadata(null));
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	[Fact]
	public void PriorityBinding_repushes_on_a_property_change() => Run(() =>
	{
		var p = new Person { Name = "a" };
		var el = new TEl();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("(Person.Name)") { Source = p });
		el.SetBinding(TEl.OutProperty, pb);
		Assert.Equal("a", el.Out);
		p.Name = "b";
		Assert.Equal("b", el.Out);
	});

	[Fact]
	public void MultiDataTrigger_follows_a_property_change() => Run(() =>
	{
		var p = new Person();
		var el = new TEl { DataContext = p };
		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("(Person.Flag)"), Value = true });
		mdt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "on" });
		style.Triggers.Add(mdt);
		el.Style = style;
		Assert.Null(el.Out);
		p.Flag = true;
		Assert.Equal("on", el.Out);
		p.Flag = false;
		Assert.Null(el.Out);
	});

	[Fact]
	public void DataTrigger_follows_a_property_change() => Run(() =>
	{
		var p = new Person();
		var el = new TEl { DataContext = p };
		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("(Person.Flag)"), Value = true };
		dt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "on" });
		style.Triggers.Add(dt);
		el.Style = style;
		Assert.Null(el.Out);
		p.Flag = true;
		Assert.Equal("on", el.Out);
	});

	[Fact]
	public void DataGridTextColumn_reads_the_cell_through_the_qualified_path() => Run(() =>
	{
		var col = new DataGridTextColumn { Binding = new Binding("(Person.Name)") };
		Assert.Equal("ann", col.GetCellText(new Person { Name = "ann" }));
	});

	[Fact]
	public void DataGrid_sorts_by_a_qualified_binding_path() => Run(() =>
	{
		var b = new Person { Name = "b" };
		var a = new Person { Name = "a" };
		var grid = new DataGrid();
		var col = new DataGridTextColumn { Binding = new Binding("(Person.Name)") };
		grid.Columns.Add(col);
		grid.ItemsSource = new[] { b, a };
		grid.ToggleSort(col);
		Assert.Equal(0, grid.AutomationRowIndex(a));
		Assert.Equal(1, grid.AutomationRowIndex(b));
	});

	[Fact]
	public void HierarchicalDataTemplate_resolves_children_through_the_qualified_path() => Run(() =>
	{
		var hdt = new HierarchicalDataTemplate { ItemsSource = new Binding("(Person.Kids)") };
		var kids = hdt.ResolveChildren(new Person())!.Cast<string>();
		Assert.Equal(new[] { "k" }, kids);
	});

	[Fact]
	public void BindingGroup_finds_the_value_through_the_qualified_path() => Run(() =>
	{
		var p = new Person { Name = "x" };
		var panel = new StackPanel { BindingGroup = new BindingGroup(), DataContext = p };
		var box = new TextBox();
		panel.Children.Add(box);
		box.SetBinding(TextBox.TextProperty, new Binding("(Person.Name)")
		{
			Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
		});
		box.Text = "y";
		Assert.True(panel.BindingGroup!.TryGetValue(p, "Name", out var value));
		Assert.Equal("y", value);
	});
}
