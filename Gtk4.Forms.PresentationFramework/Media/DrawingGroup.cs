using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Media;

/// <summary>
/// An ordered, mutable collection of <see cref="Drawing"/> objects.
/// Mirrors <c>System.Windows.Media.DrawingCollection</c>.
/// </summary>
public class DrawingCollection : ObservableCollection<Drawing>
{
	/// <summary>Creates an empty collection.</summary>
	public DrawingCollection() { }

	/// <summary>Creates a collection with the given drawings.</summary>
	public DrawingCollection(System.Collections.Generic.IEnumerable<Drawing> drawings) : base(drawings) { }
}

/// <summary>
/// A composite drawing that groups child drawings, with optional transform, clip and opacity.
/// Mirrors <c>System.Windows.Media.DrawingGroup</c>.
/// </summary>
/// <remarks>
/// <b>What paints:</b> <see cref="Draw"/> applies an optional <see cref="Opacity"/>, saves/restores
/// the Cairo state, optionally clips to <see cref="ClipGeometry"/>, applies <see cref="Transform"/>,
/// then draws each child in order.
/// <b>Object-model only:</b> <see cref="Children"/> and <see cref="Bounds"/> are available without a surface.
/// </remarks>
public class DrawingGroup : Drawing
{
	readonly FreezableChildWatcher _watcher;
	DrawingCollection? _children;

	/// <summary>Creates an empty drawing group.</summary>
	public DrawingGroup() => _watcher = new FreezableChildWatcher(() => OnChanged());

	/// <summary>Identifies <see cref="Transform"/>.</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register(
		nameof(Transform), typeof(Transform), typeof(DrawingGroup), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="ClipGeometry"/>.</summary>
	public static readonly DependencyProperty ClipGeometryProperty = DependencyProperty.Register(
		nameof(ClipGeometry), typeof(Geometry), typeof(DrawingGroup), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="Opacity"/>. Default 1.</summary>
	public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
		nameof(Opacity), typeof(double), typeof(DrawingGroup), new PropertyMetadata(1.0));

	/// <summary>The ordered child drawings.</summary>
	public DrawingCollection Children
	{
		get
		{
			if (_children == null)
			{
				_children = new DrawingCollection();
				_children.CollectionChanged += OnChildrenChanged;
			}
			return _children;
		}
	}

	/// <summary>Optional transform applied before drawing children.</summary>
	public Transform? Transform
	{
		get => (Transform?)GetValue(TransformProperty);
		set => SetValue(TransformProperty, value);
	}

	/// <summary>Optional geometry used to clip children.</summary>
	public Geometry? ClipGeometry
	{
		get => (Geometry?)GetValue(ClipGeometryProperty);
		set => SetValue(ClipGeometryProperty, value);
	}

	/// <summary>Opacity multiplier (0–1) applied to the whole group.</summary>
	public double Opacity
	{
		get => (double)GetValue(OpacityProperty)!;
		set => SetValue(OpacityProperty, value);
	}

	void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		ThrowIfFrozen();
		_watcher.Reset(Children);
		OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == TransformProperty || e.Property == ClipGeometryProperty)
			_watcher.Set(e.Property, e.NewValue);
	}

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		if (_children == null) return;
		var group = (DrawingGroup)destination;
		foreach (var child in _children)
			group.Children.Add((Drawing)(currentValueOnly ? child.CloneCurrentValue() : child.Clone()));
	}

	/// <inheritdoc/>
	public override bool CanFreeze => base.CanFreeze && (_children == null || CanFreezeAll(_children));

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		Transform?.Freeze();
		ClipGeometry?.Freeze();
		if (_children != null)
			foreach (var child in _children) child.Freeze();
	}

	/// <inheritdoc/>
	public override Rect Bounds
	{
		get
		{
			if (_children == null || _children.Count == 0)
				return Rect.Empty;
			return DrawingGroupMath.Union(_children);
		}
	}

	/// <inheritdoc/>
	internal override void Draw(Cairo.Context cr)
	{
		if (_children == null || _children.Count == 0) return;

		cr.Save();

		if (Opacity < 1.0)
			cr.PushGroup();

		if (ClipGeometry != null)
		{
			ClipGeometry.BuildPath(cr);
			cr.Clip();
		}

		if (Transform != null)
		{
			var m = Transform.Value;
			var cm = new Cairo.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
			cr.Transform(cm);
		}

		foreach (var child in _children)
			child.Draw(cr);

		if (Opacity < 1.0)
		{
			cr.PopGroupToSource();
			cr.PaintWithAlpha(Opacity);
		}

		cr.Restore();
	}
}
