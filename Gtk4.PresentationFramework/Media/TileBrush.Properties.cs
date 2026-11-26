namespace System.Windows.Media;

public abstract partial class TileBrush
{
	static bool IsStretch(object? v) => v is Stretch s && s >= Stretch.None && s <= Stretch.UniformToFill;

	static bool IsTileMode(object? v) => v is TileMode m && m >= TileMode.None && m <= TileMode.FlipXY;

	static bool IsMapping(object? v) =>
		v is BrushMappingMode m && m >= BrushMappingMode.Absolute && m <= BrushMappingMode.RelativeToBoundingBox;

	static bool IsAlignX(object? v) => v is AlignmentX a && a >= AlignmentX.Left && a <= AlignmentX.Right;

	static bool IsAlignY(object? v) => v is AlignmentY a && a >= AlignmentY.Top && a <= AlignmentY.Bottom;

	/// <summary>Identifies <see cref="Stretch"/>. Default <see cref="Media.Stretch.Fill"/>.</summary>
	public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
		nameof(Stretch), typeof(Stretch), typeof(TileBrush), new PropertyMetadata(Stretch.Fill), IsStretch);

	/// <summary>Identifies <see cref="TileMode"/>. Default <see cref="Media.TileMode.None"/>.</summary>
	public static readonly DependencyProperty TileModeProperty = DependencyProperty.Register(
		nameof(TileMode), typeof(TileMode), typeof(TileBrush), new PropertyMetadata(TileMode.None), IsTileMode);

	/// <summary>Identifies <see cref="Viewport"/>. Default (0,0,1,1).</summary>
	public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register(
		nameof(Viewport), typeof(Rect), typeof(TileBrush), new PropertyMetadata(new Rect(0, 0, 1, 1)));

	/// <summary>Identifies <see cref="ViewportUnits"/>. Default relative to the bounding box.</summary>
	public static readonly DependencyProperty ViewportUnitsProperty = DependencyProperty.Register(
		nameof(ViewportUnits), typeof(BrushMappingMode), typeof(TileBrush),
		new PropertyMetadata(BrushMappingMode.RelativeToBoundingBox), IsMapping);

	/// <summary>Identifies <see cref="Viewbox"/>. Default (0,0,1,1).</summary>
	public static readonly DependencyProperty ViewboxProperty = DependencyProperty.Register(
		nameof(Viewbox), typeof(Rect), typeof(TileBrush), new PropertyMetadata(new Rect(0, 0, 1, 1)));

	/// <summary>Identifies <see cref="ViewboxUnits"/>. Default relative to the bounding box.</summary>
	public static readonly DependencyProperty ViewboxUnitsProperty = DependencyProperty.Register(
		nameof(ViewboxUnits), typeof(BrushMappingMode), typeof(TileBrush),
		new PropertyMetadata(BrushMappingMode.RelativeToBoundingBox), IsMapping);

	/// <summary>Identifies <see cref="AlignmentX"/>. Default <see cref="Media.AlignmentX.Center"/>.</summary>
	public static readonly DependencyProperty AlignmentXProperty = DependencyProperty.Register(
		nameof(AlignmentX), typeof(AlignmentX), typeof(TileBrush), new PropertyMetadata(AlignmentX.Center), IsAlignX);

	/// <summary>Identifies <see cref="AlignmentY"/>. Default <see cref="Media.AlignmentY.Center"/>.</summary>
	public static readonly DependencyProperty AlignmentYProperty = DependencyProperty.Register(
		nameof(AlignmentY), typeof(AlignmentY), typeof(TileBrush), new PropertyMetadata(AlignmentY.Center), IsAlignY);

	/// <summary>How the content is scaled to fit the viewport.</summary>
	public Stretch Stretch
	{
		get => (Stretch)GetValue(StretchProperty)!;
		set => SetValue(StretchProperty, value);
	}

	/// <summary>How the tile repeats outside the base tile.</summary>
	public TileMode TileMode
	{
		get => (TileMode)GetValue(TileModeProperty)!;
		set => SetValue(TileModeProperty, value);
	}

	/// <summary>Position and size of the base tile: relative to the fill area or absolute per
	/// <see cref="ViewportUnits"/>.</summary>
	public Rect Viewport
	{
		get => (Rect)GetValue(ViewportProperty)!;
		set => SetValue(ViewportProperty, value);
	}

	/// <summary>Coordinate space of <see cref="Viewport"/>.</summary>
	public BrushMappingMode ViewportUnits
	{
		get => (BrushMappingMode)GetValue(ViewportUnitsProperty)!;
		set => SetValue(ViewportUnitsProperty, value);
	}

	/// <summary>Portion of the content shown in the tile: relative to the content bounds or absolute per
	/// <see cref="ViewboxUnits"/>.</summary>
	public Rect Viewbox
	{
		get => (Rect)GetValue(ViewboxProperty)!;
		set => SetValue(ViewboxProperty, value);
	}

	/// <summary>Coordinate space of <see cref="Viewbox"/>.</summary>
	public BrushMappingMode ViewboxUnits
	{
		get => (BrushMappingMode)GetValue(ViewboxUnitsProperty)!;
		set => SetValue(ViewboxUnitsProperty, value);
	}

	/// <summary>Horizontal alignment of the content in the tile.</summary>
	public AlignmentX AlignmentX
	{
		get => (AlignmentX)GetValue(AlignmentXProperty)!;
		set => SetValue(AlignmentXProperty, value);
	}

	/// <summary>Vertical alignment of the content in the tile.</summary>
	public AlignmentY AlignmentY
	{
		get => (AlignmentY)GetValue(AlignmentYProperty)!;
		set => SetValue(AlignmentYProperty, value);
	}
}
