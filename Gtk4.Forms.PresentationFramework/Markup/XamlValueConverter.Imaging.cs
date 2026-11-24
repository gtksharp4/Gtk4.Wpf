using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	// Called from RegisterBuiltIns (not a field initializer) so it runs inside the static constructor.
	static void RegisterImagingBuiltIns()
	{
		Register<Int32Rect>(ParseInt32Rect);
		Register<PixelFormat>(ParsePixelFormat);
	}

	// WPF Int32RectConverter: "x,y,width,height" (comma or whitespace separated, invariant) or "Empty", compared
	// ordinally as in WPF. Deviation: a wrong token count throws FormatException (WPF's TokenizerHelper throws
	// InvalidOperationException); ConvertValue only rewraps FormatException, OverflowException, ArgumentException.
	static Int32Rect ParseInt32Rect(string raw)
	{
		string text = raw.Trim();
		if (string.Equals(text, "Empty", StringComparison.Ordinal)) return Int32Rect.Empty;
		string[] parts = text.Split(new[] { ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 4) throw new FormatException($"\"{raw}\" is not an Int32Rect (x,y,width,height).");
		int[] v = parts.Select(p => int.Parse(p, NumberStyles.Integer, CultureInfo.InvariantCulture)).ToArray();
		return new Int32Rect(v[0], v[1], v[2], v[3]);
	}

	// WPF PixelFormatConverter: the name of a PixelFormats member, case-insensitive (WPF upper-cases it).
	// Deviation: an unknown name throws FormatException; WPF throws ArgumentException (from memory, unverified).
	static PixelFormat ParsePixelFormat(string raw)
	{
		string name = raw.Trim();
		FieldInfo? field = typeof(PixelFormats).GetFields(BindingFlags.Public | BindingFlags.Static)
			.FirstOrDefault(f => f.FieldType == typeof(PixelFormat)
				&& string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
		if (field == null) throw new FormatException($"\"{raw}\" is not a PixelFormat name.");
		return (PixelFormat)field.GetValue(null)!;
	}
}
