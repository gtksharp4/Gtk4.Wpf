using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

public partial class AutomationElement
{
	/// <summary>Returns the first element in <paramref name="scope"/> that matches, or <c>null</c>.</summary>
	/// <exception cref="ArgumentException"><paramref name="scope"/> includes Parent or Ancestors.</exception>
	public AutomationElement FindFirst(TreeScope scope, Condition condition)
	{
		var found = Find(scope, condition, firstOnly: true);
		return found.Count == 0 ? null! : found[0];
	}

	/// <summary>Returns the elements in <paramref name="scope"/> that match, in document order.</summary>
	/// <exception cref="ArgumentException"><paramref name="scope"/> includes Parent or Ancestors.</exception>
	public AutomationElementCollection FindAll(TreeScope scope, Condition condition) =>
		new AutomationElementCollection(Find(scope, condition, firstOnly: false));

	List<AutomationElement> Find(TreeScope scope, Condition condition, bool firstOnly)
	{
		if (condition == null) throw new ArgumentNullException(nameof(condition));
		if ((scope & (TreeScope.Parent | TreeScope.Ancestors)) != 0)
			throw new ArgumentException("Parent and Ancestors cannot be searched.", nameof(scope));
		if ((scope & TreeScope.Subtree) == 0)
			throw new ArgumentException("The scope must include Element, Children or Descendants.", nameof(scope));
		var request = CacheRequest.ActiveOrNull;
		return AutomationThread.Call(() =>
		{
			EnsureAvailable();
			var peers = new List<AutomationPeer>();
			ElementNavigation.Search(Peer, scope, condition, firstOnly, peers);
			var result = new List<AutomationElement>(peers.Count);
			foreach (var peer in peers)
				result.Add(Wrap(peer, request));
			return result;
		});
	}
}

/// <summary>A read-only list of automation elements.</summary>
public class AutomationElementCollection : ICollection
{
	readonly List<AutomationElement> _items;

	internal AutomationElementCollection(List<AutomationElement> items) => _items = items;

	public int Count => _items.Count;

	public bool IsSynchronized => false;

	public object SyncRoot => ((ICollection)_items).SyncRoot;

	public AutomationElement this[int index] => _items[index];

	public void CopyTo(AutomationElement[] array, int index) => _items.CopyTo(array, index);

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	public IEnumerator GetEnumerator() => _items.GetEnumerator();
}
