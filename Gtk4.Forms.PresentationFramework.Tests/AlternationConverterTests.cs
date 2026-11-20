using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class AlternationConverterTests
{
	static AlternationConverter Two()
	{
		var c = new AlternationConverter();
		c.Values.Add("a");
		c.Values.Add("b");
		return c;
	}

	[Theory]
	[InlineData(0, "a")]
	[InlineData(1, "b")]
	[InlineData(2, "a")]
	[InlineData(5, "b")]
	public void Convert_picks_the_value_at_index_modulo_count(int index, string expected)
		=> Assert.Equal(expected, Two().Convert(index, typeof(object), null, CultureInfo.InvariantCulture));

	[Fact]
	public void Convert_of_a_non_int_or_empty_list_is_UnsetValue()
	{
		Assert.Same(DependencyProperty.UnsetValue,
			Two().Convert("x", typeof(object), null, CultureInfo.InvariantCulture));
		Assert.Same(DependencyProperty.UnsetValue,
			new AlternationConverter().Convert(1, typeof(object), null, CultureInfo.InvariantCulture));
	}

	[Fact]
	public void ConvertBack_returns_the_index_or_minus_one()
	{
		Assert.Equal(1, Two().ConvertBack("b", typeof(int), null, CultureInfo.InvariantCulture));
		Assert.Equal(-1, Two().ConvertBack("z", typeof(int), null, CultureInfo.InvariantCulture));
	}

	[Fact]
	public void PropertyGroupDescription_PropertyName_is_settable()
		=> Assert.Equal("State", new PropertyGroupDescription { PropertyName = "State" }.PropertyName);
}

public class CollectionViewSourceProviderTests
{
	[Fact]
	public void Source_may_be_a_data_provider_whose_Data_is_viewed()
	{
		var provider = new ObjectDataProvider { ObjectInstance = new System.Collections.Generic.List<string> { "a", "b" } };
		var source = new CollectionViewSource { Source = provider };
		Assert.NotNull(source.View);
		Assert.Same(provider, source.Source);
	}

	[Fact]
	public void A_Source_that_is_not_enumerable_has_no_View()
		=> Assert.Null(new CollectionViewSource { Source = 5 }.View);
}
