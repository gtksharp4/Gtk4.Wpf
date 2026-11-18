namespace System.Windows.Media;

/// <summary>
/// Identifies the pixel format of a bitmap.
/// Mirrors <c>System.Windows.Media.PixelFormat</c> (struct in real WPF).
/// Only the identity is needed here; the actual format used for
/// <see cref="Imaging.RenderTargetBitmap"/> is always BGRA32 (Cairo ARGB32).
/// </summary>
public struct PixelFormat : IEquatable<PixelFormat>
{
	readonly string _name;

	internal PixelFormat(string name) => _name = name;

	/// <inheritdoc/>
	public bool Equals(PixelFormat other) => _name == other._name;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is PixelFormat pf && Equals(pf);

	/// <inheritdoc/>
	public override int GetHashCode() => _name?.GetHashCode() ?? 0;

	/// <inheritdoc/>
	public static bool operator ==(PixelFormat a, PixelFormat b) => a.Equals(b);

	/// <inheritdoc/>
	public static bool operator !=(PixelFormat a, PixelFormat b) => !a.Equals(b);

	/// <inheritdoc/>
	public override string ToString() => _name ?? string.Empty;
}

/// <summary>
/// Well-known <see cref="PixelFormat"/> values mirroring <c>System.Windows.Media.PixelFormats</c>.
/// </summary>
public static class PixelFormats
{
	/// <summary>
	/// Pre-multiplied BGRA, 32 bpp — the format WPF uses for <c>RenderTargetBitmap</c>
	/// and the format Cairo uses for <c>ARGB32</c> image surfaces.
	/// </summary>
	public static readonly PixelFormat Pbgra32 = new PixelFormat("Pbgra32");
}
