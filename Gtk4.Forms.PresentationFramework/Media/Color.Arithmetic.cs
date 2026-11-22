namespace System.Windows.Media;

public partial struct Color
{
	static byte Clamp(double v) => (byte)Math.Round(Math.Max(0, Math.Min(255, v)));

	/// <summary>
	/// Adds two colours channel-wise, clamping to a byte. Uses the layer's Sc* (non-linear) channel model.
	/// </summary>
	public static Color Add(Color color1, Color color2) => FromArgb(Clamp(color1.A + color2.A),
		Clamp(color1.R + color2.R), Clamp(color1.G + color2.G), Clamp(color1.B + color2.B));

	/// <summary>Adds two colours channel-wise.</summary>
	public static Color operator +(Color color1, Color color2) => Add(color1, color2);

	/// <summary>Scales every channel (alpha included) by <paramref name="coefficient"/>, clamping to a byte.</summary>
	public static Color Multiply(Color color, float coefficient) => FromArgb(Clamp(color.A * coefficient),
		Clamp(color.R * coefficient), Clamp(color.G * coefficient), Clamp(color.B * coefficient));

	/// <summary>Scales every channel by <paramref name="coefficient"/>.</summary>
	public static Color operator *(Color color, float coefficient) => Multiply(color, coefficient);
}
