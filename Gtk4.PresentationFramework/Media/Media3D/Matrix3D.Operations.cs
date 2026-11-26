namespace System.Windows.Media.Media3D;

public partial struct Matrix3D
{
	/// <summary>The matrix product (<paramref name="matrix1"/> applied first).</summary>
	public static Matrix3D Multiply(Matrix3D matrix1, Matrix3D matrix2)
	{
		if (matrix1._m == null) return matrix2;
		if (matrix2._m == null) return matrix1;
		var a = matrix1._m;
		var b = matrix2._m;
		var r = new double[16];
		for (int i = 0; i < 4; i++)
			for (int j = 0; j < 4; j++)
				r[i * 4 + j] = a[i * 4] * b[j] + a[i * 4 + 1] * b[4 + j] + a[i * 4 + 2] * b[8 + j]
					+ a[i * 4 + 3] * b[12 + j];
		return new Matrix3D(r);
	}

	/// <summary>The matrix product.</summary>
	public static Matrix3D operator *(Matrix3D matrix1, Matrix3D matrix2) => Multiply(matrix1, matrix2);

	/// <summary>Applies <paramref name="matrix"/> before this matrix.</summary>
	public void Prepend(Matrix3D matrix) => this = Multiply(matrix, this);

	/// <summary>Applies <paramref name="matrix"/> after this matrix.</summary>
	public void Append(Matrix3D matrix) => this = Multiply(this, matrix);

	/// <summary>Appends a rotation.</summary>
	public void Rotate(Quaternion quaternion) => Append(RotationMatrix(quaternion, default));

	/// <summary>Prepends a rotation.</summary>
	public void RotatePrepend(Quaternion quaternion) => Prepend(RotationMatrix(quaternion, default));

	/// <summary>Appends a rotation about <paramref name="center"/>.</summary>
	public void RotateAt(Quaternion quaternion, Point3D center) => Append(RotationMatrix(quaternion, center));

	/// <summary>Prepends a rotation about <paramref name="center"/>.</summary>
	public void RotateAtPrepend(Quaternion quaternion, Point3D center) =>
		Prepend(RotationMatrix(quaternion, center));

	/// <summary>Appends a scale.</summary>
	public void Scale(Vector3D scale) => Append(ScaleMatrix(scale, default));

	/// <summary>Prepends a scale.</summary>
	public void ScalePrepend(Vector3D scale) => Prepend(ScaleMatrix(scale, default));

	/// <summary>Appends a scale about <paramref name="center"/>.</summary>
	public void ScaleAt(Vector3D scale, Point3D center) => Append(ScaleMatrix(scale, center));

	/// <summary>Prepends a scale about <paramref name="center"/>.</summary>
	public void ScaleAtPrepend(Vector3D scale, Point3D center) => Prepend(ScaleMatrix(scale, center));

	/// <summary>Appends a translation.</summary>
	public void Translate(Vector3D offset) => Append(TranslationMatrix(offset));

	/// <summary>Prepends a translation.</summary>
	public void TranslatePrepend(Vector3D offset) => Prepend(TranslationMatrix(offset));

	static Matrix3D TranslationMatrix(Vector3D o) =>
		new Matrix3D(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, o.X, o.Y, o.Z, 1);

	static Matrix3D ScaleMatrix(Vector3D s, Point3D c) =>
		new Matrix3D(s.X, 0, 0, 0, 0, s.Y, 0, 0, 0, 0, s.Z, 0,
			c.X * (1 - s.X), c.Y * (1 - s.Y), c.Z * (1 - s.Z), 1);

	static Matrix3D RotationMatrix(Quaternion q, Point3D c)
	{
		double x2 = q.X + q.X, y2 = q.Y + q.Y, z2 = q.Z + q.Z;
		double xx = q.X * x2, xy = q.X * y2, xz = q.X * z2;
		double yy = q.Y * y2, yz = q.Y * z2, zz = q.Z * z2;
		double wx = q.W * x2, wy = q.W * y2, wz = q.W * z2;
		double m11 = 1 - (yy + zz), m12 = xy + wz, m13 = xz - wy;
		double m21 = xy - wz, m22 = 1 - (xx + zz), m23 = yz + wx;
		double m31 = xz + wy, m32 = yz - wx, m33 = 1 - (xx + yy);
		return new Matrix3D(m11, m12, m13, 0, m21, m22, m23, 0, m31, m32, m33, 0,
			c.X - c.X * m11 - c.Y * m21 - c.Z * m31,
			c.Y - c.X * m12 - c.Y * m22 - c.Z * m32,
			c.Z - c.X * m13 - c.Y * m23 - c.Z * m33, 1);
	}

