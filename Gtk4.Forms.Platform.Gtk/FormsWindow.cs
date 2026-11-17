using System;
using System.ComponentModel;
using System.Threading;
using Gtk;

namespace Gtk4.Forms.Platform.GTK;

public class FormsWindow : Window
{
	Application _application;
	Gdk.Size _lastSize;

	// GTK4: Window(WindowType.Toplevel) → Window() — toplevel is the only kind.
	public FormsWindow()
		: base()
	{
		SetDefaultSize(800, 600);
		SetSizeRequest(400, 400);

		MainThreadID = Thread.CurrentThread.ManagedThreadId;
		MainWindow = this;

		if (SynchronizationContext.Current == null)
			SynchronizationContext.SetSynchronizationContext(new GtkSynchronizationContext());

		// GTK4: WindowStateEvent / window-state-event signal is gone.
		// Iconify (minimise) state is not directly observable via notify:: in GTK4;
		// SendSleep/SendResume are skipped in this pass.
		// TODO GTK4: use GdkToplevel state notifications if iconify tracking is needed.
	}

	public static int MainThreadID { get; set; }
	public static Window MainWindow { get; set; }

	public void LoadApplication(Application application)
	{
		if (application == null)
			throw new ArgumentNullException(nameof(application));

		Gtk4.Forms.Application.SetCurrentApplication(application);
		_application = application;

		application.PropertyChanged += ApplicationOnPropertyChanged;
		UpdateMainPage();

		_application.SendStart();
	}

	public void SetApplicationTitle(string title)
	{
		if (string.IsNullOrEmpty(title))
			return;

		Title = title;
	}

	public void SetApplicationIcon(string icon)
	{
		if (string.IsNullOrEmpty(icon))
			return;

		// GTK4: Window.Icon (Gdk.Pixbuf) is removed; use Window.IconName (XDG theme name).
		// If the caller passes a file path the name lookup will fail silently, which is
		// preferable to crashing. TODO GTK4: expose a separate API if file-path icons are needed.
		IconName = icon;
	}

	// GTK4: delete-event is gone; close-request replaces it. Return true to veto the close.
	protected override bool OnCloseRequest()
	{
		Gtk.Application.Quit();

		return true;
	}

	void ApplicationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(Gtk4.Forms.Application.MainPage))
		{
			UpdateMainPage();
		}
	}

	// GTK4: configure-event is gone; use the OnSizeAllocated compat vfunc (Drawing.Compat.cs).
	protected override void OnSizeAllocated(Gdk.Rectangle allocation)
	{
		base.OnSizeAllocated(allocation);

		Gdk.Size newSize = new Gdk.Size(allocation.Width, allocation.Height);

		if (_lastSize != newSize)
		{
			_lastSize = newSize;
			var pageRenderer = Platform.GetRenderer(_application?.MainPage);
			pageRenderer?.SetElementSize(new Size(newSize.Width, newSize.Height));
		}
	}

	void UpdateMainPage()
	{
		if (_application.MainPage == null)
			return;

		var platformRenderer = Child as PlatformRenderer;

		if (platformRenderer != null)
		{
			RemoveChildIfExists();
			((IDisposable)platformRenderer.Platform).Dispose();
		}

		var platform = new Platform();
		platform.PlatformRenderer.SetSizeRequest(WidthRequest, HeightRequest);
		Add(platform.PlatformRenderer);
		platform.SetPage(_application.MainPage);

		Child.ShowAll();
	}

	void RemoveChildIfExists()
	{
		foreach (var child in Children)
		{
			var widget = child as Widget;

			if (widget != null)
			{
				Remove(widget);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _application != null)
		{
			_application.PropertyChanged -= ApplicationOnPropertyChanged;
		}

		base.Dispose(disposing);
	}
}