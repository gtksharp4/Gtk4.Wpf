using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class FocusManagerIsFocusScopeTests : WpfGtkTestBase
{
	[Fact]
	public void IsFocusScopeProperty_is_registered_with_a_false_default()
	{
		var p = FocusManager.IsFocusScopeProperty;
		Assert.Equal("IsFocusScope", p.Name);
		Assert.Equal(typeof(bool), p.PropertyType);
		Assert.Equal(typeof(FocusManager), p.OwnerType);
		Assert.Equal(false, p.DefaultMetadata.DefaultValue);
		Assert.IsAssignableFrom<UIPropertyMetadata>(p.DefaultMetadata);
	}

	[Fact]
	public void Accessors_round_trip_and_reject_null() => Run(() =>
	{
		var border = new Border();
		Assert.False(FocusManager.GetIsFocusScope(border));
		FocusManager.SetIsFocusScope(border, true);
		Assert.True(FocusManager.GetIsFocusScope(border));
		Assert.Throws<ArgumentNullException>(() => FocusManager.GetIsFocusScope(null!));
		Assert.Throws<ArgumentNullException>(() => FocusManager.SetIsFocusScope(null!, true));
		Assert.Throws<ArgumentNullException>(() => FocusManager.GetFocusScope(null!));
	});

	[Fact]
	public void GetFocusScope_returns_the_nearest_scope_ancestor() => Run(() =>
	{
		var root = new Border();
		var scope = new Border();
		var leaf = new Border();
		root.Child = scope;
		scope.Child = leaf;
		FocusManager.SetIsFocusScope(scope, true);
		Assert.Same(scope, FocusManager.GetFocusScope(leaf));
		Assert.Same(scope, FocusManager.GetFocusScope(scope));
	});

	[Fact]
	public void GetFocusScope_falls_back_to_the_root_without_a_scope() => Run(() =>
	{
		var root = new Border();
		var leaf = new Border();
		root.Child = leaf;
		Assert.Same(root, FocusManager.GetFocusScope(leaf));
		Assert.Same(root, FocusManager.GetFocusScope(root));
	});

	[Fact]
	public void FocusedElement_still_round_trips() => Run(() =>
	{
		var scope = new Border();
		var child = new Border();
		FocusManager.SetFocusedElement(scope, child);
		Assert.Same(child, FocusManager.GetFocusedElement(scope));
	});
}
