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
/// <b>Rendering note:</b> <see cref="Stretch"/> and <see cref="TileMode"/> affect how the brush
/// content is sized and repeated within the fill area; see <see cref="ImageBrush"/> for the
/// concrete implementation.
/// </para>
/// </summary>
public abstract class TileBrush : Brush
{
	/// <summary>
	/// How the tile content is scaled to fit the viewport.
	/// Default is <see cref="Media.Stretch.Fill"/> (matches WPF default).
	/// </summary>
	public Stretch Stretch { get; set; } = Stretch.Fill;

	/// <summary>
	/// How the tile is repeated outside the base tile viewport.
	/// Default is <see cref="TileMode.None"/>.
	/// </summary>
	public TileMode TileMode { get; set; } = TileMode.None;

	/// <summary>
	/// Position and size of the base tile in the destination coordinate space.
	/// When <see cref="ViewportUnits"/> is <see cref="BrushMappingMode.RelativeToBoundingBox"/>,
	/// values are in [0,1] relative to the fill area.
	/// Default covers the whole area (0,0,1,1).
	/// </summary>
	public Rect Viewport { get; set; } = new Rect(0, 0, 1, 1);

	/// <summary>
	/// Coordinate space for <see cref="Viewport"/>.
	/// Default is <see cref="BrushMappingMode.RelativeToBoundingBox"/>.
	/// </summary>
	public BrushMappingMode ViewportUnits { get; set; } = BrushMappingMode.RelativeToBoundingBox;

	/// <summary>
	/// Portion of the content shown in the tile. Default covers all of it (0,0,1,1). Stored only.
	/// </summary>
	public Rect Viewbox { get; set; } = new Rect(0, 0, 1, 1);

	/// <summary>
	/// Coordinate space for <see cref="Viewbox"/>. Default <see cref="BrushMappingMode.RelativeToBoundingBox"/>.
	/// Stored only.
	/// </summary>
	public BrushMappingMode ViewboxUnits { get; set; } = BrushMappingMode.RelativeToBoundingBox;

	/// <summary>Horizontal alignment of the content in the tile. Stored only.</summary>
	public AlignmentX AlignmentX { get; set; } = AlignmentX.Center;

	/// <summary>Vertical alignment of the content in the tile. Stored only.</summary>
	public AlignmentY AlignmentY { get; set; } = AlignmentY.Center;
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
