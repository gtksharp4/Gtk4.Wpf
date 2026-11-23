using System.Collections.Generic;

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
		string? ElementName { get; }

		bool IsUnder(DependencyObject ancestor);

		void ReResolve();
	}

	static readonly List<WeakReference<IContextListener>> _listeners = new List<WeakReference<IContextListener>>();

	static BindingEngine() => DependencyObject.InheritanceParentChangedHook = TreeChanged;

	internal static void RegisterContextListener(IContextListener listener)
	{
		lock (_listeners) _listeners.Add(new WeakReference<IContextListener>(listener));
	}

	internal static void UnregisterContextListener(IContextListener listener)
	{
		lock (_listeners)
			_listeners.RemoveAll(w => !w.TryGetTarget(out var l) || ReferenceEquals(l, listener));
	}

	static List<IContextListener> SnapshotListeners()
	{
		var live = new List<IContextListener>();
		lock (_listeners)
		{
			if (_listeners.Count == 0) return live;
			_listeners.RemoveAll(w =>
			{
				if (!w.TryGetTarget(out var l)) return true;
				live.Add(l);
				return false;
			});
		}
		return live;
	}

	/// <summary>A name was registered or removed (null: a whole scope changed): re-resolve what uses it.</summary>
	internal static void NameScopeChanged(string? name)
	{
		foreach (var listener in SnapshotListeners())
			if (listener.ElementName != null && (name == null || listener.ElementName == name)) listener.ReResolve();
	}

	/// <summary>The data context of <paramref name="mentor"/> changed: refresh the non-element bindings it serves.</summary>
	static void RefreshMentored(DependencyObject mentor)
	{
		foreach (var listener in SnapshotListeners())
			if (listener is BindingRecord record && record.UsesMentorDataContext(mentor)) record.Activate();
	}

	/// <summary>The tree around <paramref name="changed"/> changed: re-resolve listeners in its subtree.</summary>
	internal static void TreeChanged(DependencyObject changed)
	{
		foreach (var listener in SnapshotListeners())
			if (listener.IsUnder(changed)) listener.ReResolve();
	}

	internal sealed partial class BindingRecord : IContextListener
	{
		object? _elementSource;

		public string? ElementName => _binding.ElementName;

		internal bool UsesMentorDataContext(DependencyObject mentor) =>
			UsesDataContext && _targetRef.TryGetTarget(out var t) && !BindingTarget.IsElement(t)
			&& ReferenceEquals(BindingTarget.MentorOf(t), mentor);

		public bool IsUnder(DependencyObject ancestor) =>
			_targetRef.TryGetTarget(out var target) && BindingTarget.IsUnder(target, ancestor);

		public void ReResolve()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			if (_binding.ElementName == null
			    || !ReferenceEquals(BindingTarget.FindName(target, _binding.ElementName), _elementSource))
				Activate();
		}
	}
}
