using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>The text provider and its ranges, called directly on the provider interfaces.</summary>
public class TextProviderTests : WpfGtkTestBase
{
	static ITextProvider Text(string text, out TextBox box)
	{
		box = new TextBox { Text = text };
		return (ITextProvider)UIElementAutomationPeer.CreatePeerForElement(box)!.GetPattern(PatternInterface.Text)!;
	}

	[Fact]
	public void Document_range_and_selection_follow_the_control() => Run(() =>
	{
		var text = Text("hello world", out var box);
		Assert.Equal(SupportedTextSelection.Single, text.SupportedTextSelection);
		Assert.Equal("hello world", text.DocumentRange.GetText(-1));
		var caret = Assert.Single(text.GetSelection());
		Assert.Equal("", caret.GetText(-1));
		box.Select(6, 5);
		Assert.Equal("world", text.GetSelection()[0].GetText(-1));
		Assert.Single(text.GetVisibleRanges());
		Assert.Throws<ArgumentNullException>(() => text.RangeFromChild(null!));
	});

	[Fact]
	public void GetText_validates_maxLength() => Run(() =>
	{
		var range = Text("abc", out _).DocumentRange;
		Assert.Equal("ab", range.GetText(2));
		Assert.Equal("", range.GetText(0));
		Assert.Equal("abc", range.GetText(10));
		Assert.Throws<ArgumentOutOfRangeException>(() => range.GetText(-2));
	});

	[Fact]
	public void Move_by_character_word_and_line_clamps_and_counts() => Run(() =>
	{
		var range = Text("one two\nthree", out _).DocumentRange;
		range.ExpandToEnclosingUnit(TextUnit.Character);
		Assert.Equal("o", range.GetText(-1));
		Assert.Equal(3, range.Move(TextUnit.Character, 3));
		Assert.Equal(" ", range.GetText(-1));
		Assert.Equal(-3, range.Move(TextUnit.Character, -10));
		Assert.Equal("o", range.GetText(-1));

		var word = Text("one two\nthree", out _).DocumentRange;
		word.ExpandToEnclosingUnit(TextUnit.Word);
		Assert.Equal("one ", word.GetText(-1));
		Assert.Equal(2, word.Move(TextUnit.Word, 5));
		Assert.Equal("three", word.GetText(-1));
		Assert.Equal(0, word.Move(TextUnit.Word, 1));

		var doc = Text("one two\nthree", out _).DocumentRange;
		doc.ExpandToEnclosingUnit(TextUnit.Document);
		Assert.Equal(0, doc.Move(TextUnit.Document, 1));
		Assert.Equal("one two\nthree", doc.GetText(-1));
	});

	[Fact]
	public void Endpoints_move_by_unit_and_range_and_never_cross() => Run(() =>
	{
		var text = Text("one two three", out _);
		var range = text.DocumentRange;
		Assert.Equal(-2, range.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Word, -2));
		Assert.Equal("one ", range.GetText(-1));
		Assert.Equal(1, range.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Word, 1));
		Assert.Equal("", range.GetText(-1));
		range.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, -3);
		Assert.Equal("", range.GetText(-1));
		Assert.Equal(0, range.CompareEndpoints(TextPatternRangeEndpoint.Start, range, TextPatternRangeEndpoint.End));

		var all = text.DocumentRange;
		range.MoveEndpointByRange(TextPatternRangeEndpoint.End, all, TextPatternRangeEndpoint.End);
		Assert.Equal("ne two three", range.GetText(-1));
		Assert.True(all.CompareEndpoints(TextPatternRangeEndpoint.End, range, TextPatternRangeEndpoint.End) == 0);
		Assert.True(all.Compare(all.Clone()));
		Assert.False(all.Compare(range));
	});

	[Fact]
	public void FindText_searches_within_the_range_in_both_directions() => Run(() =>
	{
		var range = Text("abc abc", out _).DocumentRange;
		Assert.Equal(0, range.CompareEndpoints(TextPatternRangeEndpoint.Start,
			range.FindText("abc", false, false), TextPatternRangeEndpoint.Start));
		Assert.True(range.CompareEndpoints(TextPatternRangeEndpoint.Start,
			range.FindText("abc", true, false), TextPatternRangeEndpoint.Start) < 0);
		Assert.Null(range.FindText("ABC", false, false));
		Assert.NotNull(range.FindText("ABC", false, true));
		Assert.Throws<ArgumentNullException>(() => range.FindText(null!, false, false));
		Assert.Throws<ArgumentException>(() => range.FindText("", false, false));
	});

	[Fact]
	public void Ranges_of_another_provider_are_rejected_and_selection_is_single() => Run(() =>
	{
		var a = Text("abc", out _).DocumentRange;
		var b = Text("abc", out _).DocumentRange;
		Assert.Throws<ArgumentException>(() => a.Compare(b));
		Assert.Throws<InvalidOperationException>(a.AddToSelection);
		Assert.Throws<InvalidOperationException>(a.RemoveFromSelection);
		Assert.Empty(a.GetChildren());
		Assert.Same(AutomationElement.NotSupported, a.GetAttributeValue(40001));
		Assert.Null(a.FindAttribute(40001, 1, false));
	});

	[Fact]
	public void RichTextBox_exposes_the_text_pattern_over_its_buffer() => Run(() =>
	{
		var rich = new RichTextBox();
		rich.AppendText("first");
		rich.AppendText("second");
		var peer = UIElementAutomationPeer.CreatePeerForElement(rich)!;
		Assert.Equal(AutomationControlType.Document, peer.GetAutomationControlType());
		var text = (ITextProvider)peer.GetPattern(PatternInterface.Text)!;
		Assert.Equal("first\nsecond", text.DocumentRange.GetText(-1));
		var found = text.DocumentRange.FindText("second", false, false);
		found.Select();
		Assert.Equal("second", text.GetSelection()[0].GetText(-1));
		var line = text.DocumentRange;
		line.ExpandToEnclosingUnit(TextUnit.Line);
		Assert.Equal("first\n", line.GetText(-1));
	});
}
