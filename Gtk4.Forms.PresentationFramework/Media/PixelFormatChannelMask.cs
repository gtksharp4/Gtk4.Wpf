using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>The bits of one channel within a pixel (WPF <c>PixelFormatChannelMask</c>).</summary>
public struct PixelFormatChannelMask : IEquatable<PixelFormatChannelMask>
{
	readonly byte[]? _mask;

	internal PixelFormatChannelMask(byte[] mask) => _mask = mask;

	/// <summary>The channel mask bytes, least significant byte first.</summary>
	public IList<byte> Mask => new ReadOnlyCollection<byte>(_mask ?? Array.Empty<byte>());

	/// <inheritdoc/>
	public bool Equals(PixelFormatChannelMask other)
	{
		var a = _mask ?? Array.Empty<byte>();
		var b = other._mask ?? Array.Empty<byte>();
		if (a.Length != b.Length)
			return false;
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
				return false;
		}
		return true;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is PixelFormatChannelMask m && Equals(m);

	/// <inheritdoc/>
	public override int GetHashCode() => _mask?.Length ?? 0;

	/// <summary>Equality operator.</summary>
	public static bool operator ==(PixelFormatChannelMask a, PixelFormatChannelMask b) => a.Equals(b);

	/// <summary>Inequality operator.</summary>
	public static bool operator !=(PixelFormatChannelMask a, PixelFormatChannelMask b) => !a.Equals(b);
}
