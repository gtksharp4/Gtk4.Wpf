namespace System.Windows.Data;

/// <summary>
/// Live binding-expression object for a <see cref="Binding"/> on a target
/// <see cref="FrameworkElement"/> DP.  Returned by
/// <see cref="FrameworkElement.GetBindingExpression"/> and
/// <see cref="FrameworkElement.SetBinding(DependencyProperty, Binding)"/>.
/// </summary>
public class BindingExpression : BindingExpressionBase
{
	readonly WeakReference<DependencyObject> _targetRef;
	readonly DependencyProperty _dp;
	readonly Binding _binding;

	// Back-reference to the engine record so UpdateTarget/UpdateSource can call it.
	internal BindingEngine.BindingRecord? Record { get; set; }

	internal BindingExpression(DependencyObject target, DependencyProperty dp, Binding binding)
	{
		_targetRef = new WeakReference<DependencyObject>(target);
		_dp = dp;
		_binding = binding;
	}

	internal DependencyObject? TargetElement => _targetRef.TryGetTarget(out var target) ? target : null;

	internal DependencyProperty TargetDependencyProperty => _dp;

	/// <summary>The <see cref="Binding"/> that produced this expression.</summary>
	public Binding ParentBinding => _binding;

	/// <inheritdoc/>
	public override BindingBase ParentBindingBase => _binding;

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

	internal override void UpdateTargetCore()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		Record?.PushToTargetPublic(target);
	}

	internal override void UpdateSourceCore()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		Record?.WriteBackPublic(target);
	}
}
