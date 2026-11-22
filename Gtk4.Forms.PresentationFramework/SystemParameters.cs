using System.ComponentModel;

namespace System.Windows;

/// <summary>
/// System metrics. Screen size comes from the first GDK monitor when a display is open; everything else is the
/// WPF default, because GTK exposes no matching setting. GTK 4 has no work-area query, so
/// <see cref="WorkArea"/> is the whole monitor.
/// </summary>
public static class SystemParameters
{
	/// <summary>Raised when a static property changes. GTK settings are not observed, so it is never raised.</summary>
	public static event PropertyChangedEventHandler? StaticPropertyChanged
	{
		add { }
		remove { }
	}

	/// <summary>Whether a high-contrast theme is active.</summary>
	public static bool HighContrast => false;

	/// <summary>The width of the primary screen in device-independent pixels.</summary>
	public static double PrimaryScreenWidth => Screen().Width;

	/// <summary>The height of the primary screen in device-independent pixels.</summary>
	public static double PrimaryScreenHeight => Screen().Height;

	/// <summary>The area of the primary screen available to windows.</summary>
	public static Rect WorkArea => Screen();

	/// <summary>The horizontal distance a drag must travel to start.</summary>
	public static double MinimumHorizontalDragDistance => 4.0;

	/// <summary>The vertical distance a drag must travel to start.</summary>
	public static double MinimumVerticalDragDistance => 4.0;

	/// <summary>The width of a vertical scroll bar.</summary>
	public static double VerticalScrollBarWidth => 17.0;

	/// <summary>The height of a horizontal scroll bar.</summary>
	public static double HorizontalScrollBarHeight => 17.0;

	/// <summary>The width of a window resize border.</summary>
	public static Thickness WindowResizeBorderThickness => new Thickness(4);

	/// <summary>The height of a window caption (Windows default).</summary>
	public static double WindowCaptionHeight => 23.0;

	/// <summary>The thickness of the non-client frame around a window (Windows default).</summary>
	public static Thickness WindowNonClientFrameThickness => new Thickness(4, 27, 4, 4);

	/// <summary>The corner radius of a window (Windows default).</summary>
	public static CornerRadius WindowCornerRadius => new CornerRadius(8);

	/// <summary>Whether menu popups animate.</summary>
	public static bool MenuAnimation => true;

	/// <summary>How menu popups animate.</summary>
	public static Controls.Primitives.PopupAnimation MenuPopupAnimation =>
		Controls.Primitives.PopupAnimation.Fade;

	/// <summary>Key of <see cref="VerticalScrollBarWidth"/>.</summary>
	public static ResourceKey VerticalScrollBarWidthKey { get; } = new SystemResourceKey(nameof(VerticalScrollBarWidth));

	/// <summary>Key of <see cref="HorizontalScrollBarHeight"/>.</summary>
	public static ResourceKey HorizontalScrollBarHeightKey { get; } =
		new SystemResourceKey(nameof(HorizontalScrollBarHeight));

	/// <summary>Key of <see cref="MenuPopupAnimation"/>.</summary>
	public static ResourceKey MenuPopupAnimationKey { get; } = new SystemResourceKey(nameof(MenuPopupAnimation));

	/// <summary>Key of <see cref="HighContrast"/>.</summary>
	public static ResourceKey HighContrastKey { get; } = new SystemResourceKey(nameof(HighContrast));

	static Rect Screen()
	{
		try
		{
			var monitor = Gdk.Display.Default?.Monitors.GetObject(0) as Gdk.Monitor;
			if (monitor != null)
			{
				var g = monitor.Geometry;
				return new Rect(g.X, g.Y, g.Width, g.Height);
			}
		}
		catch (Exception)
		{
			// No GTK runtime or display: fall back to the default below.
		}
		return new Rect(0, 0, 1920, 1080);
	}
}
