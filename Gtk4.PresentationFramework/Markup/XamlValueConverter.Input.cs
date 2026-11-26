using System;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	// Called from the static constructor via RegisterBuiltIns.
	static void RegisterInputBuiltIns()
	{
		Register<DoubleCollection>(DoubleCollection.Parse);
		Register<Cursor>(ParseCursor);
		Register<char>(ParseChar);
		Register<DateTime>(raw => DateTime.Parse(raw, System.Globalization.CultureInfo.InvariantCulture));
		Register<System.Windows.Controls.DataGridLength>(raw => (System.Windows.Controls.DataGridLength)
			new System.Windows.Controls.DataGridLengthConverter().ConvertFromString(
				null, System.Globalization.CultureInfo.InvariantCulture, raw)!);
	}

	// WPF CharConverter: exactly one character.
	static char ParseChar(string raw)
	{
		if (raw.Length != 1) throw new FormatException($"'{raw}' is not a single character.");
		return raw[0];
	}

	/// <summary>WPF CursorConverter: a <see cref="CursorType"/> name (any case) or a cursor file path.</summary>
	static Cursor ParseCursor(string raw)
	{
		string text = raw.Trim();
		var prop = typeof(Cursors).GetProperty(text,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
		if (prop != null && prop.PropertyType == typeof(Cursor)) return (Cursor)prop.GetValue(null)!;

		if (text.IndexOfAny(new[] { '.', '/', '\\' }) >= 0) return new Cursor(text);
		throw new FormatException($"'{raw}' is not a valid cursor name.");
	}
}
