namespace System.Windows;

public partial class DependencyObject
{
	WeakReference? _inheritanceContext;

	/// <summary>
	/// The object that holds this one as a property value (WPF's inheritance context / mentor chain), for
	/// objects that are not in a tree of their own, such as freezables. The latest holder wins; WPF tracks all.
	/// </summary>
	internal DependencyObject? InheritanceContext
	{
		get => _inheritanceContext?.Target as DependencyObject;
		set
		{
			if (ReferenceEquals(InheritanceContext, value)) return;
			_inheritanceContext = value == null ? null : new WeakReference(value);
			InheritanceParentChangedHook?.Invoke(this);
		}
	}

	/// <summary>The next object up the tree: the inheritance parent, else the inheritance context.</summary>
	internal DependencyObject? ContextParent => _inheritanceParent ?? InheritanceContext;

	void UpdateContextOfLocal(object? oldLocal, object? newLocal)
	{
		if (ReferenceEquals(oldLocal, newLocal)) return;
		if (oldLocal is DependencyObject old && ReferenceEquals(old.InheritanceContext, this))
			old.InheritanceContext = null;
		if (newLocal is DependencyObject added && !ReferenceEquals(added, this)) added.InheritanceContext = this;
	}
}
