namespace System.Windows.Data;

/// <summary>
/// ElementName bindings are generated before the target is in the tree and before the names are registered, so
/// each stays registered here and is re-resolved when a name scope or the tree around its target changes. Bindings
/// on non-element targets register too: their mentor can appear later.
/// </summary>
internal static partial class BindingEngine
{
	/// <summary>Something whose source depends on a name scope or on the tree around its target.</summary>
	internal interface IContextListener
	{
		DependencyObject? Target { get; }

		string? ElementName { get; }

		void ReResolve();
	}

	static BindingEngine()
	{
		DependencyObject.InheritanceParentChangedHook = TreeChanged;
		Freezable.HasBindingsHook = HasActiveBindings;
	}

	/// <summary>A name was registered or removed (null: a whole scope changed): re-resolve what uses it.</summary>
	internal static void NameScopeChanged(string? name)
	{
		if (_listenerCount == 0) return;
		foreach (var listener in ListenersNamed(name)) listener.ReResolve();
	}

	/// <summary>The data context of <paramref name="mentor"/> changed: refresh the non-element bindings.</summary>
	static void RefreshMentored(DependencyObject mentor)
	{
		foreach (var listener in ListenersUnder(mentor))
			if (listener is BindingRecord record && record.UsesMentorDataContext(mentor)) record.Activate();
	}

	/// <summary>The tree around <paramref name="changed"/> changed: re-resolve listeners in its subtree.</summary>
	internal static void TreeChanged(DependencyObject changed)
	{
		foreach (var listener in ListenersUnder(changed)) listener.ReResolve();
	}

	internal sealed partial class BindingRecord : IContextListener
	{
		object? _elementSource;

		public DependencyObject? Target => _targetRef.TryGetTarget(out var target) ? target : null;

		public string? ElementName => _binding.ElementName;

		internal bool UsesMentorDataContext(DependencyObject mentor) =>
			UsesDataContext && _targetRef.TryGetTarget(out var t) && !BindingTarget.IsElement(t)
			&& ReferenceEquals(BindingTarget.MentorOf(t), mentor);

		public void ReResolve()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			if (_binding.ElementName == null
			    || !ReferenceEquals(BindingTarget.FindName(target, _binding.ElementName), _elementSource))
				Activate();
		}
	}
}
