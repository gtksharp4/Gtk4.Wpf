using System;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Threading;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class ThreadMarshalTests : UiaTestBase
{
	// The bridge Application.Run installs, with the owning thread being the test's GTK thread.
	sealed class MainLoopBridge : IDisposable
	{
		readonly int _gtkThread = Environment.CurrentManagedThreadId;
		readonly Func<bool> _isOn = Dispatcher.IsOnDispatcherThread;
		readonly Action<Action> _invoke = Dispatcher.InvokeAction;
		readonly Action<Action> _begin = Dispatcher.BeginInvokeAction;
		readonly Action<Action, DispatcherPriority>? _beginPriority = Dispatcher.BeginInvokePriorityAction;

		public MainLoopBridge()
		{
			Dispatcher.IsOnDispatcherThread = () => Environment.CurrentManagedThreadId == _gtkThread;
			Dispatcher.BeginInvokePriorityAction = System.Windows.Application.PostToMainLoop;
			Dispatcher.InvokeAction = action =>
			{
				if (Dispatcher.IsOnDispatcherThread()) { action(); return; }
				using var done = new ManualResetEventSlim();
				System.Windows.Application.PostToMainLoop(() => { try { action(); } finally { done.Set(); } },
					DispatcherPriority.Send);
				done.Wait();
			};
		}

		public void Dispose()
		{
			Dispatcher.IsOnDispatcherThread = _isOn;
			Dispatcher.InvokeAction = _invoke;
			Dispatcher.BeginInvokeAction = _begin;
			Dispatcher.BeginInvokePriorityAction = _beginPriority;
		}
	}

	static void OnWorker(Action work)
	{
		Exception? error = null;
		var thread = new Thread(() =>
		{
			try { work(); }
			catch (Exception e) { error = e; }
		});
		thread.Start();
		Assert.True(GtkPump.Until(() => !thread.IsAlive, 10000), "the client call never completed");
		thread.Join();
		if (error != null) throw new Exception("worker failed", error);
	}

	[Fact]
	public void Client_calls_from_another_thread_marshal_to_the_GTK_thread_and_see_the_result() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var f = OpenForm();
		int gtkThread = Environment.CurrentManagedThreadId;
		int clickThread = -1;
		f.Ok.Click += (s, e) => clickThread = Environment.CurrentManagedThreadId;
		OnWorker(() =>
		{
			var ok = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
				new PropertyCondition(AutomationElement.AutomationIdProperty, "Ok"));
			Assert.Equal("OK", ok.Current.Name);
			((InvokePattern)ok.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
			// The click was posted through the dispatcher; the call returned only after it ran.
			Assert.Equal(1, f.Clicks);
			var value = (ValuePattern)Find("Name").GetCurrentPattern(ValuePattern.Pattern);
			value.SetValue("typed");
			Assert.Equal("typed", value.Current.Value);
		});
		Assert.Equal(gtkThread, clickThread);
		Assert.Equal("typed", f.Name.Text);
	});

	[Fact]
	public void Exceptions_cross_the_thread_boundary() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		OpenForm();
		OnWorker(() =>
		{
			var invoke = (InvokePattern)Find("Off").GetCurrentPattern(InvokePattern.Pattern);
			Assert.Throws<ElementNotEnabledException>(invoke.Invoke);
			Assert.Throws<InvalidOperationException>(() => Find("Ok").GetCurrentPattern(TogglePattern.Pattern));
		});
	});

	[Fact]
	public void Events_are_delivered_on_the_GTK_thread() => Run(() =>
	{
		using var bridge = new MainLoopBridge();
		var f = OpenForm();
		int gtkThread = Environment.CurrentManagedThreadId;
		int handlerThread = -1;
		try
		{
			OnWorker(() =>
			{
				System.Windows.Automation.Automation.AddAutomationPropertyChangedEventHandler(Find("Agree"),
					TreeScope.Element, (s, e) => handlerThread = Environment.CurrentManagedThreadId,
					TogglePattern.ToggleStateProperty);
				((TogglePattern)Find("Agree").GetCurrentPattern(TogglePattern.Pattern)).Toggle();
			});
			Assert.Equal(gtkThread, handlerThread);
			Assert.True(f.Agree.IsChecked);
		}
		finally { System.Windows.Automation.Automation.RemoveAllEventHandlers(); }
	});
}
