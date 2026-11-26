using System.Windows.Markup;

namespace System.Windows.Media.Media3D;

/// <summary>Rotates 3D content about a centre.</summary>
public sealed class RotateTransform3D : Transform3D
{
	/// <summary>Identifies <see cref="Rotation"/>.</summary>
	public static readonly DependencyProperty RotationProperty = Media3DProps.Register(
		nameof(Rotation), typeof(Rotation3D), typeof(RotateTransform3D), Rotation3D.Identity);

	/// <summary>Identifies <see cref="CenterX"/>.</summary>
	public static readonly DependencyProperty CenterXProperty =
		Media3DProps.Register(nameof(CenterX), typeof(double), typeof(RotateTransform3D), 0.0);

	/// <summary>Identifies <see cref="CenterY"/>.</summary>
	public static readonly DependencyProperty CenterYProperty =
		Media3DProps.Register(nameof(CenterY), typeof(double), typeof(RotateTransform3D), 0.0);

	/// <summary>Identifies <see cref="CenterZ"/>.</summary>
	public static readonly DependencyProperty CenterZProperty =
		Media3DProps.Register(nameof(CenterZ), typeof(double), typeof(RotateTransform3D), 0.0);

	/// <summary>Creates an identity rotation.</summary>
	public RotateTransform3D() { }

	/// <summary>Creates a rotation about the origin.</summary>
	public RotateTransform3D(Rotation3D rotation) => Rotation = rotation;

	/// <summary>Creates a rotation about <paramref name="center"/>.</summary>
	public RotateTransform3D(Rotation3D rotation, Point3D center)
		: this(rotation, center.X, center.Y, center.Z) { }

	/// <summary>Creates a rotation about a centre.</summary>
	public RotateTransform3D(Rotation3D rotation, double centerX, double centerY, double centerZ)
	{
		Rotation = rotation;
		CenterX = centerX;
		CenterY = centerY;
		CenterZ = centerZ;
	}

	/// <summary>The rotation.</summary>
	public Rotation3D Rotation
	{
		get => (Rotation3D)GetValue(RotationProperty)!;
		set => SetValue(RotationProperty, value);
	}

	/// <summary>The X of the centre.</summary>
	public double CenterX
	{
		get => (double)GetValue(CenterXProperty)!;
		set => SetValue(CenterXProperty, value);
	}

	/// <summary>The Y of the centre.</summary>
	public double CenterY
	{
		get => (double)GetValue(CenterYProperty)!;
		set => SetValue(CenterYProperty, value);
	}

	/// <summary>The Z of the centre.</summary>
	public double CenterZ
	{
		get => (double)GetValue(CenterZProperty)!;
		set => SetValue(CenterZProperty, value);
	}

	/// <inheritdoc/>
	public override Matrix3D Value
	{
		get
		{
			var m = Matrix3D.Identity;
			var q = Rotation?.InternalQuaternion ?? Quaternion.Identity;
			m.RotateAt(q, new Point3D(CenterX, CenterY, CenterZ));
			return m;
		}
	}

	/// <inheritdoc/>
	public override bool IsAffine => true;

	/// <summary>Creates a modifiable clone.</summary>
	public new RotateTransform3D Clone() => (RotateTransform3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new RotateTransform3D();

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(Rotation);
}

/// <summary>Composes child transforms in order.</summary>
[ContentProperty("Children")]
public sealed class Transform3DGroup : Transform3D
{
	/// <summary>Identifies <see cref="Children"/>.</summary>
	public static readonly DependencyProperty ChildrenProperty = Media3DProps.Register(
		nameof(Children), typeof(Transform3DCollection), typeof(Transform3DGroup), Transform3DCollection.Empty);

	/// <summary>Creates an empty group.</summary>
	public Transform3DGroup() { }

	/// <summary>The child transforms.</summary>
	public Transform3DCollection Children
	{
		get => Media3DProps.Collection(this, ChildrenProperty, Transform3DCollection.Empty);
		set => SetValue(ChildrenProperty, value);
	}

	/// <inheritdoc/>
	public override Matrix3D Value
	{
		get
		{
			var result = Matrix3D.Identity;
			foreach (var child in Media3DProps.Peek<Transform3DCollection>(this, ChildrenProperty))
				result.Append(child.Value);
			return result;
		}
	}

	/// <inheritdoc/>
	public override bool IsAffine
	{
		get
		{
			foreach (var child in Media3DProps.Peek<Transform3DCollection>(this, ChildrenProperty))
				if (!child.IsAffine) return false;
			return true;
		}
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new Transform3DGroup Clone() => (Transform3DGroup)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Transform3DGroup();

	/// <inheritdoc/>
	protected override void OnFreeze() =>
		Media3DProps.FreezeAll(Media3DProps.Peek<Transform3DCollection>(this, ChildrenProperty));
}
