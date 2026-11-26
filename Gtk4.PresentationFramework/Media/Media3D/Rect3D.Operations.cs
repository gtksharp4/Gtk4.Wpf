namespace System.Windows.Media.Media3D;

public partial struct Rect3D
{
	/// <summary>Whether the point lies inside or on the box.</summary>
	public bool Contains(Point3D point) => Contains(point.X, point.Y, point.Z);

	/// <summary>Whether the coordinates lie inside or on the box.</summary>
	public bool Contains(double x, double y, double z) =>
		!IsEmpty && x >= _x && x <= _x + _sx && y >= _y && y <= _y + _sy && z >= _z && z <= _z + _sz;

	/// <summary>Whether <paramref name="rect"/> lies entirely inside this box.</summary>
	public bool Contains(Rect3D rect) =>
		!IsEmpty && !rect.IsEmpty
		&& _x <= rect._x && _y <= rect._y && _z <= rect._z
		&& _x + _sx >= rect._x + rect._sx && _y + _sy >= rect._y + rect._sy && _z + _sz >= rect._z + rect._sz;

	/// <summary>Whether the boxes overlap.</summary>
	public bool IntersectsWith(Rect3D rect) =>
		!IsEmpty && !rect.IsEmpty
		&& rect._x <= _x + _sx && rect._x + rect._sx >= _x
		&& rect._y <= _y + _sy && rect._y + rect._sy >= _y
		&& rect._z <= _z + _sz && rect._z + rect._sz >= _z;

	/// <summary>Replaces this box by its overlap with <paramref name="rect"/>.</summary>
	public void Intersect(Rect3D rect)
	{
		if (!IntersectsWith(rect))
		{
			this = Empty;
			return;
		}
		double x = Math.Max(_x, rect._x), y = Math.Max(_y, rect._y), z = Math.Max(_z, rect._z);
		_sx = Math.Min(_x + _sx, rect._x + rect._sx) - x;
		_sy = Math.Min(_y + _sy, rect._y + rect._sy) - y;
		_sz = Math.Min(_z + _sz, rect._z + rect._sz) - z;
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>The overlap of two boxes.</summary>
	public static Rect3D Intersect(Rect3D rect1, Rect3D rect2)
	{
		rect1.Intersect(rect2);
		return rect1;
	}

	/// <summary>Grows this box to include <paramref name="rect"/>.</summary>
	public void Union(Rect3D rect)
	{
		if (IsEmpty)
		{
			this = rect;
			return;
		}
		if (rect.IsEmpty) return;
		double x = Math.Min(_x, rect._x), y = Math.Min(_y, rect._y), z = Math.Min(_z, rect._z);
		_sx = Math.Max(_x + _sx, rect._x + rect._sx) - x;
		_sy = Math.Max(_y + _sy, rect._y + rect._sy) - y;
		_sz = Math.Max(_z + _sz, rect._z + rect._sz) - z;
		_x = x;
		_y = y;
		_z = z;
	}

	/// <summary>The union of two boxes.</summary>
	public static Rect3D Union(Rect3D rect1, Rect3D rect2)
	{
		rect1.Union(rect2);
		return rect1;
	}

	/// <summary>Grows this box to include a point.</summary>
	public void Union(Point3D point) => Union(new Rect3D(point, default(Size3D)));

	/// <summary>The union of a box and a point.</summary>
	public static Rect3D Union(Rect3D rect, Point3D point)
	{
		rect.Union(point);
		return rect;
	}

	/// <summary>Moves the box.</summary>
	public void Offset(double offsetX, double offsetY, double offsetZ)
	{
		Guard();
		_x += offsetX;
		_y += offsetY;
		_z += offsetZ;
	}

	/// <summary>Moves the box.</summary>
	public void Offset(Vector3D offsetVector) => Offset(offsetVector.X, offsetVector.Y, offsetVector.Z);

	/// <summary>Compares two boxes for exact equality.</summary>
	public static bool operator ==(Rect3D rect1, Rect3D rect2) =>
		rect1._x == rect2._x && rect1._y == rect2._y && rect1._z == rect2._z
		&& rect1._sx == rect2._sx && rect1._sy == rect2._sy && rect1._sz == rect2._sz;

	/// <summary>Compares two boxes for inequality.</summary>
	public static bool operator !=(Rect3D rect1, Rect3D rect2) => !(rect1 == rect2);

	/// <summary>Compares two boxes.</summary>
	public static bool Equals(Rect3D rect1, Rect3D rect2) =>
		rect1.IsEmpty ? rect2.IsEmpty
			: rect1._x.Equals(rect2._x) && rect1._y.Equals(rect2._y) && rect1._z.Equals(rect2._z)
			&& rect1._sx.Equals(rect2._sx) && rect1._sy.Equals(rect2._sy) && rect1._sz.Equals(rect2._sz);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Rect3D r && Equals(this, r);

	/// <summary>Compares this box with another.</summary>
	public bool Equals(Rect3D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => IsEmpty
		? 0
		: _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode()
			^ _sx.GetHashCode() ^ _sy.GetHashCode() ^ _sz.GetHashCode();

	/// <summary>Converts "Empty" or six numbers to a box.</summary>
	public static Rect3D Parse(string source)
	{
		if (Media3DMath.IsKeyword(source, "Empty")) return Empty;
		var v = Media3DMath.ReadDoubles(source, 6);
		return new Rect3D(v[0], v[1], v[2], v[3], v[4], v[5]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		IsEmpty ? "Empty" : Media3DMath.Join(provider, format, _x, _y, _z, _sx, _sy, _sz);
}
