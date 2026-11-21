using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>The fixed WIC palettes (WPF <c>BitmapPalettes</c>); tables match WIC exactly.</summary>
public static class BitmapPalettes
{
	static readonly uint[] _system =
	{
		0xFFC0C0C0, 0xFF808080, 0xFF800000, 0xFF008000, 0xFF000080, 0xFF808000, 0xFF800080, 0xFF008080,
	};

	static uint Level(int i, int n) => (uint)Math.Round(i * 255.0 / (n - 1), MidpointRounding.AwayFromZero);

	static BitmapPalette Make(IEnumerable<uint> argb)
	{
		var list = new List<Color>();
		foreach (uint c in argb)
			list.Add(BitmapPalette.FromArgb(c));
		return new BitmapPalette(list);
	}

	static IEnumerable<uint> Gray(int n)
	{
		for (int i = 0; i < n; i++)
			yield return 0xFF000000 | Level(i, n) * 0x010101;
	}

	// Cube with the blue index varying fastest, then green, then red.
	static IEnumerable<uint> Cube(int r, int g, int b, int system)
	{
		for (int ri = 0; ri < r; ri++)
		{
			for (int gi = 0; gi < g; gi++)
			{
				for (int bi = 0; bi < b; bi++)
					yield return 0xFF000000 | Level(ri, r) << 16 | Level(gi, g) << 8 | Level(bi, b);
			}
		}
		for (int i = 0; i < system; i++)
			yield return _system[i];
	}

	static IEnumerable<uint> WithTransparent(IEnumerable<uint> palette, bool replaceLast)
	{
		var list = new List<uint>(palette);
		if (replaceLast)
			list[list.Count - 1] = 0;
		else
			list.Add(0);
		return list;
	}

	static BitmapPalette Transparent(IEnumerable<uint> palette) => Make(WithTransparent(palette, false));

	/// <summary>Black and white.</summary>
	public static BitmapPalette BlackAndWhite => Make(Gray(2));

	/// <summary>Black and white (WIC adds no transparent entry).</summary>
	public static BitmapPalette BlackAndWhiteTransparent => Make(Gray(2));

	/// <summary>Four grays.</summary>
	public static BitmapPalette Gray4 => Make(Gray(4));

	/// <summary>Four grays (no transparent entry).</summary>
	public static BitmapPalette Gray4Transparent => Make(Gray(4));

	/// <summary>Sixteen grays.</summary>
	public static BitmapPalette Gray16 => Make(Gray(16));

	/// <summary>Sixteen grays (no transparent entry).</summary>
	public static BitmapPalette Gray16Transparent => Make(Gray(16));

	/// <summary>256 grays.</summary>
	public static BitmapPalette Gray256 => Make(Gray(256));

	/// <summary>256 grays (no transparent entry).</summary>
	public static BitmapPalette Gray256Transparent => Make(Gray(256));

	/// <summary>2x2x2 halftone plus system colours.</summary>
	public static BitmapPalette Halftone8 => Make(Cube(2, 2, 2, 8));

	/// <summary>Halftone8 plus a transparent entry.</summary>
	public static BitmapPalette Halftone8Transparent => Transparent(Cube(2, 2, 2, 8));

	/// <summary>3x3x3 halftone plus gray.</summary>
	public static BitmapPalette Halftone27 => Make(Cube(3, 3, 3, 1));

	/// <summary>Halftone27 plus a transparent entry.</summary>
	public static BitmapPalette Halftone27Transparent => Transparent(Cube(3, 3, 3, 1));

	/// <summary>4x4x4 halftone plus system colours.</summary>
	public static BitmapPalette Halftone64 => Make(Cube(4, 4, 4, 8));

	/// <summary>Halftone64 plus a transparent entry.</summary>
	public static BitmapPalette Halftone64Transparent => Transparent(Cube(4, 4, 4, 8));

	/// <summary>5x5x5 halftone plus gray.</summary>
	public static BitmapPalette Halftone125 => Make(Cube(5, 5, 5, 1));

	/// <summary>Halftone125 plus a transparent entry.</summary>
	public static BitmapPalette Halftone125Transparent => Transparent(Cube(5, 5, 5, 1));

	/// <summary>6x6x6 halftone plus system colours.</summary>
	public static BitmapPalette Halftone216 => Make(Cube(6, 6, 6, 8));

	/// <summary>Halftone216 plus a transparent entry.</summary>
	public static BitmapPalette Halftone216Transparent => Transparent(Cube(6, 6, 6, 8));

	/// <summary>6x7x6 halftone.</summary>
	public static BitmapPalette Halftone252 => Make(Cube(6, 7, 6, 0));

	/// <summary>Halftone252 plus a transparent entry.</summary>
	public static BitmapPalette Halftone252Transparent => Transparent(Cube(6, 7, 6, 0));

	/// <summary>8x8x4 halftone.</summary>
	public static BitmapPalette Halftone256 => Make(Cube(8, 8, 4, 0));

	/// <summary>Halftone256 with the last entry made transparent.</summary>
	public static BitmapPalette Halftone256Transparent =>
		Make(WithTransparent(Cube(8, 8, 4, 0), true));

	/// <summary>The 216 web-safe colours plus 8 system colours.</summary>
	public static BitmapPalette WebPalette => Make(Cube(6, 6, 6, 8));

	/// <summary>WebPalette plus a transparent entry.</summary>
	public static BitmapPalette WebPaletteTransparent => Transparent(Cube(6, 6, 6, 8));
}
