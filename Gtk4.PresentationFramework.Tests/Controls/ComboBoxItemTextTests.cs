using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Person
{
	public string Name { get; set; } = string.Empty;
	public override string ToString() => "P:" + Name;
}

public class ComboBoxItemTextTests
{
	[Fact]
	public void GetText_null_is_empty() => Assert.Equal(string.Empty, ComboBoxItemText.GetText(null, null));

	[Fact]
	public void GetText_string_item() => Assert.Equal("abc", ComboBoxItemText.GetText("abc", null));

	[Fact]
	public void GetText_ComboBoxItem_string_content() =>
		Assert.Equal("hi", ComboBoxItemText.GetText(new ComboBoxItem { Content = "hi" }, null));

	[Fact]
	public void GetText_ComboBoxItem_int_content() =>
		Assert.Equal("42", ComboBoxItemText.GetText(new ComboBoxItem { Content = 42 }, null));

	[Fact]
	public void GetText_ComboBoxItem_null_content_is_empty() =>
		Assert.Equal(string.Empty, ComboBoxItemText.GetText(new ComboBoxItem(), null));

	[Fact]
	public void GetText_ComboBoxItem_UIElement_content_uses_ToString()
	{
		var tb = new TextBlock();
		Assert.Equal(tb.ToString(), ComboBoxItemText.GetText(new ComboBoxItem { Content = tb }, null));
	}

	[Fact]
	public void GetText_DisplayMemberPath_wins() =>
		Assert.Equal("Bob", ComboBoxItemText.GetText(new Person { Name = "Bob" }, "Name"));

	[Fact]
	public void GetText_unknown_path_falls_back_to_ToString() =>
		Assert.Equal("P:Bob", ComboBoxItemText.GetText(new Person { Name = "Bob" }, "Nope"));

	[Fact]
	public void GetText_plain_object_uses_ToString() =>
		Assert.Equal("P:X", ComboBoxItemText.GetText(new Person { Name = "X" }, null));

	[Fact]
	public void IndexOfExact_case_insensitive_by_default() =>
		Assert.Equal(1, ComboBoxItemText.IndexOfExact(new[] { "a", "B" }, "b", false));

	[Fact]
	public void IndexOfExact_case_sensitive_flag() =>
		Assert.Equal(-1, ComboBoxItemText.IndexOfExact(new[] { "a", "B" }, "b", true));

	[Fact]
	public void IndexOfExact_null_text_is_no_match() =>
		Assert.Equal(-1, ComboBoxItemText.IndexOfExact(new[] { "a" }, null, false));

	[Fact]
	public void IndexOfExact_is_exact_not_prefix() =>
		Assert.Equal(-1, ComboBoxItemText.IndexOfExact(new[] { "apple" }, "app", false));

	[Fact]
	public void IndexOfExact_first_match_wins() =>
		Assert.Equal(0, ComboBoxItemText.IndexOfExact(new[] { "a", "A" }, "a", false));
}
