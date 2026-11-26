using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Converts an SVG/WPF elliptical-arc (endpoint parameterization) into one or more
/// cubic Bézier curves suitable for Cairo's <c>CurveTo</c>.
///
/// <para>Algorithm summary:</para>
/// <list type="number">
///   <item>Convert endpoint parameters to center parameters (SVG spec §B.2.4).</item>
///   <item>Split the arc into segments of at most 90° each.</item>
///   <item>Approximate each segment with a single cubic Bézier using the
///         standard κ = 4/3 · tan(θ/4) formula (maximum error ≈ 0.027% for 90°).</item>
///   <item>Apply the axis rotation and ellipse scaling to map the unit-circle
///         Bézier back to the target coordinate system.</item>
/// </list>
/// <para>Bounds note: <see cref="PathGeometry.Bounds"/> expands only over the arc's
/// end point, so an arc bulging past its chord may be under-estimated. This is a
/// documented approximation.</para>
/// </summary>
internal static class ArcToBezier
{
	/// <summary>
	/// Converts one arc into zero or more cubic Bézier segments.
	/// Returns an empty list when the arc degenerates to a straight line
	/// (zero radius, coincident endpoints, or near-zero sweep).
	/// </summary>
	/// <param name="x1">Start x (current point).</param>
	/// <param name="y1">Start y (current point).</param>
	/// <param name="rx">X radius (clamped to positive).</param>
	/// <param name="ry">Y radius (clamped to positive).</param>
	/// <param name="xRotDeg">X-axis rotation in degrees.</param>
	/// <param name="isLargeArc">Large-arc flag.</param>
	/// <param name="sweep">Sweep direction.</param>
	/// <param name="x2">End x.</param>
	/// <param name="y2">End y.</param>
	/// <returns>
	/// Each element is (cp1x, cp1y, cp2x, cp2y, ex, ey) — a cubic Bézier
	/// with two control points and an end point.
	/// </returns>
	internal static IReadOnlyList<(double cp1x, double cp1y, double cp2x, double cp2y, double ex, double ey)>
		Convert(double x1, double y1,
		        double rx, double ry, double xRotDeg,
		        bool isLargeArc, SweepDirection sweep,
		        double x2, double y2)
	{
		var result = new List<(double, double, double, double, double, double)>();

		// Degenerate: same start and end → nothing.
		if (Math.Abs(x2 - x1) < 1e-10 && Math.Abs(y2 - y1) < 1e-10)
			return result;

		// Degenerate: zero radius → straight line emitted as a zero-length bezier
		// (caller emits a LineTo instead; we signal by returning empty list).
		if (rx < 1e-10 || ry < 1e-10)
			return result;

		double phi = xRotDeg * Math.PI / 180.0;
		double cosPhi = Math.Cos(phi);
		double sinPhi = Math.Sin(phi);

		// Step 1 — transform to unit-circle space (SVG §B.2.4 step 1)
		double dx2 = (x1 - x2) / 2.0;
		double dy2 = (y1 - y2) / 2.0;
		double x1p =  cosPhi * dx2 + sinPhi * dy2;
		double y1p = -sinPhi * dx2 + cosPhi * dy2;

		// Clamp radii (SVG §B.2.4 step 2 — ensure they're large enough)
		double x1p2 = x1p * x1p, y1p2 = y1p * y1p;
		double rx2 = rx * rx, ry2 = ry * ry;
		double lambda = x1p2 / rx2 + y1p2 / ry2;
		if (lambda > 1.0)
		{
			double sqrtL = Math.Sqrt(lambda);
			rx *= sqrtL; ry *= sqrtL;
			rx2 = rx * rx; ry2 = ry * ry;
		}

		// Step 2 — compute centre in unit-circle space (SVG §B.2.4 step 2)
		double num = rx2 * ry2 - rx2 * y1p2 - ry2 * x1p2;
		double den = rx2 * y1p2 + ry2 * x1p2;
		double sq = den > 0 ? Math.Sqrt(Math.Max(0.0, num / den)) : 0.0;
		bool sameFlag = isLargeArc == (sweep == SweepDirection.Clockwise);
		double sign = sameFlag ? -1.0 : 1.0;
		double cxp =  sign * sq * rx * y1p / ry;
		double cyp = -sign * sq * ry * x1p / rx;

		// Step 3 — transform centre back to original space
		double cx = cosPhi * cxp - sinPhi * cyp + (x1 + x2) / 2.0;
		double cy = sinPhi * cxp + cosPhi * cyp + (y1 + y2) / 2.0;

		// Step 4 — compute θ1 and dθ
		double theta1 = Angle(1, 0,  (x1p - cxp) / rx, (y1p - cyp) / ry);
		double dtheta  = Angle(
			(x1p - cxp) / rx, (y1p - cyp) / ry,
			(-x1p - cxp) / rx, (-y1p - cyp) / ry);

		if (sweep == SweepDirection.Counterclockwise && dtheta > 0)
			dtheta -= 2 * Math.PI;
		else if (sweep == SweepDirection.Clockwise && dtheta < 0)
			dtheta += 2 * Math.PI;

		// Split into ≤90° segments
		int nSegs = Math.Max(1, (int)Math.Ceiling(Math.Abs(dtheta) / (Math.PI / 2)));
		double dStep = dtheta / nSegs;

		double t = theta1;
		for (int k = 0; k < nSegs; k++, t += dStep)
		{
			AppendSegment(result, cx, cy, rx, ry, cosPhi, sinPhi, t, dStep);
		}
		return result;
	}

