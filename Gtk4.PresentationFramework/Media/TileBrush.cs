namespace System.Windows.Media;

/// <summary>
/// Controls how a tiled content brush tiles to fill its destination.
/// Mirrors <c>System.Windows.Media.TileMode</c>.
/// </summary>
public enum TileMode
{
	/// <summary>No tiling; the base tile is painted once.</summary>
	None,
	/// <summary>Tile in both axes.</summary>
	Tile,
	/// <summary>Tile and flip alternate columns horizontally.</summary>
	FlipX,
	/// <summary>Tile and flip alternate rows vertically.</summary>
	FlipY,
	/// <summary>Tile and flip both axes.</summary>
	FlipXY,
}

/// <summary>
/// Abstract base for brushes that fill an area with a repeated tile (images, visuals, drawings).
/// Mirrors <c>System.Windows.Media.TileBrush</c>.
/// <para>
/// The tiling properties are dependency properties (TileBrush.Properties.cs). <see cref="DrawingBrush"/> and
/// <see cref="VisualBrush"/> paint through the shared WPF mapping in TileBrush.Mapping.cs; <see cref="ImageBrush"/>
/// still uses its own path (parity gap).
/// </para>
/// </summary>
public abstract partial class TileBrush : Brush
{
}

/// <summary>
/// Specifies how a brush's coordinate values are interpreted.
/// Mirrors <c>System.Windows.Media.BrushMappingMode</c>.
/// </summary>
public enum BrushMappingMode
{
	/// <summary>Values are absolute device-independent units.</summary>
	Absolute,
	/// <summary>Values are relative to the bounding box of the filled area.</summary>
	RelativeToBoundingBox,
}
