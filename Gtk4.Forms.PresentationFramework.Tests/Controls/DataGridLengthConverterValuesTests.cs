using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>The converter builds fresh lengths (NaN desired/display), as dotnet/wpf does.</summary>
public class DataGridLengthConverterValuesTests
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	[Theory]
	[InlineData("Auto", DataGridLengthUnitType.Auto)]
	[InlineData("SizeToCells", DataGridLengthUnitType.SizeToCells)]
	[InlineData("SizeToHeader", DataGridLengthUnitType.SizeToHeader)]
	public void Descriptive_units_have_NaN_desired_and_display_values(string text, DataGridLengthUnitType type)
	{
		var length = (DataGridLength)new DataGridLengthConverter().ConvertFrom(null, Inv, text)!;
		Assert.True(double.IsNaN(length.DesiredValue));
		Assert.True(double.IsNaN(length.DisplayValue));
		Assert.Equal(new DataGridLength(1.0, type), length);
	}

	[Fact]
	public void NaN_number_builds_a_fresh_Auto()
	{
		var length = (DataGridLength)new DataGridLengthConverter().ConvertFrom(null, Inv, double.NaN)!;
		Assert.True(double.IsNaN(length.DesiredValue));
		Assert.Equal(new DataGridLength(1.0, DataGridLengthUnitType.Auto), length);
	}
}
