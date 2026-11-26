using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Font enumeration and metrics through the real Pango font map.</summary>
public class FontModelGtkTests : WpfGtkTestBase
{
	static readonly XmlLanguage En = XmlLanguage.GetLanguage("en-US");

	[Fact]
	public void SystemFontFamilies_come_from_the_pango_font_map() => Run(() =>
	{
		var families = Fonts.SystemFontFamilies;
		Assert.NotEmpty(families);
		Assert.All(families, f => Assert.False(string.IsNullOrEmpty(f.Source)));
		Assert.Equal(families.Count, families.Select(f => f.Source).Distinct().Count());
	});

	[Fact]
	public void Family_faces_have_names_and_valid_axes() => Run(() =>
	{
		var family = Fonts.SystemFontFamilies.First();
		var faces = family.GetTypefaces();
		Assert.NotEmpty(faces);
		foreach (var face in faces)
		{
			Assert.Equal(family, face.FontFamily);
			Assert.InRange(face.Weight.ToOpenTypeWeight(), 1, 999);
			Assert.InRange(face.Stretch.ToOpenTypeStretch(), 1, 9);
			Assert.False(string.IsNullOrEmpty(face.FaceNames[En]));
		}
		Assert.Equal(faces.Count, family.FamilyTypefaces.Count);
		Assert.Equal(family.Source, family.FamilyNames[En]);
	});

	[Fact]
	public void SystemTypefaces_cover_every_family() => Run(() =>
	{
		var all = Fonts.SystemTypefaces;
		var families = Fonts.SystemFontFamilies;
		Assert.True(all.Count >= families.Count);
		Assert.Equal(families.Count, all.Select(t => t.FontFamily.Source).Distinct().Count());
	});

	[Fact]
	public void Typeface_metrics_come_from_pango() => Run(() =>
	{
		var family = Fonts.SystemFontFamilies.First();
		var t = family.GetTypefaces().First();
		Assert.InRange(t.CapsHeight, 0.3, 1.2);
		Assert.InRange(t.XHeight, 0.2, 1.0);
		Assert.True(t.XHeight < t.CapsHeight);
		Assert.True(t.UnderlineThickness > 0);
		Assert.True(family.LineSpacing > family.Baseline);
		Assert.True(family.Baseline > 0.3);
	});

	[Fact]
	public void GlyphTypeface_reports_family_face_and_metrics() => Run(() =>
	{
		var t = Fonts.SystemFontFamilies.First().GetTypefaces().First();
		Assert.True(t.TryGetGlyphTypeface(out var g));
		Assert.Equal(t.FontFamily.Source, g.FamilyNames[CultureInfoEn]);
		Assert.Equal(t.FaceNames[En], g.FaceNames[CultureInfoEn]);
		Assert.Equal(g.FamilyNames[CultureInfoEn], g.Win32FamilyNames[CultureInfoEn]);
		Assert.Equal(t.CapsHeight, g.CapsHeight, 6);
		Assert.True(g.Height > g.Baseline);
		Assert.Empty(g.Copyrights);
		Assert.Empty(g.SampleTexts);
		Assert.Empty(g.DesignerUrls);
		Assert.Throws<System.NotSupportedException>(() => g.Copyrights.Add(CultureInfoEn, "x"));
		Assert.NotNull(g.FontUri);
		Assert.Equal(0.0, g.Version);
	});

	[Fact]
	public void GlyphTypeface_covers_basic_latin() => Run(() =>
	{
		var t = Fonts.SystemFontFamilies.First(f => f.GetTypefaces().Any()).GetTypefaces().First();
		Assert.True(t.TryGetGlyphTypeface(out var g));
		Assert.True(g.GlyphCount > 0);
		Assert.Equal(g.GlyphCount, g.CharacterToGlyphMap.Count);
		Assert.All(g.CharacterToGlyphMap.Keys, k => Assert.InRange(k, 0x20, 0xFFFF));
	});

	[Fact]
	public void Requesting_bold_from_a_family_reports_the_matched_face() => Run(() =>
	{
		var family = Fonts.SystemFontFamilies.First();
		var t = new Typeface(family, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
		Assert.True(t.TryGetGlyphTypeface(out var g));
		bool hasBold = family.GetTypefaces().Any(f => f.Weight.ToOpenTypeWeight() >= 600);
		Assert.Equal(!hasBold, t.IsBoldSimulated);
		Assert.Equal(t.IsBoldSimulated, (g.StyleSimulations & StyleSimulations.BoldSimulation) != 0);
	});

	static System.Globalization.CultureInfo CultureInfoEn => System.Globalization.CultureInfo.GetCultureInfo("en-US");
}
