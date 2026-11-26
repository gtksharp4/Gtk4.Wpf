using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>An exception that escapes a GTK signal callback is printed when nothing handles it.</summary>
public class ApplicationGlibExceptionPrintTests : WpfGtkTestBase
{
	static string CaptureStderr(Action body)
	{
		var original = Console.Error;
		var writer = new StringWriter();
		Console.SetError(writer);
		try { body(); }
		finally { Console.SetError(original); }
		return writer.ToString();
	}

	[Fact]
	public void Unhandled_exception_is_printed_and_terminates() => Run(() =>
	{
		var args = new GLib.UnhandledExceptionArgs(new InvalidOperationException("layout boom"), false);

		var text = CaptureStderr(() => Application.OnGlibUnhandled(args));

		Assert.True(args.ExitApplication);
		Assert.Contains("InvalidOperationException", text);
		Assert.Contains("layout boom", text);
	});

	[Fact]
	public void Exception_already_reported_by_the_dispatcher_is_printed_and_terminates() => Run(() =>
	{
		var ex = new InvalidOperationException("queued boom");
		ex.Data[Dispatcher.ReportedKey] = true;
		var args = new GLib.UnhandledExceptionArgs(ex, false);

		var text = CaptureStderr(() => Application.OnGlibUnhandled(args));

		Assert.True(args.ExitApplication);
		Assert.Contains("queued boom", text);
	});

	[Fact]
	public void Handled_exception_is_not_printed_and_the_app_keeps_running() => Run(() =>
	{
		var dispatcher = Dispatcher.CurrentDispatcher;
		DispatcherUnhandledExceptionEventHandler handler = (_, e) => e.Handled = true;
		dispatcher.UnhandledException += handler;
		try
		{
			var args = new GLib.UnhandledExceptionArgs(new InvalidOperationException("quiet"), false);

			var text = CaptureStderr(() => Application.OnGlibUnhandled(args));

			Assert.False(args.ExitApplication);
			Assert.DoesNotContain("quiet", text);
		}
		finally { dispatcher.UnhandledException -= handler; }
	});
}
