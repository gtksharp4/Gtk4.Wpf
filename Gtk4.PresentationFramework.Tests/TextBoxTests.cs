using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for the new <see cref="TextBox"/> completeness features:
/// selection model, MaxLength, CaretIndex, TextChanged.
/// These exercise only the managed model — no backing widget is created.
/// </summary>
public class TextBoxPureTests
{
	// ── SelectionStart / Length ───────────────────────────────────────────────

	[Fact]
	public void SelectionStart_roundtrip()
	{
		var tb = new TextBox { Text = "hello" };
		tb.SelectionStart = 2;
		Assert.Equal(2, tb.SelectionStart);
	}

	[Fact]
	public void SelectionLength_roundtrip()
	{
		var tb = new TextBox { Text = "hello" };
		tb.SelectionStart  = 1;
		tb.SelectionLength = 3;
		Assert.Equal(3, tb.SelectionLength);
	}

	[Fact]
	public void SelectedText_returns_correct_slice()
	{
		var tb = new TextBox { Text = "hello world" };
		tb.SelectionStart  = 6;
		tb.SelectionLength = 5;
		Assert.Equal("world", tb.SelectedText);
	}

	[Fact]
	public void SelectedText_set_replaces_selection()
	{
		var tb = new TextBox { Text = "hello world" };
		tb.SelectionStart  = 6;
		tb.SelectionLength = 5;
		tb.SelectedText    = "there";
		Assert.Equal("hello there", tb.Text);
	}

	[Fact]
	public void SelectAll_selects_whole_text()
	{
		var tb = new TextBox { Text = "abc" };
		tb.SelectAll();
		Assert.Equal(0, tb.SelectionStart);
		Assert.Equal(3, tb.SelectionLength);
	}

	[Fact]
	public void Select_method_sets_start_and_length()
	{
		var tb = new TextBox { Text = "abcdef" };
		tb.Select(2, 3);
		Assert.Equal(2, tb.SelectionStart);
		Assert.Equal(3, tb.SelectionLength);
		Assert.Equal("cde", tb.SelectedText);
	}

	[Fact]
	public void SelectionChanged_fires_on_SelectionStart_change()
	{
		var tb = new TextBox { Text = "hello" };
		bool fired = false;
		tb.SelectionChanged += (s, e) => fired = true;
		tb.SelectionStart = 2;
		Assert.True(fired);
	}

	// ── CaretIndex ────────────────────────────────────────────────────────────

	[Fact]
	public void CaretIndex_roundtrip()
	{
		var tb = new TextBox { Text = "hello" };
		tb.CaretIndex = 3;
		Assert.Equal(3, tb.CaretIndex);
	}

	[Fact]
	public void CaretIndex_clears_selection()
	{
		var tb = new TextBox { Text = "hello" };
		tb.Select(1, 3);
		tb.CaretIndex = 2;
		Assert.Equal(0, tb.SelectionLength);
	}

	// ── MaxLength ─────────────────────────────────────────────────────────────

	[Fact]
	public void MaxLength_default_is_zero()
	{
		var tb = new TextBox();
		Assert.Equal(0, tb.MaxLength);
	}

	[Fact]
	public void MaxLength_roundtrip()
	{
		var tb = new TextBox { MaxLength = 10 };
		Assert.Equal(10, tb.MaxLength);
	}

	// ── TextChanged ──────────────────────────────────────────────────────────

	[Fact]
	public void TextChanged_fires_on_Text_set()
	{
		var tb = new TextBox();
		bool fired = false;
		tb.TextChanged += (s, e) => fired = true;
		tb.Text = "hello";
		Assert.True(fired);
	}

	[Fact]
	public void TextChanged_carries_correct_source()
	{
		var tb = new TextBox();
		TextBox? src = null;
		tb.TextChanged += (s, e) => src = e.Source;
		tb.Text = "hi";
		Assert.Same(tb, src);
	}

	// ── Clear ────────────────────────────────────────────────────────────────

	[Fact]
	public void Clear_empties_text()
	{
		var tb = new TextBox { Text = "some text" };
		tb.Clear();
		Assert.Equal(string.Empty, tb.Text);
	}

	// ── AcceptsReturn / IsReadOnly ───────────────────────────────────────────

	[Fact]
	public void AcceptsReturn_default_is_false()
	{
		var tb = new TextBox();
		Assert.False(tb.AcceptsReturn);
	}

	[Fact]
	public void IsReadOnly_default_is_false()
	{
		var tb = new TextBox();
		Assert.False(tb.IsReadOnly);
	}

	[Fact]
	public void IsReadOnly_roundtrip()
	{
		var tb = new TextBox { IsReadOnly = true };
		Assert.True(tb.IsReadOnly);
	}

	// ── AcceptsTab ───────────────────────────────────────────────────────────

	[Fact]
	public void AcceptsTab_default_is_false()
	{
		var tb = new TextBox();
		Assert.False(tb.AcceptsTab);
	}

	[Fact]
	public void AcceptsTab_roundtrip()
	{
		var tb = new TextBox { AcceptsTab = true };
		Assert.True(tb.AcceptsTab);
	}

	// ── Selection clamping on Text change ────────────────────────────────────

	[Fact]
	public void SelectionStart_clamped_when_Text_shrinks()
	{
		var tb = new TextBox { Text = "hello" };
		tb.SelectionStart = 4;
		tb.Text = "hi";
		Assert.True(tb.SelectionStart <= tb.Text.Length);
	}

	[Fact]
	public void SelectionLength_clamped_when_Text_shrinks()
	{
		var tb = new TextBox { Text = "hello world" };
		tb.SelectionStart  = 3;
		tb.SelectionLength = 7;
		tb.Text = "hi";
		// start+length must not exceed new Text.Length
		Assert.True(tb.SelectionStart + tb.SelectionLength <= tb.Text.Length);
	}

	[Fact]
	public void Clamping_before_realize_does_not_throw()
	{
		var tb = new TextBox { Text = "abcde" };
		tb.Select(2, 3);
		// Shrink text; must not throw and selection must stay in bounds
		var ex = Record.Exception(() => { tb.Text = "ab"; });
		Assert.Null(ex);
		Assert.True(tb.SelectionStart + tb.SelectionLength <= tb.Text.Length);
	}
}