using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for TextBlock layout DPs.
/// Verifies that the backing Gtk.Label receives the correct GTK property values.
/// </summary>
public class TextBlockLayoutGtkTests : WpfGtkTestBase
{
	// ── TextAlignment → Label.Justify / Xalign ───────────────────────────────

	[Fact]
	public void TextAlignment_Center_sets_label_Justify_Center_and_Xalign_half() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", TextAlignment = TextAlignment.Center };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Gtk.Justification.Center, label.Justify);
		Assert.Equal(0.5f, label.Xalign);
	});

	[Fact]
	public void TextAlignment_Right_sets_label_Justify_Right_and_Xalign_one() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", TextAlignment = TextAlignment.Right };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Gtk.Justification.Right, label.Justify);
		Assert.Equal(1f, label.Xalign);
	});

	[Fact]
	public void TextAlignment_Left_sets_label_Justify_Left_and_Xalign_zero() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", TextAlignment = TextAlignment.Left };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Gtk.Justification.Left, label.Justify);
		Assert.Equal(0f, label.Xalign);
	});

	[Fact]
	public void TextAlignment_Justify_sets_label_Justify_Fill() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", TextAlignment = TextAlignment.Justify };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Gtk.Justification.Fill, label.Justify);
	});

	[Fact]
	public void TextAlignment_changed_after_realize_updates_label() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi" };
		_ = tb.Widget; // realize
		tb.TextAlignment = TextAlignment.Right;
		Assert.Equal(Gtk.Justification.Right, ((Gtk.Label)tb.Widget).Justify);
		Assert.Equal(1f, ((Gtk.Label)tb.Widget).Xalign);
	});

	// ── TextTrimming → Label.Ellipsize ───────────────────────────────────────

	[Fact]
	public void TextTrimming_CharacterEllipsis_sets_label_Ellipsize_End() => Run(() =>
	{
		var tb = new TextBlock { Text = "long text", TextTrimming = TextTrimming.CharacterEllipsis };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Pango.EllipsizeMode.End, label.Ellipsize);
	});

	[Fact]
	public void TextTrimming_WordEllipsis_sets_label_Ellipsize_End() => Run(() =>
	{
		var tb = new TextBlock { Text = "long text", TextTrimming = TextTrimming.WordEllipsis };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Pango.EllipsizeMode.End, label.Ellipsize);
	});

	[Fact]
	public void TextTrimming_None_sets_label_Ellipsize_None() => Run(() =>
	{
		var tb = new TextBlock { Text = "text", TextTrimming = TextTrimming.None };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(Pango.EllipsizeMode.None, label.Ellipsize);
	});

	[Fact]
	public void TextTrimming_changed_after_realize_updates_label() => Run(() =>
	{
		var tb = new TextBlock { Text = "text" };
		_ = tb.Widget; // realize
		tb.TextTrimming = TextTrimming.CharacterEllipsis;
		Assert.Equal(Pango.EllipsizeMode.End, ((Gtk.Label)tb.Widget).Ellipsize);
	});

	// ── Padding → Label margins ───────────────────────────────────────────────

	[Fact]
	public void Padding_uniform_sets_all_label_margins() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", Padding = new Thickness(8) };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(8, label.MarginStart);
		Assert.Equal(8, label.MarginTop);
		Assert.Equal(8, label.MarginEnd);
		Assert.Equal(8, label.MarginBottom);
	});

	[Fact]
	public void Padding_non_uniform_sets_correct_margins() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", Padding = new Thickness(1, 2, 3, 4) };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(1, label.MarginStart);
		Assert.Equal(2, label.MarginTop);
		Assert.Equal(3, label.MarginEnd);
		Assert.Equal(4, label.MarginBottom);
	});

	[Fact]
	public void Padding_zero_sets_all_label_margins_to_zero() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi" };
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(0, label.MarginStart);
		Assert.Equal(0, label.MarginTop);
		Assert.Equal(0, label.MarginEnd);
		Assert.Equal(0, label.MarginBottom);
	});

	[Fact]
	public void Padding_changed_after_realize_updates_margins() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi" };
		_ = tb.Widget; // realize
		tb.Padding = new Thickness(5);
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal(5, label.MarginStart);
		Assert.Equal(5, label.MarginTop);
	});

	// ── LineHeight (store-only) ────────────────────────────────────────────────

	[Fact]
	public void LineHeight_set_before_realize_does_not_throw() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi", LineHeight = 24.0 };
		var ex = Record.Exception(() => _ = tb.Widget);
		Assert.Null(ex);
		Assert.Equal(24.0, tb.LineHeight);
	});

	[Fact]
	public void LineHeight_set_after_realize_does_not_throw() => Run(() =>
	{
		var tb = new TextBlock { Text = "hi" };
		_ = tb.Widget; // realize
		var ex = Record.Exception(() => tb.LineHeight = 20.0);
		Assert.Null(ex);
		Assert.Equal(20.0, tb.LineHeight);
	});
}
