using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// ElementName bindings are generated before the target is in the tree and before the names are registered, so
/// each stays registered here and is re-resolved when a name scope or the tree around its target changes.
/// </summary>
internal static partial class BindingEngine
{
	static readonly List<WeakReference<BindingRecord>> _elementNameRecords = new List<WeakReference<BindingRecord>>();

	static BindingEngine() => DependencyObject.InheritanceParentChangedHook = TreeChanged;

	static void TrackElementName(BindingRecord record)
	{
		lock (_elementNameRecords) _elementNameRecords.Add(new WeakReference<BindingRecord>(record));
	}

	static void UntrackElementName(BindingRecord record)
	{
		lock (_elementNameRecords)
			_elementNameRecords.RemoveAll(w => !w.TryGetTarget(out var r) || ReferenceEquals(r, record));
	}

	static List<BindingRecord> SnapshotElementNameRecords()
	{
		var live = new List<BindingRecord>();
		lock (_elementNameRecords)
		{
			if (_elementNameRecords.Count == 0) return live;
			_elementNameRecords.RemoveAll(w =>
			{
				if (!w.TryGetTarget(out var r)) return true;
				live.Add(r);
				return false;
			});
		}
		return live;
	}

	/// <summary>A name was registered or removed: re-resolve the bindings that use it.</summary>
	internal static void NameScopeChanged(string? name)
	{
		foreach (var record in SnapshotElementNameRecords())
			if (record.ElementName != null && (name == null || record.ElementName == name)) record.ReResolve();
	}

	/// <summary>The data context of <paramref name="mentor"/> changed: refresh the non-element bindings it serves.</summary>
	static void RefreshMentored(DependencyObject mentor)
	{
		foreach (var record in SnapshotElementNameRecords())
			if (record.UsesMentorDataContext(mentor)) record.Activate();
	}

	/// <summary>The tree around <paramref name="changed"/> changed: re-resolve bindings in its subtree.</summary>
	internal static void TreeChanged(DependencyObject changed)
	{
		foreach (var record in SnapshotElementNameRecords())
			if (record.IsTargetUnder(changed)) record.ReResolve();
	}

	internal sealed partial class BindingRecord
	{
		object? _elementSource;

		internal string? ElementName => _binding.ElementName;

		internal bool UsesMentorDataContext(DependencyObject mentor) =>
			UsesDataContext && _targetRef.TryGetTarget(out var t) && !BindingTarget.IsElement(t)
			&& ReferenceEquals(BindingTarget.MentorOf(t), mentor);

		internal bool IsTargetUnder(DependencyObject ancestor)
		{
			if (!_targetRef.TryGetTarget(out var target)) return false;
			for (var current = target; current != null; current = current.ContextParent)
				if (ReferenceEquals(current, ancestor)) return true;
			return false;
		}

		internal void ReResolve()
		{
			if (!_targetRef.TryGetTarget(out var target)) return;
			if (_binding.ElementName == null
			    || !ReferenceEquals(BindingTarget.FindName(target, _binding.ElementName), _elementSource))
				Activate();
		}
	}
}
