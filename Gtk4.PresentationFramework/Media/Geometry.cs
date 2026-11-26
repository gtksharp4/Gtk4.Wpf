using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Abstract base for all WPF geometry types.
/// Mirrors <c>System.Windows.Media.Geometry</c>.
/// </summary>
public abstract partial class Geometry : System.Windows.Media.Animation.Animatable
{
	/// <summary>Identifies <see cref="Transform"/>. Default <see cref="Media.Transform.Identity"/>.</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register(
		nameof(Transform), typeof(Transform), typeof(Geometry), new PropertyMetadata(Media.Transform.Identity));

	/// <summary>Transform applied to the geometry.</summary>
	public Transform Transform
	{
		get => (Transform)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <summary>
	/// Creates a new instance of the same type through its parameterless constructor.
	/// Geometries with a richer constructor override this.
	/// </summary>
	protected override Freezable CreateInstanceCore() =>
		(Freezable)(Activator.CreateInstance(GetType()) ?? throw new NotSupportedException(GetType().Name));

	/// <summary>
	/// The axis-aligned bounding box of this geometry (untransformed).
	/// Subclasses return their tight bounds.
	/// </summary>
	public abstract System.Windows.Rect Bounds { get; }

	/// <summary>Builds the Cairo path on <paramref name="cr"/>.</summary>
	internal abstract void BuildPath(Cairo.Context cr);

	/// <summary>An empty geometry with zero bounds.</summary>
	public static Geometry Empty { get; } = MakeEmpty();

	static Geometry MakeEmpty()
	{
		var g = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));
		g.Freeze();
		return g;
	}

	/// <summary>
	/// Parses the WPF Path Markup (mini-language) string and returns a <see cref="PathGeometry"/>.
	/// <para>
	/// Supported commands (absolute uppercase / relative lowercase):
	/// <list type="bullet">
	/// <item><c>M/m</c> — move-to (starts a new figure)</item>
	/// <item><c>L/l</c> — line-to</item>
	/// <item><c>H/h</c> — horizontal line-to</item>
	/// <item><c>V/v</c> — vertical line-to</item>
	/// <item><c>C/c</c> — cubic Bézier (6 params)</item>
	/// <item><c>Q/q</c> — quadratic Bézier (4 params)</item>
	/// <item><c>Z/z</c> — close figure</item>
	/// </list>
	/// Best-effort / unsupported: <c>A/a</c> (arc), <c>S/s</c> (smooth cubic), <c>T/t</c> (smooth quadratic).
	/// </para>
	/// </summary>
	/// <exception cref="FormatException">The string contains unrecognised commands or malformed numbers.</exception>
	public static Geometry Parse(string source) => PathMarkupParser.Parse(source);
}
