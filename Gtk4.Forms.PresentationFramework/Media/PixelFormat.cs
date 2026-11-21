using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>
/// The pixel layout of a bitmap (WPF <c>PixelFormat</c>). Identity is the format name; the
/// bits-per-pixel and channel masks are the WIC values.
/// </summary>
public struct PixelFormat : IEquatable<PixelFormat>
{
	readonly string? _name;
	readonly int _bitsPerPixel;
	readonly PixelFormatChannelMask[]? _masks;

	internal PixelFormat(string name, int bitsPerPixel, params byte[][] masks)
	{
		_name = name;
		_bitsPerPixel = bitsPerPixel;
		_masks = Array.ConvertAll(masks, m => new PixelFormatChannelMask(m));
	}

	/// <summary>Bits used by one pixel (0 for <see cref="PixelFormats.Default"/>).</summary>
	public int BitsPerPixel => _bitsPerPixel;

	/// <summary>The channel masks, in channel order.</summary>
	public IList<PixelFormatChannelMask> Masks =>
		new ReadOnlyCollection<PixelFormatChannelMask>(_masks ?? Array.Empty<PixelFormatChannelMask>());

	internal string Name => _name ?? string.Empty;

	/// <inheritdoc/>
	public bool Equals(PixelFormat other) => Name == other.Name;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is PixelFormat pf && Equals(pf);

	/// <inheritdoc/>
	public override int GetHashCode() => Name.GetHashCode();

	/// <inheritdoc/>
	public static bool operator ==(PixelFormat a, PixelFormat b) => a.Equals(b);

	/// <inheritdoc/>
	public static bool operator !=(PixelFormat a, PixelFormat b) => !a.Equals(b);

	/// <inheritdoc/>
	public override string ToString() => Name;
}
