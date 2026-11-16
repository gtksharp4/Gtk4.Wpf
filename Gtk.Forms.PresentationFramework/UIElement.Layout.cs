using System;

namespace System.Windows
{
	/// <summary>
	/// Two-pass WPF layout contract for UIElement.
	/// </summary>
	public abstract partial class UIElement
	{
		// ── Dirty flags ─────────────────────────────────────────────────────────

		bool _measureDirty = true;
		bool _arrangeDirty = true;

		/// <summary>The size arranged for this element (set by <see cref="Arrange"/>).</summary>
		public Size RenderSize { get; protected set; }

		/// <summary>The final rect this element was arranged into.</summary>
		internal Rect ArrangeRect { get; private set; }

		// ── Measure ─────────────────────────────────────────────────────────────

		/// <summary>
		/// Measures this element given <paramref name="availableSize"/>.
		/// Sets <see cref="DesiredSize"/>; derived classes override <see cref="MeasureCore"/>.
		/// </summary>
		public void Measure(Size availableSize)
		{
			if (!_measureDirty && DesiredSize != Size.Empty)
				return; // already measured with same constraints — WPF skips too
			_measureDirty = false;
			var desired = MeasureCore(availableSize);
			// Clamp to [0, availableSize].
			desired = new Size(
				Math.Max(0, Math.Min(desired.Width, double.IsPositiveInfinity(availableSize.Width) ? desired.Width : availableSize.Width)),
				Math.Max(0, Math.Min(desired.Height, double.IsPositiveInfinity(availableSize.Height) ? desired.Height : availableSize.Height)));
			DesiredSize = desired;
		}

		/// <summary>Override to compute desired size. Base returns zero.</summary>
		protected virtual Size MeasureCore(Size availableSize) => new Size(0, 0);

		// ── Arrange ─────────────────────────────────────────────────────────────

		/// <summary>
		/// Arranges this element into <paramref name="finalRect"/>.
		/// Sets <see cref="RenderSize"/> (via <see cref="ArrangeCore"/>);
		/// derived classes override <see cref="ArrangeCore"/> which is responsible for
		/// setting <see cref="RenderSize"/> to the actual rendered size.
		/// </summary>
		public void Arrange(Rect finalRect)
		{
			_arrangeDirty = false;
			ArrangeRect = finalRect;
			// Default RenderSize = finalRect size; ArrangeCore (FrameworkElement) may narrow it
			// for alignment. FrameworkElement.ArrangeCore sets RenderSize itself.
			RenderSize = new Size(finalRect.Width, finalRect.Height);
			ArrangeCore(finalRect);
		}

		/// <summary>Override to position children. Base is no-op.</summary>
		protected virtual void ArrangeCore(Rect finalRect) { }

		// ── Invalidation ────────────────────────────────────────────────────────

		/// <summary>Marks measure as dirty; a subsequent <see cref="UpdateLayout"/> will re-measure.</summary>
		public void InvalidateMeasure()
		{
			_measureDirty = true;
			_arrangeDirty = true;
		}

		/// <summary>Marks arrange as dirty.</summary>
		public void InvalidateArrange()
		{
			_arrangeDirty = true;
		}

		/// <summary>
		/// Re-runs measure and arrange if dirty, using the last-known constraints.
		/// Passes <see cref="Size.Empty"/> (infinity) as available size.
		/// </summary>
		public void UpdateLayout()
		{
			if (_measureDirty)
				Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			if (_arrangeDirty)
				Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
		}
	}
}
