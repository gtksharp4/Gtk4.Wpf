namespace System.Windows.Media;

public abstract partial class GradientBrush
{
	/// <summary>Copies the stops (deeply), mapping mode and spread method.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var b = (GradientBrush)destination;
		var stops = new GradientStopCollection();
		foreach (var stop in GradientStops)
			stops.Add(new GradientStop(stop.Color, stop.Offset));
		b.GradientStops = stops;
		b.MappingMode = MappingMode;
		b.SpreadMethod = SpreadMethod;
		b.ColorInterpolationMode = ColorInterpolationMode;
	}
}

public partial class LinearGradientBrush
{
	/// <summary>Copies the start and end points.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var b = (LinearGradientBrush)destination;
		b.StartPoint = StartPoint;
		b.EndPoint = EndPoint;
	}
}

public partial class RadialGradientBrush
{
	/// <summary>Copies the centre, origin and radii.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var b = (RadialGradientBrush)destination;
		b.Center = Center;
		b.GradientOrigin = GradientOrigin;
		b.RadiusX = RadiusX;
		b.RadiusY = RadiusY;
	}
}
