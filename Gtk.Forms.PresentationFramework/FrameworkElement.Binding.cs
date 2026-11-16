using System.Windows.Data;

namespace System.Windows
{
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
		}

		/// <summary>
		/// WPF-compatible overload: attaches <paramref name="binding"/> to
		/// <paramref name="dp"/> and returns the live <see cref="BindingExpressionBase"/>
		/// (currently returns <c>null</c>; expression objects are Phase-2).
		/// </summary>
		public BindingExpressionBase? SetBinding(DependencyProperty dp, Binding binding)
		{
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			if (binding == null) throw new ArgumentNullException(nameof(binding));
			BindingEngine.SetBinding(this, dp, binding);
			return null;
		}

		/// <summary>
		/// Returns the <see cref="BindingExpressionBase"/> for <paramref name="dp"/>,
		/// or <c>null</c> when no binding is active. Phase-2 will return a real object;
		/// for now it is a cheap null placeholder matching the WPF signature.
		/// </summary>
		public BindingExpressionBase? GetBindingExpression(DependencyProperty dp)
		{
			return null;
		}
	}
}
