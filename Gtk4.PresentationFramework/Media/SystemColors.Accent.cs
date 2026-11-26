using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// The .NET 9 accent family. GTK 4 exposes no queryable accent colour without the obsolete
/// <c>StyleContext</c>, so <c>AccentColor</c> is the default accent of dotnet/wpf's <c>AccentColorHelper</c>
/// (#0078D4). The Light1..3 and Dark1..3 shades are the Windows default palette (UISettings on an unmodified
/// install), not values sourced from WPF.
/// </summary>
static class SystemAccentPalette
{
	internal static readonly SystemColorEntry Accent = new SystemColorEntry("AccentColor", 0, 120, 212);
	internal static readonly SystemColorEntry Light1 = new SystemColorEntry("AccentColorLight1", 58, 150, 221);
	internal static readonly SystemColorEntry Light2 = new SystemColorEntry("AccentColorLight2", 118, 185, 237);
	internal static readonly SystemColorEntry Light3 = new SystemColorEntry("AccentColorLight3", 167, 216, 255);
	internal static readonly SystemColorEntry Dark1 = new SystemColorEntry("AccentColorDark1", 0, 90, 158);
	internal static readonly SystemColorEntry Dark2 = new SystemColorEntry("AccentColorDark2", 0, 78, 140);
	internal static readonly SystemColorEntry Dark3 = new SystemColorEntry("AccentColorDark3", 0, 66, 117);

	static SystemAccentPalette()
	{
		foreach (var e in new[] { Accent, Light1, Light2, Light3, Dark1, Dark2, Dark3 })
			e.Brush.Freeze();
	}
}

public static partial class SystemColors
{
	/// <summary>The accent colour variant AccentColor (Windows default palette).</summary>
	public static Color AccentColor => SystemAccentPalette.Accent.Color;

	/// <summary>The resource key for <see cref="AccentColor"/>.</summary>
	public static ResourceKey AccentColorKey => SystemAccentPalette.Accent.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColor"/>.</summary>
	public static SolidColorBrush AccentColorBrush => SystemAccentPalette.Accent.Brush;

	/// <summary>The resource key for <see cref="AccentColorBrush"/>.</summary>
	public static ResourceKey AccentColorBrushKey => SystemAccentPalette.Accent.BrushKey;

	/// <summary>The accent colour variant AccentColorLight1 (Windows default palette).</summary>
	public static Color AccentColorLight1 => SystemAccentPalette.Light1.Color;

	/// <summary>The resource key for <see cref="AccentColorLight1"/>.</summary>
	public static ResourceKey AccentColorLight1Key => SystemAccentPalette.Light1.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorLight1"/>.</summary>
	public static SolidColorBrush AccentColorLight1Brush => SystemAccentPalette.Light1.Brush;

	/// <summary>The resource key for <see cref="AccentColorLight1Brush"/>.</summary>
	public static ResourceKey AccentColorLight1BrushKey => SystemAccentPalette.Light1.BrushKey;

	/// <summary>The accent colour variant AccentColorLight2 (Windows default palette).</summary>
	public static Color AccentColorLight2 => SystemAccentPalette.Light2.Color;

	/// <summary>The resource key for <see cref="AccentColorLight2"/>.</summary>
	public static ResourceKey AccentColorLight2Key => SystemAccentPalette.Light2.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorLight2"/>.</summary>
	public static SolidColorBrush AccentColorLight2Brush => SystemAccentPalette.Light2.Brush;

	/// <summary>The resource key for <see cref="AccentColorLight2Brush"/>.</summary>
	public static ResourceKey AccentColorLight2BrushKey => SystemAccentPalette.Light2.BrushKey;

	/// <summary>The accent colour variant AccentColorLight3 (Windows default palette).</summary>
	public static Color AccentColorLight3 => SystemAccentPalette.Light3.Color;

	/// <summary>The resource key for <see cref="AccentColorLight3"/>.</summary>
	public static ResourceKey AccentColorLight3Key => SystemAccentPalette.Light3.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorLight3"/>.</summary>
	public static SolidColorBrush AccentColorLight3Brush => SystemAccentPalette.Light3.Brush;

	/// <summary>The resource key for <see cref="AccentColorLight3Brush"/>.</summary>
	public static ResourceKey AccentColorLight3BrushKey => SystemAccentPalette.Light3.BrushKey;

	/// <summary>The accent colour variant AccentColorDark1 (Windows default palette).</summary>
	public static Color AccentColorDark1 => SystemAccentPalette.Dark1.Color;

	/// <summary>The resource key for <see cref="AccentColorDark1"/>.</summary>
	public static ResourceKey AccentColorDark1Key => SystemAccentPalette.Dark1.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorDark1"/>.</summary>
	public static SolidColorBrush AccentColorDark1Brush => SystemAccentPalette.Dark1.Brush;

	/// <summary>The resource key for <see cref="AccentColorDark1Brush"/>.</summary>
	public static ResourceKey AccentColorDark1BrushKey => SystemAccentPalette.Dark1.BrushKey;

	/// <summary>The accent colour variant AccentColorDark2 (Windows default palette).</summary>
	public static Color AccentColorDark2 => SystemAccentPalette.Dark2.Color;

	/// <summary>The resource key for <see cref="AccentColorDark2"/>.</summary>
	public static ResourceKey AccentColorDark2Key => SystemAccentPalette.Dark2.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorDark2"/>.</summary>
	public static SolidColorBrush AccentColorDark2Brush => SystemAccentPalette.Dark2.Brush;

	/// <summary>The resource key for <see cref="AccentColorDark2Brush"/>.</summary>
	public static ResourceKey AccentColorDark2BrushKey => SystemAccentPalette.Dark2.BrushKey;

	/// <summary>The accent colour variant AccentColorDark3 (Windows default palette).</summary>
	public static Color AccentColorDark3 => SystemAccentPalette.Dark3.Color;

	/// <summary>The resource key for <see cref="AccentColorDark3"/>.</summary>
	public static ResourceKey AccentColorDark3Key => SystemAccentPalette.Dark3.ColorKey;

	/// <summary>The cached brush for <see cref="AccentColorDark3"/>.</summary>
	public static SolidColorBrush AccentColorDark3Brush => SystemAccentPalette.Dark3.Brush;

	/// <summary>The resource key for <see cref="AccentColorDark3Brush"/>.</summary>
	public static ResourceKey AccentColorDark3BrushKey => SystemAccentPalette.Dark3.BrushKey;
}
