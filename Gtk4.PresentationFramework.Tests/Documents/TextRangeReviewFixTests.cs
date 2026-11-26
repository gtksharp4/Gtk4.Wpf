using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextRangeReviewFixTests
{
	static FlowDocument Doc(params string[] paragraphs)
	{
		var d = new FlowDocument();
		foreach (var t in paragraphs)
		{
			var p = new Paragraph();
			p.Inlines.Add(new Run { Text = t });
			d.Blocks.Add(p);
		}
		return d;
	}

	static MemoryStream Bytes(string s) => new MemoryStream(Encoding.UTF8.GetBytes(s));

	static string Load(string rtf)
	{
		var d = new FlowDocument();
		new TextRange(d.ContentStart, d.ContentEnd).Load(Bytes(rtf), DataFormats.Rtf);
		return string.Concat(d.Blocks.OfType<Paragraph>().SelectMany(p => p.Inlines.OfType<Run>()).Select(r => r.Text));
	}

	static string Save(FlowDocument d, string format)
	{
		var ms = new MemoryStream();
		new TextRange(d.ContentStart, d.ContentEnd).Save(ms, format);
		return Encoding.UTF8.GetString(ms.ToArray());
	}

	[Theory]
	[InlineData(10)]
	[InlineData(9)]
	public void Delete_starting_outside_a_paragraph_logs_the_real_symbol_count(int end)
	{
		var d = Doc("abc", "def");
		var endPtr = d.ContentEnd;
		var mid = d.ContentStart.GetPositionAtOffset(10, LogicalDirection.Forward)!;

		new TextRange(d.ContentStart, d.ContentStart.GetPositionAtOffset(end)!).Text = "X";

		Assert.Equal(0, endPtr.CompareTo(d.ContentEnd));
		Assert.True(mid.CompareTo(d.ContentEnd) <= 0);
		Assert.Equal("e", mid.GetTextInRun(LogicalDirection.Forward).Substring(0, 1));
	}

	[Fact]
	public void Pointer_inside_the_surviving_text_keeps_pointing_at_it()
	{
		var d = Doc("abc", "def");
		var mid = d.ContentStart.GetPositionAtOffset(10, LogicalDirection.Forward)!;
		new TextRange(d.ContentStart, d.ContentStart.GetPositionAtOffset(10)!).Text = "";
		Assert.StartsWith("e", mid.GetTextInRun(LogicalDirection.Forward));
	}

	[Fact]
	public void Rtf_unicode_skips_the_ANSI_fallback_per_uc()
	{
		Assert.Equal("€ euro", Load("{\\rtf1\\ansi \\u8364\\'80 euro\\par}"));
		Assert.Equal("€x", Load("{\\rtf1\\uc0\\u8364 x\\par}"));
		Assert.Equal("x", Load("{\\rtf1 \\u-3913?x\\par}"));
		Assert.Equal("€ yz", Load("{\\rtf1\\uc2\\u8364ab yz\\par}"));
	}

	[Fact]
	public void Underline_decoration_round_trips_through_rtf_and_xaml()
	{
		var d = new FlowDocument();
		new TextRange(d.ContentStart, d.ContentEnd).Load(Bytes("{\\rtf1 {\\ul abc}\\par}"), DataFormats.Rtf);
		Assert.Contains("\\ul", Save(d, DataFormats.Rtf));
		string xaml = Save(d, DataFormats.Xaml);
		Assert.Contains("TextDecorations=\"Underline\"", xaml);

		var back = new FlowDocument();
		new TextRange(back.ContentStart, back.ContentEnd).Load(Bytes(xaml), DataFormats.Xaml);
		var run = back.Blocks.OfType<Paragraph>().First().Inlines.OfType<Run>().First();
		Assert.Contains(run.TextDecorations!, x => x.Location == TextDecorationLocation.Underline);
	}

	[Fact]
	public void Xaml_package_is_an_OPC_package_with_a_rels_entry()
	{
		var d = Doc("hello");
		var ms = new MemoryStream();
		new TextRange(d.ContentStart, d.ContentEnd).Save(ms, DataFormats.XamlPackage);
		using var zip = new ZipArchive(new MemoryStream(ms.ToArray()), ZipArchiveMode.Read);
		string rels = new StreamReader(zip.GetEntry("_rels/.rels")!.Open()).ReadToEnd();
		Assert.Contains("Target=\"/Xaml/Document.xaml\"", rels);
		string types = new StreamReader(zip.GetEntry("[Content_Types].xml")!.Open()).ReadToEnd();
		Assert.Contains("application/vnd.openxmlformats-package.relationships+xml", types);
	}

	[Fact]
	public void Xaml_package_entry_is_resolved_through_rels()
	{
		var ms = new MemoryStream();
		using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
		{
			Write(zip, "_rels/.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/"
				+ "relationships\"><Relationship Id=\"R0\" Type=\"http://schemas.microsoft.com/wpf/2005/10/xaml/"
				+ "entry\" Target=\"/Other/Part.xaml\"/></Relationships>");
			Write(zip, "Other/Part.xaml", "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/"
				+ "presentation\"><Paragraph>moved</Paragraph></Section>");
		}
		ms.Position = 0;
		var d = new FlowDocument();
		new TextRange(d.ContentStart, d.ContentEnd).Load(ms, DataFormats.XamlPackage);
		Assert.Equal("moved", string.Concat(d.Blocks.OfType<Paragraph>().SelectMany(p => p.Inlines.OfType<Run>())
			.Select(r => r.Text)));
	}

	static void Write(ZipArchive zip, string name, string content)
	{
		using var w = new StreamWriter(zip.CreateEntry(name).Open());
		w.Write(content);
	}

	[Fact]
	public void Whole_document_replace_with_nothing_leaves_one_empty_paragraph()
	{
		var d = Doc("abc", "def");
		var endPtr = d.ContentEnd;
		new TextRange(d.ContentStart, d.ContentEnd).Text = "";

		Assert.Single(d.Blocks);
		Assert.Empty(((Paragraph)d.Blocks.First()).Inlines);
		Assert.Equal("\r\n", new TextRange(d.ContentStart, d.ContentEnd).Text);
		Assert.Equal(0, endPtr.CompareTo(d.ContentEnd));
	}

	[Fact]
	public void Loading_empty_text_leaves_one_empty_paragraph()
	{
		var d = Doc("abc");
		new TextRange(d.ContentStart, d.ContentEnd).Load(new MemoryStream(), DataFormats.Text);
		Assert.Single(d.Blocks);
		Assert.Equal("\r\n", new TextRange(d.ContentStart, d.ContentEnd).Text);
	}

	[Fact]
	public void Pointer_offset_is_clamped_after_a_direct_edit()
	{
		var d = Doc("abcdef");
		var end = d.ContentEnd;
		((Run)((Paragraph)d.Blocks.First()).Inlines.First()).Text = "a";

		Assert.True(end.CompareTo(d.ContentEnd) <= 0);
		Assert.Equal(end.CompareTo(d.ContentEnd), 0);
		Assert.NotEqual(TextPointerContext.None, d.ContentStart.GetPointerContext(LogicalDirection.Forward));
	}
}
