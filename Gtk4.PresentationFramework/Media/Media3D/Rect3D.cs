using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>An axis-aligned 3D box. Mirrors <c>System.Windows.Media.Media3D.Rect3D</c>.</summary>
[TypeConverter(typeof(Rect3DConverter))]
public partial struct Rect3D : IFormattable
{
	double _x, _y, _z, _sx, _sy, _sz;

	/// <summary>Creates a box from a corner and a size.</summary>
	public Rect3D(Point3D location, Size3D size)
	{
		if (size.IsEmpty)
		{
			this = Empty;
			return;
		}
		_x = location.X;
		_y = location.Y;
		_z = location.Z;
		_sx = size.X;
		_sy = size.Y;
		_sz = size.Z;
	}

	/// <summary>Creates a box from coordinates; a negative size throws.</summary>
	public Rect3D(double x, double y, double z, double sizeX, double sizeY, double sizeZ)
	{
		if (sizeX < 0 || sizeY < 0 || sizeZ < 0)
			throw new ArgumentException("Rect3D sizes cannot be negative.");
		_x = x;
		_y = y;
		_z = z;
		_sx = sizeX;
		_sy = sizeY;
		_sz = sizeZ;
	}

	/// <summary>The empty box.</summary>
	public static Rect3D Empty { get; } = new Rect3D
	{
		_x = double.PositiveInfinity, _y = double.PositiveInfinity, _z = double.PositiveInfinity,
		_sx = double.NegativeInfinity, _sy = double.NegativeInfinity, _sz = double.NegativeInfinity,
	};

	/// <summary>Whether the box is <see cref="Empty"/>.</summary>
	public bool IsEmpty => _sx < 0;

	/// <summary>The corner with the smallest coordinates.</summary>
	public Point3D Location
	{
		get => new Point3D(_x, _y, _z);
		set
		{
			Guard();
			_x = value.X;
			_y = value.Y;
			_z = value.Z;
		}
	}

	/// <summary>The extent.</summary>
	public Size3D Size
	{
		get => IsEmpty ? Size3D.Empty : new Size3D(_sx, _sy, _sz);
		set
		{
			if (value.IsEmpty)
			{
				this = Empty;
				return;
			}
			Guard();
			_sx = value.X;
			_sy = value.Y;
			_sz = value.Z;
		}
	}

	/// <summary>The X of the location.</summary>
	public double X { get => _x; set { Guard(); _x = value; } }

	/// <summary>The Y of the location.</summary>
	public double Y { get => _y; set { Guard(); _y = value; } }

	/// <summary>The Z of the location.</summary>
	public double Z { get => _z; set { Guard(); _z = value; } }

	/// <summary>The X extent.</summary>
	public double SizeX { get => _sx; set { Guard(); _sx = NonNegative(value); } }

	/// <summary>The Y extent.</summary>
	public double SizeY { get => _sy; set { Guard(); _sy = NonNegative(value); } }

	/// <summary>The Z extent.</summary>
	public double SizeZ { get => _sz; set { Guard(); _sz = NonNegative(value); } }

	void Guard()
	{
		if (IsEmpty) throw new InvalidOperationException("Cannot modify the empty Rect3D.");
	}

	static double NonNegative(double value) =>
		value < 0 ? throw new ArgumentException("Rect3D sizes cannot be negative.") : value;
}
