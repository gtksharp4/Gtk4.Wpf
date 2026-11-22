using System.Windows.Media;

namespace System.Windows;

/// <summary>Offsets between elements: GTK's computed positions when mapped, managed arrange rects before that.</summary>
public abstract partial class UIElement
{
	internal override Matrix ChildToParentMatrix()
	{
		Vector offset = LayoutOffsetInParent();
		return new Matrix(1, 0, 0, 1, offset.X, offset.Y);
	}

	Vector LayoutOffsetInParent()
	{
		if (VisualParent is UIElement parent && IsWidgetCreated && parent.IsWidgetCreated && Widget.IsMapped)
		{
			var origin = new Graphene.Point();
			origin.X = 0;
			origin.Y = 0;
			if (Widget.ComputePoint(parent.Widget, origin, out var to)) return new Vector(to.X, to.Y);
		}
		return new Vector(ArrangeRect.X, ArrangeRect.Y);
	}

	internal override bool ParticipatesInHitTest => Visibility == Visibility.Visible && IsHitTestVisible;

	/// <summary>
	/// Hits when the point lies inside the element's render size (a simplification of WPF's content test).
	/// </summary>
	protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
	{
		Point p = hitTestParameters.HitPoint;
		Size s = RenderSize;
		return p.X >= 0 && p.Y >= 0 && p.X < s.Width && p.Y < s.Height ? new PointHitTestResult(this, p) : null;
	}

	/// <summary>
	/// Translates <paramref name="point"/> from this element's space to <paramref name="relativeTo"/>'s.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">The elements share no common ancestor.</exception>
	public Point TranslatePoint(Point point, UIElement relativeTo)
	{
		if (relativeTo == null) throw new ArgumentNullException(nameof(relativeTo));
		var transform = TransformToVisual(relativeTo);
		return transform == null ? point : transform.Transform(point);
	}
}
