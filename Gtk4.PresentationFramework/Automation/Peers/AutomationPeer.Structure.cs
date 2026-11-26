using System.Collections.Generic;

namespace System.Windows.Automation.Peers;

public abstract partial class AutomationPeer
{
	/// <summary>
	/// Compares the children now with the children last reported and raises the matching structure event:
	/// a lone new child raises ChildAdded about itself, a lone removed child ChildRemoved about it from the
	/// parent, groups the bulk forms, a different order ChildrenReordered, anything else ChildrenInvalidated.
	/// </summary>
	internal void NotifyChildrenChanged()
	{
		if (!ListenerExists(AutomationEvents.StructureChanged)) return;
		var before = _lastChildren;
		var after = GetChildren() ?? new List<AutomationPeer>();
		if (before == null) { RaiseStructureChanged(); return; }
		var added = after.FindAll(p => !before.Contains(p));
		var removed = before.FindAll(p => !after.Contains(p));
		if (added.Count == 0 && removed.Count == 0)
		{
			for (int i = 0; i < after.Count; i++)
				if (!ReferenceEquals(after[i], before[i]))
				{
					RaiseStructureChanged(StructureChangeType.ChildrenReordered, this);
					return;
				}
			return;
		}
		if (removed.Count == 0)
		{
			if (added.Count == 1) added[0].RaiseStructureChanged(StructureChangeType.ChildAdded, added[0]);
			else RaiseStructureChanged(StructureChangeType.ChildrenBulkAdded, this);
		}
		else if (added.Count == 0)
		{
			if (removed.Count == 1) RaiseStructureChanged(StructureChangeType.ChildRemoved, removed[0]);
			else RaiseStructureChanged(StructureChangeType.ChildrenBulkRemoved, this);
		}
		else RaiseStructureChanged();
	}
}
