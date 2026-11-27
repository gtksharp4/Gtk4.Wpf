using System;

namespace System.Windows.Media;

/// <summary>How <see cref="CombinedGeometry"/> merges its two geometries.</summary>
public enum GeometryCombineMode
{
	/// <summary>The area in either geometry.</summary>
	Union,

	/// <summary>The area in both geometries.</summary>
	Intersect,

	/// <summary>The area in exactly one geometry.</summary>
	Xor,

	/// <summary>The area in the first geometry and not in the second.</summary>
	Exclude,
}

/// <summary>
/// Combines two geometries with a <see cref="GeometryCombineMode"/>.
/// Mirrors <c>System.Windows.Media.CombinedGeometry</c>.
/// </summary>
/// <remarks>
/// <b>Rendering:</b> Cairo has no path boolean operations, so the fill is exact but the outline is approximate.
/// <see cref="GeometryCombineMode.Union"/> and <see cref="GeometryCombineMode.Xor"/> append both paths (non-zero
/// and even-odd fill; the Union is exact when both outlines wind the same way, as ellipses, rectangles and
/// <c>PathGeometry</c> figures normally do). <see cref="GeometryCombineMode.Intersect"/> and
/// <see cref="GeometryCombineMode.Exclude"/> clip the context to the first geometry and then to the inside or
/// outside of the second one, so a stroke is clipped to the combined area rather than following its edge. The
/// clip belongs to the caller's <c>Save</c>/<c>Restore</c> scope, as for every other geometry. Nested
/// Intersect/Exclude children and a CombinedGeometry inside a <see cref="GeometryGroup"/> are approximate for
/// the same reason.
/// </remarks>
public class CombinedGeometry : Geometry
{
	readonly FreezableChildWatcher _watcher;

	/// <summary>Creates an empty union.</summary>
	public CombinedGeometry() => _watcher = new FreezableChildWatcher(() => OnChanged());

	/// <summary>Creates a combined geometry.</summary>
	public CombinedGeometry(GeometryCombineMode mode, Geometry? geometry1, Geometry? geometry2) : this()
	{
		GeometryCombineMode = mode;
		Geometry1 = geometry1;
		Geometry2 = geometry2;
	}

	/// <summary>Identifies <see cref="GeometryCombineMode"/>.</summary>
	public static readonly DependencyProperty GeometryCombineModeProperty = DependencyProperty.Register(
		nameof(GeometryCombineMode), typeof(GeometryCombineMode), typeof(CombinedGeometry),
		new PropertyMetadata(GeometryCombineMode.Union));

	/// <summary>Identifies <see cref="Geometry1"/>.</summary>
	public static readonly DependencyProperty Geometry1Property = DependencyProperty.Register(
		nameof(Geometry1), typeof(Geometry), typeof(CombinedGeometry), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="Geometry2"/>.</summary>
	public static readonly DependencyProperty Geometry2Property = DependencyProperty.Register(
		nameof(Geometry2), typeof(Geometry), typeof(CombinedGeometry), new PropertyMetadata(null));

	/// <summary>How the two geometries are merged. Default <see cref="Media.GeometryCombineMode.Union"/>.</summary>
	public GeometryCombineMode GeometryCombineMode
	{
		get => (GeometryCombineMode)GetValue(GeometryCombineModeProperty)!;
		set => SetValue(GeometryCombineModeProperty, value);
	}

	/// <summary>The first geometry.</summary>
	public Geometry? Geometry1
	{
		get => (Geometry?)GetValue(Geometry1Property);
		set => SetValue(Geometry1Property, value);
	}

	/// <summary>The second geometry.</summary>
	public Geometry? Geometry2
	{
		get => (Geometry?)GetValue(Geometry2Property);
		set => SetValue(Geometry2Property, value);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == Geometry1Property || e.Property == Geometry2Property)
			_watcher.Set(e.Property, e.NewValue);
	}

	/// <inheritdoc/>
	public override bool CanFreeze => base.CanFreeze
		&& (Geometry1?.CanFreeze ?? true) && (Geometry2?.CanFreeze ?? true);

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		Geometry1?.Freeze();
		Geometry2?.Freeze();
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds
	{
		get
		{
			System.Windows.Rect a = Geometry1?.GetTransformedBounds() ?? System.Windows.Rect.Empty;
			System.Windows.Rect b = Geometry2?.GetTransformedBounds() ?? System.Windows.Rect.Empty;
			System.Windows.Rect r = GeometryCombineMode switch
			{
				GeometryCombineMode.Intersect => IntersectBounds(a, b),
				GeometryCombineMode.Exclude => a,
				_ => System.Windows.Rect.Union(a, b),
			};
			return r.IsEmpty ? new System.Windows.Rect(0, 0, 0, 0) : r;
		}
	}

	static System.Windows.Rect IntersectBounds(System.Windows.Rect a, System.Windows.Rect b)
	{
		if (a.IsEmpty || b.IsEmpty || !a.IntersectsWith(b)) return System.Windows.Rect.Empty;
		double x = Math.Max(a.Left, b.Left), y = Math.Max(a.Top, b.Top);
		return new System.Windows.Rect(x, y, Math.Min(a.Right, b.Right) - x, Math.Min(a.Bottom, b.Bottom) - y);
	}

	// A rectangle far larger than any widget, to turn "inside" into "outside" with the even-odd rule.
	const double Huge = 1e6;

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		Geometry? a = Geometry1;
		Geometry? b = Geometry2;
		switch (GeometryCombineMode)
		{
			case GeometryCombineMode.Intersect:
				if (a == null || b == null) { cr.NewPath(); return; }
				ClipTo(cr, a);
				cr.NewPath();
				b.BuildTransformedPath(cr);
				break;
			case GeometryCombineMode.Exclude:
				if (a == null) { cr.NewPath(); return; }
				if (b != null)
				{
					ClipTo(cr, a);
					cr.NewPath();
					cr.Rectangle(-Huge, -Huge, 2 * Huge, 2 * Huge);
					b.BuildTransformedPath(cr);
					cr.FillRule = Cairo.FillRule.EvenOdd;
					cr.Clip();
				}
				cr.NewPath();
				a.BuildTransformedPath(cr);
				break;
			default:
				a?.BuildTransformedPath(cr);
				b?.BuildTransformedPath(cr);
				cr.FillRule = GeometryCombineMode == GeometryCombineMode.Xor
					? Cairo.FillRule.EvenOdd : Cairo.FillRule.Winding;
				break;
		}
	}

	static void ClipTo(Cairo.Context cr, Geometry g)
	{
		cr.NewPath();
		g.BuildTransformedPath(cr);
		cr.Clip();
	}
}
