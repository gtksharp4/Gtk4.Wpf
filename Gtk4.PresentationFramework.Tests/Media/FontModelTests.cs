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
		Assert.Throws<ArgumentException>(() =>
			d.CopyTo(new System.Collections.Generic.KeyValuePair<XmlLanguage, string>[1], 0));
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
}
