using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Media;

/// <summary>
/// Skews content by <see cref="AngleX"/> and <see cref="AngleY"/> degrees around a centre
/// point. Mirrors <c>System.Windows.Media.SkewTransform</c>.
/// </summary>
public class SkewTransform : Transform
{
	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new SkewTransform();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		var t = (SkewTransform)destination;
		t.AngleX = AngleX; t.AngleY = AngleY; t.CenterX = CenterX; t.CenterY = CenterY;
	}

	/// <summary>Creates a skew transform.</summary>
	public SkewTransform(double angleX = 0, double angleY = 0,
		double centerX = 0, double centerY = 0)
	{
		AngleX = angleX; AngleY = angleY;
		CenterX = centerX; CenterY = centerY;
	}

	/// <summary>Horizontal skew angle in degrees.</summary>
	public double AngleX { get; set; }
	/// <summary>Vertical skew angle in degrees.</summary>
	public double AngleY { get; set; }
	/// <summary>Centre X of the skew.</summary>
	public double CenterX { get; set; }
	/// <summary>Centre Y of the skew.</summary>
	public double CenterY { get; set; }

	/// <inheritdoc/>
	public override Matrix Value
	{
		get
		{
			double tx = Math.Tan(AngleX * Math.PI / 180.0);
			double ty = Math.Tan(AngleY * Math.PI / 180.0);
			// translate to centre, skew, translate back
			return new Matrix(
				1, ty, tx, 1,
				-CenterY * tx,
				-CenterX * ty);
		}
	}
}

/// <summary>
/// An ordered list of <see cref="Transform"/> objects for <see cref="TransformGroup"/>.
/// Mirrors <c>System.Windows.Media.TransformCollection</c>.
/// </summary>
public class TransformCollection : ObservableCollection<Transform>
{
	/// <summary>Creates an empty collection.</summary>
	public TransformCollection() { }

	/// <summary>Creates a collection with the given children.</summary>
	public TransformCollection(IEnumerable<Transform> children) : base(children) { }
}

/// <summary>
/// Composites a list of <see cref="Transform"/> objects by multiplying their matrices
/// left-to-right (first child applied first). Mirrors
/// <c>System.Windows.Media.TransformGroup</c>.
/// </summary>
public class TransformGroup : Transform
{
	/// <summary>Creates an empty transform group.</summary>
	public TransformGroup()
	{
		Children = new TransformCollection();
		Children.CollectionChanged += OnChildrenChanged;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TransformGroup();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		var g = (TransformGroup)destination;
		foreach (var child in Children)
			g.Children.Add((Transform)(currentValueOnly ? child.CloneCurrentValue() : child.Clone()));
	}

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		foreach (var child in Children) child.Freeze();
	}

	void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
			foreach (Transform t in e.OldItems) t.Changed -= OnChildChanged;
		if (e.NewItems != null)
			foreach (Transform t in e.NewItems) t.Changed += OnChildChanged;
		OnChanged();
	}

	void OnChildChanged(object? sender, EventArgs e) => OnChanged();

	/// <summary>The ordered child transforms. Applied first→last.</summary>
	public TransformCollection Children { get; }

	/// <summary>
	/// Returns the product of all child matrices. Returns <see cref="Matrix.Identity"/>
	/// when <see cref="Children"/> is empty.
	/// </summary>
	public override Matrix Value
	{
		get
		{
			Matrix result = Matrix.Identity;
			foreach (Transform t in Children)
				result = result.Multiply(t.Value);
			return result;
		}
	}
}
