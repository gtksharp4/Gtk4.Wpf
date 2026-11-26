using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Gtk4.PresentationFramework.Tests.Animation;

/// <summary>Pure (no GTK widget) element with animatable properties of several types.</summary>
sealed class AnimTarget : FrameworkElement
{
	public static readonly DependencyProperty AmountProperty = DependencyProperty.Register(
		"AmountX", typeof(double), typeof(AnimTarget), new PropertyMetadata(10.0));

	public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
		"FillX", typeof(Brush), typeof(AnimTarget), new PropertyMetadata(null, OnFillChanged));

	public int FillCallbacks;

	public double Amount
	{
		get => (double)GetValue(AmountProperty)!;
		set => SetValue(AmountProperty, value);
	}

	static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((AnimTarget)d).FillCallbacks++;

	protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
}

/// <summary>A user animation deriving from the typed base, as the WPF samples do.</summary>
sealed class SquareAnimation : DoubleAnimationBase
{
	public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
		"Scale", typeof(double), typeof(SquareAnimation), new PropertyMetadata(100.0));

	public double Scale
	{
		get => (double)GetValue(ScaleProperty)!;
		set => SetValue(ScaleProperty, value);
	}

	protected override double GetCurrentValueCore(double origin, double destination, AnimationClock clock) =>
		origin + Scale * clock.CurrentProgress!.Value * clock.CurrentProgress!.Value;

	protected override Freezable CreateInstanceCore() => new SquareAnimation();
}

static class AnimationTestHelpers
{
	public static DoubleAnimation Linear(double from, double to, double seconds = 1) =>
		new DoubleAnimation { From = from, To = to, Duration = TimeSpan.FromSeconds(seconds) };

	public static void Tick(Clock clock, double seconds) => clock.Tick(TimeSpan.FromSeconds(seconds));
}
