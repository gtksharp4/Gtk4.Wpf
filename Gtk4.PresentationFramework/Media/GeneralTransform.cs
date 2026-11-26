using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>Base for transforms that map points between coordinate spaces. Mirrors WPF's GeneralTransform.</summary>
public abstract class GeneralTransform : Animatable
{
	/// <summary>The inverse transform, or <c>null</c> when there is none.</summary>
	public abstract GeneralTransform? Inverse { get; }

	/// <summary>An affine approximation of this transform, or <c>null</c>.</summary>
	public virtual Transform? AffineTransform => null;

	/// <summary>Transforms a point.</summary>
	/// <exception cref="InvalidOperationException">The transform cannot map the point.</exception>
	public Point Transform(Point point)
	{
		if (!TryTransform(point, out Point result))
			throw new InvalidOperationException("The transform could not transform the point.");
		return result;
	}

	/// <summary>Transforms a point; <c>false</c> when it cannot be mapped.</summary>
	public abstract bool TryTransform(Point inPoint, out Point result);

	/// <summary>Returns the bounding box of the transformed rectangle.</summary>
	public abstract Rect TransformBounds(Rect rect);

	/// <summary>Creates a deep copy.</summary>
	public new GeneralTransform Clone() => (GeneralTransform)base.Clone();

	/// <summary>Creates a copy holding current values.</summary>
	public new GeneralTransform CloneCurrentValue() => (GeneralTransform)base.CloneCurrentValue();
}
