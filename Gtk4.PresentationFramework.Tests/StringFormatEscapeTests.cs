using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class StringFormatEscapeTests : WpfGtkTestBase
{
	sealed class Item
	{
		public string Description { get; set; } = "Tea";
		public int Price { get; set; } = 3;
	}

	[Fact]
	public void A_leading_XAML_escape_is_dropped_from_a_MultiBinding_StringFormat() => Run(() =>
	{
		var multi = new MultiBinding { StringFormat = "{}{0} -- only {1}!", ConverterCulture = CultureInfo.InvariantCulture };
		multi.Bindings.Add(new Binding("Description"));
		multi.Bindings.Add(new Binding("Price"));
		var text = new TextBlock { DataContext = new Item() };
		text.SetBinding(TextBlock.TextProperty, multi);
		Assert.Equal("Tea -- only 3!", text.Text);
	});

	// The generated code keeps the quotes of StringFormat='{}...' (WPF's XAML parser removes both).
	[Theory]
	[InlineData("'{}{0} Page'", "Tea Page")]
	[InlineData("'{}Copy {0} to clipboard'", "Copy Tea to clipboard")]
	[InlineData("'{0}'", "'Tea'")]
	public void A_quoted_XAML_escape_StringFormat_is_unwrapped_and_a_plain_quoted_one_is_kept(string format, string expected) => Run(() =>
	{
		var text = new TextBlock { DataContext = new Item() };
		text.SetBinding(TextBlock.TextProperty, new Binding("Description") { StringFormat = format });
		Assert.Equal(expected, text.Text);
	});

	[Fact]
	public void A_quoted_XAML_escape_is_unwrapped_in_a_MultiBinding_StringFormat() => Run(() =>
	{
		var multi = new MultiBinding { StringFormat = "'{}{0}. {1}'" };
		multi.Bindings.Add(new Binding("Description"));
		multi.Bindings.Add(new Binding("Price"));
		var text = new TextBlock { DataContext = new Item() };
		text.SetBinding(TextBlock.TextProperty, multi);
		Assert.Equal("Tea. 3", text.Text);
	});

	[Fact]
	public void A_MultiBinding_StringFormat_is_not_applied_to_the_fallback_before_the_DataContext_arrives() => Run(() =>
	{
		var multi = new MultiBinding { StringFormat = "{0} -- {1}" };
		multi.Bindings.Add(new Binding("Description"));
		multi.Bindings.Add(new Binding("Price"));
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, multi);
		text.DataContext = new Item();
		Assert.Equal("Tea -- 3", text.Text);
	});

	[Fact]
	public void A_templated_ListBox_item_shows_the_formatted_text() => Run(() =>
	{
		var list = new ListBox
		{
			ItemTemplate = new DataTemplate
			{
				ItemFactory = item =>
				{
					var multi = new MultiBinding { StringFormat = "{}{0} ({1})" };
					multi.Bindings.Add(new Binding("Description"));
					multi.Bindings.Add(new Binding("Price"));
					var text = new TextBlock { DataContext = item };
					text.SetBinding(TextBlock.TextProperty, multi);
					return text;
				},
			},
		};
		list.Items.Add(new Item());
		_ = list.Widget;
		var container = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0)!;
		var presenter = Assert.IsType<ContentPresenter>(System.Windows.Media.VisualTreeHelper.GetChild(container, 0));
		var text = Assert.IsType<TextBlock>(System.Windows.Media.VisualTreeHelper.GetChild(presenter, 0));
		Assert.Equal("Tea (3)", text.Text);
	});
}
