namespace System.Windows.Media;

public static partial class VisualTreeHelper
{
	/// <summary>Returns the topmost hit under <paramref name="point"/> (in the reference's space), or null.</summary>
	/// <exception cref="ArgumentNullException"/>
	public static HitTestResult? HitTest(Visual reference, Point point)
	{
		if (reference == null) throw new ArgumentNullException(nameof(reference));
		HitTestResult? found = null;
		HitTest(reference, null, r => { found = r; return HitTestResultBehavior.Stop; },
			new PointHitTestParameters(point));
		return found;
	}

	/// <summary>
	/// Walks the visual subtree in reverse z-order, calling <paramref name="resultCallback"/> for each hit.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public static void HitTest(Visual reference, HitTestFilterCallback? filterCallback,
		HitTestResultCallback resultCallback, HitTestParameters hitTestParameters)
	{
		if (reference == null) throw new ArgumentNullException(nameof(reference));
		if (resultCallback == null) throw new ArgumentNullException(nameof(resultCallback));
		if (hitTestParameters == null) throw new ArgumentNullException(nameof(hitTestParameters));
		Walk(reference, filterCallback, resultCallback, hitTestParameters, true);
	}

	// Returns false once a callback or filter asked to stop.
	static bool Walk(Visual v, HitTestFilterCallback? filter, HitTestResultCallback callback,
		HitTestParameters p, bool isRoot)
	{
		if (!isRoot && !v.ParticipatesInHitTest) return true;
		var behavior = filter?.Invoke(v) ?? HitTestFilterBehavior.Continue;
		if (behavior == HitTestFilterBehavior.Stop) return false;
		if (behavior == HitTestFilterBehavior.ContinueSkipSelfAndChildren) return true;

		if (behavior != HitTestFilterBehavior.ContinueSkipChildren)
		{
			for (int i = v.VisualChildrenCount - 1; i >= 0; i--)
			{
				var child = v.GetVisualChild(i);
				var childParams = ToChildSpace(child, p);
				if (childParams != null && !Walk(child, filter, callback, childParams, false)) return false;
			}
		}
		if (behavior == HitTestFilterBehavior.ContinueSkipSelf) return true;

		HitTestResult? result = p switch
		{
			PointHitTestParameters pp => v.InvokeHitTestCore(pp),
			GeometryHitTestParameters gp => v.InvokeHitTestCore(gp),
			_ => null,
		};
		return result == null || callback(result) != HitTestResultBehavior.Stop;
	}

	// Geometry parameters are passed through untransformed (hit geometry is tested in the reference's space).
	static HitTestParameters? ToChildSpace(Visual child, HitTestParameters p)
	{
		if (p is not PointHitTestParameters pp) return p;
		Matrix m = child.ChildToParentMatrix();
		if (!m.HasInverse) return null;
		m.Invert();
		return new PointHitTestParameters(m.Transform(pp.HitPoint));
	}
}
