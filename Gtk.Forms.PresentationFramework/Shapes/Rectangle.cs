using System;

namespace System.Windows.Shapes
{
	/// <summary>
	/// Draws a rectangle, optionally with rounded corners.
	/// Mirrors <c>System.Windows.Shapes.Rectangle</c>.
	/// Backed by a <see cref="Gtk.DrawingArea"/> with Cairo rendering.
	/// </summary>
	public class Rectangle : Shape
	{
		// ── RadiusX DP ───────────────────────────────────────────────────────────

		/// <summary>Horizontal corner radius.</summary>
		public static readonly DependencyProperty RadiusXProperty =
			DependencyProperty.Register(nameof(RadiusX), typeof(double), typeof(Rectangle),
				new PropertyMetadata(0.0, (d, _) => ((Rectangle)d).QueueRedraw()));

		/// <summary>Gets or sets the horizontal corner radius.</summary>
		public double RadiusX
		{
			get => (double)GetValue(RadiusXProperty)!;
			set => SetValue(RadiusXProperty, value);
		}

		// ── RadiusY DP ───────────────────────────────────────────────────────────

		/// <summary>Vertical corner radius.</summary>
		public static readonly DependencyProperty RadiusYProperty =
			DependencyProperty.Register(nameof(RadiusY), typeof(double), typeof(Rectangle),
				new PropertyMetadata(0.0, (d, _) => ((Rectangle)d).QueueRedraw()));

		/// <summary>Gets or sets the vertical corner radius.</summary>
		public double RadiusY
		{
			get => (double)GetValue(RadiusYProperty)!;
			set => SetValue(RadiusYProperty, value);
		}

		// ── QueueRedraw trampoline ────────────────────────────────────────────────

		// Shape.QueueRedraw is private; we call it via the base DP callback indirectly.
		// The DP callbacks on the two DPs above call it via the cast, so this trampoline
		// is not needed — the cast in the lambda is to Rectangle, which is 'this'.

		/// <inheritdoc/>
		protected override void BuildShapePath(Cairo.Context cr, int width, int height)
		{
			double rx = Math.Min(RadiusX, width / 2.0);
			double ry = Math.Min(RadiusY, height / 2.0);

			if (rx <= 0 || ry <= 0)
			{
				cr.Rectangle(0, 0, width, height);
				return;
			}

			// Rounded rectangle
			cr.MoveTo(rx, 0);
			cr.LineTo(width - rx, 0);
			cr.Arc(width - rx, ry, ry, -Math.PI / 2, 0);
			cr.LineTo(width, height - ry);
			cr.Arc(width - rx, height - ry, ry, 0, Math.PI / 2);
			cr.LineTo(rx, height);
			cr.Arc(rx, height - ry, ry, Math.PI / 2, Math.PI);
			cr.LineTo(0, ry);
			cr.Arc(rx, ry, ry, Math.PI, Math.PI * 3 / 2);
			cr.ClosePath();
		}
	}
}
