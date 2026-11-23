using System.Windows.Data;

namespace System.Windows;

public partial class NameScope
{
	// The element whose Register/Unregister is running: only the bindings below it can see the change.
	[ThreadStatic]
	static DependencyObject? _notifying;

	static void OnNameScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		BindingEngine.NameScopeChanged(d);

	// Bindings by ElementName re-resolve when a name appears or goes.
	static void NameChanged(string name)
	{
		// Clear first: ReResolve can run user code that registers on another scope.
		var owner = _notifying;
		_notifying = null;
		if (owner != null) BindingEngine.NameScopeChanged(owner, name);
		else BindingEngine.NameScopeChanged(name);
	}

	static void NotifyingOwner(DependencyObject owner, System.Action change)
	{
		var previous = _notifying;
		_notifying = owner;
		try { change(); }
		finally { _notifying = previous; }
	}
}