	// Appends one cubic Bézier approximating an arc from angle t over span dStep.
	static void AppendSegment(
		List<(double, double, double, double, double, double)> result,
		double cx, double cy, double rx, double ry,
		double cosPhi, double sinPhi,
		double t, double dStep)
	{
		// κ = 4/3 · tan(dStep/4)
		double alpha = 4.0 / 3.0 * Math.Tan(dStep / 4.0);

		double cosT = Math.Cos(t);
		double sinT = Math.Sin(t);
		double cosT2 = Math.Cos(t + dStep);
		double sinT2 = Math.Sin(t + dStep);

		// End point of this sub-arc
		double ex = cosPhi * rx * cosT2 - sinPhi * ry * sinT2 + cx;
		double ey = sinPhi * rx * cosT2 + cosPhi * ry * sinT2 + cy;

		// Derivative direction at t (unit circle derivative is (-sinT, cosT))
		double cp1x = cosPhi * (rx * (cosT - alpha * sinT)) - sinPhi * (ry * (sinT + alpha * cosT)) + cx;
		double cp1y = sinPhi * (rx * (cosT - alpha * sinT)) + cosPhi * (ry * (sinT + alpha * cosT)) + cy;

		// Derivative direction at t+dStep
		double cp2x = cosPhi * (rx * (cosT2 + alpha * sinT2)) - sinPhi * (ry * (sinT2 - alpha * cosT2)) + cx;
		double cp2y = sinPhi * (rx * (cosT2 + alpha * sinT2)) + cosPhi * (ry * (sinT2 - alpha * cosT2)) + cy;

		result.Add((cp1x, cp1y, cp2x, cp2y, ex, ey));
	}

	// Signed angle from vector (ux,uy) to (vx,vy).
	static double Angle(double ux, double uy, double vx, double vy)
	{
		double dot = ux * vx + uy * vy;
		double len = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
		double normalizedDot = dot / len;
		if (normalizedDot < -1.0) normalizedDot = -1.0;
		else if (normalizedDot > 1.0) normalizedDot = 1.0;
		double angle = Math.Acos(normalizedDot);
		if (ux * vy - uy * vx < 0) angle = -angle;
		return angle;
	}
}
