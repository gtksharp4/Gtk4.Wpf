using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;
using DRun = System.Windows.Documents.Run;

namespace Gtk4.PresentationFramework.Tests.Documents;

public sealed record Ctx(string Name);

public class RunReviewFixTests : WpfGtkTestBase
{
	[Fact]
	public void Rejected_MultiBinding_keeps_the_existing_binding() => Run(() =>
	{
		var run = new DRun();
		var tb = new TextBlock { DataContext = new Ctx("n") };
		tb.Inlines.Add(run);
		var first = run.SetBinding(DRun.TextProperty, new Binding("Name"));
		Assert.Throws<NotSupportedException>(() => run.SetBinding(DRun.TextProperty, new MultiBinding()));
		Assert.Same(first, run.GetBindingExpression(DRun.TextProperty));
		Assert.Equal("n", run.Text);
	});

	[Fact]
	public void Run_inserted_inside_a_runs_text_splits_the_run()
	{
		var run = new DRun("abcd") { FontWeight = FontWeights.Bold };
		var para = new Paragraph(run);
		var doc = new FlowDocument(para);
		var inserted = new DRun("X", doc.ContentStart.GetPositionAtOffset(4)!);
		Assert.Equal(3, para.Inlines.Count);
		Assert.Equal("ab", ((DRun)para.Inlines[0]).Text);
		Assert.Same(inserted, para.Inlines[1]);
		var right = (DRun)para.Inlines[2];
		Assert.Equal("cd", right.Text);
		Assert.Equal(FontWeights.Bold, right.FontWeight);
		Assert.Equal("abXcd", new TextRange(doc.ContentStart, doc.ContentEnd).Text.TrimEnd('\r', '\n'));
	}

	[Fact]
	public void Run_inserted_at_a_runs_end_goes_after_it_without_splitting()
	{
		var run = new DRun("ab");
		var para = new Paragraph(run);
		var doc = new FlowDocument(para);
		var inserted = new DRun("X", doc.ContentStart.GetPositionAtOffset(4)!);
		Assert.Equal(2, para.Inlines.Count);
		Assert.Same(inserted, para.Inlines[1]);
	}

	[Fact]
	public void List_and_ListItem_own_their_children_logically()
	{
		var run = new DRun("x");
		var item = new ListItem();
		item.Blocks.Add(new Paragraph(run));
		var list = new List();
		list.ListItems.Add(item);
		var doc = new FlowDocument(list);
		Assert.Same(list, item.Parent);
		Assert.Same(item, ((Paragraph)item.Blocks[0]).Parent);
		Assert.Same(doc, list.Parent);
	}

	[Fact]
	public void Run_in_a_list_inherits_the_viewer_data_context() => Run(() =>
	{
		var run = new DRun();
		var item = new ListItem();
		item.Blocks.Add(new Paragraph(run));
		var list = new List();
		list.ListItems.Add(item);
		var viewer = new FlowDocumentScrollViewer
		{
			Document = new FlowDocument(list), DataContext = new Ctx("dc"),
		};
		run.SetBinding(DRun.TextProperty, new Binding("Name"));
		Assert.Equal("dc", run.Text);
		Assert.NotNull(viewer);
	});
}
