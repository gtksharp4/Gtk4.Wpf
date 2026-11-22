using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>
/// A rotation in 3D. Mirrors <c>System.Windows.Media.Media3D.Quaternion</c>; <c>default</c> is the
/// identity (an all-zero struct reads as identity).
/// </summary>
[TypeConverter(typeof(QuaternionConverter))]
public partial struct Quaternion : IFormattable
{
	double _x, _y, _z, _w;
	bool _notDistinguishedIdentity;

	/// <summary>Creates a quaternion from its components.</summary>
	public Quaternion(double x, double y, double z, double w)
	{
		_x = x;
		_y = y;
		_z = z;
		_w = w;
		_notDistinguishedIdentity = true;
	}

	/// <summary>Creates a rotation of <paramref name="angleInDegrees"/> about an axis.</summary>
	/// <exception cref="InvalidOperationException">The axis has zero length.</exception>
	public Quaternion(Vector3D axisOfRotation, double angleInDegrees)
	{
		angleInDegrees %= 360.0;
		double radians = angleInDegrees * (Math.PI / 180.0);
		double length = axisOfRotation.Length;
		if (length == 0) throw new InvalidOperationException("The rotation axis must not be zero.");
		Vector3D v = axisOfRotation / length * Math.Sin(0.5 * radians);
		_x = v.X;
		_y = v.Y;
		_z = v.Z;
		_w = Math.Cos(0.5 * radians);
		_notDistinguishedIdentity = true;
	}

	/// <summary>The identity rotation.</summary>
	public static Quaternion Identity { get; } = new Quaternion(0, 0, 0, 1);

	bool IsDistinguishedIdentity => !_notDistinguishedIdentity;

	void Materialize()
	{
		if (IsDistinguishedIdentity) this = Identity;
	}

	/// <summary>The X component.</summary>
	public double X { get => IsDistinguishedIdentity ? 0 : _x; set { Materialize(); _x = value; } }

	/// <summary>The Y component.</summary>
	public double Y { get => IsDistinguishedIdentity ? 0 : _y; set { Materialize(); _y = value; } }

	/// <summary>The Z component.</summary>
	public double Z { get => IsDistinguishedIdentity ? 0 : _z; set { Materialize(); _z = value; } }

	/// <summary>The W component.</summary>
	public double W { get => IsDistinguishedIdentity ? 1 : _w; set { Materialize(); _w = value; } }

	/// <summary>Whether this is the identity rotation.</summary>
	public bool IsIdentity => IsDistinguishedIdentity || (_x == 0 && _y == 0 && _z == 0 && _w == 1);

	/// <summary>Whether the quaternion has unit length.</summary>
	public bool IsNormalized =>
		IsDistinguishedIdentity || Media3DMath.IsOne(_x * _x + _y * _y + _z * _z + _w * _w);

	/// <summary>The rotation axis; (0,1,0) when the rotation is the identity.</summary>
	public Vector3D Axis
	{
		get
		{
			if (IsDistinguishedIdentity || (_x == 0 && _y == 0 && _z == 0)) return new Vector3D(0, 1, 0);
			var v = new Vector3D(_x, _y, _z);
			v.Normalize();
			return v;
		}
	}

	/// <summary>The rotation angle in degrees.</summary>
	public double Angle
	{
		get
		{
			if (IsDistinguishedIdentity) return 0;
			double y = Math.Sqrt(_x * _x + _y * _y + _z * _z);
			if (Media3DMath.AreClose(y, 0.0)) return 0;
			return Math.Atan2(y, _w) * (360.0 / Math.PI);
		}
	}

	/// <summary>Negates the vector part.</summary>
	public void Conjugate()
	{
		if (IsDistinguishedIdentity) return;
		_x = -_x;
		_y = -_y;
		_z = -_z;
	}

	/// <summary>Replaces the quaternion by its inverse.</summary>
	public void Invert()
	{
		if (IsDistinguishedIdentity) return;
		Conjugate();
		double norm2 = _x * _x + _y * _y + _z * _z + _w * _w;
		_x /= norm2;
		_y /= norm2;
		_z /= norm2;
		_w /= norm2;
	}

	/// <summary>Scales the quaternion to unit length.</summary>
	public void Normalize()
	{
		if (IsDistinguishedIdentity) return;
		double norm2 = _x * _x + _y * _y + _z * _z + _w * _w;
		if (norm2 > double.MaxValue)
		{
			double rmax = 1.0 / Math.Max(Math.Max(Math.Abs(_x), Math.Abs(_y)), Math.Max(Math.Abs(_z), Math.Abs(_w)));
			_x *= rmax;
			_y *= rmax;
			_z *= rmax;
			_w *= rmax;
			norm2 = _x * _x + _y * _y + _z * _z + _w * _w;
		}
		double inv = 1.0 / Math.Sqrt(norm2);
		_x *= inv;
		_y *= inv;
		_z *= inv;
		_w *= inv;
	}
}
