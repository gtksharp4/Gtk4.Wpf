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
		// Every ImageSource sizes itself (DrawingImage from its drawing bounds), as WPF does.
		if (Source is not { } source)
			return new Size(0, 0);
		var natural = new Size(source.Width, source.Height);
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

public partial class Image
{
	/// <inheritdoc/>
	internal override void QueueSlotResize() => _picture?.QueueResize();

	// A picture's natural size is its paintable's, so a wide image would widen a fixed Grid track; WPF clips it to
	// the slot instead (see FrameworkElement.CapMeasure).
	[Gtk.CssName("picture")]
	sealed class SlotCappedPicture : Gtk.Picture
	{
		readonly Image _owner;

		internal SlotCappedPicture(Image owner)
		{
			_owner = owner;
			Name = "GtkSlotCappedPicture";
			Overflow = Gtk.Overflow.Hidden;
		}

		protected override void OnMeasure(Gtk.Orientation orientation, int forSize, out int minimum, out int natural,
			out int minimumBaseline, out int naturalBaseline)
		{
			base.OnMeasure(orientation, forSize, out minimum, out natural, out minimumBaseline, out naturalBaseline);
			_owner.CapMeasure(orientation, ref minimum, ref natural);
		}
	}
}
