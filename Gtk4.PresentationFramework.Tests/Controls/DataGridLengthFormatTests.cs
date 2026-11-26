using System;
using System.Globalization;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class DataGridLengthFormatTests
{
	[Fact]
	public void ToString_is_culture_invariant()
	{
		var saved = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("de-DE");
			Assert.Equal("1.5", new DataGridLength(1.5).ToString());
			Assert.Equal("2.5*", new DataGridLength(2.5, DataGridLengthUnitType.Star).ToString());
		}
		finally { CultureInfo.CurrentCulture = saved; }
	}

	[Fact]
	public void Statics_carry_zero_desired_and_display_values()
	{
		Assert.Equal(0d, DataGridLength.Auto.DesiredValue);
		Assert.Equal(0d, DataGridLength.SizeToCells.DisplayValue);
		Assert.Equal(0d, DataGridLength.SizeToHeader.DesiredValue);
	}

	[Theory]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Converter_rejects_infinite_double_with_NotSupportedException(double value) =>
		Assert.Throws<NotSupportedException>(() => new DataGridLengthConverter().ConvertFrom(null, null, value));

	[Fact]
	public void Converter_rejects_infinite_float_with_NotSupportedException() =>
		Assert.Throws<NotSupportedException>(
			() => new DataGridLengthConverter().ConvertFrom(null, null, float.NegativeInfinity));
}
