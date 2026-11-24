using System.Windows.Media.Imaging;

namespace System.Windows.Controls;

public partial class Image
{
	/// <inheritdoc/>
	protected override Size MeasureOverride(Size availableSize) => MeasureArrange(availableSize);

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize) => MeasureArrange(finalSize);

	// WPF Image.MeasureArrangeHelper: the source's device-independent size scaled by Stretch/StretchDirection.
	Size MeasureArrange(Size input)
	{
		if (Source is not BitmapSource bitmap)
			return new Size(0, 0);
		var natural = new Size(bitmap.Width, bitmap.Height);
		var scale = ComputeScaleFactor(input, natural, Stretch, StretchDirection);
		return new Size(natural.Width * scale.Width, natural.Height * scale.Height);
	}

	// WPF Viewbox.ComputeScaleFactor.
	static Size ComputeScaleFactor(Size available, Size content, Media.Stretch stretch, StretchDirection direction)
	{
		double scaleX = 1.0, scaleY = 1.0;
		bool constrainedWidth = !double.IsPositiveInfinity(available.Width);
		bool constrainedHeight = !double.IsPositiveInfinity(available.Height);
		if (stretch == Media.Stretch.None || (!constrainedWidth && !constrainedHeight))
			return new Size(scaleX, scaleY);

		scaleX = content.Width == 0 ? 0.0 : available.Width / content.Width;
		scaleY = content.Height == 0 ? 0.0 : available.Height / content.Height;
		if (!constrainedWidth)
			scaleX = scaleY;
		else if (!constrainedHeight)
			scaleY = scaleX;
		else if (stretch == Media.Stretch.Uniform)
			scaleX = scaleY = Math.Min(scaleX, scaleY);
		else if (stretch == Media.Stretch.UniformToFill)
			scaleX = scaleY = Math.Max(scaleX, scaleY);

		switch (direction)
		{
			case StretchDirection.UpOnly:
				scaleX = Math.Max(1.0, scaleX);
				scaleY = Math.Max(1.0, scaleY);
				break;
			case StretchDirection.DownOnly:
				scaleX = Math.Min(1.0, scaleX);
				scaleY = Math.Min(1.0, scaleY);
				break;
		}
		return new Size(scaleX, scaleY);
	}
}
