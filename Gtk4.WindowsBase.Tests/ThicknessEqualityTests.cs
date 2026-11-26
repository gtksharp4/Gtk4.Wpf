using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class ThicknessEqualityTests
{
	[Fact]
	public void NaN_components_are_equal_to_NaN_like_WPF()
	{
		var a = new Thickness(double.NaN, 1, double.NaN, 2);
		var b = new Thickness(double.NaN, 1, double.NaN, 2);
		Assert.True(a.Equals(b));
		Assert.True(a.Equals((object)b));
		Assert.True(a == b);
		Assert.False(a != b);
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
	}

	[Fact]
	public void NaN_is_not_equal_to_a_number_and_other_sides_still_compare()
	{
		Assert.False(new Thickness(double.NaN, 0, 0, 0) == new Thickness(0, 0, 0, 0));
		Assert.True(new Thickness(double.NaN, 0, 0, 0) != new Thickness(double.NaN, 0, 0, 1));
		Assert.True(new Thickness(1, 2, 3, 4) == new Thickness(1, 2, 3, 4));
		Assert.False(new Thickness(1, 2, 3, 4).Equals(null));
	}
}
