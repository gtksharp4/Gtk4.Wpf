using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Markup;

/// <summary>An RFC 3066 language tag, as used by <c>xml:lang</c> and <c>Language</c>.</summary>
public sealed class XmlLanguage
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
		var key = ietfLanguageTag.ToLowerInvariant();
		lock (_cache)
		{
			if (!_cache.TryGetValue(key, out var language))
				_cache[key] = language = new XmlLanguage(key);
			return language;
		}
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
