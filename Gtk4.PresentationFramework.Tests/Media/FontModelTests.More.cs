using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public partial class FontModelTests
{
	[Fact]
	public void FontFamily_names_and_base_uri()
	{
		var f = new FontFamily(new Uri("file:///c:/fonts/"), "file:///c:/fonts/#Zed Sans");
		Assert.Equal("Zed Sans", f.FamilyNames[En]);
		Assert.Equal("file:///c:/fonts/", f.BaseUri!.AbsoluteUri);
		Assert.Null(new FontFamily("x").BaseUri);
		Assert.True(f.FamilyNames.IsReadOnly);
		Assert.Empty(f.FamilyMaps);
		f.FamilyMaps.Add(new FontFamilyMap { Unicode = "0000-00FF", Target = "Arial" });
		Assert.Equal(1.0, f.FamilyMaps[0].Scale);
		Assert.Throws<ArgumentNullException>(() => f.FamilyMaps.Add(null!));
	}

	[Fact]
	public void Unknown_family_yields_one_normal_typeface()
	{
		var f = new FontFamily("NoSuchFamilyXyz");
		var faces = f.GetTypefaces();
		Assert.Single(faces);
		var t = faces.First();
		Assert.Equal(FontWeights.Normal, t.Weight);
		Assert.Equal("Regular", t.FaceNames[En]);
		Assert.False(t.TryGetGlyphTypeface(out var g));
		Assert.Null(g);
	}

	[Fact]
	public void Typeface_face_names_describe_unknown_faces()
	{
		var bold = new Typeface(new FontFamily("NoSuchFamilyXyz"),
			FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
		var both = new Typeface(new FontFamily("NoSuchFamilyXyz"),
			FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
		Assert.Equal("Bold", bold.FaceNames[En]);
		Assert.Equal("Bold Italic", both.FaceNames[En]);
		Assert.True(bold.FaceNames.IsReadOnly);
	}

	[Fact]
	public void Metrics_are_positive_ratios_even_without_GTK()
	{
		var t = new Typeface("NoSuchFamilyXyz");
		Assert.InRange(t.CapsHeight, 0.01, 2);
		Assert.InRange(t.XHeight, 0.01, 2);
		Assert.True(t.UnderlineThickness > 0);
		Assert.True(t.StrikethroughThickness > 0);
		Assert.True(new FontFamily("x").LineSpacing >= new FontFamily("x").Baseline);
	}

	[Fact]
	public void SystemFonts_message_font_matches_the_defaults()
	{
		Assert.Equal("Segoe UI", SystemFonts.MessageFontFamily.Source);
		Assert.Equal(12.0, SystemFonts.MessageFontSize);
		Assert.Equal(FontStyles.Normal, SystemFonts.MessageFontStyle);
		Assert.Equal(FontWeights.Normal, SystemFonts.MessageFontWeight);
	}

	[Fact]
	public void GlyphTypeface_from_a_file_uri_is_not_supported()
	{
		Assert.Throws<ArgumentNullException>(() => new GlyphTypeface(null!));
		Assert.Throws<NotSupportedException>(() => new GlyphTypeface(new Uri("file:///c:/x.ttf")));
	}

	[Fact]
	public void StyleSimulations_flags_combine()
	{
		Assert.Equal(StyleSimulations.BoldItalicSimulation,
			StyleSimulations.BoldSimulation | StyleSimulations.ItalicSimulation);
	}

	[Fact]
	public void Fonts_folder_scan_lists_font_file_names()
	{
		var dir = Path.Combine(Path.GetTempPath(), "gf50b-fonts-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		try
		{
			File.WriteAllBytes(Path.Combine(dir, "Zed Sans.ttf"), new byte[1]);
			File.WriteAllBytes(Path.Combine(dir, "Alpha.OTF"), new byte[1]);
			File.WriteAllBytes(Path.Combine(dir, "readme.txt"), new byte[1]);
			var families = Fonts.GetFontFamilies(dir);
			Assert.Equal(2, families.Count);
			Assert.Equal(new[] { "Alpha", "Zed Sans" }, families.Select(f => f.FamilyNames[En]).ToArray());
			Assert.All(families, f => Assert.NotNull(f.BaseUri));
			Assert.Equal(2, Fonts.GetTypefaces(dir).Count);
			Assert.Equal(2, Fonts.GetFontFamilies(new Uri(dir + Path.DirectorySeparatorChar)).Count);
			Assert.Single(Fonts.GetFontFamilies(Path.Combine(dir, "Alpha.OTF")));
			Assert.Single(Fonts.GetFontFamilies(new Uri(dir + Path.DirectorySeparatorChar), "Alpha.OTF"));
		}
		finally
		{
			Directory.Delete(dir, true);
		}
	}

	[Fact]
	public void Fonts_missing_folder_is_empty_and_null_throws()
	{
		Assert.Empty(Fonts.GetFontFamilies(
			Path.Combine(Path.GetTempPath(), "gf50b-none-" + Guid.NewGuid().ToString("N"))));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetFontFamilies((string)null!));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetFontFamilies((Uri)null!));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetTypefaces(null!, null!));
	}
}
