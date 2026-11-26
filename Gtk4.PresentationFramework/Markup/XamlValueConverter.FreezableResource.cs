using System.ComponentModel;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	/// <summary>
	/// Generator hook: <c>{DynamicResource}</c> on a <see cref="DependencyObject"/> that is not an element
	/// (a Freezable such as <c>SolidColorBrush</c>). The key is resolved once, when the value is assigned,
	/// against the enclosing <paramref name="scopes"/> (innermost first) and then <paramref name="scope"/>;
	/// it is not re-evaluated when a resource changes (documented divergence from WPF). A missing key leaves
	/// the property unchanged.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetResourceValue(
		object scope, DependencyObject target, DependencyProperty dp, object key, params object?[] scopes)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		var value = StaticResourceScope.TryFind(key, scope, scopes);
		if (value != null) target.SetValue(dp, CoerceResource(dp.PropertyType, value));
	}
}
