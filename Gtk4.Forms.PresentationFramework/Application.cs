using System;
using System.Windows.Threading;

namespace System.Windows;

/// <summary>
/// Hosts the WPF application: GTK init, the startup/shutdown lifecycle, the window list and
/// application resources. The partial files split the lifecycle by concern.
/// </summary>
/// <remarks>
/// Documented gaps versus WPF: a second <c>new Application()</c> does not throw (the test suite
/// constructs several), and <see cref="SessionEnding"/> is never raised on GTK.
/// </remarks>
public partial class Application : DispatcherObject
{
	/// <summary>The most recently created <see cref="Application"/>.</summary>
	public static Application? Current { get; private set; }

	internal static void ResetCurrentForTests() => Current = null;

	/// <summary>
	/// Initialises the application, sets <see cref="Current"/>, and seeds the
	/// theme dictionary with built-in default styles via <see cref="Themes.Generic"/>.
	/// </summary>
	public Application()
	{
		Current = this;
		Themes.Generic.Register();
	}

	/// <summary>
	/// The main window. The first <see cref="Window"/> created while this application is
	/// <see cref="Current"/> becomes it; it is cleared when that window closes.
	/// </summary>
	public Window? MainWindow { get; set; }

	ResourceDictionary _resources = new ResourceDictionary();

	/// <summary>Application-level resources shared by all windows.</summary>
	public ResourceDictionary Resources
	{
		get => _resources;
		set => ReplaceResources(value);
	}

	/// <summary>
	/// URI of the XAML window to instantiate once startup completes (set by the generated
	/// App.g.cs entry-point). Loaded after <see cref="Startup"/>; a window created by a Startup handler stays
	/// <see cref="MainWindow"/>. Shutdown requested from a Startup handler is asynchronous, so the window is still
	/// created and is then closed by the posted shutdown.
	/// </summary>
	public Uri? StartupUri { get; set; }
}
