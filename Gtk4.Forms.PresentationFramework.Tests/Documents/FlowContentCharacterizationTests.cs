using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Documents;

/// <summary>Pins the rendering recorded before TextElement/FlowDocument were rebased onto FrameworkContentElement.</summary>
public class FlowContentCharacterizationTests
{
	[Fact]
	public void Styled_document_renders_unchanged()
	{
		var d = FlowContentSamples.Styled();
		Assert.Equal("<span foreground=\"#FF0000\">plain &amp; &lt;esc&gt; </span><span weight='semibold'>semi</span>"
			+ "<b><i>bold</i></b><span underline=\"single\">deco</span>\nfam\n\nin span<b>b</b><i></i><u></u>",
			d.BuildMarkup());
		Assert.Equal("plain & <esc> semibolddeco\nfam\n\nin spanb", d.BuildPlainText());
		Assert.Equal("plain & <esc> semibolddeco\nfam\nin spanb", d.GetAllText());
	}

	[Fact]
	public void Directional_document_renders_unchanged()
	{
		var d = FlowContentSamples.Directional();
		Assert.Equal("‏⁧<span lang=\"he-il\">rtl</span>⁩\n\n<span lang=\"fr-fr\">‎⁦ltr⁩\n"
			+ "‎⁦⁧<span lang=\"ar\">x</span>⁩⁩</span>", d.BuildMarkup());
		Assert.Equal("rtl\n\nltr\n\nx", d.BuildPlainText());
		Assert.Equal("rtl\nltr\n\nx", d.GetAllText());
	}

	[Fact]
	public void Lists_render_unchanged()
	{
		var d = FlowContentSamples.Lists();
		Assert.Equal("• item0\n• item1\n\n\n1 item0\n2 item1\n\n\nI item0\nII item1\n\n\n"
			+ "‏⁧• r⁩\n", d.BuildMarkup());
		Assert.Equal("• item0\n• item1\n\n\n1 item0\n2 item1\n\n\nI item0\nII item1\n\n\n• r\n",
			d.BuildPlainText());
		Assert.Equal("• item0\n• item1\n\n1 item0\n2 item1\n\nI item0\nII item1\n\n• r\n",
			d.GetAllText());
	}

	[Fact]
	public void Clr_defaults_are_unchanged()
	{
		var p = new Paragraph();
		Assert.Equal("Segoe UI", p.FontFamily.Source);
		Assert.Equal(FontWeights.Normal, p.FontWeight);
		Assert.Same(SystemColors.ControlTextBrush, p.Foreground);
		Assert.Null(p.TextAlignment);
		Assert.True(double.IsNaN(p.LineHeight));
		Assert.True(double.IsNaN(p.Margin.Left) && double.IsNaN(p.Margin.Top)
			&& double.IsNaN(p.Margin.Right) && double.IsNaN(p.Margin.Bottom));
		Assert.Equal(System.Windows.FlowDirection.LeftToRight, p.FlowDirection);
		Assert.Equal("en-us", p.Language.IetfLanguageTag);
		Assert.Throws<System.ArgumentNullException>(() => p.Language = null!);
		var d = new FlowDocument();
		Assert.Equal(12.0, d.FontSize);
		Assert.Equal("Segoe UI", d.FontFamily.Source);
		Assert.Null(d.TextAlignment);
	}
}
