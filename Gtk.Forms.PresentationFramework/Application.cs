using System;

namespace System.Windows
{
	/// <summary>
	/// Hosts the WPF application. Initialises GTK, shows <see cref="MainWindow"/> (or the
	/// window resolved from <see cref="StartupUri"/>), and runs the GTK main loop.
	/// </summary>
	/// <remarks>
	/// Phase-0: GTK is initialised via <see cref="Gtk.Application.Init()"/> and driven by
	/// <see cref="GLib.MainLoop"/>. Phase-1 refinement will bridge to the
	/// <c>Gtk4.Forms.Platform.Gtk</c> STA-thread host (<c>FormsWindow</c>,
	/// <c>GtkThemes</c>, <c>GtkSynchronizationContext</c>) when deeper integration is needed.
	/// </remarks>
	public class Application
	{
		GLib.MainLoop? _loop;

		/// <summary>The main window shown when <see cref="Run()"/> is called.</summary>
		public Window? MainWindow { get; set; }

		/// <summary>Application-level resources shared by all windows.</summary>
		public ResourceDictionary Resources { get; } = new ResourceDictionary();

		/// <summary>
		/// URI of the XAML window to instantiate on startup (set by the generated
		/// App.g.cs entry-point). Takes precedence over <see cref="MainWindow"/> when set.
		/// Phase-0: <see cref="MainWindow"/> is set directly in code instead.
		/// </summary>
		public Uri? StartupUri { get; set; }

		/// <summary>
		/// Initialises GTK and runs the main loop, showing <see cref="MainWindow"/>.
		/// Returns the GTK exit code (always 0 in Phase 0).
		/// </summary>
		public int Run()
		{
			Gtk.Application.Init();

			var window = MainWindow;
			window?.Show();

			_loop = new GLib.MainLoop();
			_loop.Run();
			return 0;
		}

		/// <summary>
		/// Convenience overload: sets <see cref="MainWindow"/> and calls <see cref="Run()"/>.
		/// </summary>
		public int Run(Window mainWindow)
		{
			MainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
			return Run();
		}

		/// <summary>
		/// Requests that the main loop terminate and the process exit.
		/// </summary>
		public void Shutdown()
		{
			if (_loop != null && _loop.IsRunning)
				_loop.Quit();
		}
	}
}
