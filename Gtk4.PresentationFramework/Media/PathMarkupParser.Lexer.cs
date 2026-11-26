using System;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>
/// Lexer helpers for <see cref="PathMarkupParser"/>: whitespace/separator skipping and
/// number reading for the Path Markup mini-language.
/// </summary>
internal static partial class PathMarkupParser
{
	static void SkipWs(string s, ref int i)
	{
		while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n'))
			i++;
	}

	static void SkipSep(string s, ref int i)
	{
		// optional whitespace, optional comma, optional whitespace
		SkipWs(s, ref i);
		if (i < s.Length && s[i] == ',') { i++; SkipWs(s, ref i); }
	}

	static bool IsNumberStart(char c) =>
		char.IsDigit(c) || c == '-' || c == '+' || c == '.';

	static double ReadNumber(string s, ref int i)
	{
		int start = i;
		if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
		while (i < s.Length && char.IsDigit(s[i])) i++;
		if (i < s.Length && s[i] == '.')
		{
			i++;
			while (i < s.Length && char.IsDigit(s[i])) i++;
		}
		if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
		{
			i++;
			if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
			while (i < s.Length && char.IsDigit(s[i])) i++;
		}
		if (i == start)
			throw new FormatException($"Expected a number at position {start} in path data.");
		return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
	}
}
