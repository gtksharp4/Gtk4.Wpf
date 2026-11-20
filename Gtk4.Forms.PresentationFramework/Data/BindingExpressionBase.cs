namespace System.Windows.Data;

/// <summary>
/// Abstract base for binding expression objects (<see cref="BindingExpression"/>,
/// <see cref="MultiBindingExpression"/>, <see cref="PriorityBindingExpression"/>).
/// </summary>
public abstract class BindingExpressionBase
{
	internal BindingExpressionBase() { }

	/// <summary>The <see cref="BindingBase"/> that produced this expression.</summary>
	public abstract BindingBase ParentBindingBase { get; }

	/// <summary>Re-reads the source value and pushes it to the target.</summary>
	public void UpdateTarget() => UpdateTargetCore();

	/// <summary>
	/// Writes the current target value back to the source, whatever the
	/// <see cref="UpdateSourceTrigger"/>.
	/// </summary>
	public void UpdateSource() => UpdateSourceCore();

	internal abstract void UpdateTargetCore();

	internal abstract void UpdateSourceCore();
}
