using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Real-GTK: an inline container added after the TextBlock's widget exists is hosted and placed.</summary>
public class TextBlockInlineLateAddGtkTests : WpfGtkTestBase
{

	static void AssertHostedLate(bool direct)
	{
		using var gtk = new GtkDispatcherScope();
		var outer = new TextBlock();
		outer.Inlines.Add(new Run("before "));
		var after = new TextBlock { Text = "next" };
		var content = direct ? (UIElement)outer : new StackPanel { Children = { outer, after } };
		var win = new WpfWindow { Width = 600, Height = 400, Content = content };
		win.Show();
		GtkState.Require(() => outer.Widget.IsMapped && outer.Widget.Width > 0, "text mapped", outer);
		var button = new Button { Content = "late", Width = 50, Height = 20 };
		try
		{
			outer.Inlines.Add(new InlineUIContainer(button));
			GtkState.Require(() => button.Widget.IsMapped && button.Widget.Width > 0,
				"late button mapped", outer, button);
			GtkPump.For(400);
			Assert.True(outer.HostsInlineObjects);
			Assert.IsType<Gtk.Overlay>(outer.Widget);
			Assert.Same(outer.Widget, button.Widget.Parent);
			var (before, edge) = outer.InlineObjectEdges(0);
			Assert.True(before > 1, $"the button sits after the first run, at {before}");
			Assert.True(button.Widget.MarginStart > 1, $"placed at {button.Widget.MarginStart}");
			Assert.True(edge > before);
		}
		finally { win.Close(); }
	}

	[Fact]
	public void A_button_added_to_Inlines_after_Show_is_hosted_in_a_StackPanel() => Run(() => AssertHostedLate(false));

	[Fact]
	public void A_button_added_to_Inlines_after_Show_is_hosted_as_window_content() => Run(() => AssertHostedLate(true));
}
