using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Real-GTK: containers reached through Span, the collection overload, removal and the trees.</summary>
public class TextBlockInlineTreeGtkTests : WpfGtkTestBase
{
	static WpfWindow Show(TextBlock outer, TextBlock inner)
	{
		var win = new WpfWindow { Width = 600, Height = 400, Content = new StackPanel { Children = { outer } } };
		win.Show();
		GtkState.Require(() => inner.IsWidgetCreated && inner.Widget.IsMapped && inner.Widget.Width > 0,
			"inline child mapped", outer, inner);
		return win;
	}

	[Fact]
	public void A_child_inside_a_Span_is_placed_between_the_Span_texts() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = new TextBlock();
		var span = new Span();
		span.Inlines.Add(new Run("a "));
		span.Inlines.Add(new InlineUIContainer(inner));
		span.Inlines.Add(new Run(" b"));
		outer.Inlines.Add(span);
		var win = Show(outer, inner);
		try
		{
			var (before, after) = outer.InlineObjectEdges(0);
			Assert.True(before > 0);
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.InRange(b.X, before - 1, before + 1);
			Assert.InRange(after - before, inner.Widget.Width - 1, inner.Widget.Width + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Inlines_Add_of_a_UIElement_wraps_it_in_a_container() => Run(() =>
	{
		var inner = new TextBlock { Text = "XYZ" };
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		outer.Inlines.Add(inner);
		var container = Assert.IsType<InlineUIContainer>(System.Linq.Enumerable.Last(outer.Inlines));
		Assert.Same(inner, container.Child);
	});

	[Fact]
	public void The_logical_tree_runs_through_the_container_and_data_context_inherits() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = new TextBlock { DataContext = "ctx", FontSize = 20 };
		var container = new InlineUIContainer(inner);
		outer.Inlines.Add(container);
		var win = Show(outer, inner);
		try
		{
			Assert.Same(container, LogicalTreeHelper.GetParent(inner));
			Assert.Same(outer, LogicalTreeHelper.GetParent(container));
			Assert.Equal("ctx", inner.DataContext);
			Assert.Equal(20, inner.FontSize);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Clearing_the_container_Child_frees_the_reserved_space() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new TextBlock { Text = "XYZ" };
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		var container = new InlineUIContainer(inner);
		outer.Inlines.Add(container);
		outer.Inlines.Add(new Run(" b"));
		var win = Show(outer, inner);
		try
		{
			Assert.True(outer.InlineObjectEdges(0).After - outer.InlineObjectEdges(0).Before > 1);
			container.Child = null;
			GtkState.Require(() => outer.InlineObjectEdges(0).After - outer.InlineObjectEdges(0).Before < 1,
				"space freed", outer, inner);
			Assert.Null(inner.Widget.Parent);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Parent_of_a_hosted_child_is_its_container_as_in_the_logical_tree() => Run(() =>
	{
		var inner = new TextBlock { Text = "XYZ" };
		var container = new InlineUIContainer(inner);
		var outer = new TextBlock();
		outer.Inlines.Add(container);
		Assert.Same(container, inner.Parent);
		Assert.Same(LogicalTreeHelper.GetParent(inner), inner.Parent);
	});
}
