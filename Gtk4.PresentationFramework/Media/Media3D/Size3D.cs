using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>The extent of a 3D box. Mirrors <c>System.Windows.Media.Media3D.Size3D</c>.</summary>
[TypeConverter(typeof(Size3DConverter))]
public struct Size3D : IFormattable
{
	double _x, _y, _z;

	/// <summary>Creates a size; a negative component throws.</summary>
	public Size3D(double x, double y, double z)
	{
		if (x < 0 || y < 0 || z < 0)
			throw new ArgumentException("Size3D dimensions cannot be negative.");
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>The empty size (negative infinity in every dimension).</summary>
	public static Size3D Empty { get; } = new Size3D
	{
		_x = double.NegativeInfinity,
		_y = double.NegativeInfinity,
		_z = double.NegativeInfinity,
	};

	/// <summary>Whether this is <see cref="Empty"/>.</summary>
	public bool IsEmpty => _x < 0;

	/// <summary>The X extent.</summary>
	public double X { get => _x; set => _x = Check(value); }

	/// <summary>The Y extent.</summary>
	public double Y { get => _y; set => _y = Check(value); }

	/// <summary>The Z extent.</summary>
	public double Z { get => _z; set => _z = Check(value); }

	double Check(double value)
	{
		if (IsEmpty) throw new InvalidOperationException("Cannot modify the empty Size3D.");
		if (value < 0) throw new ArgumentException("Size3D dimensions cannot be negative.");
		return value;
	}

	/// <summary>Converts the size to a vector.</summary>
	public static explicit operator Vector3D(Size3D size) => new Vector3D(size._x, size._y, size._z);

	/// <summary>Converts the size to a point.</summary>
	public static explicit operator Point3D(Size3D size) => new Point3D(size._x, size._y, size._z);

	/// <summary>Compares two sizes for exact equality.</summary>
	public static bool operator ==(Size3D size1, Size3D size2) =>
		size1._x == size2._x && size1._y == size2._y && size1._z == size2._z;

	/// <summary>Compares two sizes for inequality.</summary>
	public static bool operator !=(Size3D size1, Size3D size2) => !(size1 == size2);

	/// <summary>Compares two sizes.</summary>
	public static bool Equals(Size3D size1, Size3D size2) =>
		size1.IsEmpty ? size2.IsEmpty
			: size1._x.Equals(size2._x) && size1._y.Equals(size2._y) && size1._z.Equals(size2._z);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Size3D s && Equals(this, s);

	/// <summary>Compares this size with another.</summary>
	public bool Equals(Size3D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => IsEmpty ? 0 : _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();

	/// <summary>Converts "Empty" or three numbers to a size.</summary>
	public static Size3D Parse(string source)
	{
		if (Media3DMath.IsKeyword(source, "Empty")) return Empty;
		var v = Media3DMath.ReadDoubles(source, 3);
		return new Size3D(v[0], v[1], v[2]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		IsEmpty ? "Empty" : Media3DMath.Join(provider, format, _x, _y, _z);
}
