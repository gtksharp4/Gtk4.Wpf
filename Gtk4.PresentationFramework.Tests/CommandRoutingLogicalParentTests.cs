using System;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// E7: CommandManager walks LogicalParent, not InheritanceParent.
/// A CommandBinding on an ancestor is found from a descendant.
/// A KeyBinding with a RoutedCommand whose ancestor has a CommandBinding executes.
/// </summary>
public class CommandRoutingLogicalParentTests
{
	sealed class TestEl : UIElement
	{
		new internal UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}

	[Fact]
	public void CommandBinding_on_ancestor_found_via_LogicalParent()
	{
		bool executed = false;
		var cmd = new RoutedCommand("E7Ancestor", typeof(CommandRoutingLogicalParentTests));

		var grandparent = new TestEl();
		var parent = new TestEl { LogicalParent = grandparent };
		var child = new TestEl { LogicalParent = parent };

		grandparent.CommandBindings.Add(new CommandBinding(cmd, (s, e) => executed = true));

		cmd.Execute(null, child);

		Assert.True(executed);
	}

	[Fact]
	public void RoutedCommand_CanExecute_walks_LogicalParent()
	{
		var cmd = new RoutedCommand("E7CanEx", typeof(CommandRoutingLogicalParentTests));

		var parent = new TestEl();
		var child = new TestEl { LogicalParent = parent };

		parent.CommandBindings.Add(new CommandBinding(
			cmd,
			(s, e) => { },
			(s, e) => e.CanExecute = false));

		Assert.False(cmd.CanExecute(null, child));
	}

	[Fact]
	public void RoutedCommand_with_no_binding_is_not_executable_and_does_not_recurse()
	{
		// Regression: CommandManager fell back to command.CanExecute(parameter) for a
		// RoutedCommand with no CommandBinding and a null target, re-entering itself
		// → infinite recursion / stack overflow. A RoutedCommand with no matching
		// binding must report CanExecute=false and Execute must be a no-op.
		var cmd = new RoutedCommand("E7NoBinding", typeof(CommandRoutingLogicalParentTests));

		Assert.False(cmd.CanExecute(null));          // null target, no binding
		cmd.Execute(null);                            // must not throw / overflow
	}

	[Fact]
	public void KeyBinding_with_RoutedCommand_executes_via_ancestor_CommandBinding()
	{
		bool executed = false;
		var cmd = new RoutedCommand("E7Key", typeof(CommandRoutingLogicalParentTests));

		var parent = new TestEl();
		var child = new TestEl { LogicalParent = parent };

		parent.CommandBindings.Add(new CommandBinding(cmd, (s, e) => executed = true));
		child.InputBindings.Add(new KeyBinding(cmd, Key.S, ModifierKeys.Control));

		var result = child.InvokeInputBinding(
			new KeyGestureEventArgs(Key.S, ModifierKeys.Control));

		Assert.True(result);
		Assert.True(executed);
	}
}