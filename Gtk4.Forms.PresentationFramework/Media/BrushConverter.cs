using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Media;

/// <summary>
/// Converts text ("Red", "#AARRGGBB", "sc#a,r,g,b") to a <see cref="SolidColorBrush"/> and a solid brush back to
/// "#AARRGGBB". Named colours return one frozen, cached brush per name, like WPF's known-colour brushes.
/// </summary>
public class BrushConverter : TypeConverter
{
	static readonly ConcurrentDictionary<string, SolidColorBrush> _known =
		new ConcurrentDictionary<string, SolidColorBrush>(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (!(value is string text)) throw GetConvertFromException(value);
		string s = text.Trim();
		if (s.Length == 0) throw new FormatException("Empty string not allowed.");
		if (s.StartsWith("sc#", StringComparison.Ordinal)) return new SolidColorBrush(ParseScRgb(s));
		if (s[0] == '#') return new SolidColorBrush(Markup.MediaConverters.ParseColor(s));
		return _known.GetOrAdd(s, Named);
	}

	/// <inheritdoc/>
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (destinationType != typeof(string)) throw GetConvertToException(value, destinationType);
		if (value == null) return string.Empty;
		if (value is SolidColorBrush b)
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}",
				b.Color.A, b.Color.R, b.Color.G, b.Color.B);
		return value.ToString();
	}

	static SolidColorBrush Named(string name)
	{
		var prop = typeof(Colors).GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
		if (prop == null) throw new FormatException($"Token is not valid: '{name}'.");
		var brush = new SolidColorBrush((Color)prop.GetValue(null)!);
		brush.Freeze();
		return brush;
	}

	static Color ParseScRgb(string s)
	{
		string[] p = s.Substring(3).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (p.Length != 3 && p.Length != 4) throw new FormatException($"Cannot parse scRGB colour '{s}'.");
		double[] v = new double[p.Length];
		for (int i = 0; i < p.Length; i++)
			v[i] = double.Parse(p[i], NumberStyles.Float, CultureInfo.InvariantCulture);
		int o = p.Length == 4 ? 1 : 0;
		byte alpha = o == 1 ? (byte)Math.Round(Math.Max(0, Math.Min(1, v[0])) * 255) : (byte)255;
		return Color.FromArgb(alpha, Gamma(v[o]), Gamma(v[o + 1]), Gamma(v[o + 2]));
	}

	// scRGB (linear) to sRGB byte, as WPF does.
	static byte Gamma(double linear)
	{
		double c = Math.Max(0, Math.Min(1, linear));
		double s = c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
		return (byte)Math.Round(s * 255);
	}
}

[System.ComponentModel.TypeConverter(typeof(BrushConverter))]
public abstract partial class Brush
{
}
