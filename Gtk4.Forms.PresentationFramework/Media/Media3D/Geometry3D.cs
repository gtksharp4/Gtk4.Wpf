using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the 3D geometries.</summary>
public abstract class Geometry3D : Animatable
{
	internal Geometry3D() { }

	/// <summary>The axis-aligned bounding box.</summary>
	public abstract Rect3D Bounds { get; }
}

/// <summary>A triangle mesh.</summary>
public sealed class MeshGeometry3D : Geometry3D
{
	/// <summary>Identifies <see cref="Positions"/>.</summary>
	public static readonly DependencyProperty PositionsProperty = Media3DProps.Register(
		nameof(Positions), typeof(Point3DCollection), typeof(MeshGeometry3D), Point3DCollection.Empty);

	/// <summary>Identifies <see cref="Normals"/>.</summary>
	public static readonly DependencyProperty NormalsProperty = Media3DProps.Register(
		nameof(Normals), typeof(Vector3DCollection), typeof(MeshGeometry3D), Vector3DCollection.Empty);

	/// <summary>Identifies <see cref="TextureCoordinates"/>.</summary>
	public static readonly DependencyProperty TextureCoordinatesProperty = Media3DProps.Register(
		nameof(TextureCoordinates), typeof(PointCollection), typeof(MeshGeometry3D), null);

	/// <summary>Identifies <see cref="TriangleIndices"/>.</summary>
	public static readonly DependencyProperty TriangleIndicesProperty = Media3DProps.Register(
		nameof(TriangleIndices), typeof(Int32Collection), typeof(MeshGeometry3D), Int32Collection.Empty);

	ChildLinks? _links;

	/// <summary>Creates an empty mesh.</summary>
	public MeshGeometry3D() { }

	/// <summary>The vertex positions.</summary>
	public Point3DCollection Positions
	{
		get => Media3DProps.Collection(this, PositionsProperty, Point3DCollection.Empty);
		set => SetValue(PositionsProperty, value);
	}

	/// <summary>The vertex normals.</summary>
	public Vector3DCollection Normals
	{
		get => Media3DProps.Collection(this, NormalsProperty, Vector3DCollection.Empty);
		set => SetValue(NormalsProperty, value);
	}

	/// <summary>
	/// The texture coordinates. Typed as the existing <see cref="PointCollection"/> of this assembly, which is a
	/// plain list: editing it in place does not raise <c>Changed</c>; assign a new collection to repaint.
	/// </summary>
	public PointCollection TextureCoordinates
	{
		get => EnsureTextureCoordinates();
		set => SetValue(TextureCoordinatesProperty, value);
	}

	/// <summary>The triangle vertex indices, three per triangle.</summary>
	public Int32Collection TriangleIndices
	{
		get => Media3DProps.Collection(this, TriangleIndicesProperty, Int32Collection.Empty);
		set => SetValue(TriangleIndicesProperty, value);
	}

	PointCollection EnsureTextureCoordinates()
	{
		var value = (PointCollection?)GetValue(TextureCoordinatesProperty);
		if (value == null && !IsFrozen)
		{
			value = new PointCollection();
			SetValue(TextureCoordinatesProperty, value);
		}
		return value ?? new PointCollection();
	}

	/// <inheritdoc/>
	public override Rect3D Bounds
	{
		get
		{
			var bounds = Rect3D.Empty;
			foreach (var p in Media3DProps.Peek<Point3DCollection>(this, PositionsProperty))
				bounds.Union(p);
			return bounds;
		}
	}

	void RaiseChanged()
	{
		if (!IsFrozen) OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			(_links ??= new ChildLinks(RaiseChanged)).Track(e);
	}

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(
		Media3DProps.Peek<Point3DCollection>(this, PositionsProperty),
		Media3DProps.Peek<Vector3DCollection>(this, NormalsProperty),
		Media3DProps.Peek<Int32Collection>(this, TriangleIndicesProperty));

	/// <summary>Creates a modifiable clone.</summary>
	public new MeshGeometry3D Clone() => (MeshGeometry3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MeshGeometry3D();
}
