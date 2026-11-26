using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Window.DragMove and AllowsTransparency.</summary>
public class WindowTailTests : WpfGtkTestBase
{
	[Fact]
	public void DragMoveThrowsUnlessLeftButtonDown() => Run(() =>
	{
		var w = new Window();
		var ex = Assert.Throws<InvalidOperationException>(() => w.DragMove());
		Assert.Contains("DragMove", ex.Message);
	});

	[Fact]
	public void DragMoveForwardsTheRecordedPress() => Run(() =>
	{
		var w = new Window();
		(int button, double x, double y, uint time)? seen = null;
		Window.BeginMoveOverride = (_, b, x, y, t) => seen = (b, x, y, t);
		try
		{
			w.RecordPress(null, 10, 20, 777);
			w.DragMove();
		}
		finally { Window.BeginMoveOverride = null; }
		Assert.NotNull(seen);
		Assert.Equal(1, seen!.Value.button);
		Assert.Equal(777u, seen.Value.time);
		Assert.Throws<InvalidOperationException>(() => w.DragMove());
	});

	[Fact]
	public void AllowsTransparencyDefaultsFalseAndEmitsTransparentCss() => Run(() =>
	{
		var w = new Window();
		_ = w.Widget;
		Assert.False(w.AllowsTransparency);
		w.WindowStyle = WindowStyle.None;
		w.AllowsTransparency = true;
		Assert.Contains("transparent", w.AppearanceCss);
		w.AllowsTransparency = false;
		Assert.DoesNotContain("transparent", w.AppearanceCss);
	});

	[Fact]
	public void ShowingTransparentWindowWithBorderThrows() => Run(() =>
	{
		var w = new Window { AllowsTransparency = true };
		Assert.Throws<InvalidOperationException>(() => w.Show());
	});

	[Fact]
	public void AllowsTransparencyCannotChangeAfterShow() => Run(() =>
	{
		var w = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true };
		w.Show();
		try
		{
			Assert.Throws<InvalidOperationException>(() => w.AllowsTransparency = false);
			Assert.True(w.AllowsTransparency);
		}
		finally { w.Hide(); }
	});
}
