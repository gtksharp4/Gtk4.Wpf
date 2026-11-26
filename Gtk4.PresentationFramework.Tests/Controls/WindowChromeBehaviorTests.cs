using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class WindowChromeBehaviorTests : WpfGtkTestBase
{
	[Fact]
	public void A_bare_WindowChrome_undecorates_the_window() => Run(() =>
	{
		var w = new Window();
		WindowChrome.SetWindowChrome(w, new WindowChrome());
		var native = (Gtk.Window)w.Widget;
		Assert.True(w.HasShellCaption);
		Assert.False(native.Decorated);
	});

	[Fact]
	public void Changing_WindowStyle_keeps_a_chrome_window_undecorated() => Run(() =>
	{
		var w = new Window();
		WindowChrome.SetWindowChrome(w, new WindowChrome { CaptionHeight = 30 });
		var native = (Gtk.Window)w.Widget;
		Assert.False(native.Decorated);

		w.WindowStyle = WindowStyle.ToolWindow;

		Assert.False(native.Decorated);
	});

	static (Window, Button) ChromeWindow(bool withChrome)
	{
		var button = new Button { Width = 40, Height = 20, HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top };
		var w = new Window { Width = 300, Height = 200, Content = button };
		if (withChrome) WindowChrome.SetWindowChrome(w, new WindowChrome { CaptionHeight = 30 });
		return (w, button);
	}

	static bool Press(Window w, double x, double y)
	{
		var moved = false;
		Window.BeginMoveOverride = (_, _, _, _, _) => moved = true;
		try
		{
			w.RecordPress(null, x, y, 1);
			w.OnShellPressed(x, y);
		}
		finally { Window.BeginMoveOverride = null; }
		return moved;
	}

	[Fact]
	public void A_press_in_the_caption_strip_starts_a_move() => Run(() =>
	{
		var (w, _) = ChromeWindow(true);
		w.Show();
		try
		{
			GtkPump.Until(() => w.ActualWidth > 0);
			Assert.True(Press(w, 250, 5));
		}
		finally { w.Hide(); }
	});

	[Fact]
	public void A_press_below_the_caption_strip_does_not_move() => Run(() =>
	{
		var (w, _) = ChromeWindow(true);
		w.Show();
		try
		{
			GtkPump.Until(() => w.ActualWidth > 0);
			Assert.False(Press(w, 250, 100));
		}
		finally { w.Hide(); }
	});

	[Fact]
	public void A_child_that_is_hit_test_visible_in_chrome_keeps_its_press() => Run(() =>
	{
		var (w, button) = ChromeWindow(true);
		WindowChrome.SetIsHitTestVisibleInChrome(button, true);
		w.Show();
		try
		{
			GtkPump.Until(() => button.ActualWidth > 0);
			Assert.False(Press(w, 5, 5));
		}
		finally { w.Hide(); }
	});

	[Fact]
	public void A_window_without_chrome_never_moves() => Run(() =>
	{
		var (w, _) = ChromeWindow(false);
		w.Show();
		try
		{
			GtkPump.Until(() => w.ActualWidth > 0);
			Assert.False(Press(w, 250, 5));
		}
		finally { w.Hide(); }
	});
}
