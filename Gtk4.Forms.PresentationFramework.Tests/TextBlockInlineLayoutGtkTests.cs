using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Real-GTK: removal, wrapping and short-child baseline of inline containers.</summary>
public class TextBlockInlineLayoutGtkTests : WpfGtkTestBase
{
	static WpfWindow Show(TextBlock outer, FrameworkElement inner, bool direct = false)
	{
		var content = direct ? (UIElement)outer : new StackPanel { Children = { outer } };
		var win = new WpfWindow { Width = 600, Height = 400, Content = content };
		win.Show();
		GtkState.Require(() => inner.IsWidgetCreated && inner.Widget.IsMapped && inner.Widget.Width > 0,
			"inline child mapped", outer, inner);
		GtkPump.For(400);
		return win;
	}

	[Fact]
	public void Removing_the_container_through_Inlines_unhosts_the_child_and_frees_the_placeholder() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new Rectangle { Width = 60, Height = 10 };
		var container = new InlineUIContainer(inner);
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		outer.Inlines.Add(container);
		outer.Inlines.Add(new Run(" b"));
		var win = Show(outer, inner);
		try
		{
			var label = (Gtk.Label)((Gtk.Overlay)outer.Widget).Child!;
			label.Layout.GetPixelSize(out int before, out _);
			outer.Inlines.Remove(container);
			GtkPump.For(300);
			Assert.Null(inner.Widget.Parent);
			Assert.Null(inner.Parent);
			Assert.Null(LogicalTreeHelper.GetParent(inner));
			Assert.DoesNotContain('￼', label.Text ?? "");
			label.Layout.GetPixelSize(out int w, out _);
			Assert.True(w < before, $"text width {w} shrank from {before}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_child_in_wrapping_text_moves_to_a_later_line_before_the_following_run() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new Rectangle { Width = 40, Height = 10 };
		var outer = new TextBlock { TextWrapping = TextWrapping.Wrap, Width = 120 };
		outer.Inlines.Add(new Run("aaaa bbbb cccc "));
		outer.Inlines.Add(new InlineUIContainer(inner));
		outer.Inlines.Add(new Run(" dddd"));
		var win = Show(outer, inner, true);
		try
		{
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.True(b.Y >= outer.InlineFirstLineHeight - 1);
			var (before, after) = outer.InlineObjectEdges(0);
			Assert.InRange(b.X, before - 1, before + 1);
			Assert.True(after <= 120 + 1, $"box ends at {after}, inside the width");
			Assert.True(b.X + b.Width <= after + 1, $"child ends at {b.X + b.Width}, before the run at {after}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_child_shorter_than_the_ascent_sits_on_the_baseline() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new Rectangle { Width = 20, Height = 5 };
		var outer = new TextBlock { FontSize = 30 };
		outer.Inlines.Add(new Run("Ag "));
		outer.Inlines.Add(new InlineUIContainer(inner));
		outer.Inlines.Add(new Run(" Ag"));
		var win = Show(outer, inner);
		try
		{
			Assert.True(inner.Widget.ComputeBounds(outer.Widget, out var b));
			Assert.Equal(outer.InlineLabelBaseline, b.Y + b.Height, 1.0);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void An_unhosted_child_keeps_its_own_alignment() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var inner = new Rectangle { Width = 30, Height = 10, HorizontalAlignment = HorizontalAlignment.Right };
		var container = new InlineUIContainer(inner);
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		outer.Inlines.Add(container);
		var win = Show(outer, inner);
		try
		{
			Assert.Equal(Gtk.Align.Start, inner.Widget.Halign);
			outer.Inlines.Remove(container);
			GtkPump.For(200);
			Assert.Equal(Gtk.Align.End, inner.Widget.Halign);
			Assert.Equal(Gtk.Align.Center, inner.Widget.Valign);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_nested_padded_text_block_with_children_is_placed_without_doubling_its_padding() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var leaf = new Rectangle { Width = 10, Height = 10 };
		var nested = new TextBlock { Padding = new Thickness(10, 6, 0, 0) };
		nested.Inlines.Add(new Run("n "));
		nested.Inlines.Add(new InlineUIContainer(leaf));
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("a "));
		outer.Inlines.Add(new InlineUIContainer(nested));
		var win = Show(outer, nested);
		try
		{
			Assert.True(nested.HostsInlineObjects);
			var (before, _) = outer.InlineObjectEdges(0);
			Assert.InRange(nested.Widget.MarginStart, before - 1, before + 1);
		}
		finally { win.Close(); }
	});
}
