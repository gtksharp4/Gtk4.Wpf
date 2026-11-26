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
		float[] v = new float[p.Length];
		for (int i = 0; i < p.Length; i++)
			v[i] = float.Parse(p[i], NumberStyles.Float, CultureInfo.InvariantCulture);
		int o = p.Length == 4 ? 1 : 0;
		return Color.FromScRgb(o == 1 ? v[0] : 1f, v[o], v[o + 1], v[o + 2]);
	}
}

[System.ComponentModel.TypeConverter(typeof(BrushConverter))]
public abstract partial class Brush
{
}
