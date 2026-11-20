using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Markup;

/// <summary>
/// Converters for values the markup compiler hands over as text: lengths, transforms, images.
/// </summary>
static class UnitConverters
{
	/// <summary>WPF LengthConverter: "Auto" is NaN, plain numbers and px/in/cm/pt suffixes are accepted.</summary>
	internal static double ParseLength(string raw)
	{
		string s = raw.Trim();
		if (string.Equals(s, "Auto", StringComparison.OrdinalIgnoreCase))
			return double.NaN;
		return FontConverters.ParseWithUnits(s, "Length", allowNonFinite: true,
			NumberStyles.Float | NumberStyles.AllowThousands);
	}

	/// <summary>
	/// WPF TransformConverter: "Identity" or "m11,m12,m21,m22,offsetX,offsetY". Tokens are separated by
	/// whitespace or a single comma; a missing, empty or extra token is an InvalidOperationException.
	/// </summary>
	internal static Transform ParseTransform(string raw)
	{
		string s = raw.Trim();
		if (string.Equals(s, "Identity", StringComparison.Ordinal))
			return Transform.Identity;

		List<string> tokens = Tokenize(s, raw);
		if (tokens.Count < 6)
			throw new InvalidOperationException($"Premature string termination in Transform \"{raw}\".");
		if (tokens.Count > 6)
			throw new InvalidOperationException($"Extra data after the Transform in \"{raw}\".");
		double[] v = new double[6];
		for (int i = 0; i < 6; i++)
		{
			if (!double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v[i]))
				throw new FormatException($"Cannot parse Transform from \"{raw}\".");
		}
		return new MatrixTransform(new Matrix(v[0], v[1], v[2], v[3], v[4], v[5]));
	}

	static List<string> Tokenize(string s, string raw)
	{
		var tokens = new List<string>();
		int i = 0;
		while (true)
		{
			while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
			if (i >= s.Length || s[i] == ',')
				throw new InvalidOperationException($"Empty token in Transform \"{raw}\".");
			int start = i;
			while (i < s.Length && s[i] != ',' && !char.IsWhiteSpace(s[i])) i++;
			tokens.Add(s.Substring(start, i - start));
			while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
			if (i >= s.Length) return tokens;
			if (s[i] == ',') i++;
		}
	}

	/// <summary>WPF ImageSourceConverter: the string is a URI, relative or absolute.</summary>
	internal static ImageSource ParseImageSource(string raw) =>
		new System.Windows.Media.Imaging.BitmapImage(MakeUri(raw.Trim()));

	static Uri MakeUri(string s)
	{
		if (s.Length == 0)
			throw new FormatException("Cannot parse ImageSource from an empty string.");
		s = s.Replace('\\', '/');
		// A root-relative path would otherwise become file:///... on Unix; keep it app-relative.
		if (s.StartsWith("/", StringComparison.Ordinal) && !s.StartsWith("//", StringComparison.Ordinal))
			return new Uri(s, UriKind.Relative);
		// .NET cannot parse the ",,," authority of pack://application:,,, URIs; keep the host and path.
		// The URI is normalised to pack://application/..., so UriSource does not round-trip the original.
		if (s.StartsWith("pack://application:,,,", StringComparison.OrdinalIgnoreCase))
			s = "pack://application" + s.Substring("pack://application:,,,".Length);
		return new Uri(s, UriKind.RelativeOrAbsolute);
	}

	/// <summary>
	/// WPF PointCollectionConverter: numbers separated by whitespace and/or commas, read as x,y pairs.
	/// An odd count is a FormatException.
	/// </summary>
	internal static System.Windows.Shapes.PointCollection ParsePointCollection(string raw)
	{
		var result = new System.Windows.Shapes.PointCollection();
		string[] parts = raw.Split(new[] { ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length % 2 != 0)
			throw new FormatException($"Cannot parse PointCollection from \"{raw}\": odd number of coordinates.");
		for (int i = 0; i < parts.Length; i += 2)
		{
			if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)
				|| !double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
				throw new FormatException($"Cannot parse PointCollection from \"{raw}\".");
			result.Add(new Point(x, y));
		}
		return result;
	}
}
