using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>A TextBlock or Image in a fixed Grid track is clipped to it, not allowed to widen the track.</summary>
public class SlotCapGtkTests : WpfGtkTestBase
{
	const string Long = "A very long line of text that does not wrap and is wider than one hundred pixels by far";

	static (Grid Grid, Border Sibling) Layout(UIElement child, double fixedWidth = 100)
	{
		var grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(fixedWidth) });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		var sibling = new Border { Background = Brushes.Red };
		grid.Children.Add(child);
		grid.Children.Add(sibling);
		Grid.SetColumn(sibling, 1);
		return (grid, sibling);
	}

	static void Check(Grid grid, Border sibling, double fixedWidth = 100)
	{
		// Not exact: without a window manager GTK draws a client-side border inside the surface.
		Assert.Equal(grid.ActualWidth - fixedWidth, sibling.ActualWidth, 1);
	}

	[Fact]
	public void Unwrapped_TextBlock_in_a_100px_column_leaves_the_column_at_100px() => Run(() =>
	{
		var text = new TextBlock { Text = Long };
		var (grid, sibling) = Layout(text);
		var window = new WpfWindow { Width = 500, Height = 200, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Check(grid, sibling);
			Assert.InRange(text.ActualWidth, 99, 101);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Wrapping_TextBlock_in_a_100px_column_wraps_within_the_track() => Run(() =>
	{
		var text = new TextBlock { Text = Long, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top };
		var (grid, sibling) = Layout(text);
		var window = new WpfWindow { Width = 500, Height = 400, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Check(grid, sibling);
			Assert.InRange(text.ActualWidth, 99, 101);
			Assert.True(text.ActualHeight > 40, $"wrapped height {text.ActualHeight}");
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Image_wider_than_its_100px_column_leaves_the_column_at_100px() => Run(() =>
	{
		var source = BitmapSource.Create(400, 50, 96, 96, PixelFormats.Bgr24, null, new byte[400 * 50 * 3], 400 * 3);
		var image = new Image { Source = source, Stretch = Stretch.None };
		var (grid, sibling) = Layout(image);
		var window = new WpfWindow { Width = 600, Height = 200, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Check(grid, sibling);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Changing_a_ColumnDefinition_width_after_show_recaps_a_measured_TextBlock_and_Shape() => Run(() =>
	{
		var text = new TextBlock { Text = Long };
		var line = new Line { X2 = 400, Y2 = 10, Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top };
		var grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		grid.Children.Add(text);
		grid.Children.Add(line);
		Grid.SetColumn(line, 1);
		var window = new WpfWindow { Width = 600, Height = 200, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => text.ActualWidth > 0 && line.ActualWidth > 0, "layout", text);
			GtkPump.For(150);
			Assert.InRange(text.ActualWidth, 99, 101);
			Assert.InRange(line.ActualWidth, 99, 101);
			grid.ColumnDefinitions[0].Width = new GridLength(50);
			grid.ColumnDefinitions[1].Width = new GridLength(50);
			Assert.True(GtkPump.Until(() => text.ActualWidth <= 51 && line.ActualWidth <= 51),
				$"text {text.ActualWidth}, line {line.ActualWidth}");
		}
		finally { window.Close(); }
	});

	[Fact]
	public void TextBlock_and_Image_in_an_explicitly_sized_ScrollViewer_keep_their_full_extent() => Run(() =>
	{
		var text = new TextBlock { Text = Long + Long };
		var source = BitmapSource.Create(900, 400, 96, 96, PixelFormats.Bgr24, null, new byte[900 * 400 * 3], 900 * 3);
		var image = new Image { Source = source, Stretch = Stretch.None };
		var textViewer = new ScrollViewer
		{
			Width = 300, Height = 100, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, Content = text,
		};
		var imageViewer = new ScrollViewer
		{
			Width = 300, Height = 100, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = image,
		};
		var panel = new StackPanel { Children = { textViewer, imageViewer } };
		var window = new WpfWindow { Width = 600, Height = 400, Content = panel };
		try
		{
			window.Show();
			GtkState.Require(() => text.ActualWidth > 0 && image.ActualWidth > 0, "layout", text);
			GtkPump.For(150);
			Assert.True(text.ActualWidth > 400, $"text {text.ActualWidth}");
			Assert.True(image.ActualWidth > 400, $"image {image.ActualWidth}");
			Assert.True(image.ActualHeight > 150, $"image {image.ActualHeight}");
		}
		finally { window.Close(); }
	});

	[Fact]
	public void TextBlock_Padding_does_not_widen_a_fixed_track() => Run(() =>
	{
		var text = new TextBlock { Text = Long, Padding = new Thickness(10) };
		var (grid, sibling) = Layout(text);
		var window = new WpfWindow { Width = 500, Height = 200, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Check(grid, sibling);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void TextBlock_label_is_clipped_only_while_capped() => Run(() =>
	{
		var capped = new TextBlock { Text = Long };
		var free = new TextBlock { Text = "Step 1:", FontStyle = FontStyles.Italic };
		var (grid, sibling) = Layout(capped);
		var panel = new StackPanel { Children = { grid, free } };
		var window = new WpfWindow { Width = 500, Height = 200, Content = panel };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0 && free.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Assert.Equal(Gtk.Overflow.Hidden, capped.Widget.Overflow);
			Assert.Equal(Gtk.Overflow.Visible, free.Widget.Overflow);
		}
		finally { window.Close(); }
	});
}
