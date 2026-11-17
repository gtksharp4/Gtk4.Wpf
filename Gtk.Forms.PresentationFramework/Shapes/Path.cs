using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws an arbitrary shape defined by a <see cref="Geometry"/>.
/// Mirrors <c>System.Windows.Shapes.Path</c>.
/// </summary>
public class Path : Shape
{
	// ── Data DP ───────────────────────────────────────────────────────────────

	/// <summary>The geometry that defines the shape.</summary>
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(Path),
			new PropertyMetadata(null, (d, _) => ((Path)d).InvalidatePath()));

	/// <summary>Gets or sets the geometry.</summary>
	public Geometry? Data
	{
		get => (Geometry?)GetValue(DataProperty);
		set => SetValue(DataProperty, value);
	}

	void InvalidatePath() => QueueRedraw();

	/// <inheritdoc/>
	protected override void BuildShapePath(Cairo.Context cr, int width, int height)
	{
		Data?.BuildPath(cr);
	}
}