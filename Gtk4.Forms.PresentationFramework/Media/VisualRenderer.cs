namespace System.Windows.Media;

/// <summary>Paints non-UIElement visuals (and their offset/transform/opacity) onto a Cairo context.</summary>
static class VisualRenderer
{
	/// <summary>Paints the non-UIElement children of <paramref name="parent"/> in z-order.</summary>
	internal static void PaintChildren(Visual parent, Cairo.Context cr)
	{
		for (int i = 0; i < parent.VisualChildrenCount; i++)
		{
			var child = parent.GetVisualChild(i);
			if (child is not UIElement) Paint(child, cr);
		}
	}

	static void Paint(Visual v, Cairo.Context cr)
	{
		cr.Save();
		cr.Transform(v.ChildToParentMatrix().ToCairo());
		double opacity = (v as ContainerVisual)?.Opacity ?? 1.0;
		bool group = opacity < 1.0;
		if (group) cr.PushGroup();
		if (v is DrawingVisual dv && dv.Drawing != null) DrawingContextRenderer.Render(dv.Drawing, cr, paintText: true);
		PaintChildren(v, cr);
		if (group)
		{
			cr.PopGroupToSource();
			cr.PaintWithAlpha(Math.Max(0, opacity));
		}
		cr.Restore();
	}
}
