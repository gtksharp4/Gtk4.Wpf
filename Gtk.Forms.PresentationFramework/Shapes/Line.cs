namespace System.Windows.Shapes
{
	/// <summary>
	/// Draws a straight line between two points.
	/// Mirrors <c>System.Windows.Shapes.Line</c>.
	/// </summary>
	public class Line : Shape
	{
		// ── X1 DP ────────────────────────────────────────────────────────────────

		/// <summary>X coordinate of the start point.</summary>
		public static readonly DependencyProperty X1Property =
			DependencyProperty.Register(nameof(X1), typeof(double), typeof(Line),
				new PropertyMetadata(0.0, (d, _) => ((Line)d).InvalidateLine()));

		/// <summary>Gets or sets the X coordinate of the start point.</summary>
		public double X1
		{
			get => (double)GetValue(X1Property)!;
			set => SetValue(X1Property, value);
		}

		// ── Y1 DP ────────────────────────────────────────────────────────────────

		/// <summary>Y coordinate of the start point.</summary>
		public static readonly DependencyProperty Y1Property =
			DependencyProperty.Register(nameof(Y1), typeof(double), typeof(Line),
				new PropertyMetadata(0.0, (d, _) => ((Line)d).InvalidateLine()));

		/// <summary>Gets or sets the Y coordinate of the start point.</summary>
		public double Y1
		{
			get => (double)GetValue(Y1Property)!;
			set => SetValue(Y1Property, value);
		}

		// ── X2 DP ────────────────────────────────────────────────────────────────

		/// <summary>X coordinate of the end point.</summary>
		public static readonly DependencyProperty X2Property =
			DependencyProperty.Register(nameof(X2), typeof(double), typeof(Line),
				new PropertyMetadata(0.0, (d, _) => ((Line)d).InvalidateLine()));

		/// <summary>Gets or sets the X coordinate of the end point.</summary>
		public double X2
		{
			get => (double)GetValue(X2Property)!;
			set => SetValue(X2Property, value);
		}

		// ── Y2 DP ────────────────────────────────────────────────────────────────

		/// <summary>Y coordinate of the end point.</summary>
		public static readonly DependencyProperty Y2Property =
			DependencyProperty.Register(nameof(Y2), typeof(double), typeof(Line),
				new PropertyMetadata(0.0, (d, _) => ((Line)d).InvalidateLine()));

		/// <summary>Gets or sets the Y coordinate of the end point.</summary>
		public double Y2
		{
			get => (double)GetValue(Y2Property)!;
			set => SetValue(Y2Property, value);
		}

		void InvalidateLine() => QueueRedraw();

		/// <inheritdoc/>
		protected override void BuildShapePath(Cairo.Context cr, int width, int height)
		{
			cr.MoveTo(X1, Y1);
			cr.LineTo(X2, Y2);
		}
	}
}
