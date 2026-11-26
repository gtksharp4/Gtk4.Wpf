using System;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace System.Windows.Media;

/// <summary>
/// An ordered, mutable collection of <see cref="Geometry"/> objects.
/// Mirrors <c>System.Windows.Media.GeometryCollection</c>.
/// </summary>
public class GeometryCollection : Collection<Geometry>
{
	internal Action? BeforeChange;
	internal Action<Geometry?, Geometry?>? ItemChanged;

	/// <inheritdoc/>
	protected override void InsertItem(int index, Geometry item)
	{
		BeforeChange?.Invoke();
		base.InsertItem(index, item);
		ItemChanged?.Invoke(null, item);
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, Geometry item)
	{
		BeforeChange?.Invoke();
		var old = this[index];
		base.SetItem(index, item);
		ItemChanged?.Invoke(old, item);
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		BeforeChange?.Invoke();
		var old = this[index];
		base.RemoveItem(index);
		ItemChanged?.Invoke(old, null);
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		BeforeChange?.Invoke();
		var old = new Geometry[Count];
		CopyTo(old, 0);
		base.ClearItems();
		foreach (var g in old) ItemChanged?.Invoke(g, null);
	}
}

/// <summary>
/// A geometry that combines its <see cref="Children"/> into one path.
/// Mirrors <c>System.Windows.Media.GeometryGroup</c>.
/// </summary>
/// <remarks>
/// <b>Rendering gap:</b> the children paths are appended to one Cairo path filled with
/// <see cref="FillRule"/>; the outermost group's rule applies. Each child's <c>Geometry.Transform</c> is applied; adding, removing or
/// changing a child raises <c>Changed</c>.
/// </remarks>
[ContentProperty("Children")]
public class GeometryGroup : Geometry
{
	/// <summary>The geometries combined by this group.</summary>
	public GeometryCollection Children
	{
		get => _children;
		set
		{
			ThrowIfFrozen();
			Unhook(_children);
			_children = value ?? throw new ArgumentNullException(nameof(value));
			Hook(_children);
			OnChanged();
		}
	}

	/// <summary>How the combined area is filled. Default <see cref="Shapes.FillRule.EvenOdd"/>.</summary>
	public FillRule FillRule
	{
		get => _fillRule;
		set
		{
			ThrowIfFrozen();
			_fillRule = value;
			OnChanged();
		}
	}

	GeometryCollection _children = new GeometryCollection();
	FillRule _fillRule = FillRule.EvenOdd;
	EventHandler? _childChanged;

	/// <summary>Creates an empty group.</summary>
	public GeometryGroup() => Hook(_children);

	void Hook(GeometryCollection children)
	{
		_childChanged ??= (_, _) => OnChanged();
		children.BeforeChange = ThrowIfFrozen;
		children.ItemChanged = (old, added) =>
		{
			if (old != null) old.Changed -= _childChanged;
			if (added != null) added.Changed += _childChanged;
			OnChanged();
		};
		foreach (var g in children) g.Changed += _childChanged;
	}

	void Unhook(GeometryCollection children)
	{
		children.BeforeChange = null;
		children.ItemChanged = null;
		foreach (var g in children) g.Changed -= _childChanged;
	}

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var g = (GeometryGroup)destination;
		g.FillRule = FillRule;
		foreach (var child in Children)
			g.Children.Add((Geometry)(currentValueOnly ? child.CloneCurrentValue() : child.Clone()));
	}

	/// <inheritdoc/>
	public override bool CanFreeze => base.CanFreeze && CanFreezeAll(Children);

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		foreach (var child in Children) child.Freeze();
	}

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds
	{
		get
		{
			System.Windows.Rect bounds = System.Windows.Rect.Empty;
			foreach (Geometry g in Children) bounds = System.Windows.Rect.Union(bounds, g.Bounds);
			return bounds.IsEmpty ? new System.Windows.Rect(0, 0, 0, 0) : bounds;
		}
	}

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		foreach (Geometry g in Children) g.BuildTransformedPath(cr);
		// Cairo has one fill rule per fill and WPF fills the combined shape with the outermost group's rule, so the
		// outer group, which finishes last, decides.
		cr.FillRule = FillRule == FillRule.Nonzero ? Cairo.FillRule.Winding : Cairo.FillRule.EvenOdd;
	}
}
