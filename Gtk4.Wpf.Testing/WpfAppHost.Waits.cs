using System;
using System.Windows.Automation;

namespace Gtk4.Wpf.Testing;

public sealed partial class WpfAppHost
{
	/// <summary>The default for every wait.</summary>
	public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Pumps the GTK loop on the GTK thread until <paramref name="condition"/> (evaluated there) is true. Never
	/// sleeps. Throws <see cref="WaitTimeoutException"/> with the automation tree when it times out.
	/// </summary>
	public void WaitUntil(Func<bool> condition, TimeSpan? timeout = null, string? description = null)
	{
		var limit = timeout ?? DefaultTimeout;
		if (GtkThread.PumpUntil(condition, limit)) return;
		throw new WaitTimeoutException(
			$"Timed out after {limit.TotalSeconds:0.#} s waiting for {description ?? "the condition"}."
			+ Environment.NewLine + "Automation tree:" + Environment.NewLine + DumpTree());
	}

	/// <summary>Waits until <paramref name="by"/> matches an element of the main window, and returns it.</summary>
	public AutomationElement WaitFor(By by, TimeSpan? timeout = null)
	{
		AutomationElement? found = null;
		WaitUntil(() => (found = MainWindow.TryFind(by)) != null, timeout, by.Description);
		return found!;
	}

	/// <summary>Waits until every queued dispatcher and GTK idle callback has run.</summary>
	public void WaitForIdle() => GtkThread.WaitForIdle();
}
