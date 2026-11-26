using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public partial class TextRangeIOTests
{
	static TextRange All(FlowDocument d) => new TextRange(d.ContentStart, d.ContentEnd);

	static FlowDocument Formatted()
	{
		var d = new FlowDocument();
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = "plain " });
		var b = new Bold();
		b.Inlines.Add(new Run { Text = "bold" });
		p.Inlines.Add(b);
		p.Inlines.Add(new LineBreak());
		var it = new Italic();
		it.Inlines.Add(new Run { Text = "ital\u00e9 {x}\\" });
		p.Inlines.Add(it);
		var u = new Underline();
		u.Inlines.Add(new Run { Text = "und", FontFamily = new FontFamily("Arial"), FontWeight = FontWeights.Light });
		p.Inlines.Add(u);
		d.Blocks.Add(p);
		var p2 = new Paragraph();
		p2.Inlines.Add(new Run { Text = "second" });
		d.Blocks.Add(p2);
		return d;
	}

	static byte[] Save(TextRange r, string fmt)
	{
		var ms = new MemoryStream();
		r.Save(ms, fmt);
		return ms.ToArray();
	}

	static void Load(TextRange r, byte[] data, string fmt) => r.Load(new MemoryStream(data), fmt);

	[Theory]
	[InlineData("Text")]
	[InlineData("UnicodeText")]
	[InlineData("Xaml")]
	[InlineData("Rich Text Format")]
	[InlineData("XamlPackage")]
	public void CanLoad_and_CanSave_known_formats(string fmt)
	{
		var r = All(new FlowDocument());
		Assert.True(r.CanLoad(fmt));
		Assert.True(r.CanSave(fmt));
	}

	[Fact]
	public void CanLoad_unknown_is_false_and_Load_Save_throw()
	{
		var r = All(new FlowDocument());
		Assert.False(r.CanLoad("Bitmap"));
		Assert.False(r.CanSave("nope"));
		Assert.Throws<ArgumentException>(() => r.Load(new MemoryStream(), "nope"));
		Assert.Throws<ArgumentException>(() => r.Save(new MemoryStream(), "nope"));
		Assert.Throws<ArgumentNullException>(() => r.Load(null!, DataFormats.Text));
	}

	[Fact]
	public void Text_load_splits_paragraphs_on_newlines()
	{
		var d = new FlowDocument();
		Load(All(d), Encoding.UTF8.GetBytes("one\r\ntwo\nthree\n"), DataFormats.Text);
		Assert.Equal(3, d.Blocks.Count);
		Assert.Equal("one\r\ntwo\r\nthree\r\n", All(d).Text);
	}

	[Fact]
	public void Text_save_is_utf8_and_round_trips()
	{
		var d = new FlowDocument();
		Load(All(d), Encoding.UTF8.GetBytes("h\u00e9llo\nw"), DataFormats.Text);
		var bytes = Save(All(d), DataFormats.Text);
		Assert.Equal("h\u00e9llo\r\nw\r\n", Encoding.UTF8.GetString(bytes));
	}

	[Fact]
	public void UnicodeText_round_trips_as_utf16()
	{
		var d = new FlowDocument();
		Load(All(d), Encoding.Unicode.GetBytes("a\u20acb"), DataFormats.UnicodeText);
		Assert.Equal("a\u20acb\r\n", Encoding.Unicode.GetString(Save(All(d), DataFormats.UnicodeText)));
	}

	[Fact]
	public void Xaml_save_is_a_Section_fragment()
	{
		var xml = Encoding.UTF8.GetString(Save(All(Formatted()), DataFormats.Xaml));
		var doc = System.Xml.Linq.XDocument.Parse(xml);
		Assert.Equal("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Section", doc.Root!.Name.ToString());
		Assert.Contains("<Bold><Run>bold</Run></Bold>", xml);
		Assert.Contains("<LineBreak />", xml);
		Assert.Contains("FontWeight=\"Light\"", xml);
	}
}
