using System.Globalization;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class PointEqualityFormattingTests
{
	[Fact]
	public void ToString_uses_the_current_culture_list_separator()
	{
		var saved = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("de-DE");
		try
		{
			var p = new Point(1.5, 2.5);
			Assert.Equal("1,5;2,5", p.ToString());
			Assert.Equal(p.ToString(null), p.ToString());
		}
		finally { CultureInfo.CurrentCulture = saved; }
	}

	[Fact]
	public void Equals_treats_NaN_as_equal_but_operator_does_not()
	{
		var a = new Point(double.NaN, 0);
		var b = new Point(double.NaN, 0);
		Assert.True(a.Equals(b));
		Assert.True(a.Equals((object)b));
		Assert.True(Point.Equals(a, b));
		Assert.False(a == b);
		Assert.True(a != b);
		Assert.True(new Point(1, 2) == new Point(1, 2));
	}
}
