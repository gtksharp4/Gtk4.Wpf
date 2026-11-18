using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for TextBlock layout DPs and mapping helpers.
/// </summary>
public class TextBlockLayoutDpTests
{
	// ── TextAlignment default + round-trip ───────────────────────────────────

	[Fact]
	public void TextAlignment_default_is_Left()
	{
		var tb = new TextBlock();
		Assert.Equal(TextAlignment.Left, tb.TextAlignment);
	}

	[Fact]
	public void TextAlignment_roundtrip_Center()
	{
		var tb = new TextBlock { TextAlignment = TextAlignment.Center };
		Assert.Equal(TextAlignment.Center, tb.TextAlignment);
	}

	[Fact]
	public void TextAlignment_roundtrip_Right()
	{
		var tb = new TextBlock { TextAlignment = TextAlignment.Right };
		Assert.Equal(TextAlignment.Right, tb.TextAlignment);
	}

	[Fact]
	public void TextAlignment_roundtrip_Justify()
	{
		var tb = new TextBlock { TextAlignment = TextAlignment.Justify };
		Assert.Equal(TextAlignment.Justify, tb.TextAlignment);
	}

	// ── TextTrimming default + round-trip ────────────────────────────────────

	[Fact]
	public void TextTrimming_default_is_None()
	{
		var tb = new TextBlock();
		Assert.Equal(TextTrimming.None, tb.TextTrimming);
	}

	[Fact]
	public void TextTrimming_roundtrip_CharacterEllipsis()
	{
		var tb = new TextBlock { TextTrimming = TextTrimming.CharacterEllipsis };
		Assert.Equal(TextTrimming.CharacterEllipsis, tb.TextTrimming);
	}

	[Fact]
	public void TextTrimming_roundtrip_WordEllipsis()
	{
		var tb = new TextBlock { TextTrimming = TextTrimming.WordEllipsis };
		Assert.Equal(TextTrimming.WordEllipsis, tb.TextTrimming);
	}

	// ── Padding default + round-trip ─────────────────────────────────────────

	[Fact]
	public void Padding_default_is_zero()
	{
		var tb = new TextBlock();
		Assert.Equal(new Thickness(0), tb.Padding);
	}

	[Fact]
	public void Padding_roundtrip_uniform()
	{
		var tb = new TextBlock { Padding = new Thickness(8) };
		Assert.Equal(new Thickness(8), tb.Padding);
	}

	[Fact]
	public void Padding_roundtrip_non_uniform()
	{
		var tb = new TextBlock { Padding = new Thickness(1, 2, 3, 4) };
		Assert.Equal(new Thickness(1, 2, 3, 4), tb.Padding);
	}

	// ── LineHeight default + round-trip ──────────────────────────────────────

	[Fact]
	public void LineHeight_default_is_NaN()
	{
		var tb = new TextBlock();
		Assert.True(double.IsNaN(tb.LineHeight));
	}

	[Fact]
	public void LineHeight_roundtrip_value()
	{
		var tb = new TextBlock { LineHeight = 24.0 };
		Assert.Equal(24.0, tb.LineHeight);
	}

	[Fact]
	public void LineHeight_roundtrip_NaN()
	{
		var tb = new TextBlock { LineHeight = 20.0 };
		tb.LineHeight = double.NaN;
		Assert.True(double.IsNaN(tb.LineHeight));
	}

	// ── Set before realize → no throw, persists ──────────────────────────────

	[Fact]
	public void TextAlignment_set_before_widget_does_not_throw()
	{
		var tb = new TextBlock();
		var ex = Record.Exception(() => tb.TextAlignment = TextAlignment.Center);
		Assert.Null(ex);
		Assert.Equal(TextAlignment.Center, tb.TextAlignment);
	}

	[Fact]
	public void TextTrimming_set_before_widget_does_not_throw()
	{
		var tb = new TextBlock();
		var ex = Record.Exception(() => tb.TextTrimming = TextTrimming.CharacterEllipsis);
		Assert.Null(ex);
		Assert.Equal(TextTrimming.CharacterEllipsis, tb.TextTrimming);
	}

	[Fact]
	public void Padding_set_before_widget_does_not_throw()
	{
		var tb = new TextBlock();
		var ex = Record.Exception(() => tb.Padding = new Thickness(4));
		Assert.Null(ex);
		Assert.Equal(new Thickness(4), tb.Padding);
	}

	[Fact]
	public void LineHeight_set_before_widget_does_not_throw()
	{
		var tb = new TextBlock();
		var ex = Record.Exception(() => tb.LineHeight = 18.0);
		Assert.Null(ex);
		Assert.Equal(18.0, tb.LineHeight);
	}

	// ── MapTextAlignment helper ───────────────────────────────────────────────

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

	// ── MapTextTrimming helper ────────────────────────────────────────────────

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
