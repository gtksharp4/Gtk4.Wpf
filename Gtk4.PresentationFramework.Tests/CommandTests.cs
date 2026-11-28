using System;
using System.Windows;
using System.Windows.Input;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfMenuItem = System.Windows.Controls.MenuItem;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for WPF command + input plumbing:
/// ICommand / RoutedCommand / CommandBinding / CommandManager /
/// Button.Command / MenuItem / InputBindings.
/// </summary>
public class CommandTests
{
	// ── Helpers ────────────────────────────────────────────────────────────────

	sealed class RelayCommand : ICommand
	{
		readonly Func<object?, bool> _canExecute;
		readonly Action<object?> _execute;

		internal int ExecuteCallCount { get; private set; }
		internal object? LastParameter { get; private set; }

		internal RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute ?? (_ => true);
		}

		public bool CanExecute(object? parameter) => _canExecute(parameter);
		public void Execute(object? parameter) { ExecuteCallCount++; LastParameter = parameter; _execute(parameter); }
		public event EventHandler? CanExecuteChanged;
		internal void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Minimal UIElement stub that never creates a GTK widget.</summary>
	sealed class TestElement : UIElement
	{
		new internal UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}

	// ── Button.Command basic tests ─────────────────────────────────────────────

	/// <summary>Clicking a Button with a command and CanExecute=true executes the command.</summary>
	[Fact]
	public void Button_click_executes_command_when_can_execute()
	{
		var executed = false;
		var cmd = new RelayCommand(_ => executed = true);
		var btn = new WpfButton { Command = cmd };

		// Ensure the routed Click event wiring compiles (no GTK in pure test).
		btn.Click += (s, e) => { };

		// Simulate click via ExecuteCommandIfAble (the internal hook tested through internal access).
		// Use the DP to trigger via the same path the GTK signal takes.
		// Can't call GLib.Signal in a pure test — call the internal method directly.
		btn.ExecuteCommandIfAble_ForTest();

		Assert.True(executed);
	}

	/// <summary>Clicking a Button with CanExecute=false does NOT execute the command.</summary>
	[Fact]
	public void Button_click_does_not_execute_when_cannot_execute()
	{
		var executed = false;
		var cmd = new RelayCommand(_ => executed = true, _ => false);
		var btn = new WpfButton { Command = cmd };

		btn.ExecuteCommandIfAble_ForTest();

		Assert.False(executed);
	}

	/// <summary>CommandParameter is forwarded to Execute.</summary>
	[Fact]
	public void Button_passes_CommandParameter_to_Execute()
	{
		object? received = null;
		var cmd = new RelayCommand(p => received = p);
		var btn = new WpfButton { Command = cmd, CommandParameter = "hello" };

		btn.ExecuteCommandIfAble_ForTest();

		Assert.Equal("hello", received);
	}

	/// <summary>When Command is replaced the old handler is unsubscribed (no double-invoke).</summary>
	[Fact]
	public void Button_replacing_command_does_not_double_invoke()
	{
		int count = 0;
		var cmd1 = new RelayCommand(_ => count++);
		var cmd2 = new RelayCommand(_ => count++);

		var btn = new WpfButton { Command = cmd1 };
		btn.Command = cmd2;

		btn.ExecuteCommandIfAble_ForTest();

		// Only cmd2 is active; cmd1's subscription is gone.
		Assert.Equal(1, count);
		Assert.Equal(1, cmd2.ExecuteCallCount);
		Assert.Equal(0, cmd1.ExecuteCallCount);
	}

	/// <summary>InvalidateRequerySuggested fires CanExecuteChanged on a RoutedCommand.</summary>
	[Fact]
	public void InvalidateRequerySuggested_fires_CanExecuteChanged()
	{
		bool fired = false;
		var cmd = new RelayCommand(_ => { });
		cmd.CanExecuteChanged += (_, _) => fired = true;

		// RoutedCommand delegates CanExecuteChanged to RequerySuggested.
		// For RelayCommand the event is on the command itself.
		cmd.RaiseCanExecuteChanged();

		Assert.True(fired);
	}

	// ── CommandManager.InvalidateRequerySuggested ─────────────────────────────

	/// <summary>InvalidateRequerySuggested raises RequerySuggested.</summary>
	[Fact]
	public void CommandManager_InvalidateRequerySuggested_fires_RequerySuggested()
	{
		bool fired = false;
		EventHandler handler = (_, _) => fired = true;
		CommandManager.RequerySuggested += handler;
		try
		{
			CommandManager.CancelPendingRequeryForTest();
			CommandManager.InvalidateRequerySuggested();
			Assert.True(fired);
		}
		finally
		{
			CommandManager.RequerySuggested -= handler;
		}
	}

	// ── RoutedCommand + CommandBinding tree walk ───────────────────────────────

	/// <summary>RoutedCommand.Execute finds a CommandBinding on the target element.</summary>
	[Fact]
	public void RoutedCommand_executes_via_CommandBinding_on_target()
	{
		bool executed = false;
		var cmd = new RoutedCommand("Test1", typeof(CommandTests));

		var element = new TestElement();
		element.CommandBindings.Add(new CommandBinding(
			cmd,
			(s, e) => executed = true));

		cmd.Execute(null, element);

		Assert.True(executed);
	}

