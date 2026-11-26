namespace System.Windows.Media;

/// <summary>Whether hit testing continues after a result.</summary>
public enum HitTestResultBehavior { Stop = 0, Continue = 1 }

/// <summary>How a filter steers the hit-test walk.</summary>
public enum HitTestFilterBehavior
{
	ContinueSkipSelfAndChildren = 0,
	ContinueSkipChildren = 2,
	ContinueSkipSelf = 4,
	Continue = 6,
	Stop = 8,
}

/// <summary>How a hit geometry relates to a visual's geometry.</summary>
public enum IntersectionDetail { NotCalculated = 0, Empty = 1, FullyInside = 2, FullyContains = 3, Intersects = 4 }

/// <summary>Receives each hit result.</summary>
public delegate HitTestResultBehavior HitTestResultCallback(HitTestResult result);

/// <summary>Filters visuals before they are hit tested.</summary>
public delegate HitTestFilterBehavior HitTestFilterCallback(DependencyObject potentialHitTestTarget);

/// <summary>Base for hit-test parameters.</summary>
public abstract class HitTestParameters
{
	internal HitTestParameters() { }
}

/// <summary>Hit-tests a point.</summary>
public class PointHitTestParameters : HitTestParameters
{
	/// <summary>Creates parameters for <paramref name="point"/>.</summary>
	public PointHitTestParameters(Point point) => HitPoint = point;

	/// <summary>The point to test.</summary>
	public Point HitPoint { get; }
}

/// <summary>Hit-tests a geometry.</summary>
public class GeometryHitTestParameters : HitTestParameters
{
	/// <summary>Creates parameters for <paramref name="geometry"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	public GeometryHitTestParameters(Geometry geometry) =>
		HitGeometry = geometry ?? throw new ArgumentNullException(nameof(geometry));

	/// <summary>The geometry to test.</summary>
	public Geometry HitGeometry { get; }
}

/// <summary>Result of a point hit test.</summary>
public class PointHitTestResult : HitTestResult
{
	/// <summary>Creates a result.</summary>
	public PointHitTestResult(Visual visualHit, Point pointHit) : base(visualHit) => PointHit = pointHit;

	/// <summary>The hit point in the hit visual's space.</summary>
	public Point PointHit { get; }
}

/// <summary>Result of a geometry hit test.</summary>
public class GeometryHitTestResult : HitTestResult
{
	/// <summary>Creates a result.</summary>
	public GeometryHitTestResult(Visual visualHit, IntersectionDetail intersectionDetail) : base(visualHit) =>
		IntersectionDetail = intersectionDetail;

	/// <summary>How the geometries intersect.</summary>
	public IntersectionDetail IntersectionDetail { get; }
}
