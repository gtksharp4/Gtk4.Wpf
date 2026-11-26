using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A freezable with a live binding cannot be frozen, as in WPF.</summary>
public class BoundFreezableTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		FillRule _rule = FillRule.Nonzero;
		public event PropertyChangedEventHandler? PropertyChanged;
		public FillRule Rule
		{
			get => _rule;
			set { _rule = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rule))); }
		}
	}

	static PathGeometry Bound(Vm vm)
	{
		var geometry = new PathGeometry();
		BindingOperations.SetBinding(geometry, PathGeometry.FillRuleProperty, new Binding("Rule") { Source = vm });
		return geometry;
	}

	[Fact]
	public void A_bound_geometry_cannot_be_frozen_and_keeps_following_its_source() => Run(() =>
	{
		var vm = new Vm();
		var geometry = Bound(vm);
		Assert.False(geometry.CanFreeze);
		Assert.Throws<System.InvalidOperationException>(() => geometry.Freeze());
		vm.Rule = FillRule.EvenOdd;
		Assert.Equal(FillRule.EvenOdd, geometry.FillRule);
	});

	[Fact]
	public void Clearing_the_binding_makes_the_geometry_freezable_again() => Run(() =>
	{
		var geometry = Bound(new Vm());
		BindingOperations.ClearBinding(geometry, PathGeometry.FillRuleProperty);
		Assert.True(geometry.CanFreeze);
		geometry.Freeze();
		Assert.True(geometry.IsFrozen);
	});

	[Fact]
	public void A_group_holding_a_bound_geometry_cannot_be_frozen() => Run(() =>
	{
		var bound = Bound(new Vm());
		var plain = new PathGeometry();
		var group = new GeometryGroup();
		group.Children.Add(plain);
		group.Children.Add(bound);
		Assert.False(group.CanFreeze);
		Assert.Throws<System.InvalidOperationException>(() => group.Freeze());
		Assert.False(plain.IsFrozen);
		BindingOperations.ClearBinding(bound, PathGeometry.FillRuleProperty);
		Assert.True(group.CanFreeze);
	});

	[Fact]
	public void A_brush_whose_drawing_is_bound_cannot_be_frozen() => Run(() =>
	{
		var geometry = Bound(new Vm());
		var brush = new DrawingBrush(new GeometryDrawing { Geometry = geometry });
		Assert.False(brush.CanFreeze);
	});
}
