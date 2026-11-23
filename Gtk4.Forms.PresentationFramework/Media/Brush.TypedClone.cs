namespace System.Windows.Media;

/// <summary>Typed clone members of <see cref="Brush"/>.</summary>
public abstract partial class Brush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new Brush Clone() => (Brush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new Brush CloneCurrentValue() => (Brush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="SolidColorBrush"/>.</summary>
public partial class SolidColorBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new SolidColorBrush Clone() => (SolidColorBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new SolidColorBrush CloneCurrentValue() => (SolidColorBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="GradientBrush"/>.</summary>
public abstract partial class GradientBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new GradientBrush Clone() => (GradientBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new GradientBrush CloneCurrentValue() => (GradientBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="LinearGradientBrush"/>.</summary>
public partial class LinearGradientBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new LinearGradientBrush Clone() => (LinearGradientBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new LinearGradientBrush CloneCurrentValue() => (LinearGradientBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="RadialGradientBrush"/>.</summary>
public partial class RadialGradientBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new RadialGradientBrush Clone() => (RadialGradientBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new RadialGradientBrush CloneCurrentValue() => (RadialGradientBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="TileBrush"/>.</summary>
public abstract partial class TileBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new TileBrush Clone() => (TileBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new TileBrush CloneCurrentValue() => (TileBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="ImageBrush"/>.</summary>
public partial class ImageBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new ImageBrush Clone() => (ImageBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new ImageBrush CloneCurrentValue() => (ImageBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="DrawingBrush"/>.</summary>
public partial class DrawingBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new DrawingBrush Clone() => (DrawingBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new DrawingBrush CloneCurrentValue() => (DrawingBrush)base.CloneCurrentValue();
}

/// <summary>Typed clone members of <see cref="VisualBrush"/>.</summary>
public partial class VisualBrush
{
	/// <summary>Creates a modifiable clone.</summary>
	public new VisualBrush Clone() => (VisualBrush)base.Clone();

	/// <summary>Creates a modifiable clone of the current values.</summary>
	public new VisualBrush CloneCurrentValue() => (VisualBrush)base.CloneCurrentValue();
}
