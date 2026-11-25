using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Animation;

/// <summary>The KeyFrameAnimation sample's (Canvas.Left) animation against WPF's key-frame and spline math.</summary>
public class KeyFrameCanvasLeftTests : WpfGtkTestBase
{
	const double X1 = 0.25, Y1 = 0.5, X2 = 0.75, Y2 = 1;

	// WPF's KeySpline: the y of the cubic Bezier (0,0) (x1,y1) (x2,y2) (1,1) at the parameter where x equals progress.
	static double Spline(double progress)
	{
		double lo = 0, hi = 1;
		for (int i = 0; i < 60; i++)
		{
			double t = (lo + hi) / 2;
			double x = 3 * (1 - t) * (1 - t) * t * X1 + 3 * (1 - t) * t * t * X2 + t * t * t;
			if (x < progress) lo = t; else hi = t;
		}
		double u = (lo + hi) / 2;
		return 3 * (1 - u) * (1 - u) * u * Y1 + 3 * (1 - u) * u * u * Y2 + u * u * u;
	}

	static double Lerp(double a, double b, double p) => a + (b - a) * p;

	// Expected Canvas.Left at clock time t within one 15 s iteration; the base value is 10.
	static double Expected(double t)
	{
		if (t <= 5) return Lerp(10, 500, t / 5);
		if (t <= 10) return Lerp(500, 200, Spline((t - 5) / 5));
		if (t <= 12) return Lerp(200, 290, (t - 10) / 2);
		if (t <= 13.5) return Lerp(290, 300, (t - 12) / 1.5);
		return Lerp(300, 0, Spline((t - 13.5) / 1.5));
	}

	static DoubleKeyFrame Spl(double value, double seconds) => new SplineDoubleKeyFrame(
		value, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds)), new KeySpline(X1, Y1, X2, Y2));

	static DoubleKeyFrame Lin(double value, double seconds) =>
		new LinearDoubleKeyFrame(value, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(seconds)));

	static (Storyboard Board, Rectangle Rect, Canvas Canvas) Build()
	{
		var rect = new Rectangle { Width = 100, Height = 100 };
		Canvas.SetLeft(rect, 10);
		Canvas.SetTop(rect, 100);
		var canvas = new Canvas { Width = 600, Height = 300 };
		canvas.Children.Add(rect);
		var anim = new DoubleAnimationUsingKeyFrames();
		anim.KeyFrames.Add(Lin(500, 5));
		anim.KeyFrames.Add(Spl(200, 10));
		anim.KeyFrames.Add(Lin(290, 12));
		anim.KeyFrames.Add(Lin(300, 13.5));
		anim.KeyFrames.Add(Spl(0, 15));
		Storyboard.SetTarget(anim, rect);
		Storyboard.SetTargetProperty(anim, new PropertyPath("(Canvas.Left)"));
		var board = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
		board.Children.Add(anim);
		return (board, rect, canvas);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2.5)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7.5)]
	[InlineData(9)]
	[InlineData(11)]
	[InlineData(12.75)]
	[InlineData(14)]
	[InlineData(14.9)]
	public void Canvas_Left_follows_the_key_frames_and_splines(double seconds) => Run(() =>
	{
		var (board, rect, _) = Build();
		board.Begin(rect, true);
		board.Seek(rect, TimeSpan.FromSeconds(seconds), TimeSeekOrigin.BeginTime);
		Assert.Equal(Expected(seconds), Canvas.GetLeft(rect), 0.5);
	});

	[Theory]
	[InlineData(16, 1)]
	[InlineData(21, 6)]
	[InlineData(30.5, 0.5)]
	public void Forever_restarts_at_the_first_key_value_after_the_duration(double seconds, double wrapped) => Run(() =>
	{
		var (board, rect, _) = Build();
		board.Begin(rect, true);
		board.Seek(rect, TimeSpan.FromSeconds(seconds), TimeSeekOrigin.BeginTime);
		Assert.Equal(Expected(wrapped), Canvas.GetLeft(rect), 0.5);
	});

	[Fact]
	public void The_rectangle_widget_moves_with_Canvas_Left() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (board, rect, canvas) = Build();
		var win = new System.Windows.Window { Width = 700, Height = 400, Content = canvas };
		win.Show();
		GtkState.Require(() => rect.Widget.IsMapped && rect.Widget.Width > 0, "rect mapped", canvas, rect);
		double X() => rect.Widget.ComputeBounds(canvas.Widget, out var b) ? b.X : double.NaN;
		try
		{
			Assert.Equal(10, X(), 1.0);
			board.Begin(rect, true);
			foreach (double t in new[] { 2.5, 7.5, 12.75 })
			{
				board.Seek(rect, TimeSpan.FromSeconds(t), TimeSeekOrigin.BeginTime);
				GtkState.Require(() => Math.Abs(X() - Expected(t)) < 1.0, $"x at {t}s", canvas, rect);
			}
		}
		finally { win.Close(); }
	});
}
