using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class PaginationBatchingTests : WpfGtkTestBase
{
	sealed class CountingViewer : FlowDocumentPageViewer
	{
		internal int Refreshes;

		internal override void OnPagesRefreshed()
		{
			Refreshes++;
			base.OnPagesRefreshed();
		}
	}

	static DynamicDocumentPaginator Paginator(FlowDocument doc) =>
		(DynamicDocumentPaginator)((IDocumentPaginatorSource)doc).DocumentPaginator;

	static string Words(int n) => string.Join(" ", System.Linq.Enumerable.Repeat("word", n));

	[Fact]
	public void Fifty_paragraphs_added_in_one_turn_raise_one_batch() => Run(() =>
	{
		var doc = new FlowDocument();
		var paginator = Paginator(doc);
		int changed = 0, completed = 0;
		paginator.PagesChanged += (s, e) => changed++;
		paginator.PaginationCompleted += (s, e) => completed++;
		using (new GtkDispatcherScope())
		{
			for (int i = 0; i < 50; i++) doc.Blocks.Add(new Paragraph(new Run("paragraph " + i)));
			Assert.Equal(0, changed);
			GtkPump.For(150);
		}
		Assert.Equal(1, changed);
		Assert.Equal(1, completed);
		Assert.True(paginator.IsPageCountValid);
	});

	[Fact]
	public void Synchronous_readers_flush_the_pending_repagination() => Run(() =>
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("one")));
		var paginator = Paginator(doc);
		Assert.Equal(1, paginator.PageCount);
		int changed = 0;
		paginator.PagesChanged += (s, e) => changed++;
		using (new GtkDispatcherScope())
		{
			for (int i = 0; i < 300; i++) doc.Blocks.Add(new Paragraph(new Run("paragraph " + i)));
			Assert.False(paginator.IsPageCountValid);
			Assert.Equal(1, changed);
			Assert.True(paginator.PageCount > 1);
			Assert.True(paginator.IsPageCountValid);
			GtkPump.For(150);
		}
		Assert.Equal(1, changed);
	});

	[Fact]
	public void A_nested_run_text_edit_changes_the_page_count() => Run(() =>
	{
		var run = new Run("short");
		var doc = new FlowDocument(new Paragraph(run));
		var paginator = Paginator(doc);
		Assert.Equal(1, paginator.PageCount);
		run.Text = Words(40000);
		Assert.True(paginator.PageCount > 1);
	});

	[Fact]
	public void Nested_collection_edits_repaginate() => Run(() =>
	{
		var para = new Paragraph();
		var section = new Section();
		section.Blocks.Add(para);
		var list = new List();
		var item = new ListItem();
		var cellParagraph = new Paragraph();
		var cell = new TableCell(cellParagraph);
		var row = new TableRow();
		var group = new TableRowGroup();
		var table = new Table();
		var doc = new FlowDocument();
		doc.Blocks.Add(section);
		doc.Blocks.Add(list);
		doc.Blocks.Add(table);
		var paginator = Paginator(doc);
		int changed = 0;
		paginator.PagesChanged += (s, e) => changed++;
		void Expect(Action edit)
		{
			int before = changed;
			using (new GtkDispatcherScope())
			{
				edit();
				GtkPump.For(100);
			}
			Assert.Equal(before + 1, changed);
		}
		Expect(() => para.Inlines.Add(new Run("a")));
		Expect(() => section.Blocks.Add(new Paragraph()));
		Expect(() => list.ListItems.Add(item));
		Expect(() => item.Blocks.Add(new Paragraph()));
		Expect(() => table.RowGroups.Add(group));
		Expect(() => group.Rows.Add(row));
		Expect(() => row.Cells.Add(cell));
		Expect(() => cell.Blocks.Add(new Paragraph()));
	});

	[Fact]
	public void Tag_and_DataContext_changes_do_not_repaginate_but_layout_properties_do() => Run(() =>
	{
		var run = new Run("x");
		var doc = new FlowDocument(new Paragraph(run));
		var paginator = Paginator(doc);
		_ = paginator.PageCount;
		int changed = 0;
		paginator.PagesChanged += (s, e) => changed++;
		using (new GtkDispatcherScope())
		{
			doc.Tag = 1;
			doc.DataContext = new object();
			doc.Name = "n";
			run.Tag = "t";
			run.DataContext = 5;
			GtkPump.For(100);
			Assert.Equal(0, changed);
			run.FontSize = 40;
			GtkPump.For(100);
			Assert.Equal(1, changed);
			doc.PageWidth = 300;
			GtkPump.For(100);
			Assert.Equal(2, changed);
		}
	});

	[Fact]
	public void The_page_viewer_refreshes_its_status_once_per_batch() => Run(() =>
	{
		var doc = new FlowDocument();
		var viewer = new CountingViewer { Document = doc };
		int baseline;
		using (new GtkDispatcherScope())
		{
			GtkPump.For(100);
			baseline = viewer.Refreshes;
			for (int i = 0; i < 50; i++) doc.Blocks.Add(new Paragraph(new Run("paragraph " + i)));
			GtkPump.For(150);
		}
		Assert.Equal(baseline + 1, viewer.Refreshes);
	});
}
