using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>A freezable list of <see cref="Model3D"/>.</summary>
public sealed partial class Model3DCollection : Animatable
{
	internal static readonly Model3DCollection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public Model3DCollection() { }

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public Model3DCollection(IEnumerable<Model3D> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var m in collection) Add(m);
	}

	static Model3DCollection Frozen()
	{
		var c = new Model3DCollection();
		c.Freeze();
		return c;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Model3DCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new Model3DCollection Clone() => (Model3DCollection)base.Clone();
}

/// <summary>A freezable list of <see cref="Material"/>.</summary>
public sealed partial class MaterialCollection : Animatable
{
	internal static readonly MaterialCollection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public MaterialCollection() { }

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public MaterialCollection(IEnumerable<Material> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var m in collection) Add(m);
	}

	static MaterialCollection Frozen()
	{
		var c = new MaterialCollection();
		c.Freeze();
		return c;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new MaterialCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new MaterialCollection Clone() => (MaterialCollection)base.Clone();
}

/// <summary>A freezable list of <see cref="Transform3D"/>.</summary>
public sealed partial class Transform3DCollection : Animatable
{
	internal static readonly Transform3DCollection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public Transform3DCollection() { }

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public Transform3DCollection(IEnumerable<Transform3D> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var t in collection) Add(t);
	}

	static Transform3DCollection Frozen()
	{
		var c = new Transform3DCollection();
		c.Freeze();
		return c;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Transform3DCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new Transform3DCollection Clone() => (Transform3DCollection)base.Clone();
}
