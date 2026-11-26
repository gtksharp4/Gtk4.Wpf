using System.Windows.Data;

namespace System.Windows;

public partial class FrameworkContentElement
{
	/// <summary>
	/// Attaches <paramref name="binding"/> to <paramref name="dp"/>. Only <see cref="Binding"/> is supported on
	/// content elements.
	/// </summary>
	/// <exception cref="NotSupportedException">A MultiBinding or PriorityBinding is given.</exception>
	public BindingExpressionBase? SetBinding(DependencyProperty dp, BindingBase binding)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (binding == null) throw new ArgumentNullException(nameof(binding));
		return BindingEngine.SetBinding(this, dp, binding);
	}

	/// <summary>Attaches a binding to <paramref name="path"/> of the data context.</summary>
	public BindingExpression? SetBinding(DependencyProperty dp, string path)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return SetBinding(dp, new Binding(path)) as BindingExpression;
	}

	/// <summary>Returns the live binding expression for <paramref name="dp"/>, or null.</summary>
	public BindingExpression? GetBindingExpression(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return BindingEngine.GetBindingExpression(this, dp) as BindingExpression;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		TargetChangeTracker.Dispatch(this, e);
		if (e.Property == DataContextProperty) BindingEngine.RefreshBindings(this);
	}
}
