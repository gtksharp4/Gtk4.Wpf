using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Automation;

public class PeerCoreTests : WpfGtkTestBase
{
	sealed class Plain : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class CustomPeerButton : Button
	{
		protected override AutomationPeer? OnCreateAutomationPeer() => new CustomPeer(this);
	}

	sealed class CustomPeer : ButtonAutomationPeer
	{
		public CustomPeer(CustomPeerButton owner) : base(owner) { }

		protected override string GetClassNameCore() => "Custom";

		protected override string GetNameCore() => "from-peer";
	}

	[Fact]
	public void Element_without_a_peer_override_has_no_peer() => Run(() =>
	{
		Assert.Null(UIElementAutomationPeer.CreatePeerForElement(new Plain()));
		Assert.Null(UIElementAutomationPeer.CreatePeerForElement(new StackPanel()));
	});

	[Fact]
	public void CreatePeerForElement_caches_and_FromElement_does_not_create() => Run(() =>
	{
		var button = new Button();
		Assert.Null(UIElementAutomationPeer.FromElement(button));
		var peer = UIElementAutomationPeer.CreatePeerForElement(button);
		Assert.IsType<ButtonAutomationPeer>(peer);
		Assert.Same(peer, UIElementAutomationPeer.CreatePeerForElement(button));
		Assert.Same(peer, UIElementAutomationPeer.FromElement(button));
		Assert.Same(button, ((UIElementAutomationPeer)peer!).Owner);
	});

	[Fact]
	public void Null_element_gives_a_null_peer() => Run(() =>
	{
		Assert.Null(UIElementAutomationPeer.CreatePeerForElement(null!));
		Assert.Null(UIElementAutomationPeer.FromElement(null!));
	});

	[Fact]
	public void Custom_OnCreateAutomationPeer_override_is_used() => Run(() =>
	{
		var peer = UIElementAutomationPeer.CreatePeerForElement(new CustomPeerButton())!;
		Assert.Equal("Custom", peer.GetClassName());
		Assert.Equal("from-peer", peer.GetName());
		Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
	});

	[Fact]
	public void Name_prefers_AutomationProperties_then_labelling_element_then_content() => Run(() =>
	{
		var button = new Button { Content = "OK" };
		var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
		Assert.Equal("OK", peer.GetName());
		var label = new TextBlock { Text = "Accept" };
		AutomationProperties.SetLabeledBy(button, label);
		Assert.Equal("Accept", peer.GetName());
		AutomationProperties.SetName(button, "Explicit");
		Assert.Equal("Explicit", peer.GetName());
	});

	[Fact]
	public void AutomationId_falls_back_to_the_element_name() => Run(() =>
	{
		var box = new TextBox { Name = "UserName" };
		var peer = UIElementAutomationPeer.CreatePeerForElement(box)!;
		Assert.Equal("UserName", peer.GetAutomationId());
		AutomationProperties.SetAutomationId(box, "txtUser");
		Assert.Equal("txtUser", peer.GetAutomationId());
	});

	[Fact]
	public void Property_wrappers_follow_the_owner() => Run(() =>
	{
		var box = new TextBox { IsEnabled = false };
		AutomationProperties.SetHelpText(box, "Enter a name");
		AutomationProperties.SetIsRequiredForForm(box, true);
		var peer = UIElementAutomationPeer.CreatePeerForElement(box)!;
		Assert.False(peer.IsEnabled());
		Assert.Equal("Enter a name", peer.GetHelpText());
		Assert.True(peer.IsRequiredForForm());
		Assert.True(peer.IsControlElement());
		Assert.True(peer.IsContentElement());
		Assert.False(peer.IsPassword());
		Assert.Equal("edit", peer.GetLocalizedControlType());
		Assert.Equal(AutomationOrientation.None, peer.GetOrientation());
	});

	[Fact]
	public void Peer_tree_mirrors_the_visual_tree_and_skips_elements_without_peers() => Run(() =>
	{
		var button = new Button { Content = "Go" };
		var check = new CheckBox();
		var inner = new StackPanel();
		var box = new TextBox();
		inner.Children.Add(check);
		inner.Children.Add(box);
		var root = new StackPanel();
		root.Children.Add(button);
		root.Children.Add(inner);
		var window = new Window { Content = root };

		var windowPeer = UIElementAutomationPeer.CreatePeerForElement(window)!;
		var children = windowPeer.GetChildren()!;

		Assert.Equal(new AutomationPeer[]
		{
			UIElementAutomationPeer.CreatePeerForElement(button)!,
			UIElementAutomationPeer.CreatePeerForElement(check)!,
			UIElementAutomationPeer.CreatePeerForElement(box)!,
		}, children);
		foreach (var child in children)
			Assert.Same(windowPeer, child.GetParent());
		Assert.Null(windowPeer.GetParent());
	});

	[Fact]
	public void Leaf_peer_has_no_children() => Run(() =>
	{
		Assert.Null(UIElementAutomationPeer.CreatePeerForElement(new Button())!.GetChildren());
	});

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