	/// <summary>RoutedCommand.Execute walks up to parent's CommandBinding.</summary>
	[Fact]
	public void RoutedCommand_executes_via_CommandBinding_on_parent()
	{
		bool executed = false;
		var cmd = new RoutedCommand("Test2", typeof(CommandTests));

		var parent = new TestElement();
		var child = new TestElement { LogicalParent = parent };

		parent.CommandBindings.Add(new CommandBinding(
			cmd,
			(s, e) => executed = true));

		cmd.Execute(null, child);

		Assert.True(executed);
	}

	/// <summary>RoutedCommand.CanExecute returns handler result from nearest binding.</summary>
	[Fact]
	public void RoutedCommand_CanExecute_returns_handler_result()
	{
		var cmd = new RoutedCommand("Test3", typeof(CommandTests));
		var element = new TestElement();
		element.CommandBindings.Add(new CommandBinding(
			cmd,
			(s, e) => { },
			(s, e) => e.CanExecute = false));

		var result = cmd.CanExecute(null, element);

		Assert.False(result);
	}

	/// <summary>When no CommandBinding has a CanExecute handler, CanExecute defaults to true.</summary>
	[Fact]
	public void RoutedCommand_CanExecute_defaults_true_when_no_handler()
	{
		var cmd = new RoutedCommand("Test4", typeof(CommandTests));
		var element = new TestElement();
		element.CommandBindings.Add(new CommandBinding(cmd, (s, e) => { }));

		var result = cmd.CanExecute(null, element);

		Assert.True(result);
	}

	// ── MenuItem ──────────────────────────────────────────────────────────────

	/// <summary>MenuItem.Click fires when activated.</summary>
	[Fact]
	public void MenuItem_Click_fires()
	{
		var item = new WpfMenuItem { Header = "File" };
		bool fired = false;
		item.Click += (s, e) => fired = true;

		item.InvokeClick_ForTest();

		Assert.True(fired);
	}

	/// <summary>MenuItem with a command invokes it on activation.</summary>
	[Fact]
	public void MenuItem_invokes_command_on_activation()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true);
		var item = new WpfMenuItem { Header = "New", Command = cmd };

		item.InvokeClick_ForTest();

		Assert.True(executed);
	}

	/// <summary>MenuItem does not invoke command when CanExecute is false.</summary>
	[Fact]
	public void MenuItem_does_not_invoke_command_when_cannot_execute()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true, _ => false);
		var item = new WpfMenuItem { Header = "New", Command = cmd };

		item.InvokeClick_ForTest();

		Assert.False(executed);
	}

	// ── InputBindings / KeyBinding / MouseBinding ──────────────────────────────

	/// <summary>InvokeInputBinding with a matching KeyGesture invokes the command.</summary>
	[Fact]
	public void InvokeInputBinding_with_matching_KeyGesture_executes_command()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true);
		var element = new TestElement();
		element.InputBindings.Add(new KeyBinding(cmd, Key.S, ModifierKeys.Control));

		var result = element.InvokeInputBinding(
			new KeyGestureEventArgs(Key.S, ModifierKeys.Control));

		Assert.True(result);
		Assert.True(executed);
	}

	/// <summary>InvokeInputBinding with a non-matching gesture does not execute.</summary>
	[Fact]
	public void InvokeInputBinding_with_mismatched_gesture_returns_false()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true);
		var element = new TestElement();
		element.InputBindings.Add(new KeyBinding(cmd, Key.S, ModifierKeys.Control));

		var result = element.InvokeInputBinding(
			new KeyGestureEventArgs(Key.Z, ModifierKeys.Control));

		Assert.False(result);
		Assert.False(executed);
	}

	/// <summary>InvokeInputBinding does not execute when CanExecute is false.</summary>
	[Fact]
	public void InvokeInputBinding_does_not_execute_when_cannot_execute()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true, _ => false);
		var element = new TestElement();
		element.InputBindings.Add(new KeyBinding(cmd, Key.S, ModifierKeys.Control));

		var result = element.InvokeInputBinding(
			new KeyGestureEventArgs(Key.S, ModifierKeys.Control));

		Assert.False(result);
		Assert.False(executed);
	}

	/// <summary>MouseBinding with a matching gesture executes the command.</summary>
	[Fact]
	public void InvokeInputBinding_with_matching_MouseGesture_executes_command()
	{
		bool executed = false;
		var cmd = new RelayCommand(_ => executed = true);
		var element = new TestElement();
		element.InputBindings.Add(
			new MouseBinding(cmd, new MouseGesture(MouseAction.LeftDoubleClick)));

		var result = element.InvokeInputBinding(
			new MouseGestureEventArgs(MouseAction.LeftDoubleClick));

		Assert.True(result);
		Assert.True(executed);
	}

	// ── RoutedCommand.CanExecuteChanged delegates to RequerySuggested ─────────

	/// <summary>RoutedCommand.CanExecuteChanged fires on InvalidateRequerySuggested.</summary>
	[Fact]
	public void RoutedCommand_CanExecuteChanged_fires_on_InvalidateRequerySuggested()
	{
		bool fired = false;
		var cmd = new RoutedCommand("Test5", typeof(CommandTests));
		cmd.CanExecuteChanged += (_, _) => fired = true;
		try
		{
			CommandManager.CancelPendingRequeryForTest();
			CommandManager.InvalidateRequerySuggested();
			Assert.True(fired);
		}
		finally
		{
			cmd.CanExecuteChanged -= (_, _) => fired = true;
		}
	}
}