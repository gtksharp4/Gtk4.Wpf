namespace System.Windows.Media;

public abstract partial class TileBrush
{
	/// <summary>Copies the tiling state.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var b = (TileBrush)destination;
		b.Stretch = Stretch;
		b.TileMode = TileMode;
		b.Viewport = Viewport;
		b.ViewportUnits = ViewportUnits;
		b.Viewbox = Viewbox;
		b.ViewboxUnits = ViewboxUnits;
		b.AlignmentX = AlignmentX;
		b.AlignmentY = AlignmentY;
	}
}

public partial class ImageBrush
{
	/// <summary>Copies the image source reference.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		((ImageBrush)destination).ImageSource = ImageSource;
	}
}

public partial class VisualBrush
{
	/// <summary>Copies the visual reference.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var b = (VisualBrush)destination;
		b.Visual = Visual;
		b.AutoLayoutContent = AutoLayoutContent;
	}
}
