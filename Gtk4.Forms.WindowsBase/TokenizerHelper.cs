using System;
using System.Globalization;

namespace System.Windows;

/// <summary>Splits the comma/whitespace separated numeric lists used by WPF value parsing.</summary>
static class TokenizerHelper
{
	/// <summary>The list separator for a culture: ';' when the decimal separator is ',', else ','.</summary>
	internal static char GetNumericListSeparator(IFormatProvider? provider)
	{
		var info = NumberFormatInfo.GetInstance(provider);
		return info.NumberDecimalSeparator.Length > 0 && info.NumberDecimalSeparator[0] == ',' ? ';' : ',';
	}

	/// <summary>A cursor over a parse source, using the invariant list separator ','.</summary>
	internal struct Reader
	{
		readonly string _s;
		int _i;
		bool _afterSeparator;

		internal Reader(string source)
		{
			_s = source ?? throw new ArgumentNullException(nameof(source));
			_i = 0;
			_afterSeparator = false;
		}

		/// <summary>
		/// Next token, or null at the end. Consumes one trailing separator.
		/// An empty token or a trailing separator is an <see cref="InvalidOperationException"/>.
		/// </summary>
		internal string? Next()
		{
			while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
			if (_i >= _s.Length)
			{
				if (_afterSeparator) throw new InvalidOperationException("Trailing separator after the last token.");
				return null;
			}
			if (_s[_i] == ',') throw new InvalidOperationException("Empty token encountered while parsing the value.");
			int start = _i;
			while (_i < _s.Length && !char.IsWhiteSpace(_s[_i]) && _s[_i] != ',') _i++;
			string token = _s.Substring(start, _i - start);
			while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
			_afterSeparator = _i < _s.Length && _s[_i] == ',';
			if (_afterSeparator) _i++;
			return token;
		}

		internal string NextRequired() =>
			Next() ?? throw new InvalidOperationException("Premature string termination while parsing the value.");

		internal void LastRequired()
		{
			while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
			if (_i < _s.Length)
				throw new InvalidOperationException("Extra data encountered after the last expected token.");
			if (_afterSeparator) throw new InvalidOperationException("Trailing separator after the last token.");
		}

		internal double NextDouble() => Convert.ToDouble(NextRequired(), CultureInfo.InvariantCulture);
	}
}
