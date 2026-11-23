using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Bindings on freezables, animations and <see cref="CollectionViewSource"/> resolve through the mentor.</summary>
public class NonElementBindingTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		FillRule _rule = FillRule.Nonzero;
		double _to = 10;
		public event PropertyChangedEventHandler? PropertyChanged;
		public FillRule Rule
		{
			get => _rule;
			set { _rule = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rule))); }
		}
		public double To
		{
			get => _to;
			set { _to = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To))); }
		}
		public int[] Items { get; } = { 1, 2, 3 };
	}

	[Fact]
	public void PathGeometry_FillRule_follows_the_DataContext_of_the_Path() => Run(() =>
	{
		var geometry = new PathGeometry();
		var path = new Path { Data = geometry };
		var vm = new Vm { Rule = FillRule.Nonzero };
		BindingOperations.SetBinding(geometry, PathGeometry.FillRuleProperty, new Binding("Rule"));
		Assert.Equal(FillRule.EvenOdd, geometry.FillRule);

		path.DataContext = vm;
		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
		vm.Rule = FillRule.EvenOdd;
		Assert.Equal(FillRule.EvenOdd, geometry.FillRule);
		vm.Rule = FillRule.Nonzero;
		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
	});

	[Fact]
	public void Brush_Color_binds_through_ElementName_of_the_owning_element() => Run(() =>
	{
		var root = new StackPanel();
		var source = new Border { Tag = Colors.Red };
		var brush = new SolidColorBrush(Colors.Blue);
		var target = new Border { Background = brush };
		root.Children.Add(source);
		root.Children.Add(target);
		root.RegisterName("src", source);

		BindingOperations.SetBinding(brush, SolidColorBrush.ColorProperty,
			new Binding("Tag") { ElementName = "src" });

		Assert.Equal(Colors.Red, brush.Color);
		source.Tag = Colors.Green;
		Assert.Equal(Colors.Green, brush.Color);
	});

	[Fact]
	public void Binding_on_a_freezable_resolves_when_it_gains_a_mentor_later() => Run(() =>
	{
		var geometry = new PathGeometry();
		BindingOperations.SetBinding(geometry, PathGeometry.FillRuleProperty, new Binding("Rule"));
		var path = new Path { DataContext = new Vm { Rule = FillRule.Nonzero } };

		path.Data = geometry;

		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
	});

	[Fact]
	public void Explicit_Source_needs_no_mentor() => Run(() =>
	{
		var geometry = new PathGeometry();
		BindingOperations.SetBinding(geometry, PathGeometry.FillRuleProperty,
			new Binding("Rule") { Source = new Vm { Rule = FillRule.Nonzero } });
		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
	});

	[Fact]
	public void Animation_To_binding_does_not_throw_and_follows_the_source() => Run(() =>
	{
		var animation = new DoubleAnimation();
		var vm = new Vm { To = 40 };
		var expr = BindingOperations.SetBinding(animation, DoubleAnimation.ToProperty,
			new Binding("To") { Source = vm });

		Assert.NotNull(expr);
		Assert.Equal(40.0, animation.To);
		vm.To = 50;
		Assert.Equal(50.0, animation.To);
	});

	[Fact]
	public void Storyboard_children_resolve_the_DataContext_of_the_starting_element() => Run(() =>
	{
		var element = new Border { DataContext = new Vm { To = 12 } };
		var animation = new DoubleAnimation { Duration = new Duration(System.TimeSpan.FromSeconds(1)) };
		BindingOperations.SetBinding(animation, DoubleAnimation.ToProperty, new Binding("To"));
		var storyboard = new Storyboard();
		storyboard.Children.Add(animation);
		Storyboard.SetTarget(animation, element);
		Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));

		storyboard.Begin(element);

		Assert.Equal(12.0, animation.To);
	});

	[Fact]
	public void CollectionViewSource_Source_binds_to_the_DataContext_of_the_resource_owner() => Run(() =>
	{
		var vm = new Vm();
		var panel = new StackPanel();
		var cvs = new CollectionViewSource();
		panel.Resources["items"] = cvs;
		BindingOperations.SetBinding(cvs, CollectionViewSource.SourceProperty, new Binding("Items"));

		panel.DataContext = vm;

		Assert.Same(vm.Items, cvs.Source);
	});

	[Fact]
	public void Binding_a_frozen_freezable_throws() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Red);
		brush.Freeze();
		Assert.Throws<System.InvalidOperationException>(() =>
			BindingOperations.SetBinding(brush, SolidColorBrush.ColorProperty, new Binding("X") { Source = this }));
	});

	[Fact]
	public void ClearBinding_stops_a_freezable_binding() => Run(() =>
	{
		var geometry = new PathGeometry();
		var vm = new Vm { Rule = FillRule.Nonzero };
		BindingOperations.SetBinding(geometry, PathGeometry.FillRuleProperty,
			new Binding("Rule") { Source = vm });
		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
		BindingOperations.ClearBinding(geometry, PathGeometry.FillRuleProperty);
		vm.Rule = FillRule.EvenOdd;
		Assert.Equal(FillRule.Nonzero, geometry.FillRule);
	});
}
