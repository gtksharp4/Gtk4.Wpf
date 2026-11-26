using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>A displacement in 3D space. Mirrors <c>System.Windows.Media.Media3D.Vector3D</c>.</summary>
[TypeConverter(typeof(Vector3DConverter))]
public struct Vector3D : IFormattable
{
	double _x, _y, _z;

	/// <summary>Creates a vector.</summary>
	public Vector3D(double x, double y, double z)
	{
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>The X component.</summary>
	public double X { get => _x; set => _x = value; }

	/// <summary>The Y component.</summary>
	public double Y { get => _y; set => _y = value; }

	/// <summary>The Z component.</summary>
	public double Z { get => _z; set => _z = value; }

	/// <summary>The length of the vector.</summary>
	public double Length => Math.Sqrt(_x * _x + _y * _y + _z * _z);

	/// <summary>The squared length of the vector.</summary>
	public double LengthSquared => _x * _x + _y * _y + _z * _z;

	/// <summary>Scales the vector to unit length (guarding against overflow).</summary>
	public void Normalize()
	{
		double max = Math.Max(Math.Abs(_x), Math.Max(Math.Abs(_y), Math.Abs(_z)));
		if (max == 0) return;
		_x /= max;
		_y /= max;
		_z /= max;
		this /= Length;
	}

	/// <summary>Negates the vector.</summary>
	public void Negate()
	{
		_x = -_x;
		_y = -_y;
		_z = -_z;
	}

	/// <summary>Adds two vectors.</summary>
	public static Vector3D Add(Vector3D vector1, Vector3D vector2) =>
		new Vector3D(vector1._x + vector2._x, vector1._y + vector2._y, vector1._z + vector2._z);

	/// <summary>Adds a vector to a point.</summary>
	public static Point3D Add(Vector3D vector, Point3D point) =>
		new Point3D(vector._x + point.X, vector._y + point.Y, vector._z + point.Z);

	/// <summary>Subtracts two vectors.</summary>
	public static Vector3D Subtract(Vector3D vector1, Vector3D vector2) =>
		new Vector3D(vector1._x - vector2._x, vector1._y - vector2._y, vector1._z - vector2._z);

	/// <summary>Subtracts a point from a vector.</summary>
	public static Point3D Subtract(Vector3D vector, Point3D point) =>
		new Point3D(vector._x - point.X, vector._y - point.Y, vector._z - point.Z);

	/// <summary>Scales a vector.</summary>
	public static Vector3D Multiply(Vector3D vector, double scalar) =>
		new Vector3D(vector._x * scalar, vector._y * scalar, vector._z * scalar);

	/// <summary>Scales a vector.</summary>
	public static Vector3D Multiply(double scalar, Vector3D vector) => Multiply(vector, scalar);

	/// <summary>Divides a vector by a scalar.</summary>
	public static Vector3D Divide(Vector3D vector, double scalar) => Multiply(vector, 1.0 / scalar);

	/// <summary>Transforms a vector by a matrix (translation is ignored).</summary>
	public static Vector3D Multiply(Vector3D vector, Matrix3D matrix) => matrix.Transform(vector);

	/// <summary>The dot product.</summary>
	public static double DotProduct(Vector3D vector1, Vector3D vector2) =>
		vector1._x * vector2._x + vector1._y * vector2._y + vector1._z * vector2._z;

	/// <summary>The cross product.</summary>
	public static Vector3D CrossProduct(Vector3D vector1, Vector3D vector2) =>
		new Vector3D(
			vector1._y * vector2._z - vector1._z * vector2._y,
			vector1._z * vector2._x - vector1._x * vector2._z,
			vector1._x * vector2._y - vector1._y * vector2._x);

	/// <summary>The angle between two vectors in degrees.</summary>
	public static double AngleBetween(Vector3D vector1, Vector3D vector2)
	{
		vector1.Normalize();
		vector2.Normalize();
		double theta = DotProduct(vector1, vector2) < 0
			? Math.PI - 2.0 * Math.Asin((-vector1 - vector2).Length / 2.0)
			: 2.0 * Math.Asin((vector1 - vector2).Length / 2.0);
		return theta * (180.0 / Math.PI);
	}

	/// <summary>Negates a vector.</summary>
	public static Vector3D operator -(Vector3D vector) => new Vector3D(-vector._x, -vector._y, -vector._z);

	/// <summary>Adds two vectors.</summary>
	public static Vector3D operator +(Vector3D vector1, Vector3D vector2) => Add(vector1, vector2);

	/// <summary>Subtracts two vectors.</summary>
	public static Vector3D operator -(Vector3D vector1, Vector3D vector2) => Subtract(vector1, vector2);

	/// <summary>Adds a vector to a point.</summary>
	public static Point3D operator +(Vector3D vector, Point3D point) => Add(vector, point);

	/// <summary>Subtracts a point from a vector.</summary>
	public static Point3D operator -(Vector3D vector, Point3D point) => Subtract(vector, point);

	/// <summary>Scales a vector.</summary>
	public static Vector3D operator *(Vector3D vector, double scalar) => Multiply(vector, scalar);

	/// <summary>Scales a vector.</summary>
	public static Vector3D operator *(double scalar, Vector3D vector) => Multiply(vector, scalar);

	/// <summary>Divides a vector by a scalar.</summary>
	public static Vector3D operator /(Vector3D vector, double scalar) => Divide(vector, scalar);

	/// <summary>Transforms a vector by a matrix.</summary>
	public static Vector3D operator *(Vector3D vector, Matrix3D matrix) => matrix.Transform(vector);

	/// <summary>Converts the vector to a point.</summary>
	public static explicit operator Point3D(Vector3D vector) => new Point3D(vector._x, vector._y, vector._z);

	/// <summary>Converts the vector to a size using absolute values.</summary>
	public static explicit operator Size3D(Vector3D vector) =>
		new Size3D(Math.Abs(vector._x), Math.Abs(vector._y), Math.Abs(vector._z));

	/// <summary>Compares two vectors for exact equality.</summary>
	public static bool operator ==(Vector3D vector1, Vector3D vector2) =>
		vector1._x == vector2._x && vector1._y == vector2._y && vector1._z == vector2._z;

	/// <summary>Compares two vectors for inequality.</summary>
	public static bool operator !=(Vector3D vector1, Vector3D vector2) => !(vector1 == vector2);

	/// <summary>Compares two vectors.</summary>
	public static bool Equals(Vector3D vector1, Vector3D vector2) =>
		vector1._x.Equals(vector2._x) && vector1._y.Equals(vector2._y) && vector1._z.Equals(vector2._z);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Vector3D v && Equals(this, v);

	/// <summary>Compares this vector with another.</summary>
	public bool Equals(Vector3D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();

	/// <summary>Converts three comma or whitespace separated numbers to a vector.</summary>
	public static Vector3D Parse(string source)
	{
		var v = Media3DMath.ReadDoubles(source, 3);
		return new Vector3D(v[0], v[1], v[2]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		Media3DMath.Join(provider, format, _x, _y, _z);
}
