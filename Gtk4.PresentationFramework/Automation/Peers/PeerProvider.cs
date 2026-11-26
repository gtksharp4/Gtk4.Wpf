using System.Windows.Automation.Provider;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes an <see cref="AutomationPeer"/> to clients as an <see cref="IRawElementProviderSimple"/>.</summary>
sealed class PeerProvider : IRawElementProviderSimple
{
	internal PeerProvider(AutomationPeer peer) => Peer = peer;

	internal AutomationPeer Peer { get; }

	public ProviderOptions ProviderOptions => ProviderOptions.ServerSideProvider;

	public IRawElementProviderSimple? HostRawElementProvider => null;

	public object? GetPatternProvider(int patternId)
	{
		var pattern = PatternMap.ToInterface(patternId);
		return pattern.HasValue ? Peer.GetPattern(pattern.Value) : null;
	}

	public object? GetPropertyValue(int propertyId)
	{
		var peer = Peer;
		if (Is(propertyId, AutomationElementIdentifiers.NameProperty)) return peer.GetName();
		if (Is(propertyId, AutomationElementIdentifiers.AutomationIdProperty)) return peer.GetAutomationId();
		if (Is(propertyId, AutomationElementIdentifiers.ControlTypeProperty))
			return 50000 + (int)peer.GetAutomationControlType();
		if (Is(propertyId, AutomationElementIdentifiers.LocalizedControlTypeProperty))
			return peer.GetLocalizedControlType();
		if (Is(propertyId, AutomationElementIdentifiers.ClassNameProperty)) return peer.GetClassName();
		if (Is(propertyId, AutomationElementIdentifiers.HelpTextProperty)) return peer.GetHelpText();
		if (Is(propertyId, AutomationElementIdentifiers.IsEnabledProperty)) return peer.IsEnabled();
		if (Is(propertyId, AutomationElementIdentifiers.IsOffscreenProperty)) return peer.IsOffscreen();
		if (Is(propertyId, AutomationElementIdentifiers.HasKeyboardFocusProperty)) return peer.HasKeyboardFocus();
		if (Is(propertyId, AutomationElementIdentifiers.IsKeyboardFocusableProperty))
			return peer.IsKeyboardFocusable();
		if (Is(propertyId, AutomationElementIdentifiers.IsControlElementProperty)) return peer.IsControlElement();
		if (Is(propertyId, AutomationElementIdentifiers.IsContentElementProperty)) return peer.IsContentElement();
		if (Is(propertyId, AutomationElementIdentifiers.IsPasswordProperty)) return peer.IsPassword();
		if (Is(propertyId, AutomationElementIdentifiers.IsRequiredForFormProperty))
			return peer.IsRequiredForForm();
		if (Is(propertyId, AutomationElementIdentifiers.ItemStatusProperty)) return peer.GetItemStatus();
		if (Is(propertyId, AutomationElementIdentifiers.ItemTypeProperty)) return peer.GetItemType();
		if (Is(propertyId, AutomationElementIdentifiers.AcceleratorKeyProperty)) return peer.GetAcceleratorKey();
		if (Is(propertyId, AutomationElementIdentifiers.AccessKeyProperty)) return peer.GetAccessKey();
		if (Is(propertyId, AutomationElementIdentifiers.OrientationProperty))
			return (OrientationType)(int)peer.GetOrientation();
		if (Is(propertyId, AutomationElementIdentifiers.BoundingRectangleProperty))
			return peer.GetBoundingRectangle();
		if (Is(propertyId, AutomationElementIdentifiers.FrameworkIdProperty)) return "WPF";
		if (Is(propertyId, AutomationElementIdentifiers.LabeledByProperty))
		{
			var label = peer.GetLabeledBy();
			return label == null ? null : peer.ProviderFromPeer(label);
		}
		return null;
	}

	static bool Is(int id, AutomationProperty property) => id == property.Id;
}

static class PatternMap
{
	// PatternInterface order -> UI Automation pattern id.
	static readonly int[] _ids =
	{
		10000, 10001, 10002, 10003, 10004, 10017, 10005, 10006, 10007, 10008, 10009, 10010, 10011, 10012,
		10013, 10015, 10016, 10014, 10019, 10020, 10021,
	};

	internal static PatternInterface? ToInterface(int patternId)
	{
		for (int i = 0; i < _ids.Length; i++)
			if (_ids[i] == patternId)
				return (PatternInterface)i;
		return null;
	}
}
