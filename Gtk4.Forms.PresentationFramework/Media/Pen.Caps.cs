namespace System.Windows.Media;

// Stroke geometry members of Pen (caps, join, miter limit), with WPF defaults.
public partial class Pen
{
	/// <summary>Cap at the start of an open figure. Default <see cref="PenLineCap.Flat"/>.</summary>
	public PenLineCap StartLineCap { get; set; } = PenLineCap.Flat;

	/// <summary>Cap at the end of an open figure. Default <see cref="PenLineCap.Flat"/>.</summary>
	public PenLineCap EndLineCap { get; set; } = PenLineCap.Flat;

	/// <summary>Cap at both ends of each dash. Default <see cref="PenDashCap.Flat"/>.</summary>
	public PenDashCap DashCap { get; set; } = PenDashCap.Flat;

	/// <summary>How segments join at a vertex. Default <see cref="PenLineJoin.Miter"/>.</summary>
	public PenLineJoin LineJoin { get; set; } = PenLineJoin.Miter;

	/// <summary>Miter length limit as a multiple of half the thickness. Default 10.</summary>
	public double MiterLimit { get; set; } = 10.0;
}
