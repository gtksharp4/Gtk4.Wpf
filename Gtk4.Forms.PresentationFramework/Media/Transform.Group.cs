using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Skews content by <see cref="AngleX"/> and <see cref="AngleY"/> degrees around a centre
/// point. Mirrors <c>System.Windows.Media.SkewTransform</c>.
/// </summary>
public class SkewTransform : Transform
{
	/// <summary>Creates a skew transform.</summary>
	public SkewTransform(double angleX = 0, double angleY = 0,
		double centerX = 0, double centerY = 0)
	{
		AngleX = angleX; AngleY = angleY;
		CenterX = centerX; CenterY = centerY;
	}

	/// <summary>Horizontal skew angle in degrees.</summary>
	public double AngleX { get; set; }
	/// <summary>Vertical skew angle in degrees.</summary>
	public double AngleY { get; set; }
	/// <summary>Centre X of the skew.</summary>
	public double CenterX { get; set; }
	/// <summary>Centre Y of the skew.</summary>
	public double CenterY { get; set; }

	/// <inheritdoc/>
	public override Matrix Value
	{
		get
		{
			double tx = Math.Tan(AngleX * Math.PI / 180.0);
			double ty = Math.Tan(AngleY * Math.PI / 180.0);
			// translate to centre, skew, translate back
			return new Matrix(
				1, ty, tx, 1,
				-CenterY * tx,
				-CenterX * ty);
		}
	}
}

/// <summary>
/// An ordered list of <see cref="Transform"/> objects for <see cref="TransformGroup"/>.
/// Mirrors <c>System.Windows.Media.TransformCollection</c>.
/// </summary>
public class TransformCollection : List<Transform>
{
	/// <summary>Creates an empty collection.</summary>
	public TransformCollection() { }

	/// <summary>Creates a collection with the given children.</summary>
	public TransformCollection(IEnumerable<Transform> children) : base(children) { }
}

/// <summary>
/// Composites a list of <see cref="Transform"/> objects by multiplying their matrices
/// left-to-right (first child applied first). Mirrors
/// <c>System.Windows.Media.TransformGroup</c>.
/// </summary>
public class TransformGroup : Transform
{
	/// <summary>Creates an empty transform group.</summary>
	public TransformGroup()
	{
		Children = new TransformCollection();
	}

	/// <summary>The ordered child transforms. Applied first→last.</summary>
	public TransformCollection Children { get; }

	/// <summary>
	/// Returns the product of all child matrices. Returns <see cref="Matrix.Identity"/>
	/// when <see cref="Children"/> is empty.
	/// </summary>
	public override Matrix Value
	{
		get
		{
			Matrix result = Matrix.Identity;
			foreach (Transform t in Children)
				result = result.Multiply(t.Value);
			return result;
		}
	}
}
