using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the 3D models.</summary>
public abstract class Model3D : Animatable
{
	/// <summary>Identifies <see cref="Transform"/>.</summary>
	public static readonly DependencyProperty TransformProperty = Media3DProps.Register(
		nameof(Transform), typeof(Transform3D), typeof(Model3D), Transform3D.Identity);

	ChildLinks? _links;

	internal Model3D() { }

	/// <summary>The transform applied to the model.</summary>
	public Transform3D Transform
	{
		get => (Transform3D)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <summary>The bounding box of the model, with <see cref="Transform"/> applied.</summary>
	public Rect3D Bounds => Transform.TransformBounds(BoundsCore);

	internal abstract Rect3D BoundsCore { get; }

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
	protected override void OnFreeze() => Media3DProps.FreezeAll(Transform);
}

/// <summary>A model made of other models.</summary>
[ContentProperty("Children")]
public sealed class Model3DGroup : Model3D
{
	/// <summary>Identifies <see cref="Children"/>.</summary>
	public static readonly DependencyProperty ChildrenProperty = Media3DProps.Register(
		nameof(Children), typeof(Model3DCollection), typeof(Model3DGroup), Model3DCollection.Empty);

	/// <summary>Creates an empty group.</summary>
	public Model3DGroup() { }

	/// <summary>The child models.</summary>
	public Model3DCollection Children
	{
		get => Media3DProps.Collection(this, ChildrenProperty, Model3DCollection.Empty);
		set => SetValue(ChildrenProperty, value);
	}

	internal override Rect3D BoundsCore
	{
		get
		{
			var bounds = Rect3D.Empty;
			foreach (var child in Media3DProps.Peek<Model3DCollection>(this, ChildrenProperty))
				bounds.Union(child.Bounds);
			return bounds;
		}
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new Model3DGroup Clone() => (Model3DGroup)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Model3DGroup();

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		base.OnFreeze();
		Media3DProps.FreezeAll(Media3DProps.Peek<Model3DCollection>(this, ChildrenProperty));
	}
}

/// <summary>A model that draws a geometry with materials.</summary>
public sealed class GeometryModel3D : Model3D
{
	/// <summary>Identifies <see cref="Geometry"/>.</summary>
	public static readonly DependencyProperty GeometryProperty =
		Media3DProps.Register(nameof(Geometry), typeof(Geometry3D), typeof(GeometryModel3D), null);

	/// <summary>Identifies <see cref="Material"/>.</summary>
	public static readonly DependencyProperty MaterialProperty =
		Media3DProps.Register(nameof(Material), typeof(Material), typeof(GeometryModel3D), null);

	/// <summary>Identifies <see cref="BackMaterial"/>.</summary>
	public static readonly DependencyProperty BackMaterialProperty =
		Media3DProps.Register(nameof(BackMaterial), typeof(Material), typeof(GeometryModel3D), null);

	/// <summary>Creates an empty model.</summary>
	public GeometryModel3D() { }

	/// <summary>Creates a model.</summary>
	public GeometryModel3D(Geometry3D geometry, Material material)
	{
		Geometry = geometry;
		Material = material;
	}

	/// <summary>The geometry.</summary>
	public Geometry3D? Geometry
	{
		get => (Geometry3D?)GetValue(GeometryProperty);
		set => SetValue(GeometryProperty, value);
	}

	/// <summary>The material of the front faces.</summary>
	public Material? Material
	{
		get => (Material?)GetValue(MaterialProperty);
		set => SetValue(MaterialProperty, value);
	}

	/// <summary>The material of the back faces; null hides them.</summary>
	public Material? BackMaterial
	{
		get => (Material?)GetValue(BackMaterialProperty);
		set => SetValue(BackMaterialProperty, value);
	}

	internal override Rect3D BoundsCore => Geometry?.Bounds ?? Rect3D.Empty;

	/// <summary>Creates a modifiable clone.</summary>
	public new GeometryModel3D Clone() => (GeometryModel3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new GeometryModel3D();

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		base.OnFreeze();
		Media3DProps.FreezeAll(Geometry, Material, BackMaterial);
	}
}
