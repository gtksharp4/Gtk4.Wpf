using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace System.Windows;

/// <summary>
/// Automation hooks of <see cref="UIElement"/>: the cached automation peer and the push of peer state
/// (role, name, state) to the GTK 4 accessible API.
/// </summary>
public abstract partial class UIElement
{
	AutomationPeer? _automationPeer;

	/// <summary>Creates the automation peer of this element, or <c>null</c> when it has none.</summary>
	protected virtual AutomationPeer? OnCreateAutomationPeer() => null;

	internal AutomationPeer? ExistingAutomationPeer => _automationPeer;

	// Like WPF, a null result is not cached, so a later call asks again.
	internal AutomationPeer? GetAutomationPeer() => _automationPeer ??= OnCreateAutomationPeer();

	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/>: pushes <see cref="AutomationProperties"/> values and, when
	/// the element has a peer, its role, name and state. The role is set here because GTK freezes it once
	/// the widget's accessible context is realized. Unlike WPF, which creates peers when a client asks, the
	/// peer is created here, so a throwing <see cref="OnCreateAutomationPeer"/> is logged and skipped: it
	/// must never break widget creation.
	/// </summary>
	partial void ApplyAutomationOnCreated(Gtk.Widget widget)
	{
		UIElementAutomationPeer? peer = null;
		try { peer = GetAutomationPeer() as UIElementAutomationPeer; }
		catch (Exception e) { LogPeerFailure("OnCreateAutomationPeer", e); }
		AutomationAccessiblePush.Push(widget, this, created: true);
		try { peer?.PushAccessible(widget, created: true); }
		catch (Exception e) { LogPeerFailure("peer", e); }
		try { UIElementAutomationPeer.ResyncLabelledElements(this); }
		catch (Exception e) { LogPeerFailure("labelled-by", e); }
	}

	void LogPeerFailure(string what, Exception e) =>
		System.Diagnostics.Trace.TraceWarning($"{GetType().Name} {what} threw {e.GetType().Name}: {e.Message}");

	// Called when a selector's selection changed, so its items' selected state reaches GTK.
	internal void OnAutomationSelectionChanged()
	{
		if (_automationPeer is UIElementAutomationPeer peer && HasBackingWidget)
			peer.PushAccessible(Widget, created: false);
	}

	// Called from FrameworkElement.OnPropertyChanged.
	internal void OnAutomationPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (_automationPeer is not UIElementAutomationPeer peer) return;
		peer.OwnerPropertyChanged(e);
		if (HasBackingWidget && peer.AffectsAccessible(e.Property))
			peer.PushAccessible(Widget, created: false);
	}
}
