using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>A point in 3D space. Mirrors <c>System.Windows.Media.Media3D.Point3D</c>.</summary>
[TypeConverter(typeof(Point3DConverter))]
public struct Point3D : IFormattable
{
	double _x, _y, _z;

	/// <summary>Creates a point.</summary>
	public Point3D(double x, double y, double z)
	{
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>The X coordinate.</summary>
	public double X { get => _x; set => _x = value; }

	/// <summary>The Y coordinate.</summary>
	public double Y { get => _y; set => _y = value; }

	/// <summary>The Z coordinate.</summary>
	public double Z { get => _z; set => _z = value; }

	/// <summary>Moves the point by the given offsets.</summary>
	public void Offset(double offsetX, double offsetY, double offsetZ)
	{
		_x += offsetX;
		_y += offsetY;
		_z += offsetZ;
	}

	/// <summary>Adds a vector to a point.</summary>
	public static Point3D Add(Point3D point, Vector3D vector) =>
		new Point3D(point._x + vector.X, point._y + vector.Y, point._z + vector.Z);

	/// <summary>Subtracts a vector from a point.</summary>
	public static Point3D Subtract(Point3D point, Vector3D vector) =>
		new Point3D(point._x - vector.X, point._y - vector.Y, point._z - vector.Z);

	/// <summary>The vector from <paramref name="point2"/> to <paramref name="point1"/>.</summary>
	public static Vector3D Subtract(Point3D point1, Point3D point2) =>
		new Vector3D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point3D Multiply(Point3D point, Matrix3D matrix) => matrix.Transform(point);

	/// <summary>Adds a vector to a point.</summary>
	public static Point3D operator +(Point3D point, Vector3D vector) => Add(point, vector);

	/// <summary>Subtracts a vector from a point.</summary>
	public static Point3D operator -(Point3D point, Vector3D vector) => Subtract(point, vector);

	/// <summary>The vector between two points.</summary>
	public static Vector3D operator -(Point3D point1, Point3D point2) => Subtract(point1, point2);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point3D operator *(Point3D point, Matrix3D matrix) => matrix.Transform(point);

	/// <summary>Converts the point to a vector.</summary>
	public static explicit operator Vector3D(Point3D point) => new Vector3D(point._x, point._y, point._z);

	/// <summary>Converts the point to a homogeneous point with W = 1.</summary>
	public static explicit operator Point4D(Point3D point) => new Point4D(point._x, point._y, point._z, 1.0);

	/// <summary>Compares two points for exact equality.</summary>
	public static bool operator ==(Point3D point1, Point3D point2) =>
		point1._x == point2._x && point1._y == point2._y && point1._z == point2._z;

	/// <summary>Compares two points for inequality.</summary>
	public static bool operator !=(Point3D point1, Point3D point2) => !(point1 == point2);

	/// <summary>Compares two points.</summary>
	public static bool Equals(Point3D point1, Point3D point2) =>
		point1._x.Equals(point2._x) && point1._y.Equals(point2._y) && point1._z.Equals(point2._z);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Point3D p && Equals(this, p);

	/// <summary>Compares this point with another.</summary>
	public bool Equals(Point3D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();

	/// <summary>Converts three comma or whitespace separated numbers to a point.</summary>
	public static Point3D Parse(string source)
	{
		var v = Media3DMath.ReadDoubles(source, 3);
		return new Point3D(v[0], v[1], v[2]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		Media3DMath.Join(provider, format, _x, _y, _z);
}
