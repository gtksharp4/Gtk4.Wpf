using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Animation;

public class PropertyPathNameScopeTests
{
	[Fact]
	public void PropertyPath_keeps_path_and_parameters()
	{
		var p = new PropertyPath("(0).(1)[2]", UIElement.OpacityProperty, UIElement.RenderTransformProperty);
		Assert.Equal("(0).(1)[2]", p.Path);
		Assert.Equal(2, p.PathParameters.Count);
		var single = new PropertyPath(UIElement.OpacityProperty);
		Assert.Equal("(0)", single.Path);
		Assert.Same(UIElement.OpacityProperty, single.PathParameters[0]);
		Assert.Throws<ArgumentNullException>(() => new PropertyPath((string)null!));
	}

	[Fact]
	public void NameScope_registers_finds_and_unregisters()
	{
		var scope = new NameScope();
		var el = new AnimTargetElement();
		scope.RegisterName("a", el);
		Assert.Same(el, scope.FindName("a"));
		Assert.Null(scope.FindName("b"));
		scope.UnregisterName("a");
		Assert.Null(scope.FindName("a"));
		Assert.Throws<ArgumentException>(() => scope.UnregisterName("a"));
		Assert.Throws<ArgumentNullException>(() => scope.RegisterName("x", null!));
		scope.RegisterName("dup", el);
		Assert.Throws<ArgumentException>(() => scope.RegisterName("dup", el));
	}

	[Fact]
	public void NameScope_attached_property_round_trips_and_feeds_FindName()
	{
		var root = new AnimTargetElement();
		var child = new AnimTargetElement();
		var scope = new NameScope();
		scope.RegisterName("kid", child);
		NameScope.SetNameScope(root, scope);
		Assert.Same(scope, NameScope.GetNameScope(root));
		Assert.Same(child, root.FindName("kid"));
	}

	[Fact]
	public void RegisterName_and_UnregisterName_on_a_framework_element()
	{
		var root = new AnimTargetElement();
		var child = new AnimTargetElement();
		root.RegisterName("c", child);
		Assert.Same(child, root.FindName("c"));
		root.UnregisterName("c");
		Assert.Null(root.FindName("c"));
	}

	sealed class AnimTargetElement : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException();
	}
}
