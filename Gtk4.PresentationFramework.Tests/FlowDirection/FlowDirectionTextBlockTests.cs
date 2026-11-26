using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

/// <summary>A TextBlock label's paragraph direction follows FlowDirection, not the first strong character.</summary>
public class FlowDirectionTextBlockTests : WpfGtkTestBase
{
	static Pango.Direction ParagraphDirection(TextBlock block) => ((Gtk.Label)block.Widget).Layout.GetLine(0).ResolvedDirection;

	[Fact]
	public void An_RTL_TextBlock_with_neutral_text_gets_an_Rtl_paragraph() => Run(() =>
	{
		var block = new TextBlock { Text = "Hello!", FlowDirection = FlowDirection.RightToLeft };

		Assert.Equal(Pango.Direction.Rtl, ParagraphDirection(block));
		Assert.Equal("Hello!", block.Text);
	});

	[Fact]
	public void Switching_FlowDirection_refreshes_the_label() => Run(() =>
	{
		var block = new TextBlock { Text = "Hello!", FlowDirection = FlowDirection.RightToLeft };
		_ = block.Widget;

		block.FlowDirection = FlowDirection.LeftToRight;

		Assert.Equal(Pango.Direction.Ltr, ParagraphDirection(block));
		Assert.Equal("Hello!", block.Text);
	});

	[Fact]
	public void An_explicit_LTR_TextBlock_in_an_RTL_panel_with_Arabic_text_stays_Ltr() => Run(() =>
	{
		var block = new TextBlock { Text = "مرحبا", FlowDirection = FlowDirection.LeftToRight };
		var panel = new StackPanel { FlowDirection = FlowDirection.RightToLeft };
		panel.Children.Add(block);

		Assert.Equal(Pango.Direction.Ltr, ParagraphDirection(block));
	});

	[Fact]
	public void A_default_TextBlock_pushes_its_text_unchanged() => Run(() =>
	{
		var block = new TextBlock { Text = "Hello!" };

		Assert.Equal("Hello!", ((Gtk.Label)block.Widget).Text);
	});
}
