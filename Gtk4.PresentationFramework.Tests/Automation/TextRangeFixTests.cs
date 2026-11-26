using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>Text ranges that outlive an edit, and the Move contract for degenerate and zero-count moves.</summary>
public class TextRangeFixTests : WpfGtkTestBase
{
	static ITextProvider Text(string text, out TextBox box)
	{
		box = new TextBox { Text = text };
		return (ITextProvider)UIElementAutomationPeer.CreatePeerForElement(box)!.GetPattern(PatternInterface.Text)!;
	}

	[Fact]
	public void A_range_is_clamped_when_the_text_shrinks_under_it() => Run(() =>
	{
		var text = Text("hello world", out var box);
		var range = text.DocumentRange;
		box.Text = "hi";
		Assert.Equal("hi", range.GetText(-1));
		Assert.NotNull(range.FindText("i", false, false));
		Assert.Equal(0, range.Move(TextUnit.Character, 0));
		range.Select();
		Assert.Equal("hi", box.SelectedText);
		var tail = text.DocumentRange;
		box.Text = "hello world";
		tail.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, 9);
		box.Text = "abc";
		Assert.Equal("", tail.GetText(-1));
		tail.Move(TextUnit.Character, 1);
		tail.Select();
		Assert.Equal(0, box.SelectionLength);
	});

	[Fact]
	public void Move_keeps_a_degenerate_range_degenerate_and_zero_count_changes_nothing() => Run(() =>
	{
		var text = Text("hello world", out var box);
		box.Select(3, 0);
		var caret = text.GetSelection()[0];
		Assert.Equal(1, caret.Move(TextUnit.Word, 1));
		Assert.Equal("", caret.GetText(-1));
		Assert.Equal(0, caret.CompareEndpoints(TextPatternRangeEndpoint.Start, caret, TextPatternRangeEndpoint.End));
		caret.Select();
		Assert.Equal(6, box.SelectionStart);
		Assert.Equal(0, box.SelectionLength);

		var range = text.DocumentRange;
		range.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, 2);
		Assert.Equal(0, range.Move(TextUnit.Word, 0));
		Assert.Equal("llo world", range.GetText(-1));
	});
}
