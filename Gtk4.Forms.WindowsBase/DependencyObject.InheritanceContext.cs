namespace System.Windows;

public partial class DependencyObject
{
	WeakReference? _inheritanceContext;

	/// <summary>
	/// The object that holds this one as a property value (WPF's inheritance context / mentor chain), for
	/// freezables (and non-element objects in a resource dictionary), which are not in a tree of their own. The
	/// latest holder wins; WPF tracks all.
	/// </summary>
	internal DependencyObject? InheritanceContext
	{
		get => _inheritanceContext?.Target as DependencyObject;
		set
		{
			if (ReferenceEquals(InheritanceContext, value)) return;
			if (value != null && ContextChainReaches(value, this)) return;
			_inheritanceContext = value == null ? null : new WeakReference(value);
			InheritanceParentChangedHook?.Invoke(this);
		}
	}

	/// <summary>The next object up the tree: the inheritance parent, else the inheritance context.</summary>
	internal DependencyObject? ContextParent => _inheritanceParent ?? InheritanceContext;

	void UpdateContextOfLocal(object? oldLocal, object? newLocal)
	{
		if (ReferenceEquals(oldLocal, newLocal)) return;
		if (oldLocal is Freezable old && ReferenceEquals(old.InheritanceContext, this))
			old.InheritanceContext = null;
		if (newLocal is Freezable added) added.InheritanceContext = this;
	}

	// A context that would close a loop is ignored: the chain is only ever walked upward.
	static bool ContextChainReaches(DependencyObject start, DependencyObject target)
	{
		for (var (current, steps) = (start, 0); current != null && steps < 1000; current = current.ContextParent, steps++)
			if (ReferenceEquals(current, target)) return true;
		return false;
	}
}
