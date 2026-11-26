namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>Whether a binding is live on <paramref name="target"/>; a bound freezable cannot be frozen.</summary>
	static bool HasActiveBindings(DependencyObject target) =>
		_bindings.TryGetValue(target, out var records) && records.Count > 0;
}
