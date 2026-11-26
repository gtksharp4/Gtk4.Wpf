namespace System.Windows;

public partial class DependencyObject
{
	WeakReference? _inheritanceContext;
	bool _hasMultipleContexts;

	/// <summary>
	/// The object that holds this one as a property value (WPF's inheritance context / mentor chain), for
	/// unfrozen freezables (and non-element objects in a resource dictionary), which are not in a tree of their
	/// own. A second holder clears it, as in WPF (<c>HasMultipleInheritanceContexts</c>).
	/// </summary>
	internal DependencyObject? InheritanceContext
	{
		get => _inheritanceContext?.Target as DependencyObject;
		set
		{
			var old = InheritanceContext;
			if (ReferenceEquals(old, value)) return;
			if (value != null && ContextChainReaches(value, this)) return;
			_inheritanceContext = value == null ? null : new WeakReference(value);
			DependencyObjectInheritance.TrackContext(this, old, value);
			InheritanceParentChangedHook?.Invoke(this);
		}
	}

	/// <summary>The next object up the tree: the inheritance parent, else the inheritance context.</summary>
	internal DependencyObject? ContextParent => _inheritanceParent ?? InheritanceContext;

	void RemoveLocalValue(DependencyProperty dp, object? oldLocal)
	{
		_localValues.Remove(dp);
		UpdateContextOfLocal(oldLocal, null);
	}

	void UpdateContextOfLocal(object? oldLocal, object? newLocal)
	{
		if (ReferenceEquals(oldLocal, newLocal)) return;
		if (oldLocal is Freezable old && ReferenceEquals(old.InheritanceContext, this))
			old.InheritanceContext = null;
		if (newLocal is Freezable { IsFrozen: false } added) added.AddContextHolder(this);
	}

	internal void AddContextHolder(DependencyObject holder)
	{
		var current = InheritanceContext;
		if (current == null && !_hasMultipleContexts)
		{
			InheritanceContext = holder;
		}
		else if (current != null && !ReferenceEquals(current, holder))
		{
			_hasMultipleContexts = true;
			InheritanceContext = null;
		}
	}

	// A context that would close a loop is ignored: the chain is only ever walked upward.
	static bool ContextChainReaches(DependencyObject start, DependencyObject target)
	{
		var current = start;
		for (int steps = 0; current != null && steps < 1000; current = current.ContextParent, steps++)
			if (ReferenceEquals(current, target)) return true;
		return false;
	}
}
