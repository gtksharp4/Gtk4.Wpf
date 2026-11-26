using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media.Imaging;

/// <summary>An indexed colour table of at most 256 colours (WPF <c>BitmapPalette</c>).</summary>
public sealed class BitmapPalette : Freezable
{
	readonly Color[] _colors;

	/// <summary>Creates a palette from <paramref name="colors"/> (1 to 256 entries).</summary>
	public BitmapPalette(IList<Color> colors)
	{
		if (colors == null)
			throw new ArgumentNullException(nameof(colors));
		if (colors.Count == 0)
			throw new ArgumentException("The palette must contain at least one colour.", nameof(colors));
		if (colors.Count > 256)
			throw new ArgumentException("A palette holds at most 256 colours.", nameof(colors));
		_colors = new Color[colors.Count];
		colors.CopyTo(_colors, 0);
		Freeze();
	}

	/// <summary>Creates an optimised palette of at most <paramref name="maxColorCount"/> colours
	/// for <paramref name="bitmapSource"/>.</summary>
	public BitmapPalette(BitmapSource bitmapSource, int maxColorCount)
	{
		if (bitmapSource == null)
			throw new ArgumentNullException(nameof(bitmapSource));
		if (maxColorCount < 1 || maxColorCount > 256)
			throw new ArgumentOutOfRangeException(nameof(maxColorCount));
		_colors = PaletteBuilder.Build(bitmapSource, maxColorCount);
		Freeze();
	}

	/// <summary>The palette colours.</summary>
	public IList<Color> Colors => new ReadOnlyCollection<Color>(_colors);

	/// <summary>Creates a copy of this palette.</summary>
	public new BitmapPalette Clone() => new BitmapPalette(_colors);

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapPalette(_colors);

	internal Color[] ColorArray => _colors;

	internal static Color FromArgb(uint argb) =>
		Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
}
