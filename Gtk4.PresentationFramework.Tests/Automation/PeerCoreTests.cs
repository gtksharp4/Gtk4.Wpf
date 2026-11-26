using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public partial class PeerCoreTests : WpfGtkTestBase
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

	sealed class ThrowingPeerButton : Button
	{
		protected override AutomationPeer? OnCreateAutomationPeer() => throw new InvalidOperationException("boom");
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
	public void A_throwing_OnCreateAutomationPeer_does_not_break_widget_creation() => Run(() =>
	{
		var button = new ThrowingPeerButton { Content = "x" };
		var window = new Window { Content = button };
		window.Show();
		Assert.True(GtkPump.Until(() => window.Widget.IsMapped));
		Assert.NotNull(button.Widget);
		Assert.True(button.Widget.IsMapped);
		Assert.Null(UIElementAutomationPeer.FromElement(button));
		Assert.Throws<InvalidOperationException>(() => UIElementAutomationPeer.CreatePeerForElement(button));
	});

	[Fact]
	public void Creating_the_widget_creates_the_peer_unlike_WPF() => Run(() =>
	{
		var button = new Button();
		Assert.Null(UIElementAutomationPeer.FromElement(button));
		_ = button.Widget;
		Assert.IsType<ButtonAutomationPeer>(UIElementAutomationPeer.FromElement(button));
	});
}
