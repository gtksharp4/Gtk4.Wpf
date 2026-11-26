using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for Pango markup escaping, <see cref="InlineCollection"/> behaviour,
/// and Run FontWeight/FontStyle rendering.
/// No GTK loop required.
/// </summary>
public class TextBlockMarkupTests
{
	// ── Markup escaping ───────────────────────────────────────────────────────────

	[Fact]
	public void Escape_less_than_in_Run_text()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "a<b" });
		Assert.Equal("a&lt;b", tb.BuildMarkup());
	}

	[Fact]
	public void Escape_ampersand_in_Run_text()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "rock&roll" });
		Assert.Equal("rock&amp;roll", tb.BuildMarkup());
	}

	[Fact]
	public void Escape_greater_than_in_Run_text()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "5>3" });
		Assert.Equal("5&gt;3", tb.BuildMarkup());
	}

	// ── InlineCollection ─────────────────────────────────────────────────────────

	[Fact]
	public void InlineCollection_Add_increases_Count()
	{
		var col = new InlineCollection();
		col.Add(new Run { Text = "x" });
		Assert.Equal(1, col.Count);
	}

	[Fact]
	public void InlineCollection_Clear_removes_all()
	{
		var col = new InlineCollection();
		col.Add(new Run { Text = "x" });
		col.Clear();
		Assert.Equal(0, col.Count);
	}

	[Fact]
	public void InlineCollection_Changed_fires_on_Add()
	{
		var col = new InlineCollection();
		bool fired = false;
		col.Changed += () => { fired = true; };
		col.Add(new Run());
		Assert.True(fired);
	}

	// ── Run FontWeight / FontStyle DP on TextElement ──────────────────────────────

	[Fact]
	public void Run_FontWeight_Bold_emits_b_tags()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "w", FontWeight = FontWeights.Bold });
		Assert.Equal("<b>w</b>", tb.BuildMarkup());
	}

	[Fact]
	public void Run_FontStyle_Italic_emits_i_tags()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "i", FontStyle = FontStyles.Italic });
		Assert.Equal("<i>i</i>", tb.BuildMarkup());
	}

	// ── V6: non-Bold font weights ──────────────────────────────────────────────────

	[Fact]
	public void Run_FontWeight_SemiBold_emits_weight_span()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "sb", FontWeight = FontWeights.SemiBold });
		var markup = tb.BuildMarkup();
		Assert.Contains("<span weight=", markup);
		Assert.Contains("sb", markup);
	}

	[Fact]
	public void Run_FontWeight_Light_emits_weight_span()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "l", FontWeight = FontWeights.Light });
		var markup = tb.BuildMarkup();
		Assert.Contains("<span weight=", markup);
	}

	[Fact]
	public void Run_FontWeight_Normal_emits_no_weight_markup()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "n", FontWeight = FontWeights.Normal });
		// Normal weight (400) should not emit any weight span.
		Assert.DoesNotContain("<span weight=", tb.BuildMarkup());
		Assert.DoesNotContain("<b>", tb.BuildMarkup());
	}
}