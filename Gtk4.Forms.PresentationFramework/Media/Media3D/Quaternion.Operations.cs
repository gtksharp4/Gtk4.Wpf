namespace System.Windows.Media.Media3D;

public partial struct Quaternion
{
	/// <summary>Adds two quaternions component-wise.</summary>
	public static Quaternion Add(Quaternion left, Quaternion right) =>
		new Quaternion(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

	/// <summary>Subtracts two quaternions component-wise.</summary>
	public static Quaternion Subtract(Quaternion left, Quaternion right) =>
		new Quaternion(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

	/// <summary>Composes two rotations (<paramref name="left"/> applied after <paramref name="right"/>).</summary>
	public static Quaternion Multiply(Quaternion left, Quaternion right)
	{
		if (left.IsDistinguishedIdentity) return right;
		if (right.IsDistinguishedIdentity) return left;
		double x = left._w * right._x + left._x * right._w + left._y * right._z - left._z * right._y;
		double y = left._w * right._y + left._y * right._w + left._z * right._x - left._x * right._z;
		double z = left._w * right._z + left._z * right._w + left._x * right._y - left._y * right._x;
		double w = left._w * right._w - left._x * right._x - left._y * right._y - left._z * right._z;
		return new Quaternion(x, y, z, w);
	}

	/// <summary>Adds two quaternions.</summary>
	public static Quaternion operator +(Quaternion left, Quaternion right) => Add(left, right);

	/// <summary>Subtracts two quaternions.</summary>
	public static Quaternion operator -(Quaternion left, Quaternion right) => Subtract(left, right);

	/// <summary>Composes two rotations.</summary>
	public static Quaternion operator *(Quaternion left, Quaternion right) => Multiply(left, right);

	/// <summary>Spherical interpolation along the shortest path.</summary>
	public static Quaternion Slerp(Quaternion from, Quaternion to, double t) => Slerp(from, to, t, true);

	/// <summary>Spherical interpolation between two rotations.</summary>
	public static Quaternion Slerp(Quaternion from, Quaternion to, double t, bool useShortestPath)
	{
		if (t == 0) return from;
		if (t == 1) return to;
		if (from.IsDistinguishedIdentity) from = Identity;
		if (to.IsDistinguishedIdentity) to = Identity;
		double cosOmega = from._x * to._x + from._y * to._y + from._z * to._z + from._w * to._w;
		if (useShortestPath && cosOmega < 0)
		{
			cosOmega = -cosOmega;
			to = new Quaternion(-to._x, -to._y, -to._z, -to._w);
		}
		double scaleFrom, scaleTo;
		if (cosOmega > 1.0 - 1e-6)
		{
			scaleFrom = 1.0 - t;
			scaleTo = t;
		}
		else
		{
			double sinOmega = Math.Sqrt(1.0 - cosOmega * cosOmega);
			double omega = Math.Atan2(sinOmega, cosOmega);
			scaleFrom = Math.Sin((1.0 - t) * omega) / sinOmega;
			scaleTo = Math.Sin(t * omega) / sinOmega;
		}
		return new Quaternion(
			from._x * scaleFrom + to._x * scaleTo, from._y * scaleFrom + to._y * scaleTo,
			from._z * scaleFrom + to._z * scaleTo, from._w * scaleFrom + to._w * scaleTo);
	}

	/// <summary>Compares two quaternions for exact equality.</summary>
	public static bool operator ==(Quaternion q1, Quaternion q2) =>
		q1.X == q2.X && q1.Y == q2.Y && q1.Z == q2.Z && q1.W == q2.W;

	/// <summary>Compares two quaternions for inequality.</summary>
	public static bool operator !=(Quaternion q1, Quaternion q2) => !(q1 == q2);

	/// <summary>Compares two quaternions.</summary>
	public static bool Equals(Quaternion q1, Quaternion q2) =>
		q1.X.Equals(q2.X) && q1.Y.Equals(q2.Y) && q1.Z.Equals(q2.Z) && q1.W.Equals(q2.W);

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Quaternion q && Equals(this, q);

	/// <summary>Compares this quaternion with another.</summary>
	public bool Equals(Quaternion value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();

	/// <summary>Converts "Identity" or four numbers to a quaternion.</summary>
	public static Quaternion Parse(string source)
	{
		if (Media3DMath.IsKeyword(source, "Identity")) return Identity;
		var v = Media3DMath.ReadDoubles(source, 4);
		return new Quaternion(v[0], v[1], v[2], v[3]);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		IsIdentity ? "Identity" : Media3DMath.Join(provider, format, X, Y, Z, W);
}