	/// <summary>Transforms a point (with perspective divide when the matrix is not affine).</summary>
	public Point3D Transform(Point3D point)
	{
		double x = point.X * M11 + point.Y * M21 + point.Z * M31 + OffsetX;
		double y = point.X * M12 + point.Y * M22 + point.Z * M32 + OffsetY;
		double z = point.X * M13 + point.Y * M23 + point.Z * M33 + OffsetZ;
		if (!IsAffine)
		{
			double w = point.X * M14 + point.Y * M24 + point.Z * M34 + M44;
			x /= w;
			y /= w;
			z /= w;
		}
		return new Point3D(x, y, z);
	}

	/// <summary>Transforms points in place.</summary>
	public void Transform(Point3D[] points)
	{
		if (points == null) return;
		for (int i = 0; i < points.Length; i++) points[i] = Transform(points[i]);
	}

	/// <summary>Transforms a vector (translation is ignored).</summary>
	public Vector3D Transform(Vector3D vector) => new Vector3D(
		vector.X * M11 + vector.Y * M21 + vector.Z * M31,
		vector.X * M12 + vector.Y * M22 + vector.Z * M32,
		vector.X * M13 + vector.Y * M23 + vector.Z * M33);

	/// <summary>Transforms vectors in place.</summary>
	public void Transform(Vector3D[] vectors)
	{
		if (vectors == null) return;
		for (int i = 0; i < vectors.Length; i++) vectors[i] = Transform(vectors[i]);
	}

	/// <summary>Transforms a homogeneous point.</summary>
	public Point4D Transform(Point4D point) => new Point4D(
		point.X * M11 + point.Y * M21 + point.Z * M31 + point.W * OffsetX,
		point.X * M12 + point.Y * M22 + point.Z * M32 + point.W * OffsetY,
		point.X * M13 + point.Y * M23 + point.Z * M33 + point.W * OffsetZ,
		point.X * M14 + point.Y * M24 + point.Z * M34 + point.W * M44);

	/// <summary>Transforms homogeneous points in place.</summary>
	public void Transform(Point4D[] points)
	{
		if (points == null) return;
		for (int i = 0; i < points.Length; i++) points[i] = Transform(points[i]);
	}

	/// <summary>Compares two matrices for exact equality.</summary>
	public static bool operator ==(Matrix3D matrix1, Matrix3D matrix2) => Equals(matrix1, matrix2);

	/// <summary>Compares two matrices for inequality.</summary>
	public static bool operator !=(Matrix3D matrix1, Matrix3D matrix2) => !Equals(matrix1, matrix2);

	/// <summary>Compares two matrices.</summary>
	public static bool Equals(Matrix3D matrix1, Matrix3D matrix2)
	{
		for (int i = 0; i < 16; i++)
			if (!matrix1.At(i).Equals(matrix2.At(i))) return false;
		return true;
	}

	/// <inheritdoc/>
	public override bool Equals(object? o) => o is Matrix3D m && Equals(this, m);

	/// <summary>Compares this matrix with another.</summary>
	public bool Equals(Matrix3D value) => Equals(this, value);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		int hash = 0;
		for (int i = 0; i < 16; i++) hash ^= At(i).GetHashCode();
		return hash;
	}

	/// <summary>Converts "Identity" or sixteen numbers to a matrix.</summary>
	public static Matrix3D Parse(string source)
	{
		if (Media3DMath.IsKeyword(source, "Identity")) return Identity;
		return new Matrix3D(Media3DMath.ReadDoubles(source, 16));
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		IsIdentity ? "Identity" : Media3DMath.Join(provider, format, ToElements());
}
