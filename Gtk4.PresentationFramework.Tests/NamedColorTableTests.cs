using System.Reflection;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The 141 named colours and brushes match WPF's <c>Colors</c>/<c>Brushes</c> tables.</summary>
public partial class NamedColorTableTests
{
	static uint Pack(Color c) => ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;

	[Fact]
	public void Table_has_141_entries_and_Colors_has_no_others()
	{
		Assert.Equal(141, _table.Length);
		Assert.Equal(141, typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Length);
		Assert.Equal(141, typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static).Length);
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Colors_property_has_WPF_value(string name, uint argb)
	{
		var prop = typeof(Colors).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
		Assert.NotNull(prop);
		Assert.Equal(argb, Pack((Color)prop!.GetValue(null)!));
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Brushes_property_is_SolidColorBrush_of_the_colour(string name, uint argb)
	{
		var prop = typeof(Brushes).GetProperty(name, BindingFlags.Public | BindingFlags.Static);
		Assert.NotNull(prop);
		var brush = Assert.IsType<SolidColorBrush>(prop!.GetValue(null));
		Assert.Equal(argb, Pack(brush.Color));
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Names_parse_case_insensitively_as_Color_and_Brush(string name, uint argb)
	{
		foreach (var spelling in new[] { name, name.ToLowerInvariant(), name.ToUpperInvariant() })
		{
			var color = (Color)XamlValueConverter.ConvertByType(typeof(Color), spelling)!;
			Assert.Equal(argb, Pack(color));
			var brush = Assert.IsType<SolidColorBrush>(XamlValueConverter.ConvertByType(typeof(Brush), spelling));
			Assert.Equal(argb, Pack(brush.Color));
		}
	}

	[Theory]
	[InlineData("Honeydew")]
	[InlineData("WhiteSmoke")]
	public void Sample_failure_names_parse(string name) =>
		Assert.NotNull(XamlValueConverter.ConvertByType(typeof(Color), name));

	[Fact]
	public void Unknown_name_throws_FormatException() =>
		Assert.Throws<System.FormatException>(() => XamlValueConverter.ConvertByType(typeof(Color), "NotAColour"));

	[Theory]
	[MemberData(nameof(Names))]
	public void Values_agree_with_System_Drawing_known_colours(string name, uint argb)
	{
		var known = System.Drawing.Color.FromName(name);
		Assert.True(known.IsKnownColor, name);
		Assert.Equal(argb, (uint)known.ToArgb());
	}
}
