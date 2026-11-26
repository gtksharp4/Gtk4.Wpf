using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public partial class PeerCoreTests
{
	[Fact]
	public void ListenerExists_and_events_reach_listeners_only_when_registered() => Run(() =>
	{
		var peer = UIElementAutomationPeer.CreatePeerForElement(new Button())!;
		var seen = new List<string>();
		void OnEvent(AutomationPeer p, AutomationEvents e) => seen.Add("event:" + e);
		void OnProp(AutomationPeer p, AutomationProperty prop, object? o, object? n) =>
			seen.Add($"prop:{prop.Id}:{o}->{n}");

		peer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
		Assert.False(AutomationPeer.ListenerExists(AutomationEvents.StructureChanged));

		AutomationPeer.AddListener(AutomationEvents.StructureChanged);
		AutomationPeer.AddListener(AutomationEvents.PropertyChanged);
		AutomationPeer.EventRaised += OnEvent;
		AutomationPeer.PropertyChangedRaised += OnProp;
		try
		{
			Assert.True(AutomationPeer.ListenerExists(AutomationEvents.StructureChanged));
			peer.InvalidatePeer();
			peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, "a", "b");
			peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, "same", "same");
		}
		finally
		{
			AutomationPeer.EventRaised -= OnEvent;
			AutomationPeer.PropertyChangedRaised -= OnProp;
			AutomationPeer.RemoveListener(AutomationEvents.StructureChanged);
			AutomationPeer.RemoveListener(AutomationEvents.PropertyChanged);
		}
		Assert.Equal(new[] { "event:StructureChanged", "prop:30005:a->b" }, seen);
		Assert.False(AutomationPeer.ListenerExists(AutomationEvents.StructureChanged));
	});

	[Fact]
	public void EventsSource_redirects_events() => Run(() =>
	{
		var source = UIElementAutomationPeer.CreatePeerForElement(new Button())!;
		var other = UIElementAutomationPeer.CreatePeerForElement(new CheckBox())!;
		other.EventsSource = source;
		AutomationPeer? raisedFor = null;
		void OnEvent(AutomationPeer p, AutomationEvents e) => raisedFor = p;
		AutomationPeer.AddListener(AutomationEvents.AsyncContentLoaded);
		AutomationPeer.EventRaised += OnEvent;
		try { other.RaiseAutomationEvent(AutomationEvents.AsyncContentLoaded); }
		finally
		{
			AutomationPeer.EventRaised -= OnEvent;
			AutomationPeer.RemoveListener(AutomationEvents.AsyncContentLoaded);
		}
		Assert.Same(source, raisedFor);
	});

	[Fact]
	public void Provider_bridge_exposes_patterns_and_properties() => Run(() =>
	{
		var button = new Button { Content = "Go" };
		var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
		var provider = new PeerProviderProbe(peer).Provider;
		Assert.Same(peer, new PeerProviderProbe(peer).PeerOf(provider));
		Assert.Equal("Go", provider.GetPropertyValue(AutomationElementIdentifiers.NameProperty.Id));
		Assert.Equal(50000, provider.GetPropertyValue(AutomationElementIdentifiers.ControlTypeProperty.Id));
		Assert.Same(peer, provider.GetPatternProvider(InvokePatternIdentifiers.Pattern.Id));
		Assert.Null(provider.GetPatternProvider(TogglePatternIdentifiers.Pattern.Id));
		Assert.Null(provider.GetPatternProvider(99999));
	});

	sealed class PeerProviderProbe : AutomationPeer
	{
		readonly AutomationPeer _peer;

		public PeerProviderProbe(AutomationPeer peer) => _peer = peer;

		public System.Windows.Automation.Provider.IRawElementProviderSimple Provider => ProviderFromPeer(_peer);

		public AutomationPeer? PeerOf(System.Windows.Automation.Provider.IRawElementProviderSimple p) =>
			PeerFromProvider(p);

		public override object? GetPattern(PatternInterface patternInterface) => null;

		protected override string GetClassNameCore() => "";
		protected override string GetNameCore() => "";
		protected override string GetAutomationIdCore() => "";
		protected override string GetHelpTextCore() => "";
		protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;
		protected override Rect GetBoundingRectangleCore() => Rect.Empty;
		protected override Point GetClickablePointCore() => default;
		protected override List<AutomationPeer>? GetChildrenCore() => null;
		protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;
		protected override bool HasKeyboardFocusCore() => false;
		protected override bool IsContentElementCore() => false;
		protected override bool IsControlElementCore() => false;
		protected override bool IsEnabledCore() => true;
		protected override bool IsKeyboardFocusableCore() => false;
		protected override bool IsOffscreenCore() => false;
		protected override bool IsPasswordCore() => false;
		protected override bool IsRequiredForFormCore() => false;
		protected override void SetFocusCore() { }
	}
}
