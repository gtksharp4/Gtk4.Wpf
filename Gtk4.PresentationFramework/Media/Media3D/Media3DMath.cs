using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Tolerances and parsing helpers shared by the 3D math types.</summary>
static class Media3DMath
{
	const double Epsilon = 2.2204460492503131e-16;

	internal static bool IsZero(double value) => Math.Abs(value) < 10.0 * Epsilon;

	internal static bool IsOne(double value) => Math.Abs(value - 1.0) < 10.0 * Epsilon;

	internal static bool AreClose(double a, double b)
	{
		if (a == b) return true;
		double eps = (Math.Abs(a) + Math.Abs(b) + 10.0) * Epsilon;
		double delta = a - b;
		return -eps < delta && eps > delta;
	}

	internal static string Join(IFormatProvider? provider, string? format, params double[] values)
	{
		char sep = TokenizerHelper.GetNumericListSeparator(provider);
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < values.Length; i++)
		{
			if (i > 0) sb.Append(sep);
			sb.Append(values[i].ToString(format, provider));
		}
		return sb.ToString();
	}

	internal static bool IsKeyword(string source, string keyword) =>
		string.Equals(source.Trim(), keyword, StringComparison.Ordinal);

	internal static double[] ReadDoubles(string source, int count)
	{
		var reader = new TokenizerHelper.Reader(source);
		var values = new double[count];
		for (int i = 0; i < count; i++) values[i] = reader.NextDouble();
		reader.LastRequired();
		return values;
	}
}
