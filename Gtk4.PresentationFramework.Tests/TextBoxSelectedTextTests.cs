using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Covers <see cref="TextBox.SelectedText"/> set (replace-selection) and
/// <see cref="TextBox.CaretIndex"/> clamping — the gaps left by the main suite.
/// </summary>
public class TextBoxSelectedTextTests
{
	[Fact]
	public void SelectedText_set_replaces_selection_and_collapses_caret()
	{
		var tb = new TextBox { Text = "abcdef" };
		tb.Select(2, 3); // "cde"

		tb.SelectedText = "XY";

		// "ab" + "XY" + "f"; caret collapses to end of inserted text (WPF behaviour).
		Assert.Equal("abXYf", tb.Text);
		Assert.Equal(4, tb.SelectionStart);
		Assert.Equal(0, tb.SelectionLength);
	}

	[Fact]
	public void SelectedText_get_returns_selected_substring()
	{
		var tb = new TextBox { Text = "hello world" };
		tb.Select(6, 5);
		Assert.Equal("world", tb.SelectedText);
	}

	[Fact]
	public void CaretIndex_clamped_to_text_length()
	{
		var tb = new TextBox { Text = "abc" };
		tb.CaretIndex = 99;
		Assert.Equal(3, tb.CaretIndex);
	}
}
