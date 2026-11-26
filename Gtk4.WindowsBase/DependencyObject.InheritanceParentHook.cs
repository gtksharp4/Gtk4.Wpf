namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>
	/// Raised after <see cref="InheritanceParent"/> changed, for layers above (name resolution of bindings).
	/// </summary>
	internal static Action<DependencyObject>? InheritanceParentChangedHook;
}
