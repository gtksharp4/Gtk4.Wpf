namespace System.Windows.Data;

/// <summary>
/// Live binding-expression object for a <see cref="MultiBinding"/> on a target DP.
/// Returned by <see cref="FrameworkElement.GetBindingExpression"/> when a
/// <see cref="MultiBinding"/> is active.
/// </summary>
public class MultiBindingExpression : BindingExpressionBase
{
	readonly WeakReference<FrameworkElement> _targetRef;
	readonly DependencyProperty _dp;
	readonly MultiBinding _binding;

	internal MultiBindingExpression(
		FrameworkElement target,
		DependencyProperty dp,
		MultiBinding binding)
	{
		_targetRef = new WeakReference<FrameworkElement>(target);
		_dp = dp;
		_binding = binding;
	}

	/// <summary>The <see cref="MultiBinding"/> that produced this expression.</summary>
	public MultiBinding ParentMultiBinding => _binding;

	/// <inheritdoc/>
	public override BindingBase ParentBindingBase => _binding;

	internal override void UpdateTargetCore()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		MultiBindingEngine.Refresh(target, _dp);
	}

	internal override void UpdateSourceCore()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		MultiBindingEngine.UpdateSource(target, _dp);
	}
}