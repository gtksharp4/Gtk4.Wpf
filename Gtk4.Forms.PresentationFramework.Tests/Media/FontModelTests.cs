using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class FontModelTests
{
	static readonly XmlLanguage En = XmlLanguage.GetLanguage("en-US");

	[Fact]
	public void LanguageSpecificStringDictionary_validates_arguments()
	{
		var d = new LanguageSpecificStringDictionary();
		Assert.Throws<ArgumentNullException>(() => d.Add(null!, "x"));
		Assert.Throws<ArgumentNullException>(() => d.Add(En, null!));
		Assert.Throws<ArgumentNullException>(() => d[null!] = "x");
		Assert.Throws<ArgumentNullException>(() => d[En] = null!);
		Assert.Throws<ArgumentNullException>(() => d.ContainsKey(null!));
		Assert.Throws<ArgumentNullException>(() => d.TryGetValue(null!, out _));
		Assert.Throws<ArgumentNullException>(() => d.Remove(null!));
		Assert.Throws<ArgumentNullException>(() => d[null!].ToString());
	}

	[Fact]
	public void LanguageSpecificStringDictionary_behaves_as_a_dictionary()
	{
		var d = new LanguageSpecificStringDictionary();
		d.Add(En, "one");
		Assert.Throws<ArgumentException>(() => d.Add(En, "dup"));
		d[XmlLanguage.GetLanguage("fr-FR")] = "un";
		Assert.Equal(2, d.Count);
		Assert.True(d.TryGetValue(En, out var v));
		Assert.Equal("one", v);
		Assert.True(d.Contains(new System.Collections.Generic.KeyValuePair<XmlLanguage, string>(En, "one")));
		var copy = new System.Collections.Generic.KeyValuePair<XmlLanguage, string>[3];
		d.CopyTo(copy, 1);
		Assert.Equal(2, copy.Count(p => p.Key != null));
		Assert.Throws<ArgumentException>(() => d.CopyTo(new System.Collections.Generic.KeyValuePair<XmlLanguage, string>[1], 0));
		Assert.True(d.Remove(En));
		Assert.False(d.Remove(En));
		d.Clear();
		Assert.Empty(d);
		Assert.False(d.IsReadOnly);
	}

	[Fact]
	public void Read_only_dictionary_rejects_changes()
	{
		var d = LanguageSpecificStringDictionary.Single("en-US", "Arial");
		Assert.True(d.IsReadOnly);
		Assert.Equal("Arial", d[En]);
		Assert.Throws<NotSupportedException>(() => d.Add(XmlLanguage.GetLanguage("fr"), "x"));
		Assert.Throws<NotSupportedException>(() => d[En] = "x");
		Assert.Throws<NotSupportedException>(() => d.Clear());
		Assert.Throws<NotSupportedException>(() => d.Remove(En));
	}

	[Fact]
	public void Typeface_constructors_set_properties_and_validate()
	{
		var t = new Typeface("Arial");
		Assert.Equal("Arial", t.FontFamily.Source);
		Assert.Equal(FontStyles.Normal, t.Style);
		Assert.Equal(FontWeights.Normal, t.Weight);
		Assert.Equal(FontStretches.Normal, t.Stretch);
		var t2 = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Bold, FontStretches.Condensed,
			new FontFamily("Courier"));
		Assert.Equal(FontStyles.Italic, t2.Style);
		Assert.Equal(FontWeights.Bold, t2.Weight);
		Assert.Equal(FontStretches.Condensed, t2.Stretch);
		Assert.Throws<ArgumentNullException>(() => new Typeface((string)null!));
		Assert.Throws<ArgumentNullException>(
			() => new Typeface(null!, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal));
	}

	[Fact]
	public void Typeface_has_value_equality()
	{
		var a = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
		var b = new Typeface(new FontFamily("arial"), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
		var c = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
		var d = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal,
			new FontFamily("Courier"));
		Assert.Equal(a, b);
		Assert.Equal(a.GetHashCode(), b.GetHashCode());
		Assert.NotEqual(a, c);
		Assert.NotEqual(a, d);
		Assert.False(a.Equals(null));
	}

	[Fact]
	public void FamilyTypeface_defaults_and_equality()
	{
		var f = new FamilyTypeface();
		Assert.Equal(FontStyles.Normal, f.Style);
		Assert.Equal(FontWeights.Normal, f.Weight);
		Assert.Equal(FontStretches.Normal, f.Stretch);
		Assert.Empty(f.AdjustedFaceNames);
		f.AdjustedFaceNames.Add(En, "Regular");
		Assert.Equal("Regular", f.AdjustedFaceNames[En]);
		Assert.Equal(new FamilyTypeface(), f);
		Assert.NotEqual(new FamilyTypeface { Weight = FontWeights.Bold }, f);
	}

	[Fact]
	public void FamilyTypefaceCollection_is_a_list_that_rejects_null()
	{
		var c = new FontFamily("NoSuchFamilyXyz").FamilyTypefaces;
		Assert.Empty(c);
		c.Add(new FamilyTypeface());
		c.Insert(0, new FamilyTypeface { Weight = FontWeights.Bold });
		Assert.Equal(2, c.Count);
		Assert.Equal(0, c.IndexOf(c[0]));
		Assert.Throws<ArgumentNullException>(() => c.Add(null!));
		c.RemoveAt(0);
		Assert.Single(c);
		c.Clear();
		Assert.Empty(c);
	}

	[Fact]
	public void FontFamily_existing_behaviour_is_unchanged()
	{
		var a = new FontFamily("Segoe UI, Arial");
		Assert.Equal("Segoe UI, Arial", a.Source);
		Assert.Equal(new FontFamily("segoe ui, arial"), a);
		Assert.Equal("Segoe UI, Arial", a.ToString());
		Assert.Equal(string.Empty, new FontFamily().Source);
		Assert.Throws<ArgumentNullException>(() => new FontFamily((string)null!));
	}

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
		var bold = new Typeface(new FontFamily("NoSuchFamilyXyz"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
		var both = new Typeface(new FontFamily("NoSuchFamilyXyz"), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
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
		Assert.Equal(StyleSimulations.BoldItalicSimulation, StyleSimulations.BoldSimulation | StyleSimulations.ItalicSimulation);
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
		Assert.Empty(Fonts.GetFontFamilies(Path.Combine(Path.GetTempPath(), "gf50b-none-" + Guid.NewGuid().ToString("N"))));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetFontFamilies((string)null!));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetFontFamilies((Uri)null!));
		Assert.Throws<ArgumentNullException>(() => Fonts.GetTypefaces(null!, null!));
	}
}
