using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace System.Windows.Media;

/// <summary>
/// An ordered list of <see cref="double"/> values.  Used for
/// <c>StrokeDashArray</c> on shapes, where values are multiples of stroke thickness.
/// Mirrors <c>System.Windows.Media.DoubleCollection</c>.
/// </summary>
public sealed class DoubleCollection : Collection<double>
{
	/// <summary>Creates an empty collection.</summary>
	public DoubleCollection() { }

	/// <summary>
	/// Parses a whitespace- or comma-separated string of doubles.
	/// </summary>
	/// <param name="source">
	/// E.g. <c>"2 1 3"</c> or <c>"2,1,3"</c> → {2, 1, 3}.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="FormatException">A token is not a valid double.</exception>
	public static DoubleCollection Parse(string source)
	{
		if (source is null) throw new ArgumentNullException(nameof(source));

		var result = new DoubleCollection();
		var text = source.Trim();

		if (text.Length == 0)
			return result;

		int start = 0;
		for (int i = 0; i <= text.Length; i++)
		{
			bool end = i == text.Length;
			bool sep = !end && (text[i] == ',' || char.IsWhiteSpace(text[i]));

			if (sep || end)
			{
				var token = text.Substring(start, i - start).Trim();
				if (token.Length != 0)
				{
					if (!double.TryParse(token, NumberStyles.Float,
						CultureInfo.InvariantCulture, out double val))
					{
						throw new FormatException(
							$"Token '{token}' is not a valid double.");
					}
					result.Add(val);
				}
				start = i + 1;
			}
		}

		return result;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		if (Count == 0) return string.Empty;
		var sb = new StringBuilder();
		for (int i = 0; i < Count; i++)
		{
			if (i > 0) sb.Append(' ');
			sb.Append(this[i].ToString(CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}
}
