using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

public class BooleanToVisibilityConverterTests
{
	static readonly BooleanToVisibilityConverter C = new BooleanToVisibilityConverter();

	static object? Convert(object? v) => C.Convert(v, typeof(Visibility), null, CultureInfo.InvariantCulture);

	static object? Back(object? v) => C.ConvertBack(v, typeof(bool), null, CultureInfo.InvariantCulture);

	[Fact]
	public void True_converts_to_Visible() => Assert.Equal(Visibility.Visible, Convert(true));

	[Fact]
	public void False_converts_to_Collapsed() => Assert.Equal(Visibility.Collapsed, Convert(false));

	[Fact]
	public void Null_and_non_bool_convert_to_Collapsed()
	{
		Assert.Equal(Visibility.Collapsed, Convert(null));
		Assert.Equal(Visibility.Collapsed, Convert("true"));
	}

	[Fact]
	public void Nullable_bool_true_is_Visible_and_null_is_Collapsed()
	{
		bool? t = true;
		bool? n = null;
		Assert.Equal(Visibility.Visible, Convert(t));
		Assert.Equal(Visibility.Collapsed, Convert(n));
	}

	[Fact]
	public void ConvertBack_maps_Visible_to_true_and_everything_else_to_false()
	{
		Assert.Equal(true, Back(Visibility.Visible));
		Assert.Equal(false, Back(Visibility.Hidden));
		Assert.Equal(false, Back(Visibility.Collapsed));
		Assert.Equal(false, Back(null));
	}
}
