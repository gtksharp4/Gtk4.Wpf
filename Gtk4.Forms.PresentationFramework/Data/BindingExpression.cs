namespace System.Windows.Data;

/// <summary>
/// Live binding-expression object for a <see cref="Binding"/> on a target
/// <see cref="FrameworkElement"/> DP.  Returned by
/// <see cref="FrameworkElement.GetBindingExpression"/> and
/// <see cref="FrameworkElement.SetBinding(DependencyProperty, Binding)"/>.
/// </summary>
public class BindingExpression : BindingExpressionBase
{
	readonly WeakReference<FrameworkElement> _targetRef;
	readonly DependencyProperty _dp;
	readonly Binding _binding;

	// Back-reference to the engine record so UpdateTarget/UpdateSource can call it.
	internal BindingEngine.BindingRecord? Record { get; set; }

	internal BindingExpression(FrameworkElement target, DependencyProperty dp, Binding binding)
	{
		_targetRef = new WeakReference<FrameworkElement>(target);
		_dp = dp;
		_binding = binding;
	}

	/// <summary>The <see cref="Binding"/> that produced this expression.</summary>
	public Binding ParentBinding => _binding;

	/// <summary>
	/// The resolved source object (the object the path is evaluated on).
	/// Returns <c>null</c> when the target has been collected or the source cannot be resolved.
	/// </summary>
	public object? ResolvedSource
	{
		get
		{
			if (!_targetRef.TryGetTarget(out var target)) return null;
			return Record?.GetResolvedSource(target);
		}
	}

	/// <summary>
	/// The root data item (same as <see cref="ResolvedSource"/> for simple paths;
	/// for multi-segment paths it is the top object).
	/// </summary>
	public object? DataItem => ResolvedSource;

	/// <summary>
	/// Re-reads the source value and pushes it to the target DP.
	/// Equivalent to triggering a source-change notification.
	/// </summary>
	public void UpdateTarget()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		Record?.PushToTargetPublic(target);
	}

	/// <summary>
	/// Writes the current target DP value back to the source (honours
	/// <see cref="UpdateSourceTrigger.Explicit"/>).
	/// </summary>
	public void UpdateSource()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		Record?.WriteBackPublic(target);
	}
}