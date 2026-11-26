namespace System.Windows.Media;

/// <summary>
/// Pure, static stretch-rectangle computations shared by tile brushes and image rendering.
/// All methods are side-effect free and fully unit-testable without GTK.
/// </summary>
internal static class StretchMath
{
	/// <summary>
	/// Computes the destination rectangle (within a <paramref name="destWidth"/> ×
	/// <paramref name="destHeight"/> box) into which a source image of
	/// <paramref name="srcWidth"/> × <paramref name="srcHeight"/> pixels should be drawn,
	/// given the requested <paramref name="stretch"/> mode.
	/// </summary>
	/// <returns>
	/// (x, y, width, height) of the draw rectangle in destination space.
	/// </returns>
	internal static (double x, double y, double w, double h) ComputeDestRect(
		double srcWidth, double srcHeight,
		double destWidth, double destHeight,
		Stretch stretch)
	{
		if (srcWidth <= 0 || srcHeight <= 0)
			return (0, 0, destWidth, destHeight);

		switch (stretch)
		{
			case Stretch.None:
			{
				// Natural size, centred.
				double x = (destWidth - srcWidth) / 2.0;
				double y = (destHeight - srcHeight) / 2.0;
				return (x, y, srcWidth, srcHeight);
			}

			case Stretch.Fill:
				// Stretch to fill exactly (no aspect preservation).
				return (0, 0, destWidth, destHeight);

			case Stretch.Uniform:
			{
				// Scale uniformly to fit inside; may leave empty bands.
				double scale = Math.Min(destWidth / srcWidth, destHeight / srcHeight);
				double w = srcWidth * scale;
				double h = srcHeight * scale;
				return ((destWidth - w) / 2.0, (destHeight - h) / 2.0, w, h);
			}

			case Stretch.UniformToFill:
			{
				// Scale uniformly to fill; may clip.
				double scale = Math.Max(destWidth / srcWidth, destHeight / srcHeight);
				double w = srcWidth * scale;
				double h = srcHeight * scale;
				return ((destWidth - w) / 2.0, (destHeight - h) / 2.0, w, h);
			}

			default:
				return (0, 0, destWidth, destHeight);
		}
	}
}
