using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for the TextBlock layout mapping helpers
/// (<c>MapTextAlignment</c> / <c>MapTextTrimming</c>).
/// </summary>
public class TextBlockLayoutMappingTests
{
	[Fact]
	public void MapTextAlignment_Left_returns_GtkLeft_xalign0()
	{
		var (j, x) = TextBlock.MapTextAlignment(TextAlignment.Left);
		Assert.Equal(Gtk.Justification.Left, j);
		Assert.Equal(0f, x);
	}

	[Fact]
	public void MapTextAlignment_Center_returns_GtkCenter_xalign05()
	{
		var (j, x) = TextBlock.MapTextAlignment(TextAlignment.Center);
		Assert.Equal(Gtk.Justification.Center, j);
		Assert.Equal(0.5f, x);
	}

	[Fact]
	public void MapTextAlignment_Right_returns_GtkRight_xalign1()
	{
		var (j, x) = TextBlock.MapTextAlignment(TextAlignment.Right);
		Assert.Equal(Gtk.Justification.Right, j);
		Assert.Equal(1f, x);
	}

	[Fact]
	public void MapTextAlignment_Justify_returns_GtkFill_xalign0()
	{
		var (j, x) = TextBlock.MapTextAlignment(TextAlignment.Justify);
		Assert.Equal(Gtk.Justification.Fill, j);
		Assert.Equal(0f, x);
	}

	[Fact]
	public void MapTextTrimming_None_returns_EllipsizeModeNone()
	{
		Assert.Equal(Pango.EllipsizeMode.None, TextBlock.MapTextTrimming(TextTrimming.None));
	}

	[Fact]
	public void MapTextTrimming_CharacterEllipsis_returns_End()
	{
		Assert.Equal(Pango.EllipsizeMode.End, TextBlock.MapTextTrimming(TextTrimming.CharacterEllipsis));
	}

	[Fact]
	public void MapTextTrimming_WordEllipsis_returns_End()
	{
		Assert.Equal(Pango.EllipsizeMode.End, TextBlock.MapTextTrimming(TextTrimming.WordEllipsis));
	}
}
