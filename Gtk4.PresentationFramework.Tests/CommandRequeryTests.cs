using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// WPF semantics of CommandManager.InvalidateRequerySuggested: one coalesced, asynchronous raise at Background
/// priority on the dispatcher thread, callable from any thread, plus automatic suggestions after input.
/// </summary>
public class CommandRequeryTests : WpfGtkTestBase
{
	// Installs the bridge Application.Run installs, so BeginInvoke is queued on the GLib loop instead of running inline.
	sealed class MainLoopBridge : IDisposable
	{
		readonly int _gtkThread = Environment.CurrentManagedThreadId;
		readonly Func<bool> _isOn = Dispatcher.IsOnDispatcherThread;
		readonly Action<Action> _begin = Dispatcher.BeginInvokeAction;
		readonly Action<Action, DispatcherPriority>? _beginPriority = Dispatcher.BeginInvokePriorityAction;

		public MainLoopBridge()
		{
			CommandManager.CancelPendingRequeryForTest();
			Dispatcher.IsOnDispatcherThread = () => Environment.CurrentManagedThreadId == _gtkThread;
			Dispatcher.BeginInvokePriorityAction = Application.PostToMainLoop;
		}

		public void Dispose()
		{
			CommandManager.CancelPendingRequeryForTest();
			Dispatcher.IsOnDispatcherThread = _isOn;
			Dispatcher.BeginInvokeAction = _begin;
			Dispatcher.BeginInvokePriorityAction = _beginPriority;
		}
	}

	sealed class Counter
	{
		public int Raises;
		public int ThreadId = -1;
		public readonly EventHandler Handler;

		public Counter() => Handler = (_, _) =>
		{
			Raises++;
			ThreadId = Environment.CurrentManagedThreadId;
		};
	}

	// Pumps until the first raise, then a little longer to show there is no second one.
	static int PumpRaises(Counter counter)
	{
		GtkPump.Until(() => counter.Raises > 0);
		GtkPump.For(150);
		return counter.Raises;
	}

	[Fact]
	public void Calls_in_one_turn_raise_once_after_the_call_returns() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			CommandManager.InvalidateRequerySuggested();
			CommandManager.InvalidateRequerySuggested();
			CommandManager.InvalidateRequerySuggested();

			Assert.Equal(0, counter.Raises);
			Assert.Equal(1, PumpRaises(counter));
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void A_new_call_after_the_raise_raises_again() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			CommandManager.InvalidateRequerySuggested();
			Assert.Equal(1, PumpRaises(counter));
			CommandManager.InvalidateRequerySuggested();
			GtkPump.Until(() => counter.Raises > 1);
			Assert.Equal(2, counter.Raises);
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void Call_from_a_pool_thread_raises_on_the_GTK_thread() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			Exception? error = null;
			var thread = new Thread(() =>
			{
				try { CommandManager.InvalidateRequerySuggested(); }
				catch (Exception e) { error = e; }
			});
			thread.Start();
			Assert.True(GtkPump.Until(() => !thread.IsAlive));
			thread.Join();
			Assert.Null(error);

			Assert.Equal(1, PumpRaises(counter));
			Assert.Equal(Environment.CurrentManagedThreadId, counter.ThreadId);
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void Key_release_leads_to_one_raise() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		var button = new WpfButton();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			button.RaiseKeyUpForTest(Key.A);
			Assert.Equal(1, PumpRaises(counter));
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void Mouse_button_release_leads_to_one_raise() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		var button = new WpfButton();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			button.RaiseMouseButton(MouseButton.Left, new Point(1, 1), 1, pressed: false);
			Assert.Equal(1, PumpRaises(counter));
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void Mouse_move_does_not_suggest_a_requery() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		var button = new WpfButton();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			button.RaiseMouseMove(new Point(1, 1));
			GtkPump.For(150);
			Assert.Equal(0, counter.Raises);
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});

	[Fact]
	public void Keyboard_focus_move_leads_to_one_raise() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		var first = new WpfButton();
		var second = new WpfButton();
		Keyboard.SetFocused(first);
		CommandManager.CancelPendingRequeryForTest();
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			Keyboard.SetFocused(second);
			Assert.Equal(1, PumpRaises(counter));
		}
		finally
		{
			CommandManager.RequerySuggested -= counter.Handler;
			Keyboard.ClearFocused(second);
		}
	});

	[Fact]
	public void A_command_source_updates_after_input_without_a_manual_call() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		bool allow = false;
		var command = new RoutedCommand("Requery", typeof(CommandRequeryTests));
		var host = new WpfButton();
		host.CommandBindings.Add(new CommandBinding(command, (_, _) => { }, (_, e) => e.CanExecute = allow));
		var source = new WpfButton { Command = command, CommandTarget = host };
		Assert.False(source.IsEnabled);

		allow = true;
		host.RaiseKeyUpForTest(Key.A);
		GtkPump.Until(() => source.IsEnabled);

		Assert.True(source.IsEnabled);
	});

	[Fact]
	public void Selecting_text_enables_a_Copy_button_without_a_manual_requery() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var box = new WpfTextBox { Text = "hello world" };
		var copy = new WpfButton { Command = ApplicationCommands.Copy, CommandTarget = box };
		Assert.False(copy.IsEnabled);

		box.Select(0, 5);
		GtkPump.Until(() => copy.IsEnabled);

		Assert.True(copy.IsEnabled);
		box.Select(0, 0);
		GtkPump.Until(() => !copy.IsEnabled);
		Assert.False(copy.IsEnabled);
	});

	[Fact]
	public void An_idle_loop_raises_no_further_requery() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var counter = new Counter();
		var source = new WpfButton { Command = ApplicationCommands.Copy, CommandTarget = new WpfTextBox() };
		CommandManager.RequerySuggested += counter.Handler;
		try
		{
			CommandManager.InvalidateRequerySuggested();
			Assert.Equal(1, PumpRaises(counter));
			GtkPump.For(300);
			Assert.Equal(1, counter.Raises);
			GC.KeepAlive(source);
		}
		finally { CommandManager.RequerySuggested -= counter.Handler; }
	});
}
