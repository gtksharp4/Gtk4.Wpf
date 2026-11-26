namespace System.Windows.Media;

public abstract partial class Visual
{
	/// <summary>Matrix mapping this visual's space into its parent's (row-vector convention).</summary>
	internal virtual Matrix ChildToParentMatrix() => Matrix.Identity;

	Matrix ToAncestorMatrix(Visual ancestor)
	{
		Matrix m = Matrix.Identity;
		for (Visual? v = this; v != null && !ReferenceEquals(v, ancestor); v = v.VisualParent)
			m = m * v.ChildToParentMatrix();
		return m;
	}

	/// <summary>Transform from this visual's space to that of <paramref name="ancestor"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">The argument is not an ancestor.</exception>
	public GeneralTransform TransformToAncestor(Visual ancestor)
	{
		if (ancestor == null) throw new ArgumentNullException(nameof(ancestor));
		if (!ancestor.IsAncestorOf(this))
			throw new InvalidOperationException("The specified Visual is not an ancestor of this Visual.");
		return new MatrixTransform(ToAncestorMatrix(ancestor));
	}

	/// <summary>
	/// Transform from this visual's space to that of <paramref name="descendant"/>; null if not invertible.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">The argument is not a descendant.</exception>
	public GeneralTransform? TransformToDescendant(Visual descendant)
	{
		if (descendant == null) throw new ArgumentNullException(nameof(descendant));
		if (!descendant.IsDescendantOf(this))
			throw new InvalidOperationException("The specified Visual is not a descendant of this Visual.");
		Matrix m = descendant.ToAncestorMatrix(this);
		if (!m.HasInverse) return null;
		m.Invert();
		return new MatrixTransform(m);
	}

	/// <summary>Transform from this visual's space to that of <paramref name="visual"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">The visuals share no common ancestor.</exception>
	public GeneralTransform? TransformToVisual(Visual visual)
	{
		if (visual == null) throw new ArgumentNullException(nameof(visual));
		var common = FindCommonVisualAncestor(visual) as Visual
			?? throw new InvalidOperationException("The visuals do not share a common ancestor.");
		Matrix target = visual.ToAncestorMatrix(common);
		if (!target.HasInverse) return null;
		target.Invert();
		return new MatrixTransform(ToAncestorMatrix(common) * target);
	}
}
