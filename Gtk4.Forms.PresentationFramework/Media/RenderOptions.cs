using System;

namespace System.Windows.Media;

/// <summary>Bitmap scaling quality hint.</summary>
public enum BitmapScalingMode
{
	/// <summary>No preference.</summary>
	Unspecified = 0,
	/// <summary>Fast, low-quality scaling (same value as <see cref="Linear"/>).</summary>
	LowQuality = 1,
	/// <summary>High-quality scaling (same value as <see cref="Fant"/>).</summary>
	HighQuality = 2,
	/// <summary>Bilinear interpolation.</summary>
	Linear = 1,
	/// <summary>Fant resampling.</summary>
	Fant = 2,
	/// <summary>Nearest-neighbour sampling.</summary>
	NearestNeighbor = 3,
}

/// <summary>Edge antialiasing hint.</summary>
public enum EdgeMode
{
	/// <summary>No preference.</summary>
	Unspecified = 0,
	/// <summary>Edges are not antialiased.</summary>
	Aliased = 1,
}

/// <summary>Caching hint for rendered content.</summary>
public enum CachingHint
{
	/// <summary>No preference.</summary>
	Unspecified = 0,
	/// <summary>The content is cached.</summary>
	Cache = 1,
}

/// <summary>ClearType text rendering hint.</summary>
public enum ClearTypeHint
{
	/// <summary>Use ClearType when the surface allows it.</summary>
	Auto = 0,
	/// <summary>Force ClearType.</summary>
	Enabled = 1,
}

/// <summary>
/// Attached rendering hints. GTK renders through Cairo, which has no per-element equivalents, so the values are
/// stored and readable but do not change drawing (a documented gap); the object model matches WPF.
/// </summary>
public static class RenderOptions
{
	/// <summary>Identifies the <c>BitmapScalingMode</c> attached property.</summary>
	public static readonly DependencyProperty BitmapScalingModeProperty = DependencyProperty.RegisterAttached(
		"BitmapScalingMode", typeof(BitmapScalingMode), typeof(RenderOptions),
		new UIPropertyMetadata(BitmapScalingMode.Unspecified), v => v != null && Enum.IsDefined(typeof(BitmapScalingMode), v));

	/// <summary>Identifies the <c>EdgeMode</c> attached property.</summary>
	public static readonly DependencyProperty EdgeModeProperty = DependencyProperty.RegisterAttached(
		"EdgeMode", typeof(EdgeMode), typeof(RenderOptions),
		new UIPropertyMetadata(EdgeMode.Unspecified), v => v != null && Enum.IsDefined(typeof(EdgeMode), v));

	/// <summary>Identifies the <c>CachingHint</c> attached property.</summary>
	public static readonly DependencyProperty CachingHintProperty = DependencyProperty.RegisterAttached(
		"CachingHint", typeof(CachingHint), typeof(RenderOptions),
		new UIPropertyMetadata(CachingHint.Unspecified), v => v != null && Enum.IsDefined(typeof(CachingHint), v));

	/// <summary>Identifies the <c>CacheInvalidationThresholdMinimum</c> attached property.</summary>
	public static readonly DependencyProperty CacheInvalidationThresholdMinimumProperty =
		DependencyProperty.RegisterAttached("CacheInvalidationThresholdMinimum", typeof(double), typeof(RenderOptions),
			new UIPropertyMetadata(0.707107));

	/// <summary>Identifies the <c>CacheInvalidationThresholdMaximum</c> attached property.</summary>
	public static readonly DependencyProperty CacheInvalidationThresholdMaximumProperty =
		DependencyProperty.RegisterAttached("CacheInvalidationThresholdMaximum", typeof(double), typeof(RenderOptions),
			new UIPropertyMetadata(1.414214));

	/// <summary>Identifies the <c>ClearTypeHint</c> attached property.</summary>
	public static readonly DependencyProperty ClearTypeHintProperty = DependencyProperty.RegisterAttached(
		"ClearTypeHint", typeof(ClearTypeHint), typeof(RenderOptions),
		new UIPropertyMetadata(ClearTypeHint.Auto), v => v != null && Enum.IsDefined(typeof(ClearTypeHint), v));

	/// <summary>Sets the bitmap scaling mode of <paramref name="target"/>.</summary>
	public static void SetBitmapScalingMode(DependencyObject target, BitmapScalingMode bitmapScalingMode) =>
		Required(target).SetValue(BitmapScalingModeProperty, bitmapScalingMode);

	/// <summary>Gets the bitmap scaling mode of <paramref name="target"/>.</summary>
	public static BitmapScalingMode GetBitmapScalingMode(DependencyObject target) =>
		(BitmapScalingMode)Required(target).GetValue(BitmapScalingModeProperty)!;

	/// <summary>Sets the edge mode of <paramref name="target"/>.</summary>
	public static void SetEdgeMode(DependencyObject target, EdgeMode edgeMode) =>
		Required(target).SetValue(EdgeModeProperty, edgeMode);

	/// <summary>Gets the edge mode of <paramref name="target"/>.</summary>
	public static EdgeMode GetEdgeMode(DependencyObject target) =>
		(EdgeMode)Required(target).GetValue(EdgeModeProperty)!;

	/// <summary>Sets the caching hint of <paramref name="target"/>.</summary>
	public static void SetCachingHint(DependencyObject target, CachingHint cachingHint) =>
		Required(target).SetValue(CachingHintProperty, cachingHint);

	/// <summary>Gets the caching hint of <paramref name="target"/>.</summary>
	public static CachingHint GetCachingHint(DependencyObject target) =>
		(CachingHint)Required(target).GetValue(CachingHintProperty)!;

	/// <summary>Sets the minimum cache invalidation threshold of <paramref name="target"/>.</summary>
	public static void SetCacheInvalidationThresholdMinimum(DependencyObject target, double minimum) =>
		Required(target).SetValue(CacheInvalidationThresholdMinimumProperty, minimum);

	/// <summary>Gets the minimum cache invalidation threshold of <paramref name="target"/>.</summary>
	public static double GetCacheInvalidationThresholdMinimum(DependencyObject target) =>
		(double)Required(target).GetValue(CacheInvalidationThresholdMinimumProperty)!;

	/// <summary>Sets the maximum cache invalidation threshold of <paramref name="target"/>.</summary>
	public static void SetCacheInvalidationThresholdMaximum(DependencyObject target, double maximum) =>
		Required(target).SetValue(CacheInvalidationThresholdMaximumProperty, maximum);

	/// <summary>Gets the maximum cache invalidation threshold of <paramref name="target"/>.</summary>
	public static double GetCacheInvalidationThresholdMaximum(DependencyObject target) =>
		(double)Required(target).GetValue(CacheInvalidationThresholdMaximumProperty)!;

	/// <summary>Sets the ClearType hint of <paramref name="target"/>.</summary>
	public static void SetClearTypeHint(DependencyObject target, ClearTypeHint clearTypeHint) =>
		Required(target).SetValue(ClearTypeHintProperty, clearTypeHint);

	/// <summary>Gets the ClearType hint of <paramref name="target"/>.</summary>
	public static ClearTypeHint GetClearTypeHint(DependencyObject target) =>
		(ClearTypeHint)Required(target).GetValue(ClearTypeHintProperty)!;

	static DependencyObject Required(DependencyObject target) =>
		target ?? throw new ArgumentNullException(nameof(target));
}
