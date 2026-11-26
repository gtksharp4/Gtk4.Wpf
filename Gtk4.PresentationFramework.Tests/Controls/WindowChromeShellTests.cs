using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Gtk4.PresentationFramework.Tests.XamlBacked;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class WindowChromeShellTests : WpfGtkTestBase
{
	[Fact]
	public void Defaults_match_WPF()
	{
		var c = new WindowChrome();
		Assert.IsAssignableFrom<Freezable>(c);
		Assert.Equal(0.0, (double)WindowChrome.CaptionHeightProperty.DefaultMetadata.DefaultValue!);
		Assert.Equal(default(Thickness), WindowChrome.ResizeBorderThicknessProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(default(Thickness), WindowChrome.GlassFrameThicknessProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(new CornerRadius(0), WindowChrome.CornerRadiusProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(23.0, c.CaptionHeight);
		Assert.Equal(new Thickness(4), c.ResizeBorderThickness);
		Assert.Equal(new Thickness(4, 27, 4, 4), c.GlassFrameThickness);
		Assert.Equal(new CornerRadius(8), c.CornerRadius);
		Assert.True(c.UseAeroCaptionButtons);
		Assert.Equal(NonClientFrameEdges.None, c.NonClientFrameEdges);
		Assert.Equal(new Thickness(-1), WindowChrome.GlassFrameCompleteThickness);
		var meta = (FrameworkPropertyMetadata)WindowChrome.IsHitTestVisibleInChromeProperty.DefaultMetadata;
		Assert.True(meta.Inherits);
	}

	[Fact]
	public void Flags_and_directions_have_the_WPF_values()
	{
		Assert.Equal(15, (int)(NonClientFrameEdges.Left | NonClientFrameEdges.Top | NonClientFrameEdges.Right
			| NonClientFrameEdges.Bottom));
		Assert.NotNull(typeof(NonClientFrameEdges).GetCustomAttributes(typeof(FlagsAttribute), false));
		Assert.Equal(8, (int)ResizeGripDirection.Left);
		Assert.Equal(0, (int)ResizeGripDirection.None);
	}

	[Fact]
	public void Invalid_values_are_rejected()
	{
		var c = new WindowChrome();
		Assert.Throws<ArgumentException>(() => c.CaptionHeight = -1);
		Assert.Throws<ArgumentException>(() => c.CaptionHeight = double.NaN);
		Assert.Throws<ArgumentException>(() => c.ResizeBorderThickness = new Thickness(-2));
		Assert.Throws<ArgumentException>(() => c.CornerRadius = new CornerRadius(-1));
		c.GlassFrameThickness = WindowChrome.GlassFrameCompleteThickness;
		Assert.Throws<ArgumentException>(() => c.GlassFrameThickness = new Thickness(-3));
	}

	[Fact]
	public void Attached_properties_round_trip()
	{
		var window = new Window();
		var button = new Button();
		Assert.Null(WindowChrome.GetWindowChrome(window));
		var chrome = new WindowChrome { CaptionHeight = 20 };
		WindowChrome.SetWindowChrome(window, chrome);
		Assert.Same(chrome, WindowChrome.GetWindowChrome(window));
		Assert.False(WindowChrome.GetIsHitTestVisibleInChrome(button));
		WindowChrome.SetIsHitTestVisibleInChrome(button, true);
		Assert.True(WindowChrome.GetIsHitTestVisibleInChrome(button));
		Assert.Equal(ResizeGripDirection.None, WindowChrome.GetResizeGripDirection(button));
		WindowChrome.SetResizeGripDirection(button, ResizeGripDirection.BottomRight);
		Assert.Equal(ResizeGripDirection.BottomRight, WindowChrome.GetResizeGripDirection(button));
		Assert.Throws<ArgumentNullException>(() => WindowChrome.GetWindowChrome(null!));
	}

	[Fact]
	public void Markup_resolves_the_Shell_namespace_and_attached_properties()
	{
		var w = new ChromeWindow();
		Assert.Equal(30.0, WindowChrome.GetWindowChrome(w)!.CaptionHeight);
		Assert.True(WindowChrome.GetIsHitTestVisibleInChrome(w.CloseButton));
	}

	[Fact]
	public void A_caption_area_makes_the_GTK_window_undecorated()
	{
		Run(() =>
		{
			var w = new Window();
			var native = (Gtk.Window)w.Widget;
			Assert.True(native.Decorated);
			var chrome = new WindowChrome { CaptionHeight = 0 };
			WindowChrome.SetWindowChrome(w, chrome);
			Assert.True(native.Decorated);
			chrome.CaptionHeight = 30;
			Assert.False(native.Decorated);
			chrome.CaptionHeight = 0;
			Assert.True(native.Decorated);
			WindowChrome.SetWindowChrome(w, null);
			Assert.True(native.Decorated);
		});
	}

	[Fact]
	public void A_child_that_sets_false_under_a_true_panel_is_not_hit_test_visible_in_chrome()
	{
		Run(() =>
		{
			var button = new Button { Width = 80, Height = 40 };
			WindowChrome.SetIsHitTestVisibleInChrome(button, false);
			var panel = new StackPanel();
			WindowChrome.SetIsHitTestVisibleInChrome(panel, true);
			panel.Children.Add(button);
			var w = new Window { Content = panel, Width = 300, Height = 200 };
			WindowChrome.SetWindowChrome(w, new WindowChrome { CaptionHeight = 100 });
			w.Show();
			Assert.True(GtkPump.Until(() => button.Widget.IsMapped && button.Widget.Width > 0));
			Assert.True(button.Widget.ComputePoint(w.Widget, new Graphene.Point { X = 5, Y = 5 }, out var at));

			Assert.False(w.IsHitTestVisibleInChromeAt(at.X, at.Y));
		});
	}
}
