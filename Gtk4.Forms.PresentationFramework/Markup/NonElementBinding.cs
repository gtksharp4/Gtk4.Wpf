using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace System.Windows.Markup;

/// <summary>
/// Generated-code entry for a <c>{Binding}</c> on a <see cref="DependencyObject"/> that is not an element (an
/// animation, a freezable, a <c>CollectionViewSource</c>). The binding engine serves elements only, so such a
/// binding is not evaluated: the property keeps its local value instead of the markup throwing at load.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class NonElementBinding
{
	/// <summary>
	/// Binds through <see cref="BindingOperations"/>; <c>null</c> (and a trace warning) when the engine has
	/// no support for the target.
	/// </summary>
	public static BindingExpressionBase? TrySetBinding(
		DependencyObject target, DependencyProperty dp, BindingBase binding)
	{
		try
		{
			return BindingOperations.SetBinding(target, dp, binding);
		}
		catch (NotSupportedException)
		{
			var path = (binding as Binding)?.Path?.Path;
			Trace.TraceWarning("Binding on non-element " + target.GetType().Name + "." + dp.Name
				+ " (path '" + path + "') is not evaluated.");
			return null;
		}
	}
}
