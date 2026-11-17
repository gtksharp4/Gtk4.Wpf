using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Platform.GTK;

/// <summary>
/// Reports the real screen geometry rather than the hardcoded 800x600 the GTK2 backend used.
/// This matters beyond cosmetics: <c>FlyoutPage.ShouldShowSplitMode</c> asks
/// <c>Device.Info.CurrentOrientation.IsLandscape()</c>, so a stubbed orientation silently
/// picks the wrong flyout behaviour on desktop.
/// </summary>
public class GtkDeviceInfo : DeviceInfo
{
	// Fallbacks for a headless/screenless process, where Gdk.Display.Default is null.
	const int FallbackWidth = 800;
	const int FallbackHeight = 600;

	public GtkDeviceInfo()
	{
		CurrentOrientation = GetOrientation();

		// GTK4: Gdk.Screen is removed; use Gdk.Display.Default.
		// Monitor-list changes fire GLib.IListModel.ItemsChanged on Display.Monitors.
		var display = Gdk.Display.Default;

		if (display != null)
		{
			display.Monitors.ItemsChanged += (o, args) => CurrentOrientation = GetOrientation();
		}
	}

	public override Size PixelScreenSize
	{
		get
		{
			var scaled = ScaledScreenSize;
			var scale = ScalingFactor;

			return new Size(scaled.Width * scale, scaled.Height * scale);
		}
	}

	public override Size ScaledScreenSize
	{
		get
		{
			var geometry = GetPrimaryMonitorGeometry();

			return new Size(geometry.Width, geometry.Height);
		}
	}

	public override double ScalingFactor
	{
		get
		{
			var monitor = GetPrimaryMonitor();

			// Gdk reports an integer scale factor (1 for normal, 2 for HiDPI).
			return monitor?.ScaleFactor ?? 1;
		}
	}

	static Gdk.Monitor GetPrimaryMonitor()
	{
		var display = Gdk.Display.Default;

		if (display == null)
			return null;

		// GTK4: PrimaryMonitor and GetMonitor(n) are removed.
		// Display.Monitors is a GLib.IListModel; item 0 covers displays that report
		// no primary monitor (normal under Xvfb and some Wayland compositors).
		var monitors = display.Monitors;

		if (monitors == null || monitors.NItems == 0)
			return null;

		return monitors.GetObject(0) as Gdk.Monitor;
	}

	static Gdk.Rectangle GetPrimaryMonitorGeometry()
	{
		var monitor = GetPrimaryMonitor();

		if (monitor == null)
			return new Gdk.Rectangle(0, 0, FallbackWidth, FallbackHeight);

		return monitor.Geometry;
	}

	static DeviceOrientation GetOrientation()
	{
		var geometry = GetPrimaryMonitorGeometry();

		return geometry.Width >= geometry.Height
			? DeviceOrientation.Landscape
			: DeviceOrientation.Portrait;
	}
}