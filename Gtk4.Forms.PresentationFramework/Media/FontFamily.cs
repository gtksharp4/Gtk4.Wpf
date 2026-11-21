using System;

namespace System.Windows.Media;

/// <summary>
/// A font family name, or a comma-separated fallback list ("Segoe UI, Arial").
/// Mirrors <c>System.Windows.Media.FontFamily</c>; the GTK side only needs the
/// <see cref="Source"/> string (rendered as CSS <c>font-family</c>), so there is no typeface model.
/// </summary>
public partial class FontFamily : IEquatable<FontFamily>
{
	/// <summary>Creates a family with an empty <see cref="Source"/>.</summary>
	public FontFamily() { Source = string.Empty; }

	/// <summary>Creates a family from a name or comma-separated fallback list.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="familyName"/> is null.</exception>
	public FontFamily(string familyName)
	{
		Source = familyName ?? throw new ArgumentNullException(nameof(familyName));
	}

	/// <summary>The family name or fallback list exactly as supplied.</summary>
	public string Source { get; }

	/// <inheritdoc/>
	public bool Equals(FontFamily? other) =>
		other != null && string.Equals(Source, other.Source, StringComparison.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as FontFamily);

	/// <inheritdoc/>
	public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Source);

	/// <inheritdoc/>
	public override string ToString() => Source;
}
