using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>Navigates the automation tree through a view: the elements that satisfy a condition.</summary>
public sealed class TreeWalker
{
	public static readonly TreeWalker RawViewWalker = new TreeWalker(Automation.RawViewCondition);
	public static readonly TreeWalker ControlViewWalker = new TreeWalker(Automation.ControlViewCondition);
	public static readonly TreeWalker ContentViewWalker = new TreeWalker(Automation.ContentViewCondition);

	public TreeWalker(Condition condition) =>
		Condition = condition ?? throw new ArgumentNullException(nameof(condition));

	/// <summary>The condition an element must satisfy to be in the view.</summary>
	public Condition Condition { get; }

	public AutomationElement GetParent(AutomationElement element) => GetParent(element, null);

	public AutomationElement GetParent(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => ElementNavigation.Parent(peer, filter));

	public AutomationElement GetFirstChild(AutomationElement element) => GetFirstChild(element, null);

	public AutomationElement GetFirstChild(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => FirstOrNull(ElementNavigation.Children(peer, filter), last: false));

	public AutomationElement GetLastChild(AutomationElement element) => GetLastChild(element, null);

	public AutomationElement GetLastChild(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => FirstOrNull(ElementNavigation.Children(peer, filter), last: true));

	public AutomationElement GetNextSibling(AutomationElement element) => GetNextSibling(element, null);

	public AutomationElement GetNextSibling(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => ElementNavigation.Sibling(peer, filter, 1));

	public AutomationElement GetPreviousSibling(AutomationElement element) => GetPreviousSibling(element, null);

	public AutomationElement GetPreviousSibling(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => ElementNavigation.Sibling(peer, filter, -1));

	/// <summary>Returns the element if it is in the view, otherwise its nearest ancestor that is.</summary>
	public AutomationElement Normalize(AutomationElement element) => Normalize(element, null);

	public AutomationElement Normalize(AutomationElement element, CacheRequest? request) =>
		Walk(element, request, (peer, filter) => ElementNavigation.Normalize(peer, filter));

	static AutomationPeer? FirstOrNull(System.Collections.Generic.List<AutomationPeer> peers, bool last) =>
		peers.Count == 0 ? null : peers[last ? peers.Count - 1 : 0];

	AutomationElement Walk(AutomationElement element, CacheRequest? request,
		Func<AutomationPeer, Condition, AutomationPeer?> step)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		request ??= CacheRequest.ActiveOrNull;
		var snapshot = request?.Clone();
		return AutomationThread.Call(() =>
		{
			element.EnsureAvailable();
			var peer = step(element.Peer, Condition);
			return peer == null ? null! : AutomationElement.Wrap(peer, snapshot);
		});
	}
}
