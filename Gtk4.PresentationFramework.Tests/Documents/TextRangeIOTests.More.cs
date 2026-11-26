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
	[Fact]
	public void Xaml_round_trips_formatting()
	{
		var bytes = Save(All(Formatted()), DataFormats.Xaml);
		var d = new FlowDocument();
		Load(All(d), bytes, DataFormats.Xaml);
		Assert.Equal(2, d.Blocks.Count);
		var inl = ((Paragraph)d.Blocks[0]).Inlines.ToList();
		Assert.IsType<Run>(inl[0]);
		Assert.IsType<Bold>(inl[1]);
		Assert.IsType<LineBreak>(inl[2]);
		Assert.IsType<Italic>(inl[3]);
		var run = (Run)((Underline)inl[4]).Inlines[0];
		Assert.Equal("Arial", run.FontFamily.Source);
		Assert.Equal(FontWeights.Light, run.FontWeight);
		Assert.Equal(All(Formatted()).Text, All(d).Text);
	}

	[Fact]
	public void Xaml_load_reads_wpf_style_content_and_rejects_bad_xml()
	{
		var xaml = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
			+ "xml:space=\"preserve\">"
			+ "<Paragraph FontSize=\"20\"><Run FontWeight=\"Bold\" Foreground=\"#FF0000\">hi</Run> "
			+ "there</Paragraph></Section>";
		var d = new FlowDocument();
		Load(All(d), Encoding.UTF8.GetBytes(xaml), DataFormats.Xaml);
		var p = (Paragraph)d.Blocks[0];
		Assert.Equal(2, p.Inlines.Count);
		Assert.Equal(FontWeights.Bold, p.Inlines[0].FontWeight);
		Assert.Equal("hi there\r\n", All(d).Text);
		Assert.Throws<ArgumentException>(() => Load(All(d), Encoding.UTF8.GetBytes("<Section"), DataFormats.Xaml));
	}

	[Fact]
	public void Xaml_save_of_a_sub_range_clips_the_text()
	{
		var d = Formatted();
		var r = new TextRange(d.ContentStart.GetPositionAtOffset(4)!, d.ContentStart.GetPositionAtOffset(7)!);
		var xml = Encoding.UTF8.GetString(Save(r, DataFormats.Xaml));
		Assert.Contains("<Run>ain</Run>", xml);
		Assert.DoesNotContain("second", xml);
	}

	[Fact]
	public void Rtf_save_has_the_subset_and_escapes()
	{
		var rtf = Encoding.ASCII.GetString(Save(All(Formatted()), DataFormats.Rtf));
		Assert.StartsWith("{\\rtf1", rtf);
		Assert.Contains("{\\b bold}", rtf);
		Assert.Contains("\\line ", rtf);
		Assert.Contains("\\u233?", rtf);
		Assert.Contains("\\{x\\}\\\\", rtf);
		Assert.Contains("{\\ul und}", rtf);
		Assert.Equal(2, rtf.Split(new[] { "\\par\r" }, StringSplitOptions.None).Length - 1);
	}

	[Fact]
	public void Rtf_round_trips_text_and_character_formatting()
	{
		var d = new FlowDocument();
		Load(All(d), Save(All(Formatted()), DataFormats.Rtf), DataFormats.Rtf);
		Assert.Equal(2, d.Blocks.Count);
		Assert.Equal("plain bold\r\nital\u00e9 {x}\\und\r\nsecond\r\n", All(d).Text);
		var bold = ((Paragraph)d.Blocks[0]).Inlines.OfType<Run>().Single(r => r.Text == "bold");
		Assert.Equal(FontWeights.Bold, bold.FontWeight);
	}

	[Fact]
	public void Rtf_load_reads_hex_escapes_and_skips_tables()
	{
		var rtf = "{\\rtf1\\ansi{\\fonttbl{\\f0 Arial;}}{\\colortbl;\\red0;}\\pard caf\\'e9 \\i it\\i0  \\u8364?\\par}";
		var d = new FlowDocument();
		Load(All(d), Encoding.ASCII.GetBytes(rtf), DataFormats.Rtf);
		Assert.Equal("caf\u00e9 it \u20ac\r\n", All(d).Text);
		Assert.Throws<ArgumentException>(() => Load(All(d), Encoding.ASCII.GetBytes("plain"), DataFormats.Rtf));
	}

	[Fact]
	public void XamlPackage_round_trips_through_a_zip()
	{
		var bytes = Save(All(Formatted()), DataFormats.XamlPackage);
		Assert.Equal((byte)'P', bytes[0]);
		var d = new FlowDocument();
		Load(All(d), bytes, DataFormats.XamlPackage);
		Assert.Equal(All(Formatted()).Text, All(d).Text);
		Assert.Throws<ArgumentException>(() => Load(All(d), new byte[] { 1, 2, 3, 4 }, DataFormats.XamlPackage));
	}

	[Fact]
	public void Load_into_a_sub_range_keeps_the_surrounding_text()
	{
		var d = new FlowDocument();
		Load(All(d), Encoding.UTF8.GetBytes("abcdef"), DataFormats.Text);
		var r = new TextRange(d.ContentStart.GetPositionAtOffset(4)!, d.ContentStart.GetPositionAtOffset(6)!);
		Load(r, Encoding.UTF8.GetBytes("1\n2\n"), DataFormats.Text);
		Assert.Equal("ab1\r\n2ef\r\n", All(d).Text);
	}

	[Fact]
	public void Selection_of_a_RichTextBox_saves_and_loads()
	{
		var rtb = new System.Windows.Controls.RichTextBox();
		Load(rtb.Selection, Encoding.UTF8.GetBytes("hello"), DataFormats.Text);
		Assert.Equal("hello\r\n", Encoding.UTF8.GetString(Save(rtb.Selection, DataFormats.Text)));
	}
}
