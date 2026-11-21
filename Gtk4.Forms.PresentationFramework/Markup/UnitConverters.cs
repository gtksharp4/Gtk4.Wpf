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

	/// <summary>
	/// WPF UriTypeConverter: relative or absolute, keeping the original text (<c>OriginalString</c>); the empty
	/// string is an empty relative URI. Only a string .NET cannot parse as written (the <c>:,,,</c> authority of
	/// <c>pack://application:,,,</c>) falls back to the normalised form of <see cref="MakeUri"/>.
	/// </summary>
	internal static Uri ParseUri(string raw)
	{
		var s = raw.Trim();
		if (s.Length == 0) return new Uri("", UriKind.Relative);
		// .NET reads a leading '/' as file:/// on Unix; WPF keeps it relative (app-root). A real Unix file path
		// also starts with '/', so it stays absolute when its parent directory exists below the filesystem root.
		if (s[0] == '/' && !s.StartsWith("//", StringComparison.Ordinal))
			return IsUnixFilePath(s, IO.Path.DirectorySeparatorChar == '/', IO.Directory.Exists)
				? new Uri(s, UriKind.Absolute)
				: new Uri(s, UriKind.Relative);
		try { return new Uri(s, UriKind.RelativeOrAbsolute); }
		catch (UriFormatException) { return MakeUri(s); }
	}

	/// <summary>
	/// True for an absolute Unix filesystem path (<c>/home/u/a.png</c>) as opposed to WPF's app-root-relative
	/// form (<c>/images/a.png</c>, <c>/a.xaml</c>): only on Unix, and only when the parent directory exists and is
	/// not the root itself.
	/// </summary>
	internal static bool IsUnixFilePath(string s, bool isUnix, Func<string, bool> directoryExists)
	{
		if (!isUnix) return false;
		int slash = s.LastIndexOf('/');
		if (slash <= 0) return false;
		return directoryExists(s.Substring(0, slash));
	}

	static Uri MakeUri(string s)
	{
		if (s.Length == 0)
			throw new FormatException("Cannot parse ImageSource from an empty string.");
		s = s.Replace('\\', '/');
		// A root-relative path would otherwise become file:///... on Unix; keep it app-relative, unless it is a
		// real Unix file path (see IsUnixFilePath).
		if (s.StartsWith("/", StringComparison.Ordinal) && !s.StartsWith("//", StringComparison.Ordinal))
			return IsUnixFilePath(s, IO.Path.DirectorySeparatorChar == '/', IO.Directory.Exists)
				? new Uri(s, UriKind.Absolute)
				: new Uri(s, UriKind.Relative);
		// .NET cannot parse the ",,," authority of pack://application:,,, URIs; keep the host and path.
		// The URI is normalised to pack://application/..., so UriSource does not round-trip the original.
		if (s.StartsWith("pack://application:,,,", StringComparison.OrdinalIgnoreCase))
			s = "pack://application" + s.Substring("pack://application:,,,".Length);
		return new Uri(s, UriKind.RelativeOrAbsolute);
	}

	/// <summary>
	/// WPF PointCollectionConverter: x,y pairs whose tokens are separated by whitespace and at most one comma.
	/// </summary>
	/// <exception cref="FormatException">An empty token (e.g. <c>1,,2</c>) or a token that is not a number.</exception>
	/// <exception cref="InvalidOperationException">An odd number of coordinates.</exception>
	internal static System.Windows.Shapes.PointCollection ParsePointCollection(string raw)
	{
		var result = new System.Windows.Shapes.PointCollection();
		List<string> tokens = Tokenize(raw);
		if (tokens.Count % 2 != 0)
			throw new InvalidOperationException($"Cannot parse PointCollection from \"{raw}\": missing y value.");
		for (int i = 0; i < tokens.Count; i += 2)
			result.Add(new Point(ParseCoordinate(tokens[i], raw), ParseCoordinate(tokens[i + 1], raw)));
		return result;
	}

	static double ParseCoordinate(string token, string raw)
	{
		if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
			throw new FormatException($"Cannot parse PointCollection from \"{raw}\": \"{token}\" is not a number.");
		return d;
	}

	static List<string> Tokenize(string raw)
	{
		var tokens = new List<string>();
		int i = 0;
		bool afterComma = false;
		while (true)
		{
			while (i < raw.Length && char.IsWhiteSpace(raw[i])) i++;
			if (i >= raw.Length)
			{
				if (afterComma)
					throw new FormatException($"Cannot parse PointCollection from \"{raw}\": trailing comma.");
				return tokens;
			}
			if (raw[i] == ',') throw new FormatException($"Cannot parse PointCollection from \"{raw}\": empty token.");
			int start = i;
			while (i < raw.Length && !char.IsWhiteSpace(raw[i]) && raw[i] != ',') i++;
			tokens.Add(raw.Substring(start, i - start));
			while (i < raw.Length && char.IsWhiteSpace(raw[i])) i++;
			afterComma = i < raw.Length && raw[i] == ',';
			if (afterComma) i++;
		}
	}
}

