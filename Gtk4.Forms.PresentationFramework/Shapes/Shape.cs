using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Base class for all WPF shape elements.  Each shape is backed by a
/// <see cref="Gtk.DrawingArea"/> and paints itself via Cairo using
/// <see cref="Gtk.DrawingArea.DrawFunc"/>.
/// <para>
/// Rendering teardown is deterministic: the Cairo draw callback is cleared in
/// <c>Dispose(true)</c> only, never from the finalizer (repo rule).
/// </para>
/// </summary>
public abstract partial class Shape : FrameworkElement, IDisposable
{
	// ── Fill DP ──────────────────────────────────────────────────────────────

	/// <summary>The brush used to fill the shape interior.</summary>
	public static readonly DependencyProperty FillProperty =
		DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(Shape),
			new PropertyMetadata(null, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the fill brush.</summary>
	public Brush? Fill
	{
		get => (Brush?)GetValue(FillProperty);
		set => SetValue(FillProperty, value);
	}

	// ── Stroke DP ────────────────────────────────────────────────────────────

	/// <summary>The brush used to paint the shape outline.</summary>
	public static readonly DependencyProperty StrokeProperty =
		DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(Shape),
			new PropertyMetadata(null, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the stroke brush.</summary>
	public Brush? Stroke
	{
		get => (Brush?)GetValue(StrokeProperty);
		set => SetValue(StrokeProperty, value);
	}

	// ── StrokeThickness DP ───────────────────────────────────────────────────

	/// <summary>Width of the shape's outline in device-independent units.</summary>
	public static readonly DependencyProperty StrokeThicknessProperty =
		DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(Shape),
			new PropertyMetadata(1.0, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the stroke thickness.</summary>
	public double StrokeThickness
	{
		get => (double)GetValue(StrokeThicknessProperty)!;
		set => SetValue(StrokeThicknessProperty, value);
	}

	// ── Stretch DP ───────────────────────────────────────────────────────────

	/// <summary>How the shape content is stretched to fill its bounds.</summary>
	public static readonly DependencyProperty StretchProperty =
		DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(Shape),
			new PropertyMetadata(Stretch.None, (d, _) => ((Shape)d).QueueRedraw()));

	/// <summary>Gets or sets the stretch mode.</summary>
	public Stretch Stretch
	{
		get => (Stretch)GetValue(StretchProperty)!;
		set => SetValue(StretchProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.DrawingArea? _area;
	bool _disposed;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_area = new Gtk.DrawingArea();
		_area.DrawFunc = CairoDrawFunc.Wrap(OnDraw);
		return _area;
	}

	/// <summary>Forces the DrawingArea to re-render on the next GTK frame.</summary>
	protected void QueueRedraw() => _area?.QueueDraw();

	internal override void OnClipChanged() => QueueRedraw();

	/// <summary>Paints the shape into <paramref name="cr"/> exactly as the drawing area does.</summary>
	internal void PaintTo(Cairo.Context cr, int width, int height) => PaintShape(cr, width, height);

	internal override bool PaintsClipItself => true;

	// ── Draw callback ─────────────────────────────────────────────────────────

	void OnDraw(Cairo.Context cr, int width, int height)
	{
		if (width <= 0 || height <= 0) return;
		PaintShape(cr, width, height);
	}

	/// <summary>
	/// Builds the shape path and paints fill + stroke.
	/// Subclasses override <see cref="BuildShapePath"/> to define their geometry.
	/// </summary>
	void PaintShape(Cairo.Context cr, int width, int height)
	{
		cr.Save();
		ApplyClip(cr);

		BuildShapePath(cr, width, height);

		var fill = Fill;
		var stroke = Stroke;
		double sw = StrokeThickness;

		if (fill != null && stroke != null && sw > 0)
		{
			fill.ApplyToCairo(cr, width, height);
			cr.FillPreserve();
			stroke.ApplyToCairo(cr, width, height);
			cr.LineWidth = sw;
			StrokeStyle.Apply(cr,
				StrokeDashArray, StrokeDashOffset, sw,
				StrokeStartLineCap, StrokeLineJoin, StrokeMiterLimit);
			cr.Stroke();
		}
		else if (fill != null)
		{
			fill.ApplyToCairo(cr, width, height);
			cr.Fill();
		}
		else if (stroke != null && sw > 0)
		{
			stroke.ApplyToCairo(cr, width, height);
			cr.LineWidth = sw;
			StrokeStyle.Apply(cr,
				StrokeDashArray, StrokeDashOffset, sw,
				StrokeStartLineCap, StrokeLineJoin, StrokeMiterLimit);
			cr.Stroke();
		}

		cr.Restore();
	}

	/// <summary>
	/// Subclasses build their Cairo path into <paramref name="cr"/> for a widget of
	/// <paramref name="width"/> × <paramref name="height"/> pixels.
	/// </summary>
	protected abstract void BuildShapePath(Cairo.Context cr, int width, int height);

	// ── IDisposable ───────────────────────────────────────────────────────────

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases managed resources.</summary>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed) return;
		if (disposing && _area != null)
		{
			_area.DrawFunc = null!;
			_area = null;
		}
		_disposed = true;
	}
}

