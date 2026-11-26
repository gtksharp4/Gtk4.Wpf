using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AccessKeys;

public class AccessKeyParserTests
{
	[Theory]
	[InlineData("_Open", "Open", "O", 0)]
	[InlineData("__Open", "_Open", null, -1)]
	[InlineData("Open_", "Open_", null, -1)]
	[InlineData("A__B", "A_B", null, -1)]
	[InlineData("a_b_c", "ab_c", "b", 1)]
	[InlineData("_", "_", null, -1)]
	[InlineData("", "", null, -1)]
	[InlineData(null, "", null, -1)]
	[InlineData("___O", "_O", "O", 1)]
	[InlineData("Save _As", "Save As", "A", 5)]
	[InlineData("A__", "A_", null, -1)]
	[InlineData("a__b_c", "a_bc", "c", 3)]
	public void Parse_follows_WPF_rules(string? text, string display, string? key, int index)
	{
		var r = AccessKeyParser.Parse(text);
		Assert.Equal(display, r.DisplayText);
		Assert.Equal(key, r.AccessKey);
		Assert.Equal(index, r.KeyIndex);
	}

	[Fact]
	public void Parse_keeps_a_surrogate_pair_key_whole()
	{
		var r = AccessKeyParser.Parse("_\U0001F600x");
		Assert.Equal("\U0001F600x", r.DisplayText);
		Assert.Equal("\U0001F600", r.AccessKey);
		Assert.Equal(0, r.KeyIndex);
	}

	[Fact]
	public void Text_without_underscore_is_returned_by_reference()
	{
		var text = new string('x', 3) + "yz";
		Assert.Same(text, AccessKeyParser.Parse(text).DisplayText);
		Assert.Same(text, AccessKeyParser.RemoveAccessKeyMarker(text));
	}

	[Theory]
	[InlineData("_Open", "_Open")]
	[InlineData("__Open", "__Open")]
	[InlineData("Open_", "Open__")]
	[InlineData("A__B", "A__B")]
	[InlineData("a_b_c", "a_b__c")]
	[InlineData("x_", "x__")]
	[InlineData("", "")]
	[InlineData("plain", "plain")]
	[InlineData("___O", "__" + "_O")]
	public void ToGtkMnemonic_doubles_literals_and_marks_only_the_key(string wpf, string gtk)
		=> Assert.Equal(gtk, AccessKeyParser.ToGtkMnemonic(wpf));
}
