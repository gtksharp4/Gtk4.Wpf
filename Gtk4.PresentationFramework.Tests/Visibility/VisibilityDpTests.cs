using System;
using System.Windows;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>Metadata of Visibility / IsVisible / IsEnabled / IsHitTestVisible.</summary>
public class VisibilityDpTests
{
	[Fact]
	public void Visibility_property_is_registered_on_UIElement_with_Visible_default()
	{
		var p = UIElement.VisibilityProperty;
		Assert.Equal("Visibility", p.Name);
		Assert.Equal(typeof(UIElement), p.OwnerType);
		Assert.Equal(typeof(Visibility), p.PropertyType);
		Assert.Equal(Visibility.Visible, p.GetMetadata(typeof(UIElement)).DefaultValue);
	}

	[Fact]
	public void IsEnabled_defaults_to_true_and_is_owned_by_UIElement()
	{
		var p = UIElement.IsEnabledProperty;
		Assert.Equal("IsEnabled", p.Name);
		Assert.Equal(typeof(UIElement), p.OwnerType);
		Assert.Equal(true, p.GetMetadata(typeof(UIElement)).DefaultValue);
		Assert.NotNull(p.GetMetadata(typeof(UIElement)).CoerceValueCallback);
	}

	[Fact]
	public void IsHitTestVisible_defaults_to_true_with_a_coerce_callback()
	{
		var p = UIElement.IsHitTestVisibleProperty;
		Assert.Equal("IsHitTestVisible", p.Name);
		Assert.Equal(true, p.GetMetadata(typeof(UIElement)).DefaultValue);
		Assert.NotNull(p.GetMetadata(typeof(UIElement)).CoerceValueCallback);
	}

	[Fact]
	public void IsVisible_is_read_only_and_defaults_to_false()
	{
		var p = UIElement.IsVisibleProperty;
		Assert.Equal("IsVisible", p.Name);
		Assert.True(p.IsReadOnly);
		Assert.Equal(false, p.GetMetadata(typeof(UIElement)).DefaultValue);
		Assert.Throws<InvalidOperationException>(() => new VisElement().SetValue(p, true));
	}

	[Fact]
	public void Window_Visibility_default_is_Collapsed()
	{
		// The override is registered in Window's static constructor; do not depend on test order.
		System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WpfWindow).TypeHandle);
		Assert.Equal(Visibility.Collapsed, UIElement.VisibilityProperty.GetMetadata(typeof(WpfWindow)).DefaultValue);
		Assert.Equal(Visibility.Visible, UIElement.VisibilityProperty.GetMetadata(typeof(VisElement)).DefaultValue);
	}

	[Fact]
	public void Accessors_round_trip()
	{
		var el = new VisElement { Visibility = Visibility.Hidden, IsEnabled = false, IsHitTestVisible = false };
		Assert.Equal(Visibility.Hidden, el.Visibility);
		Assert.False(el.IsEnabled);
		Assert.False(el.IsHitTestVisible);
	}

	[Fact]
	public void IsHitTestVisible_is_coerced_by_the_parent()
	{
		var parent = new VisElement();
		var child = new VisElement(parent);
		parent.IsHitTestVisible = false;
		Assert.False(child.IsHitTestVisible);
		parent.IsHitTestVisible = true;
		Assert.True(child.IsHitTestVisible);
	}

	[Fact]
	public void IsHitTestVisibleChanged_fires_parent_first_and_only_on_change()
	{
		var parent = new VisElement();
		var child = new VisElement(parent);
		var log = new System.Collections.Generic.List<string>();
		parent.IsHitTestVisibleChanged += (s, e) => log.Add("parent");
		child.IsHitTestVisibleChanged += (s, e) => log.Add("child");
		parent.IsHitTestVisible = false;
		parent.IsHitTestVisible = false;
		Assert.Equal(new[] { "parent", "child" }, log);
	}
}
