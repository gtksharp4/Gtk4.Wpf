using System.ComponentModel;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	/// <summary>
	/// Layer hook: a XAML-attribute <c>{DynamicResource}</c>. Resolves live like
	/// <see cref="FrameworkElement.SetResourceReference"/>, but keeps the value at local precedence (as WPF does),
	/// so a style trigger on the same property does not override it.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetLocalResourceReference(FrameworkElement target, DependencyProperty dp, object key)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		target.SetLocalResourceReference(dp, key);
	}
}
