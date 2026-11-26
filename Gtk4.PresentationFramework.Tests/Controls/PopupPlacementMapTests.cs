using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure unit tests for <see cref="PopupPlacementMap.ToGtkPosition"/>.
/// No GTK runtime required.
/// </summary>
public class PopupPlacementMapTests
{
	[Theory]
	[InlineData(PlacementMode.Bottom,        Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.Top,           Gtk.PositionType.Top)]
	[InlineData(PlacementMode.Left,          Gtk.PositionType.Left)]
	[InlineData(PlacementMode.Right,         Gtk.PositionType.Right)]
	// Fallback modes all map to Bottom
	[InlineData(PlacementMode.Center,        Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.Relative,      Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.RelativePoint, Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.Absolute,      Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.AbsolutePoint, Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.Mouse,         Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.MousePoint,    Gtk.PositionType.Bottom)]
	[InlineData(PlacementMode.Custom,        Gtk.PositionType.Bottom)]
	public void ToGtkPosition_returns_expected(PlacementMode mode, Gtk.PositionType expected)
	{
		Assert.Equal(expected, PopupPlacementMap.ToGtkPosition(mode));
	}

	[Fact]
	public void ToGtkPosition_unknown_value_returns_Bottom()
	{
		// An unrecognised future enum value must not throw.
		var result = PopupPlacementMap.ToGtkPosition((PlacementMode)999);
		Assert.Equal(Gtk.PositionType.Bottom, result);
	}
}
