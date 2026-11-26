namespace System.Windows.Data;

/// <summary>
/// Live expression object returned when a <see cref="PriorityBinding"/> is applied
/// via <see cref="BindingOperations.SetBinding"/> or
/// <see cref="FrameworkElement.SetBinding(DependencyProperty, BindingBase)"/>.
/// </summary>
public sealed class PriorityBindingExpression : BindingExpressionBase
{
	readonly WeakReference<FrameworkElement> _targetRef;
	readonly DependencyProperty _dp;
	readonly PriorityBinding _pb;

	/// <summary>The <see cref="PriorityBinding"/> that produced this expression.</summary>
	public PriorityBinding ParentPriorityBinding => _pb;

	internal PriorityBindingExpression(
		FrameworkElement target,
		DependencyProperty dp,
		PriorityBinding pb)
	{
		_targetRef = new WeakReference<FrameworkElement>(target);
		_dp = dp;
		_pb = pb;
	}

	/// <inheritdoc/>
	public override BindingBase ParentBindingBase => _pb;

	// A priority binding only reads its sources.
	internal override void UpdateSourceCore() { }

	internal override void UpdateTargetCore()
	{
		if (!_targetRef.TryGetTarget(out var target)) return;
		// No engine-record reference needed: re-activate via public Refresh.
		PriorityBindingEngine.ClearForDp(target, _dp);
		PriorityBindingEngine.SetBinding(target, _dp, _pb);
	}
}
