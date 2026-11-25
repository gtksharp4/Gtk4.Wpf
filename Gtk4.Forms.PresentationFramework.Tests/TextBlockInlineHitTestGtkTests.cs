using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Real-GTK: hosted inline children must not shadow the label's own picking (links).</summary>
public class TextBlockInlineHitTestGtkTests : WpfGtkTestBase
{
	static bool IsOrInside(Gtk.Widget? w, Gtk.Widget ancestor)
	{
		for (; w != null; w = w.Parent)
			if (w.Handle == ancestor.Handle) return true;
		return false;
	}

	[Fact]
	public void A_hyperlink_next_to_an_inline_child_is_still_picked_through_the_label() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var button = new Button { Content = "B" };
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("before "));
		var link = new Hyperlink { NavigateUri = new System.Uri("http://x") };
		link.Inlines.Add(new Run("link"));
		outer.Inlines.Add(link);
		outer.Inlines.Add(new InlineUIContainer(button));
		outer.Inlines.Add(new Run(" after"));
		var win = new WpfWindow { Width = 600, Height = 400, Content = new StackPanel { Children = { outer } } };
		win.Show();
		try
		{
			GtkState.Require(
				() => button.Widget.IsMapped && button.Widget.ComputeBounds(button.Widget, out var bb) && bb.Width > 0,
				"button mapped", outer, button);
			GtkPump.For(400);
			var overlay = (Gtk.Overlay)outer.Widget;
			var label = (Gtk.Label)overlay.Child!;
			Assert.True(label.ComputeBounds(overlay, out var origin));
			label.GetLayoutOffsets(out int dx, out int dy);
			int index = Encoding.UTF8.GetByteCount((label.Text ?? "").Substring(0, label.Text!.IndexOf("link")));
			var pos = label.Layout.IndexToPos(index + 1);
			double x = origin.X + dx + pos.X / 1024.0 + 2, y = origin.Y + dy + (pos.Y + pos.Height / 2) / 1024.0;
			var tp = overlay.Pick(x, y, Gtk.PickFlags.Default);
			Assert.True(IsOrInside(tp, label), $"text pick reaches the label: {tp?.GetType().Name} at {x},{y}");

			Assert.True(button.Widget.ComputeBounds(overlay, out var b) && b.Width > 0 && b.Height > 0);
			var hit = overlay.Pick(b.X + b.Width / 2, b.Y + b.Height / 2, Gtk.PickFlags.Default);
			Assert.True(IsOrInside(hit, button.Widget), "button pick reaches the button");
		}
		finally { win.Close(); }
	});
}
