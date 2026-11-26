using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the 3D transforms. Mirrors <c>System.Windows.Media.Media3D.Transform3D</c>.</summary>
public abstract class Transform3D : Animatable
{
	ChildLinks? _links;

	internal Transform3D() { }

	/// <summary>The identity transform (frozen).</summary>
	public static Transform3D Identity { get; } = MakeIdentity();

	static Transform3D MakeIdentity()
	{
		var t = new MatrixTransform3D(Matrix3D.Identity);
		t.Freeze();
		return t;
	}

	/// <summary>The matrix of the transform.</summary>
	public abstract Matrix3D Value { get; }

	/// <summary>Whether the transform is affine.</summary>
	public abstract bool IsAffine { get; }

	/// <summary>The inverse transform, or null when there is none.</summary>
	public Transform3D? Inverse
	{
		get
		{
			var m = Value;
			if (!m.HasInverse) return null;
			m.Invert();
			return new MatrixTransform3D(m);
		}
	}

	/// <summary>Transforms a point.</summary>
	public Point3D Transform(Point3D point) => Value.Transform(point);

	/// <summary>Transforms a vector (translation ignored).</summary>
	public Vector3D Transform(Vector3D vector) => Value.Transform(vector);

	/// <summary>Transforms a homogeneous point.</summary>
	public Point4D Transform(Point4D point) => Value.Transform(point);

	/// <summary>Transforms points in place.</summary>
	public void Transform(Point3D[] points) => Value.Transform(points);

	/// <summary>Transforms vectors in place.</summary>
	public void Transform(Vector3D[] vectors) => Value.Transform(vectors);

	/// <summary>Transforms a point; false (and the input) when the transform cannot be applied.</summary>
	public bool TryTransform(Point3D inputPoint, out Point3D outputPoint)
	{
		outputPoint = Transform(inputPoint);
		return true;
	}

	/// <summary>The axis-aligned box of a transformed box.</summary>
	public Rect3D TransformBounds(Rect3D rect)
	{
		if (rect.IsEmpty) return rect;
		var m = Value;
		var result = Rect3D.Empty;
		for (int i = 0; i < 8; i++)
		{
			var corner = new Point3D(
				rect.X + ((i & 1) == 0 ? 0 : rect.SizeX),
				rect.Y + ((i & 2) == 0 ? 0 : rect.SizeY),
				rect.Z + ((i & 4) == 0 ? 0 : rect.SizeZ));
			result.Union(m.Transform(corner));
		}
		return result;
	}

	void RaiseChanged()
	{
		if (!IsFrozen) OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			(_links ??= new ChildLinks(RaiseChanged)).Track(e);
	}
}
