namespace System.Windows.Media;

/// <summary>A visual that hosts child visuals. Mirrors WPF's ContainerVisual.</summary>
public class ContainerVisual : Visual
{
	VisualCollection? _children;
	double _opacity = 1.0;
	Transform? _transform;
	Vector _offset;
	Geometry? _clip;

	/// <summary>The child visuals, in z-order (last on top).</summary>
	public VisualCollection Children => _children ??= new VisualCollection(this);

	/// <summary>The visual parent.</summary>
	public DependencyObject? Parent => VisualParent;

	/// <summary>Opacity applied to this visual and its children (0..1).</summary>
	public double Opacity
	{
		get => _opacity;
		set
		{
			if (_opacity == value) return;
			_opacity = value;
			InvalidateRender();
		}
	}

	/// <summary>Transform applied to this visual's space before <see cref="Offset"/>.</summary>
	public Transform? Transform
	{
		get => _transform;
		set
		{
			_transform = value;
			InvalidateRender();
		}
	}

	/// <summary>Translation within the parent, applied after <see cref="Transform"/>.</summary>
	public Vector Offset
	{
		get => _offset;
		set
		{
			_offset = value;
			InvalidateRender();
		}
	}

	/// <summary>Clip applied to this visual and its children. Stored; the render layer does not clip.</summary>
	public Geometry? Clip
	{
		get => _clip;
		set
		{
			_clip = value;
			InvalidateRender();
		}
	}

	/// <summary>Bounds of this visual's own drawing; <see cref="Rect.Empty"/> when it draws nothing.</summary>
	public Rect ContentBounds => GetContentBounds();

	/// <summary>Union of the content bounds of this visual and its container descendants (in this space).</summary>
	public Rect DescendantBounds
	{
		get
		{
			Rect bounds = GetContentBounds();
			foreach (var child in Children)
			{
				if (child is not ContainerVisual c) continue;
				Rect cb = c.DescendantBounds;
				if (!cb.IsEmpty) bounds = Rect.Union(bounds, Transform_(c.ChildToParentMatrix(), cb));
			}
			return bounds;
		}
	}

	static Rect Transform_(Matrix m, Rect r) => Media.Transform.BoundsOf(m, r);

	/// <summary>Hit-tests this subtree at <paramref name="point"/>; the topmost hit or null.</summary>
	public HitTestResult? HitTest(Point point) => VisualTreeHelper.HitTest(this, point);

	/// <summary>Hit-tests this subtree, reporting through the callbacks.</summary>
	public void HitTest(HitTestFilterCallback? filterCallback, HitTestResultCallback resultCallback,
		HitTestParameters hitTestParameters) =>
		VisualTreeHelper.HitTest(this, filterCallback, resultCallback, hitTestParameters);

	internal virtual Rect GetContentBounds() => Rect.Empty;

	/// <inheritdoc/>
	protected internal override int VisualChildrenCount => _children == null ? 0 : _children.Count;

	/// <inheritdoc/>
	protected internal override Visual GetVisualChild(int index)
	{
		if (_children == null) throw new ArgumentOutOfRangeException(nameof(index));
		return _children[index];
	}

	internal override Matrix ChildToParentMatrix()
	{
		Matrix m = _transform?.Value ?? Matrix.Identity;
		m.Translate(_offset.X, _offset.Y);
		return m;
	}
}
