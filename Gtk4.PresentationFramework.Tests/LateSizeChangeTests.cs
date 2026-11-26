using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A Width/Height set after the GTK widget exists (for example a late binding) reaches the widget.</summary>
public class LateSizeChangeTests : WpfGtkTestBase
{
	[Fact]
	public void Width_and_Height_set_after_show_resize_the_element() => Run(() =>
	{
		var canvas = new Canvas { Width = 50, Height = 20 };
		var stack = new StackPanel();
		stack.Children.Add(canvas);
		var win = new WpfWindow { Content = stack, Width = 400, Height = 400 };
		try
		{
			win.Show();
			GtkPump.For(300);
			canvas.Width = 123;
			canvas.Height = 45;
			GtkPump.For(300);
			Assert.Equal(123, canvas.ActualWidth);
			Assert.Equal(45, canvas.ActualHeight);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_size_bound_to_another_element_follows_it_after_show() => Run(() =>
	{
		var source = new Canvas { Width = 90, Height = 60, Name = "src" };
		var mirror = new Canvas();
		var stack = new StackPanel();
		stack.Children.Add(source);
		stack.Children.Add(mirror);
		var win = new WpfWindow { Content = stack, Width = 400, Height = 400 };
		NameScope.SetNameScope(win, new NameScope());
		win.RegisterName("src", source);
		try
		{
			win.Show();
			GtkPump.For(300);
			mirror.SetBinding(FrameworkElement.WidthProperty,
				new Binding { Path = new PropertyPath("(Canvas.ActualWidth)"), ElementName = "src" });
			mirror.SetBinding(FrameworkElement.HeightProperty,
				new Binding { Path = new PropertyPath("(Canvas.ActualHeight)"), ElementName = "src" });
			GtkPump.For(300);
			Assert.Equal(90, mirror.ActualWidth);
			Assert.Equal(60, mirror.ActualHeight);
		}
		finally { win.Close(); }
	});
}
