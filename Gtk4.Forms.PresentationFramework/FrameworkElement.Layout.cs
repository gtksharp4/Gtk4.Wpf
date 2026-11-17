using System;

namespace System.Windows;

/// <summary>Horizontal alignment of an element within its allocated slot.</summary>
public enum HorizontalAlignment
{
	/// <summary>Align to the left edge.</summary>
	Left,
	/// <summary>Center horizontally.</summary>
	Center,
	/// <summary>Align to the right edge.</summary>
	Right,
	/// <summary>Stretch to fill available width (default).</summary>
	Stretch
}

/// <summary>Vertical alignment of an element within its allocated slot.</summary>
public enum VerticalAlignment
{
	/// <summary>Align to the top edge.</summary>
	Top,
	/// <summary>Center vertically.</summary>
	Center,
	/// <summary>Align to the bottom edge.</summary>
	Bottom,
	/// <summary>Stretch to fill available height (default).</summary>
	Stretch
}

public abstract partial class FrameworkElement
{
	// ── MinWidth / MaxWidth / MinHeight / MaxHeight ──────────────────────────

	/// <summary>Minimum width constraint.</summary>
	public static readonly DependencyProperty MinWidthProperty =
		DependencyProperty.Register(nameof(MinWidth), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(0.0, inherits: false,
				affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Maximum width constraint (default = ∞).</summary>
	public static readonly DependencyProperty MaxWidthProperty =
		DependencyProperty.Register(nameof(MaxWidth), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(double.PositiveInfinity, inherits: false,
				affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Minimum height constraint.</summary>
	public static readonly DependencyProperty MinHeightProperty =
		DependencyProperty.Register(nameof(MinHeight), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(0.0, inherits: false,
				affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Maximum height constraint (default = ∞).</summary>
	public static readonly DependencyProperty MaxHeightProperty =
		DependencyProperty.Register(nameof(MaxHeight), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(double.PositiveInfinity, inherits: false,
				affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Horizontal alignment within the layout slot. Default is Stretch.</summary>
	public static readonly DependencyProperty HorizontalAlignmentProperty =
		DependencyProperty.Register(nameof(HorizontalAlignment), typeof(HorizontalAlignment),
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, inherits: false,
				affectsMeasure: false, affectsArrange: true, affectsRender: false));

	/// <summary>Vertical alignment within the layout slot. Default is Stretch.</summary>
	public static readonly DependencyProperty VerticalAlignmentProperty =
		DependencyProperty.Register(nameof(VerticalAlignment), typeof(VerticalAlignment),
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(VerticalAlignment.Stretch, inherits: false,
				affectsMeasure: false, affectsArrange: true, affectsRender: false));

	/// <summary>Gets or sets minimum width.</summary>
	public double MinWidth
	{
		get => (double)(GetValue(MinWidthProperty) ?? 0.0);
		set => SetValue(MinWidthProperty, value);
	}

	/// <summary>Gets or sets maximum width.</summary>
	public double MaxWidth
	{
		get => (double)(GetValue(MaxWidthProperty) ?? double.PositiveInfinity);
		set => SetValue(MaxWidthProperty, value);
	}

	/// <summary>Gets or sets minimum height.</summary>
	public double MinHeight
	{
		get => (double)(GetValue(MinHeightProperty) ?? 0.0);
		set => SetValue(MinHeightProperty, value);
	}

	/// <summary>Gets or sets maximum height.</summary>
	public double MaxHeight
	{
		get => (double)(GetValue(MaxHeightProperty) ?? double.PositiveInfinity);
		set => SetValue(MaxHeightProperty, value);
	}

	/// <summary>Gets or sets horizontal alignment.</summary>
	public HorizontalAlignment HorizontalAlignment
	{
		get => (HorizontalAlignment)(GetValue(HorizontalAlignmentProperty) ?? HorizontalAlignment.Stretch);
		set => SetValue(HorizontalAlignmentProperty, value);
	}

	/// <summary>Gets or sets vertical alignment.</summary>
	public VerticalAlignment VerticalAlignment
	{
		get => (VerticalAlignment)(GetValue(VerticalAlignmentProperty) ?? VerticalAlignment.Stretch);
		set => SetValue(VerticalAlignmentProperty, value);
	}

	// ── MeasureCore ─────────────────────────────────────────────────────────

	/// <summary>
	/// WPF MeasureCore: deflates available size by Margin, clamps by Width/Min/Max,
	/// calls MeasureOverride, then inflates by Margin. DesiredSize includes Margin.
	/// </summary>
	protected override Size MeasureCore(Size availableSize)
	{
		var margin = Margin;
		double mW = margin.Left + margin.Right;
		double mH = margin.Top + margin.Bottom;

		// Available space for content (after margin).
		double contentW = Math.Max(0, availableSize.Width - mW);
		double contentH = Math.Max(0, availableSize.Height - mH);

		// Apply explicit Width/Height overrides.
		double w = Width;
		double h = Height;
		double minW = MinWidth, maxW = MaxWidth, minH = MinHeight, maxH = MaxHeight;

		// If Width is explicit, pass it as the constraint.
		if (!double.IsNaN(w)) contentW = Math.Max(minW, Math.Min(w, maxW));
		else contentW = Math.Max(minW, Math.Min(contentW, maxW));

		if (!double.IsNaN(h)) contentH = Math.Max(minH, Math.Min(h, maxH));
		else contentH = Math.Max(minH, Math.Min(contentH, maxH));

		var constraint = new Size(contentW, contentH);
		var overrideSize = MeasureOverride(constraint);

		// Apply explicit override again to the result.
		double resultW = double.IsNaN(w) ? overrideSize.Width : w;
		double resultH = double.IsNaN(h) ? overrideSize.Height : h;

		// Clamp to min/max.
		resultW = Math.Max(minW, Math.Min(resultW, maxW));
		resultH = Math.Max(minH, Math.Min(resultH, maxH));

		// Add margin back for DesiredSize.
		return new Size(resultW + mW, resultH + mH);
	}

	/// <summary>Override to measure children. Default returns Size(0,0).</summary>
	protected virtual Size MeasureOverride(Size availableSize) => new Size(0, 0);

	// ── ArrangeCore ─────────────────────────────────────────────────────────

	/// <summary>
	/// WPF ArrangeCore: applies alignment and margin within the slot, then calls
	/// ArrangeOverride with the inner content rect.
	/// </summary>
	protected override void ArrangeCore(Rect finalRect)
	{
		var margin = Margin;
		double mW = margin.Left + margin.Right;
		double mH = margin.Top + margin.Bottom;

		// Inner slot after removing margin.
		double innerW = Math.Max(0, finalRect.Width - mW);
		double innerH = Math.Max(0, finalRect.Height - mH);

		// Content size: explicit Width/Height if set; else DesiredSize minus margin.
		double w = Width;
		double h = Height;
		double contentW = double.IsNaN(w) ? Math.Max(0, DesiredSize.Width - mW) : w;
		double contentH = double.IsNaN(h) ? Math.Max(0, DesiredSize.Height - mH) : h;

		// Clamp.
		contentW = Math.Max(MinWidth, Math.Min(contentW, MaxWidth));
		contentH = Math.Max(MinHeight, Math.Min(contentH, MaxHeight));

		// Alignment.
		var ha = HorizontalAlignment;
		var va = VerticalAlignment;

		double arrangeW = ha == HorizontalAlignment.Stretch ? innerW : contentW;
		double arrangeH = va == VerticalAlignment.Stretch ? innerH : contentH;

		// Cap at inner slot.
		arrangeW = Math.Min(arrangeW, innerW);
		arrangeH = Math.Min(arrangeH, innerH);

		double x = finalRect.X + margin.Left;
		double y = finalRect.Y + margin.Top;

		if (ha == HorizontalAlignment.Center)
			x += (innerW - arrangeW) / 2;
		else if (ha == HorizontalAlignment.Right)
			x += innerW - arrangeW;

		if (va == VerticalAlignment.Center)
			y += (innerH - arrangeH) / 2;
		else if (va == VerticalAlignment.Bottom)
			y += innerH - arrangeH;

		ArrangeOverride(new Size(arrangeW, arrangeH));
		RenderSize = new Size(arrangeW, arrangeH);
	}

	/// <summary>Override to arrange children. Default returns finalSize unchanged.</summary>
	protected virtual Size ArrangeOverride(Size finalSize) => finalSize;
}