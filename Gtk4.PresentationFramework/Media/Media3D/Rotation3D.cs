using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the 3D rotations.</summary>
public abstract class Rotation3D : Animatable
{
	internal Rotation3D() { }

	/// <summary>The identity rotation (frozen).</summary>
	public static Rotation3D Identity { get; } = MakeIdentity();

	static Rotation3D MakeIdentity()
	{
		var r = new QuaternionRotation3D();
		r.Freeze();
		return r;
	}

	internal abstract Quaternion InternalQuaternion { get; }
}

/// <summary>A rotation of <see cref="Angle"/> degrees about <see cref="Axis"/>.</summary>
public sealed class AxisAngleRotation3D : Rotation3D
{
	/// <summary>Identifies <see cref="Axis"/>.</summary>
	public static readonly DependencyProperty AxisProperty = Media3DProps.Register(
		nameof(Axis), typeof(Vector3D), typeof(AxisAngleRotation3D), new Vector3D(0, 1, 0));

	/// <summary>Identifies <see cref="Angle"/>.</summary>
	public static readonly DependencyProperty AngleProperty =
		Media3DProps.Register(nameof(Angle), typeof(double), typeof(AxisAngleRotation3D), 0.0);

	/// <summary>Creates a rotation of 0 degrees about the Y axis.</summary>
	public AxisAngleRotation3D() { }

	/// <summary>Creates a rotation.</summary>
	public AxisAngleRotation3D(Vector3D axis, double angle)
	{
		Axis = axis;
		Angle = angle;
	}

	/// <summary>The rotation axis.</summary>
	public Vector3D Axis
	{
		get => (Vector3D)GetValue(AxisProperty)!;
		set => SetValue(AxisProperty, value);
	}

	/// <summary>The rotation angle in degrees.</summary>
	public double Angle
	{
		get => (double)GetValue(AngleProperty)!;
		set => SetValue(AngleProperty, value);
	}

	internal override Quaternion InternalQuaternion
	{
		get
		{
			var axis = Axis;
			return axis.X == 0 && axis.Y == 0 && axis.Z == 0 ? Quaternion.Identity : new Quaternion(axis, Angle);
		}
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new AxisAngleRotation3D Clone() => (AxisAngleRotation3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new AxisAngleRotation3D();
}

/// <summary>A rotation given by a <see cref="Media3D.Quaternion"/>.</summary>
public sealed class QuaternionRotation3D : Rotation3D
{
	/// <summary>Identifies <see cref="Quaternion"/>.</summary>
	public static readonly DependencyProperty QuaternionProperty = Media3DProps.Register(
		nameof(Quaternion), typeof(Quaternion), typeof(QuaternionRotation3D), Media3D.Quaternion.Identity);

	/// <summary>Creates an identity rotation.</summary>
	public QuaternionRotation3D() { }

	/// <summary>Creates a rotation.</summary>
	public QuaternionRotation3D(Quaternion quaternion) => Quaternion = quaternion;

	/// <summary>The quaternion.</summary>
	public Quaternion Quaternion
	{
		get => (Quaternion)GetValue(QuaternionProperty)!;
		set => SetValue(QuaternionProperty, value);
	}

	internal override Quaternion InternalQuaternion => Quaternion;

	/// <summary>Creates a modifiable clone.</summary>
	public new QuaternionRotation3D Clone() => (QuaternionRotation3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new QuaternionRotation3D();
}
