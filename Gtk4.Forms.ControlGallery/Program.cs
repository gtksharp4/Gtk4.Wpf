using System;
using GLib;
using Gtk4.Forms;
using Gtk4.Forms.ControlGallery.GTK;
using Gtk4.Forms.Controls;
using Gtk4.Forms.Platform.GTK;
using Gtk4.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace Gtk4.Forms.ControlGallery.GTK;

class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		ExceptionManager.UnhandledException += OnUnhandledException;

		// GtkOpenGL.Init() is gone for good: it existed only to bootstrap OpenTK's
		// Toolkit, and OpenGLView now runs on Gtk.GLArea, which needs no initialisation
		// (plan M6 §9.2).
		GtkThemes.Init();
		Gtk.Application.Init();

		// The Maps renderer lives in Gtk4.Forms.Maps.GTK, so that assembly has to be
		// loaded before Registrar scans - naming a type in it is what forces the load.
		Forms.Init(new[] { typeof(Gtk4.Forms.Maps.GTK.MapRenderer).Assembly });
		Gtk4.Forms.Maps.GTK.FormsMaps.Init();
		var app = new App();
		var window = new FormsWindow();
		window.LoadApplication(app);
		window.SetApplicationTitle("Gtk4.Forms GTK# Backend");
		window.SetApplicationIcon("gtk4logo.png");
		window.Show();
		Gtk.Application.Run();
	}

	private static void OnUnhandledException(UnhandledExceptionArgs args)
	{
		System.Diagnostics.Debug.WriteLine($"Unhandled GTK# exception: {args.ExceptionObject}");
	}
}

public class DisposePageRenderer : PageRenderer
{
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((DisposePage)Element).SendRendererDisposed();
		}

		base.Dispose(disposing);
	}
}

public class DisposeLabelRenderer : LabelRenderer
{
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			((DisposeLabel)Element).SendRendererDisposed();
		}

		base.Dispose(disposing);
	}
}