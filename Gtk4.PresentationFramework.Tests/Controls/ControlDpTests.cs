using System;
using System.Windows;
using System.Windows.Media;
using Xunit;
using WpfBorder = System.Windows.Controls.Border;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfImage = System.Windows.Controls.Image;
using WpfLabel = System.Windows.Controls.Label;
using WpfRadioButton = System.Windows.Controls.RadioButton;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) tests: DP defaults, CLR wrapper round-trips, event firing.
/// </summary>
public class ControlDpTests
{
	// ── TextBox ──────────────────────────────────────────────────────────────

	[Fact]
	public void TextBox_Text_default_is_empty()
	{
		var tb = new WpfTextBox();
		Assert.Equal(string.Empty, tb.Text);
	}

	[Fact]
	public void TextBox_Text_roundtrip()
	{
		var tb = new WpfTextBox();
		tb.Text = "hello";
		Assert.Equal("hello", tb.Text);
	}

	[Fact]
	public void TextBox_TextChanged_fires_on_set()
	{
		var tb = new WpfTextBox();
		bool fired = false;
		tb.TextChanged += (s, e) => { fired = true; };
		tb.Text = "x";
		Assert.True(fired);
	}

	[Fact]
	public void TextBox_TextChanged_not_fired_when_value_unchanged()
	{
		var tb = new WpfTextBox { Text = "same" };
		int count = 0;
		tb.TextChanged += (s, e) => { count++; };
		tb.Text = "same";
		// DP skips change callback when value is identical.
		Assert.Equal(0, count);
	}

	// ── CheckBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void CheckBox_IsChecked_default_is_false()
	{
		var cb = new WpfCheckBox();
		Assert.Equal(false, cb.IsChecked);
	}

	[Fact]
	public void CheckBox_IsChecked_roundtrip()
	{
		var cb = new WpfCheckBox();
		cb.IsChecked = true;
		Assert.True(cb.IsChecked);
	}

	[Fact]
	public void CheckBox_Checked_event_fires()
	{
		var cb = new WpfCheckBox();
		bool fired = false;
		cb.Checked += (s, e) => { fired = true; };
		cb.IsChecked = true;
		Assert.True(fired);
	}

	[Fact]
	public void CheckBox_Unchecked_event_fires()
	{
		var cb = new WpfCheckBox { IsChecked = true };
		bool fired = false;
		cb.Unchecked += (s, e) => { fired = true; };
		cb.IsChecked = false;
		Assert.True(fired);
	}

	// ── RadioButton ──────────────────────────────────────────────────────────

	[Fact]
	public void RadioButton_IsChecked_default_is_false()
	{
		var rb = new WpfRadioButton();
		Assert.False(rb.IsChecked);
	}

	[Fact]
	public void RadioButton_IsChecked_roundtrip()
	{
		var rb = new WpfRadioButton();
		rb.IsChecked = true;
		Assert.True(rb.IsChecked);
	}

	[Fact]
	public void RadioButton_GroupName_default_is_empty()
	{
		var rb = new WpfRadioButton();
		Assert.Equal(string.Empty, rb.GroupName);
	}

	[Fact]
	public void RadioButton_GroupName_roundtrip()
	{
		var rb = new WpfRadioButton();
		rb.GroupName = "group1";
		Assert.Equal("group1", rb.GroupName);
	}

	// ── ComboBox ─────────────────────────────────────────────────────────────

	[Fact]
	public void ComboBox_SelectedIndex_default_is_minus_one()
	{
		var cb = new WpfComboBox();
		Assert.Equal(-1, cb.SelectedIndex);
	}

	[Fact]
	public void ComboBox_SelectedItem_default_is_null()
	{
		var cb = new WpfComboBox();
		Assert.Null(cb.SelectedItem);
	}

	[Fact]
	public void ComboBox_ItemsSource_roundtrip()
	{
		var cb = new WpfComboBox();
		var items = new[] { "a", "b", "c" };
		cb.ItemsSource = items;
		Assert.Same(items, cb.ItemsSource);
	}

	[Fact]
	public void ComboBox_SelectionChanged_fires_when_SelectedItem_set()
	{
		var cb = new WpfComboBox();
		cb.ItemsSource = new[] { "x", "y" };

		bool fired = false;
		cb.SelectionChanged += (s, e) => { fired = true; };
		cb.SelectedItem = "y";
		Assert.True(fired);
	}

	// ── Image ────────────────────────────────────────────────────────────────

	[Fact]
	public void Image_Source_default_is_null()
	{
		var img = new WpfImage();
		Assert.Null(img.Source);
	}

	[Fact]
	public void Image_Source_roundtrip()
	{
		var img = new WpfImage();
		var src = ImageSource.FromFile("test.png");
		img.Source = src;
		Assert.Same(src, img.Source);
	}

	[Fact]
	public void ImageSource_FromFile_stores_path()
	{
		var src = ImageSource.FromFile("/tmp/x.jpg");
		Assert.Equal("/tmp/x.jpg", src.FilePath);
	}

	// ── Border ───────────────────────────────────────────────────────────────

	[Fact]
	public void Border_Child_default_is_null()
	{
		var b = new WpfBorder();
		Assert.Null(b.Child);
	}

	[Fact]
	public void Border_Child_roundtrip()
	{
		var b = new WpfBorder();
		var tb = new WpfTextBlock { Text = "hi" };
		b.Child = tb;
		Assert.Same(tb, b.Child);
	}

	[Fact]
	public void Border_BorderThickness_default_is_zero()
	{
		var b = new WpfBorder();
		Assert.Equal(new Thickness(0), b.BorderThickness);
	}

	[Fact]
	public void Border_Padding_roundtrip()
	{
		var b = new WpfBorder();
		b.Padding = new Thickness(4);
		Assert.Equal(new Thickness(4), b.Padding);
	}

	// ── Label ────────────────────────────────────────────────────────────────

	[Fact]
	public void Label_Content_default_is_null()
	{
		var lbl = new WpfLabel();
		Assert.Null(lbl.Content);
	}

	[Fact]
	public void Label_Content_roundtrip()
	{
		var lbl = new WpfLabel();
		lbl.Content = "Press me";
		Assert.Equal("Press me", lbl.Content);
	}
}