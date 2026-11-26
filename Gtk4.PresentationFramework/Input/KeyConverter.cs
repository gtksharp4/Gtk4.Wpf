using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Input;

/// <summary>Converts between <see cref="Key"/> values and their string names.</summary>
public class KeyConverter : TypeConverter
{
	static readonly Dictionary<string, Key> _aliases = new Dictionary<string, Key>(StringComparer.OrdinalIgnoreCase)
	{
		["CTRL"] = Key.LeftCtrl, ["CONTROL"] = Key.LeftCtrl, ["ALT"] = Key.LeftAlt, ["SHIFT"] = Key.LeftShift,
		["ESC"] = Key.Escape, ["PGUP"] = Key.PageUp, ["PGDN"] = Key.PageDown, ["INS"] = Key.Insert,
		["DEL"] = Key.Delete, ["WIN"] = Key.LWin, ["WINDOWS"] = Key.LWin, ["BACKSPACE"] = Key.Back,
		["BKSP"] = Key.Back, ["BS"] = Key.Back, ["ENTER"] = Key.Return, ["PLUS"] = Key.OemPlus,
		["MINUS"] = Key.OemMinus, ["COMMA"] = Key.OemComma, ["PERIOD"] = Key.OemPeriod,
		["SEMICOLON"] = Key.OemSemicolon, ["QUESTION"] = Key.OemQuestion, ["TILDE"] = Key.OemTilde,
		["OPENBRACKETS"] = Key.OemOpenBrackets, ["CLOSEBRACKETS"] = Key.OemCloseBrackets, ["PIPE"] = Key.OemPipe,
		["QUOTES"] = Key.OemQuotes, ["BACKSLASH"] = Key.OemBackslash, ["BREAK"] = Key.Cancel,
		["PRINTSCREEN"] = Key.PrintScreen, ["APPS"] = Key.Apps, ["PAUSE"] = Key.Pause, ["SCROLLLOCK"] = Key.Scroll,
		["NUMLOCK"] = Key.NumLock,
	};

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	/// <exception cref="NotSupportedException">The text names no key.</exception>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is not string text) throw new NotSupportedException("A Key can only be converted from a string.");
		text = text.Trim();
		if (text.Length == 0) return Key.None;
		if (text.Length == 1)
		{
			char c = text[0];
			if (c >= '0' && c <= '9') return Key.D0 + (c - '0');
			if (char.IsLetter(c) && Enum.TryParse(char.ToUpperInvariant(c).ToString(), out Key letter)) return letter;
		}
		if (_aliases.TryGetValue(text, out var alias)) return alias;
		if (Enum.TryParse(text, ignoreCase: true, out Key key) && Enum.IsDefined(typeof(Key), key)) return key;
		throw new NotSupportedException($"'{text}' is not a valid Key.");
	}

	/// <inheritdoc/>
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType != typeof(string) || value is not Key key)
			throw new NotSupportedException();
		return key >= Key.D0 && key <= Key.D9 ? ((int)(key - Key.D0)).ToString(CultureInfo.InvariantCulture)
			: key.ToString();
	}
}
