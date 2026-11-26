using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Media;

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
	public override bool CanFreeze => base.CanFreeze && CanFreezeAll(Children);

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
