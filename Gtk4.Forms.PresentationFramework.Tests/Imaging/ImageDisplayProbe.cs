using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using WpfImage = System.Windows.Controls.Image;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Imaging;

/// <summary>Shows <c>Window > ScrollViewer > StackPanel > Image</c> like the imaging samples and measures it.</summary>
sealed class ImageDisplayProbe : IDisposable
{
	public readonly WpfWindow Window;
	public readonly WpfImage Image;
	readonly GtkDispatcherScope _gtk = new GtkDispatcherScope();

	public ImageDisplayProbe(ImageSource? source, Stretch stretch = Stretch.None, bool scroll = true,
		Action<WpfImage>? configure = null)
	{
		Image = new WpfImage { Source = source, Stretch = stretch, Margin = new Thickness(20) };
		configure?.Invoke(Image);
		var panel = new StackPanel
		{
			Orientation = Orientation.Vertical,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		panel.Children.Add(Image);
		UIElement content = panel;
		if (scroll)
			content = new ScrollViewer { Content = panel };
		Window = new WpfWindow { Title = "Imaging probe", Width = 600, Height = 500, Content = content };
		Window.Show();
		GtkState.Require(() => Image.Widget.IsMapped && Image.Widget.Width > 0 || Image.Source == null,
			"image mapped", Image);
		GtkPump.For(150);
	}

	public WidgetPixels ImagePixels() => WidgetPixels.Capture(Image.Widget);

	public WidgetPixels WindowPixels() => WidgetPixels.Capture(Window.Widget);

	public string Dump()
	{
		var pic = (Gtk.Picture)Image.Widget;
		var p = pic.Paintable;
		var sb = new System.Text.StringBuilder();
		if (Image.Source is BitmapSource b)
			sb.Append($"src={b.GetType().Name} {b.PixelWidth}x{b.PixelHeight} {b.Format} dpi={b.DpiX}; ");
		sb.Append($"paintable={p?.GetType().Name} {p?.IntrinsicWidth}x{p?.IntrinsicHeight}; ");
		pic.Measure(Gtk.Orientation.Horizontal, -1, out int minW, out int natW, out _, out _);
		pic.Measure(Gtk.Orientation.Vertical, -1, out int minH, out int natH, out _, out _);
		sb.Append($"pic measure w={minW}/{natW} h={minH}/{natH}; pic alloc={pic.Width}x{pic.Height}; ");
		sb.Append($"desired={Image.DesiredSize} actual={Image.ActualWidth}x{Image.ActualHeight}; ");
		sb.Append($"nonTransparent img={ImagePixels().NonTransparent()} win={WindowPixels().NonTransparent()}");
		return sb.ToString();
	}

	public void Dispose()
	{
		Window.Close();
		_gtk.Dispose();
	}
}
