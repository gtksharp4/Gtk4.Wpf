using System.Windows.Data;

namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>
	/// Routes every effective-value change into <see cref="TargetChangeTracker"/> so
	/// TwoWay and OneWayToSource bindings can observe target DP mutations.
	/// </summary>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		TargetChangeTracker.Dispatch(this, e);
		SyncLayoutToWidget(e);
		if (e.Property == WidthProperty || e.Property == HeightProperty)
			Shapes.Shape.RefreshSlotCaps(this);
		OnAutomationPropertyChanged(e);
	}

	/// <summary>
	/// Attaches <paramref name="binding"/> to <paramref name="dp"/> and returns the live
	/// <see cref="BindingExpressionBase"/> (a <see cref="BindingExpression"/> for simple
	/// bindings, or <see cref="MultiBindingExpression"/> for multi-bindings).
	/// </summary>
	public BindingExpressionBase? SetBinding(DependencyProperty dp, Binding binding)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (binding == null) throw new ArgumentNullException(nameof(binding));
		return BindingEngine.SetBinding(this, dp, binding);
	}

	/// <summary>
	/// Attaches a <see cref="BindingBase"/> (<see cref="Binding"/>,
	/// <see cref="MultiBinding"/>, or <see cref="PriorityBinding"/>) to
	/// <paramref name="dp"/> and returns the live expression.
	/// </summary>
	public BindingExpressionBase? SetBinding(DependencyProperty dp, BindingBase binding)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (binding == null) throw new ArgumentNullException(nameof(binding));
		return BindingEngine.SetBinding(this, dp, binding);
	}

	/// <summary>
	/// Returns the live <see cref="BindingExpression"/> for <paramref name="dp"/>, or <c>null</c> when no
	/// <see cref="Binding"/> is active (use <see cref="BindingOperations.GetBindingExpressionBase"/> for the
	/// multi and priority kinds).
	/// </summary>
	public BindingExpression? GetBindingExpression(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return BindingEngine.GetBindingExpression(this, dp) as BindingExpression;
	}
}