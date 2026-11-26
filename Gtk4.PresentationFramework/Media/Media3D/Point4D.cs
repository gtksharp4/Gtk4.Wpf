using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>A homogeneous 3D point. Mirrors <c>System.Windows.Media.Media3D.Point4D</c>.</summary>
[TypeConverter(typeof(Point4DConverter))]
public struct Point4D : IFormattable
{
	double _x, _y, _z, _w;

	/// <summary>Creates a point.</summary>
	public Point4D(double x, double y, double z, double w)
	{
		_x = x;
		_y = y;
		_z = z;
		_w = w;
	}

	/// <summary>The X coordinate.</summary>
	public double X { get => _x; set => _x = value; }

	/// <summary>The Y coordinate.</summary>
	public double Y { get => _y; set => _y = value; }

	/// <summary>The Z coordinate.</summary>
	public double Z { get => _z; set => _z = value; }

	/// <summary>The W coordinate.</summary>
	public double W { get => _w; set => _w = value; }

	/// <summary>Moves the point by the given offsets.</summary>
	public void Offset(double deltaX, double deltaY, double deltaZ, double deltaW)
	{
		_x += deltaX;
		_y += deltaY;
		_z += deltaZ;
		_w += deltaW;
	}

	/// <summary>Adds two points.</summary>
	public static Point4D Add(Point4D point1, Point4D point2) =>
		new Point4D(point1._x + point2._x, point1._y + point2._y, point1._z + point2._z, point1._w + point2._w);

	/// <summary>Subtracts two points.</summary>
	public static Point4D Subtract(Point4D point1, Point4D point2) =>
		new Point4D(point1._x - point2._x, point1._y - point2._y, point1._z - point2._z, point1._w - point2._w);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point4D Multiply(Point4D point, Matrix3D matrix) => matrix.Transform(point);

	/// <summary>Adds two points.</summary>
	public static Point4D operator +(Point4D point1, Point4D point2) => Add(point1, point2);

	/// <summary>Subtracts two points.</summary>
	public static Point4D operator -(Point4D point1, Point4D point2) => Subtract(point1, point2);

	/// <summary>Transforms a point by a matrix.</summary>
	public static Point4D operator *(Point4D point, Matrix3D matrix) => matrix.Transform(point);

	/// <summary>Compares two points for exact equality.</summary>
	public static bool operator ==(Point4D point1, Point4D point2) =>
		point1._x == point2._x && point1._y == point2._y && point1._z == point2._z && point1._w == point2._w;

	/// <summary>Compares two points for inequality.</summary>
	public static bool operator !=(Point4D point1, Point4D point2) => !(point1 == point2);

	/// <summary>Compares two points.</summary>
	public static bool Equals(Point4D point1, Point4D point2) =>
		point1._x.Equals(point2._x) && point1._y.Equals(point2._y)
		&& point1._z.Equals(point2._z) && point1._w.Equals(point2._w);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Point4D p && Equals(this, p);

	/// <summary>Compares this point with another.</summary>
	public bool Equals(Point4D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode() ^ _w.GetHashCode();

	/// <summary>Converts four comma or whitespace separated numbers to a point.</summary>
	public static Point4D Parse(string source)
	{
		var v = Media3DMath.ReadDoubles(source, 4);
		return new Point4D(v[0], v[1], v[2], v[3]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		Media3DMath.Join(provider, format, _x, _y, _z, _w);
}
