using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Inheritance contexts of freezables: none when frozen, none when held by several objects.</summary>
public class FreezableContextTests : WpfGtkTestBase
{
	[Fact]
	public void A_frozen_freezable_gets_no_context_and_raises_no_tree_change() => Run(() =>
	{
		var red = new SolidColorBrush(Colors.Red);
		red.Freeze();
		var seen = 0;
		var previous = DependencyObject.InheritanceParentChangedHook;
		DependencyObject.InheritanceParentChangedHook = d =>
		{
			if (ReferenceEquals(d, red)) seen++;
			previous?.Invoke(d);
		};
		try
		{
			var border = new Border { Background = red };
			Assert.Same(red, border.Background);
		}
		finally
		{
			DependencyObject.InheritanceParentChangedHook = previous;
		}
		Assert.Null(red.InheritanceContext);
		Assert.Equal(0, seen);
	});

	[Fact]
	public void An_unfrozen_freezable_has_the_context_of_its_only_holder() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Blue);
		var border = new Border { Background = brush };
		Assert.Same(border, brush.InheritanceContext);
		border.Background = null;
		Assert.Null(brush.InheritanceContext);
	});

	[Fact]
	public void An_unfrozen_freezable_held_by_two_objects_has_no_context() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Blue);
		var first = new Border { Background = brush };
		var second = new Border { Background = brush };
		Assert.Null(brush.InheritanceContext);
		Assert.Same(brush, first.Background);
		Assert.Same(brush, second.Background);
	});
}
