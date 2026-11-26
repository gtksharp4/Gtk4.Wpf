namespace System.Windows.Automation.Peers;

public partial class UIElementAutomationPeer
{
	/// <summary>The GTK role this peer wants on its widget, or <c>null</c> to keep the widget's own.</summary>
	internal virtual Gtk.AccessibleRole? AccessibleRole => AutomationRoleMap.For(GetAutomationControlType());

	/// <summary>Whether a change of <paramref name="property"/> must be pushed to GTK again.</summary>
	internal virtual bool AffectsAccessible(DependencyProperty property) =>
		property == UIElement.IsEnabledProperty || property == AutomationProperties.NameProperty
		|| property == AutomationProperties.HelpTextProperty
		|| property == AutomationProperties.AcceleratorKeyProperty || property == AutomationProperties.LabeledByProperty
		|| property == AutomationProperties.IsRequiredForFormProperty;

	/// <summary>The widget that carries the accessible node; the element's widget unless a wrapper hides it.</summary>
	internal virtual Gtk.Widget Target(Gtk.Widget widget) => widget;

	/// <summary>Raises automation events for a change of an owner property.</summary>
	internal virtual void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e) { }

	/// <summary>Pushes peer-specific state (checked, value, expanded...) to <paramref name="widget"/>.</summary>
	internal virtual void PushState(Gtk.Widget widget) { }

	/// <summary>
	/// Pushes role (only when <paramref name="created"/>: GTK freezes it once the AT context is realized),
	/// the explicit name (AutomationProperties.Name or the labelling element) and state to
	/// <paramref name="widget"/>; a name GTK derives from the content is left to GTK.
	/// </summary>
	internal void PushAccessible(Gtk.Widget widget, bool created)
	{
		widget = Target(widget);
		if (created)
		{
			var role = AccessibleRole;
			if (role.HasValue && widget.AccessibleRole != role.Value)
				widget.AccessibleRole = role.Value;
		}
		var name = AutomationProperties.GetName(Owner);
		if (string.IsNullOrEmpty(name)) name = GetLabeledBy()?.GetName();
		SyncLabel(widget, name);
		SyncKeyShortcuts(widget, GetAcceleratorKey());
		SyncRequired(widget, IsRequiredForForm());
		SyncLabelledBy(widget);
		PushState(widget);
	}

	bool _labelPushed;
	bool _requiredPushed;
	bool _keysPushed;
	bool _labelledByPushed;

	// Push a value, or reset the property when it was pushed before and is now empty.
	void SyncLabel(Gtk.Widget widget, string? name)
	{
		if (!string.IsNullOrEmpty(name)) PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.Label, name!);
		else if (_labelPushed) PeerAccessible.ResetProperty(widget, Gtk.AccessibleProperty.Label);
		_labelPushed = !string.IsNullOrEmpty(name);
	}

	void SyncRequired(Gtk.Widget widget, bool required)
	{
		if (required) PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.Required, true);
		else if (_requiredPushed) PeerAccessible.ResetProperty(widget, Gtk.AccessibleProperty.Required);
		_requiredPushed = required;
	}

	// The text goes verbatim: WPF's "Ctrl+Shift+C" is not rewritten into ARIA's "Control+Shift+C".
	void SyncKeyShortcuts(Gtk.Widget widget, string keys)
	{
		if (!string.IsNullOrEmpty(keys)) PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.KeyShortcuts, keys);
		else if (_keysPushed) PeerAccessible.ResetProperty(widget, Gtk.AccessibleProperty.KeyShortcuts);
		_keysPushed = !string.IsNullOrEmpty(keys);
	}
}
