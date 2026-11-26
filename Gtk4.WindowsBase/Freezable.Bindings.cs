using System.Collections;

namespace System.Windows;

public abstract partial class Freezable
{
	/// <summary>Set by the binding engine: whether a binding is live on the object.</summary>
	internal static Func<DependencyObject, bool>? HasBindingsHook;

	// A bound freezable keeps receiving source values, so it (and every holder of it) cannot be frozen.
	bool BlockedByBindingOrChild() =>
		(HasBindingsHook?.Invoke(this) ?? false) || HasLocalFreezable(child => !child.IsFrozen && !child.CanFreeze);

	/// <summary>Whether every freezable in <paramref name="children"/> is frozen or can still be frozen.</summary>
	internal static bool CanFreezeAll(IEnumerable children)
	{
		foreach (var child in children)
			if (child is Freezable { IsFrozen: false, CanFreeze: false })
				return false;
		return true;
	}
}
