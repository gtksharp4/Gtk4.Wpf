using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>Headless DP, Text and selection-box tests.</summary>
public class ComboBoxEditableTests
{
	[Fact]
	public void Defaults()
	{
		var cb = new ComboBox();
		Assert.False(cb.IsDropDownOpen);
		Assert.False(cb.IsEditable);
		Assert.False(cb.IsReadOnly);
		Assert.False(cb.StaysOpenOnEdit);
		Assert.Equal(string.Empty, cb.Text);
		Assert.Equal(string.Empty, cb.SelectionBoxItem);
		Assert.False(cb.IsSelectionBoxHighlighted);
		Assert.True(cb.MaxDropDownHeight > 0);
	}

	[Fact]
	public void IsDropDownOpen_is_two_way_by_default() =>
		Assert.True(((FrameworkPropertyMetadata)ComboBox.IsDropDownOpenProperty.GetMetadata(typeof(ComboBox)))
			.BindsTwoWayByDefault);

	[Fact]
	public void Text_metadata()
	{
		var m = (FrameworkPropertyMetadata)ComboBox.TextProperty.GetMetadata(typeof(ComboBox));
		Assert.True(m.BindsTwoWayByDefault);
		Assert.True(m.Journal);
		Assert.Equal(UpdateSourceTrigger.LostFocus, m.DefaultUpdateSourceTrigger);
	}

	[Fact]
	public void ReadOnly_keys_reject_SetValue()
	{
		var cb = new ComboBox();
		foreach (var dp in new[] { ComboBox.SelectionBoxItemProperty, ComboBox.SelectionBoxItemTemplateProperty,
			ComboBox.SelectionBoxItemStringFormatProperty, ComboBox.IsSelectionBoxHighlightedProperty })
		{
			Assert.True(dp.IsReadOnly);
			Assert.Throws<System.InvalidOperationException>(() => cb.SetValue(dp, null));
		}
	}

	[Fact]
	public void Text_follows_selection()
	{
		var cb = new ComboBox { ItemsSource = new[] { "one", "two" } };
		cb.SelectedItem = "two";
		Assert.Equal("two", cb.Text);
		cb.SelectedItem = null;
		Assert.Equal(string.Empty, cb.Text);
	}

	[Fact]
	public void Text_from_ComboBoxItem_content()
	{
		var cb = new ComboBox();
		var a = new ComboBoxItem { Content = "Alpha" };
		cb.Items.Add(a);
		cb.SelectedItem = a;
		Assert.Equal("Alpha", cb.Text);
		Assert.Equal("Alpha", cb.SelectionBoxItem);
	}

	[Fact]
	public void Setting_Text_selects_match_case_insensitive()
	{
		var cb = new ComboBox { ItemsSource = new[] { "one", "Two" } };
		cb.Text = "two";
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Equal("Two", cb.SelectedItem);
	}

	[Fact]
	public void Setting_Text_case_sensitive_no_match_clears()
	{
		var cb = new ComboBox { ItemsSource = new[] { "one", "Two" }, IsTextSearchCaseSensitive = true };
		cb.SelectedItem = "one";
		cb.Text = "two";
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
	}

	[Fact]
	public void Editable_keeps_typed_text_when_no_match()
	{
		var cb = new ComboBox { ItemsSource = new[] { "one" }, IsEditable = true };
		cb.Text = "zzz";
		Assert.Equal("zzz", cb.Text);
		Assert.Equal(-1, cb.SelectedIndex);
	}

	[Fact]
	public void Editable_typing_keeps_case_but_selects_match()
	{
		var cb = new ComboBox { ItemsSource = new[] { "Two" }, IsEditable = true };
		cb.Text = "two";
		Assert.Equal("two", cb.Text);
		Assert.Equal("Two", cb.SelectedItem);
	}

	[Fact]
	public void Text_uses_DisplayMemberPath()
	{
		var cb = new ComboBox { DisplayMemberPath = "Length", ItemsSource = new[] { "abc" } };
		cb.SelectedItem = "abc";
		Assert.Equal("3", cb.Text);
	}

	[Fact]
	public void Text_matches_inline_items()
	{
		var cb = new ComboBox();
		cb.Items.Add(new ComboBoxItem { Content = "A" });
		var b = new ComboBoxItem { Content = "B" };
		cb.Items.Add(b);
		cb.Text = "B";
		Assert.Same(b, cb.SelectedItem);
	}

	[Fact]
	public void ComboBoxItem_IsHighlighted_is_read_only_false()
	{
		Assert.True(ComboBoxItem.IsHighlightedProperty.IsReadOnly);
		Assert.False(new ComboBoxItem().IsHighlighted);
	}
}
