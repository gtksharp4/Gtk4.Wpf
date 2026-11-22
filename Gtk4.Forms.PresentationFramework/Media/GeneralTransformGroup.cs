using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Media;

/// <summary>An ordered list of <see cref="GeneralTransform"/> objects.</summary>
public sealed class GeneralTransformCollection : ObservableCollection<GeneralTransform>
{
}

/// <summary>Applies its children in order. Mirrors WPF's GeneralTransformGroup.</summary>
public sealed class GeneralTransformGroup : GeneralTransform
{
	GeneralTransformCollection _children = new GeneralTransformCollection();

	/// <summary>Creates an empty group.</summary>
	public GeneralTransformGroup() => _children.CollectionChanged += OnChildrenChanged;

	/// <summary>The child transforms, applied first to last.</summary>
	public GeneralTransformCollection Children
	{
		get => _children;
		set
		{
			ThrowIfFrozen();
			_children.CollectionChanged -= OnChildrenChanged;
			_children = value ?? new GeneralTransformCollection();
			_children.CollectionChanged += OnChildrenChanged;
			OnChanged();
		}
	}

	void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnChanged();

	/// <inheritdoc/>
	public override GeneralTransform? Inverse
	{
		get
		{
			var inv = new GeneralTransformGroup();
			for (int i = _children.Count - 1; i >= 0; i--)
			{
				var child = _children[i].Inverse;
				if (child == null) return null;
				inv.Children.Add(child);
			}
			return inv;
		}
	}

	/// <inheritdoc/>
	public override bool TryTransform(Point inPoint, out Point result)
	{
		result = inPoint;
		foreach (var t in _children)
			if (!t.TryTransform(result, out result)) return false;
		return true;
	}

	/// <inheritdoc/>
	public override Rect TransformBounds(Rect rect)
	{
		foreach (var t in _children) rect = t.TransformBounds(rect);
		return rect;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new GeneralTransformGroup();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var g = (GeneralTransformGroup)destination;
		foreach (var c in _children)
			g.Children.Add(currentValueOnly ? c.CloneCurrentValue() : c.Clone());
	}
}
