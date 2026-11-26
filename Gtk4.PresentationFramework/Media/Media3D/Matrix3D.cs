using System.ComponentModel;

namespace System.Windows.Media.Media3D;

/// <summary>
/// A 4x4 transform in row-vector convention. Mirrors <c>System.Windows.Media.Media3D.Matrix3D</c>;
/// <c>default</c> is the identity.
/// </summary>
[TypeConverter(typeof(Matrix3DConverter))]
public partial struct Matrix3D : IFormattable
{
	// Row-major storage; null reads as identity so default(Matrix3D) is Identity.
	double[]? _m;

	/// <summary>Creates a matrix from its 16 elements.</summary>
	public Matrix3D(
		double m11, double m12, double m13, double m14,
		double m21, double m22, double m23, double m24,
		double m31, double m32, double m33, double m34,
		double offsetX, double offsetY, double offsetZ, double m44)
	{
		_m = new[] { m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, offsetX, offsetY, offsetZ, m44 };
	}

	Matrix3D(double[] m) => _m = m;

	/// <summary>The identity matrix.</summary>
	public static Matrix3D Identity => new Matrix3D(IdentityElements());

	static double[] IdentityElements() => new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

	double[] Elements() => _m == null ? IdentityElements() : (double[])_m.Clone();

	double At(int i) => _m == null ? (i % 5 == 0 ? 1.0 : 0.0) : _m[i];

	void Set(int i, double value)
	{
		_m = Elements();
		_m[i] = value;
	}

	/// <summary>Element M11.</summary>
	public double M11 { get => At(0); set => Set(0, value); }

	/// <summary>Element M12.</summary>
	public double M12 { get => At(1); set => Set(1, value); }

	/// <summary>Element M13.</summary>
	public double M13 { get => At(2); set => Set(2, value); }

	/// <summary>Element M14.</summary>
	public double M14 { get => At(3); set => Set(3, value); }

	/// <summary>Element M21.</summary>
	public double M21 { get => At(4); set => Set(4, value); }

	/// <summary>Element M22.</summary>
	public double M22 { get => At(5); set => Set(5, value); }

	/// <summary>Element M23.</summary>
	public double M23 { get => At(6); set => Set(6, value); }

	/// <summary>Element M24.</summary>
	public double M24 { get => At(7); set => Set(7, value); }

	/// <summary>Element M31.</summary>
	public double M31 { get => At(8); set => Set(8, value); }

	/// <summary>Element M32.</summary>
	public double M32 { get => At(9); set => Set(9, value); }

	/// <summary>Element M33.</summary>
	public double M33 { get => At(10); set => Set(10, value); }

	/// <summary>Element M34.</summary>
	public double M34 { get => At(11); set => Set(11, value); }

	/// <summary>Element M44.</summary>
	public double M44 { get => At(15); set => Set(15, value); }

	/// <summary>The X translation (M41).</summary>
	public double OffsetX { get => At(12); set => Set(12, value); }

	/// <summary>The Y translation (M42).</summary>
	public double OffsetY { get => At(13); set => Set(13, value); }

	/// <summary>The Z translation (M43).</summary>
	public double OffsetZ { get => At(14); set => Set(14, value); }

	/// <summary>Whether this is the identity matrix.</summary>
	public bool IsIdentity
	{
		get
		{
			for (int i = 0; i < 16; i++)
				if (At(i) != (i % 5 == 0 ? 1.0 : 0.0)) return false;
			return true;
		}
	}

	/// <summary>Whether the last column is (0, 0, 0, 1).</summary>
	public bool IsAffine => M14 == 0 && M24 == 0 && M34 == 0 && M44 == 1;

	/// <summary>Resets to the identity matrix.</summary>
	public void SetIdentity() => _m = null;

	/// <summary>The determinant.</summary>
	public double Determinant => Det(Elements());

	/// <summary>Whether the matrix can be inverted.</summary>
	public bool HasInverse => !Media3DMath.IsZero(Determinant);

	/// <summary>Inverts the matrix.</summary>
	/// <exception cref="InvalidOperationException">The matrix is not invertible.</exception>
	public void Invert()
	{
		var e = Elements();
		double det = Det(e);
		if (Media3DMath.IsZero(det)) throw new InvalidOperationException("The matrix is not invertible.");
		var r = new double[16];
		for (int row = 0; row < 4; row++)
			for (int col = 0; col < 4; col++)
				r[col * 4 + row] = ((row + col) % 2 == 0 ? 1 : -1) * Minor(e, row, col) / det;
		_m = r;
	}

	static double Minor(double[] e, int row, int col)
	{
		var s = new double[9];
		int k = 0;
		for (int i = 0; i < 4; i++)
		{
			if (i == row) continue;
			for (int j = 0; j < 4; j++)
				if (j != col) s[k++] = e[i * 4 + j];
		}
		return s[0] * (s[4] * s[8] - s[5] * s[7]) - s[1] * (s[3] * s[8] - s[5] * s[6])
			+ s[2] * (s[3] * s[7] - s[4] * s[6]);
	}

	static double Det(double[] e)
	{
		double det = 0;
		for (int col = 0; col < 4; col++)
			det += (col % 2 == 0 ? 1 : -1) * e[col] * Minor(e, 0, col);
		return det;
	}

	internal static Matrix3D FromElements(double[] e) => new Matrix3D(e);

	internal double[] ToElements() => Elements();
}
