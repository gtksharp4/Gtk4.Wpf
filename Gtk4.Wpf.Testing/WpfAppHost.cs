using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// An unmodified WPF application running in the test process on the GTK thread, exposed through UI Automation.
/// Every member may be called from the test thread; calls marshal to the GTK thread.
/// </summary>
public sealed partial class WpfAppHost : IDisposable
{
	bool _disposed;

	WpfAppHost(Application application, Window window)
	{
		Application = application;
		Window = window;
		Input = new UiInput(this);
		Native = new NativeInput(this);
	}

	/// <summary>The running application.</summary>
	public Application Application { get; }

	/// <summary>The main window as a WPF object. Touch it only through <see cref="Invoke(Action)"/>.</summary>
	public Window Window { get; }

	/// <summary>Routed keyboard and mouse input.</summary>
	public UiInput Input { get; }

	/// <summary>Opt-in operating-system input; see <see cref="NativeInput"/>.</summary>
	public NativeInput Native { get; }

	/// <summary>The main window as a UI Automation element: the default scope of <see cref="Find"/>.</summary>
	public AutomationElement MainWindow => GtkThread.Invoke(() =>
		AutomationElement.Wrap(UIElementAutomationPeer.CreatePeerForElement(Window)!, null));

	/// <summary>Starts <typeparamref name="TApp"/>: its startup shows the <c>StartupUri</c> window.</summary>
	public static WpfAppHost Start<TApp>() where TApp : Application => Start(typeof(TApp));

	/// <summary>Starts an application of <paramref name="applicationType"/> as <c>Application.Run</c> would.</summary>
	public static WpfAppHost Start(Type applicationType)
	{
		if (applicationType == null) throw new ArgumentNullException(nameof(applicationType));
		return Launch(() =>
		{
			var app = (Application)Activator.CreateInstance(applicationType)!;
			// A generated App.xaml partial class wires StartupUri and resources here.
			applicationType.GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance,
				null, Type.EmptyTypes, null)?.Invoke(app, null);
			app.DoStartup(Array.Empty<string>());
			return (app, app.MainWindow
				?? throw new InvalidOperationException(
					$"{applicationType.Name} showed no window: set StartupUri or create one in OnStartup."));
		});
	}

	/// <summary>Starts a new <see cref="Application"/> and shows the window the factory returns.</summary>
	public static WpfAppHost Start(Func<Window> windowFactory)
	{
		if (windowFactory == null) throw new ArgumentNullException(nameof(windowFactory));
		return Launch(() =>
		{
			var app = new Application();
			var window = windowFactory();
			app.PrepareMainWindow(window);
			window.Show();
			return (app, window);
		});
	}

	static WpfAppHost Launch(Func<(Application App, Window Window)> create)
	{
		if (GtkThread.UnavailableReason is { } reason)
			throw new InvalidOperationException(reason);
		return GtkThread.Invoke(() =>
		{
			var (app, window) = create();
			var host = new WpfAppHost(app, window);
			try
			{
				if (!GtkThread.PumpUntil(() => window.Widget.IsMapped && window.Widget.Width > 0,
					TimeSpan.FromSeconds(10)))
					throw new WaitTimeoutException("The main window was not mapped within 10 s."
						+ Environment.NewLine + "Automation tree:" + Environment.NewLine + host.DumpTree());
				GtkThread.WaitForIdle();
			}
			catch
			{
				host.Dispose();
				throw;
			}
			return host;
		});
	}

	/// <summary>Runs <paramref name="action"/> on the GTK thread, for direct access to WPF objects.</summary>
	public void Invoke(Action action) => GtkThread.Invoke(action);

	/// <summary>Runs <paramref name="func"/> on the GTK thread and returns its value.</summary>
	public T Invoke<T>(Func<T> func) => GtkThread.Invoke(func);

	/// <summary>Finds the first descendant of the main window matching <paramref name="by"/>, or throws.</summary>
	public AutomationElement Find(By by) => MainWindow.Find(by);

	/// <summary>Finds the first descendant of the main window matching <paramref name="by"/>, or <c>null</c>.</summary>
	public AutomationElement? TryFind(By by) => MainWindow.TryFind(by);

	/// <summary>Finds every descendant of the main window matching <paramref name="by"/>.</summary>
	public AutomationElement[] FindAll(By by) => MainWindow.FindAll(by);

	/// <summary>The automation tree of every window of the application.</summary>
	public string DumpTree() => TreeDump.Of(AutomationElement.RootElement);

	/// <summary>Closes every window and detaches the automation subscriptions.</summary>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		GtkThread.Invoke(() =>
		{
			System.Windows.Automation.Automation.RemoveAllEventHandlers();
			foreach (var window in Application.Windows.Cast<Window>().ToArray())
				window.Close();
			try { GtkThread.WaitForIdle(TimeSpan.FromSeconds(2)); }
			catch (WaitTimeoutException) { } // a busy app must not block teardown
			Application.ResetCurrentForTests();
		});
	}
}
