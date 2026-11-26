using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Markup;

/// <summary>An RFC 3066 language tag, as used by <c>xml:lang</c> and <c>Language</c>.</summary>
public class XmlLanguage
{
	static readonly Dictionary<string, XmlLanguage> _cache = new Dictionary<string, XmlLanguage>();

	XmlLanguage(string tag) => IetfLanguageTag = tag;

	/// <summary>The empty language.</summary>
	public static XmlLanguage Empty => GetLanguage(string.Empty);

	/// <summary>The lower-cased language tag.</summary>
	public string IetfLanguageTag { get; }

	/// <summary>Returns the cached instance for <paramref name="ietfLanguageTag"/>.</summary>
	public static XmlLanguage GetLanguage(string ietfLanguageTag)
	{
		if (ietfLanguageTag == null) throw new ArgumentNullException(nameof(ietfLanguageTag));
		if (ietfLanguageTag.Length > 0 && !IsValidTag(ietfLanguageTag))
			throw new ArgumentException(
				"'" + ietfLanguageTag + "' language tag must be empty or must conform to grammar defined in IETF RFC 3066.",
				nameof(ietfLanguageTag));
		var key = ietfLanguageTag.ToLowerInvariant();
		lock (_cache)
		{
			if (!_cache.TryGetValue(key, out var language))
				_cache[key] = language = new XmlLanguage(key);
			return language;
		}
	}

	// RFC 3066: subtags of 1-8 ASCII letters or digits separated by '-'; the first is alphabetic.
	static bool IsValidTag(string tag)
	{
		int length = 0;
		bool first = true;
		foreach (char c in tag)
		{
			if (c == '-')
			{
				if (length == 0) return false;
				length = 0;
				first = false;
				continue;
			}
			bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
			bool digit = c >= '0' && c <= '9';
			if (!letter && !(digit && !first)) return false;
			if (++length > 8) return false;
		}
		return length > 0;
	}

	/// <summary>Gets the culture for the tag, falling back to the invariant culture.</summary>
	public CultureInfo GetSpecificCulture()
	{
		try { return CultureInfo.CreateSpecificCulture(IetfLanguageTag); }
		catch (ArgumentException) { return CultureInfo.InvariantCulture; }
	}

	/// <inheritdoc/>
	public override string ToString() => IetfLanguageTag;
}
