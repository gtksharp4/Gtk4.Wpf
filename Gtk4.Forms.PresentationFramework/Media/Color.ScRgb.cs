using System;

namespace System.Windows.Media;

/// <summary>
/// scRGB access as in WPF: Sc* are linear-light values, converted to and from the byte channels through the sRGB
/// transfer function. WPF additionally keeps the float values it was given; this struct stores only the bytes (its
/// layout and equality are unchanged), so a value read back is the byte's linear value.
/// </summary>
public partial struct Color
{
	/// <summary>Alpha in [0,1] (alpha is linear, not transfer-encoded).</summary>
	public float ScA
	{
		get => A / 255f;
		set => A = AlphaToByte(value);
	}

	/// <summary>Red as linear-light scRGB.</summary>
	public float ScR
	{
		get => SrgbToScRgb(R);
		set => R = ScRgbToSrgb(value);
	}

	/// <summary>Green as linear-light scRGB.</summary>
	public float ScG
	{
		get => SrgbToScRgb(G);
		set => G = ScRgbToSrgb(value);
	}

	/// <summary>Blue as linear-light scRGB.</summary>
	public float ScB
	{
		get => SrgbToScRgb(B);
		set => B = ScRgbToSrgb(value);
	}

	/// <summary>Creates a colour from scRGB components (alpha linear, colour channels linear-light).</summary>
	public static Color FromScRgb(float a, float r, float g, float b) =>
		FromArgb(AlphaToByte(a), ScRgbToSrgb(r), ScRgbToSrgb(g), ScRgbToSrgb(b));

	static byte AlphaToByte(float a) => a <= 0f ? (byte)0 : a >= 1f ? (byte)255 : (byte)(a * 255f + 0.5f);

	static byte ScRgbToSrgb(float v)
	{
		if (!(v > 0f)) return 0;
		if (v <= 0.0031308f) return (byte)(255f * v * 12.92f + 0.5f);
		if (v < 1f) return (byte)(255f * (1.055f * (float)Math.Pow(v, 1.0 / 2.4) - 0.055f) + 0.5f);
		return 255;
	}

	static float SrgbToScRgb(byte value)
	{
		float v = value / 255f;
		if (!(v > 0f)) return 0f;
		if (v <= 0.04045f) return v / 12.92f;
		if (v < 1f) return (float)Math.Pow((v + 0.055) / 1.055, 2.4);
		return 1f;
	}
}
