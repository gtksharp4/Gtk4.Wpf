using System;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Markup;

/// <summary>Converters for values the markup compiler hands over as text: lengths, transforms, images, commands.</summary>
static class UnitConverters
{
	/// <summary>WPF LengthConverter: "Auto" is NaN, plain numbers and px/in/cm/pt suffixes are accepted.</summary>
	internal static double ParseLength(string raw)
	{
		string s = raw.Trim();
		if (string.Equals(s, "Auto", StringComparison.OrdinalIgnoreCase))
			return double.NaN;
		return FontConverters.ParseWithUnits(s, "Length", allowNonFinite: true);
	}

	/// <summary>WPF TransformConverter: "Identity" or "m11,m12,m21,m22,offsetX,offsetY".</summary>
	internal static Transform ParseTransform(string raw)
	{
		string s = raw.Trim();
		if (string.Equals(s, "Identity", StringComparison.Ordinal))
			return new MatrixTransform(Matrix.Identity);

		string[] parts = s.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 6)
			throw new FormatException($"Cannot parse Transform from \"{raw}\".");
		double[] v = new double[6];
		for (int i = 0; i < 6; i++)
		{
			if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]))
				throw new FormatException($"Cannot parse Transform from \"{raw}\".");
		}
		return new MatrixTransform(new Matrix(v[0], v[1], v[2], v[3], v[4], v[5]));
	}

	/// <summary>WPF ImageSourceConverter: the string is a URI, relative or absolute.</summary>
	internal static ImageSource ParseImageSource(string raw) =>
		new System.Windows.Media.Imaging.BitmapImage(MakeUri(raw.Trim()));

	static Uri MakeUri(string s)
	{
		if (s.Length == 0)
			throw new FormatException("Cannot parse ImageSource from an empty string.");
		// .NET cannot parse the ",,," authority of pack://application:,,, URIs; keep the host and path.
		s = s.Replace('\\', '/');
		if (s.StartsWith("pack://application:,,,", StringComparison.OrdinalIgnoreCase))
			s = "pack://application" + s.Substring("pack://application:,,,".Length);
		return new Uri(s, UriKind.RelativeOrAbsolute);
	}
}
