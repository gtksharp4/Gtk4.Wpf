namespace System.Windows.Data;

/// <summary>
/// Static helpers that set, clear, and query bindings on
/// <see cref="DependencyObject"/>s.  Routes through the existing engine so
/// all binding semantics (INPC, TwoWay, expressions) are preserved.
/// </summary>
public static class BindingOperations
{
	// ── SetBinding ───────────────────────────────────────────────────────────

	/// <summary>
	/// Applies <paramref name="binding"/> to <paramref name="dp"/> on
	/// <paramref name="target"/>.  Accepts <see cref="Binding"/>,
	/// <see cref="MultiBinding"/>, and <see cref="PriorityBinding"/>.
	/// </summary>
	public static BindingExpressionBase? SetBinding(
		DependencyObject target,
		DependencyProperty dp,
		BindingBase binding)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (binding == null) throw new ArgumentNullException(nameof(binding));

		if (target is Freezable { IsFrozen: true })
			throw new InvalidOperationException("Cannot bind a property of a frozen Freezable.");

		// All binding kinds (Binding, MultiBinding, PriorityBinding) route through
		// BindingEngine.SetBinding so the expression map stays in one place.
		return BindingEngine.SetBinding(target, dp, binding);
	}

	// ── ClearBinding / ClearAllBindings ──────────────────────────────────────

	/// <summary>Removes the binding (any kind) on <paramref name="dp"/>.</summary>
	public static void ClearBinding(DependencyObject target, DependencyProperty dp)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		if (dp == null) throw new ArgumentNullException(nameof(dp));

		// BindingEngine.ClearBinding is the single authority: it clears simple,
		// multi, and priority records plus the expression map entry.
		BindingEngine.ClearBinding(target, dp);
	}

	/// <summary>Removes all bindings on <paramref name="target"/>.</summary>
	public static void ClearAllBindings(DependencyObject target)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		BindingEngine.ClearAllBindings(target);
	}

	// ── GetBinding / GetBindingExpression / GetMultiBindingExpression ─────────

	/// <summary>
	/// Returns the <see cref="Binding"/> set on <paramref name="dp"/>, or
	/// <c>null</c> if the active binding is of a different kind or absent.
	/// </summary>
	public static Binding? GetBinding(DependencyObject target, DependencyProperty dp)
	{
		var expr = GetBindingExpressionBase(target, dp);
		return (expr as BindingExpression)?.ParentBinding;
	}

	/// <summary>
	/// Returns the live <see cref="BindingExpression"/> for a simple
	/// <see cref="Binding"/>, or <c>null</c>.
	/// </summary>
	public static BindingExpression? GetBindingExpression(
		DependencyObject target,
		DependencyProperty dp)
	{
		var expr = GetBindingExpressionBase(target, dp);
		return expr as BindingExpression;
	}

	/// <summary>
	/// Returns the live <see cref="MultiBindingExpression"/> for a
	/// <see cref="MultiBinding"/>, or <c>null</c>.
	/// </summary>
	public static MultiBindingExpression? GetMultiBindingExpression(
		DependencyObject target,
		DependencyProperty dp)
	{
		var expr = GetBindingExpressionBase(target, dp);
		return expr as MultiBindingExpression;
	}

	/// <summary>
	/// Returns the live <see cref="BindingExpressionBase"/> for any binding
	/// kind on <paramref name="dp"/>, or <c>null</c>.
	/// </summary>
	public static BindingExpressionBase? GetBindingExpressionBase(
		DependencyObject target,
		DependencyProperty dp)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return BindingEngine.GetBindingExpression(target, dp);
	}

	// ── IsDataBound ──────────────────────────────────────────────────────────

	/// <summary>
	/// Returns <c>true</c> when any binding (simple, multi, or priority) is
	/// active on <paramref name="dp"/> of <paramref name="target"/>.
	/// </summary>
	public static bool IsDataBound(DependencyObject target, DependencyProperty dp)
	{
		return GetBindingExpressionBase(target, dp) != null;
	}
}
