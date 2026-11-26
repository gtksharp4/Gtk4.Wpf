using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;
using DRun = System.Windows.Documents.Run;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class ContentBindingVerifyTests : WpfGtkTestBase
{
	static Binding Ancestor(System.Type type, int level = 1) => new Binding("Name")
	{
		RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = type, AncestorLevel = level },
	};

	[Fact]
	public void ListItem_is_a_TextElement_and_takes_its_properties()
	{
		var item = new ListItem();
		Assert.IsAssignableFrom<TextElement>(item);
		item.FontWeight = FontWeights.Bold;
		item.Foreground = Brushes.Red;
		Assert.Equal(FontWeights.Bold, item.FontWeight);
		Assert.Equal(Colors.Red, ((SolidColorBrush)item.Foreground).Color);
		item.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(FlowDirection.RightToLeft, item.FlowDirection);
	}

	[Fact]
	public void FindAncestor_from_a_Run_finds_the_enclosing_TextBlock() => Run(() =>
	{
		var tb = new TextBlock { Name = "tb" };
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, Ancestor(typeof(TextBlock)));
		Assert.Equal("tb", run.Text);
	});

	[Fact]
	public void FindAncestor_from_a_Run_finds_the_enclosing_Paragraph() => Run(() =>
	{
		var run = new DRun();
		var para = new Paragraph(run) { Name = "para" };
		var doc = new FlowDocument(para);
		run.SetBinding(DRun.TextProperty, Ancestor(typeof(Paragraph)));
		Assert.Equal("para", run.Text);
		Assert.NotNull(doc);
	});

	[Fact]
	public void FindAncestor_level_counts_from_the_content_elements_parents() => Run(() =>
	{
		var run = new DRun();
		var inner = new Span { Name = "inner" };
		inner.Inlines.Add(run);
		var outer = new Span { Name = "outer" };
		outer.Inlines.Add(inner);
		var tb = new TextBlock();
		tb.Inlines.Add(outer);
		run.SetBinding(DRun.TextProperty, Ancestor(typeof(Span), 2));
		Assert.Equal("outer", run.Text);
	});
}
