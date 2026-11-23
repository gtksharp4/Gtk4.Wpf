using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class SystemColorsAccentTests
{
	[Fact]
	public void Accent_colours_match_the_windows_default_palette()
	{
		Assert.Equal(Color.FromRgb(0, 120, 212), SystemColors.AccentColor);
		Assert.Equal(Color.FromRgb(58, 150, 221), SystemColors.AccentColorLight1);
		Assert.Equal(Color.FromRgb(118, 185, 237), SystemColors.AccentColorLight2);
		Assert.Equal(Color.FromRgb(167, 216, 255), SystemColors.AccentColorLight3);
		Assert.Equal(Color.FromRgb(0, 90, 158), SystemColors.AccentColorDark1);
		Assert.Equal(Color.FromRgb(0, 78, 140), SystemColors.AccentColorDark2);
		Assert.Equal(Color.FromRgb(0, 66, 117), SystemColors.AccentColorDark3);
		Assert.Equal(Color.FromRgb(0, 120, 212), SystemColors.AccentColorBrush.Color);
	}

	[Fact]
	public void Brushes_carry_the_colour_and_are_frozen_and_cached()
	{
		Assert.Equal(SystemColors.AccentColorDark2, SystemColors.AccentColorDark2Brush.Color);
		Assert.True(SystemColors.AccentColorBrush.IsFrozen);
		Assert.Same(SystemColors.AccentColorLight3Brush, SystemColors.AccentColorLight3Brush);
	}

	[Fact]
	public void Keys_are_stable_and_distinct()
	{
		var keys = new[]
		{
			SystemColors.AccentColorKey, SystemColors.AccentColorLight1Key, SystemColors.AccentColorLight2Key,
			SystemColors.AccentColorLight3Key, SystemColors.AccentColorDark1Key, SystemColors.AccentColorDark2Key,
			SystemColors.AccentColorDark3Key, SystemColors.AccentColorBrushKey, SystemColors.AccentColorLight1BrushKey,
			SystemColors.AccentColorLight2BrushKey, SystemColors.AccentColorLight3BrushKey,
			SystemColors.AccentColorDark1BrushKey, SystemColors.AccentColorDark2BrushKey,
			SystemColors.AccentColorDark3BrushKey,
		};
		Assert.Equal(14, new System.Collections.Generic.HashSet<ResourceKey>(keys).Count);
		Assert.Same(SystemColors.AccentColorBrushKey, SystemColors.AccentColorBrushKey);
	}

	[Fact]
	public void Keys_resolve_through_FindResource()
	{
		var element = new System.Windows.Controls.Border();
		Assert.Equal(SystemColors.AccentColorDark1, element.FindResource(SystemColors.AccentColorDark1Key));
		Assert.Same(SystemColors.AccentColorBrush, element.FindResource(SystemColors.AccentColorBrushKey));
		Assert.Same(SystemColors.AccentColorLight2Brush, element.TryFindResource(SystemColors.AccentColorLight2BrushKey));
	}
}
