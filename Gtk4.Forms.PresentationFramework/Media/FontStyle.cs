namespace System.Windows;

/// <summary>Represents the style of a typeface (normal, italic, or oblique).</summary>
[System.ComponentModel.TypeConverter(typeof(FontStyleConverter))]
public struct FontStyle : IEquatable<FontStyle>
{
	readonly string _name;

	internal FontStyle(string name) { _name = name; }

	/// <inheritdoc/>
	public bool Equals(FontStyle other) => _name == other._name;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is FontStyle fs && Equals(fs);
	/// <inheritdoc/>
	public override int GetHashCode() => _name?.GetHashCode() ?? 0;
	/// <inheritdoc/>
	public static bool operator ==(FontStyle a, FontStyle b) => a._name == b._name;
	/// <inheritdoc/>
	public static bool operator !=(FontStyle a, FontStyle b) => a._name != b._name;
	/// <inheritdoc/>
	public override string ToString() => _name ?? "Normal";
}

/// <summary>Provides predefined <see cref="FontStyle"/> values.</summary>
public static class FontStyles
{
	/// <summary>Normal (upright) style.</summary>
	public static FontStyle Normal { get; } = new FontStyle("Normal");
	/// <summary>Italic style.</summary>
	public static FontStyle Italic { get; } = new FontStyle("Italic");
	/// <summary>Oblique style.</summary>
	public static FontStyle Oblique { get; } = new FontStyle("Oblique");
}