using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class AutomationPropertiesItemsTests : WpfGtkTestBase
{
	sealed class Stub : UIElement
	{
		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException();
	}

	static object? Provided(AutomationPeer peer, AutomationProperty id) =>
		new PeerProvider(peer).GetPropertyValue(id.Id);

	static string ParamOf(Action act) => Assert.Throws<ArgumentNullException>(act).ParamName!;

	[Fact]
	public void Defaults()
	{
		var e = new Stub();
		Assert.Equal(string.Empty, AutomationProperties.GetAcceleratorKey(e));
		Assert.Equal(string.Empty, AutomationProperties.GetItemStatus(e));
		Assert.Equal(AutomationHeadingLevel.None, AutomationProperties.GetHeadingLevel(e));
	}

	[Fact]
	public void RoundTrip()
	{
		var e = new Stub();
		AutomationProperties.SetAcceleratorKey(e, "Ctrl+Shift+C");
		AutomationProperties.SetItemStatus(e, "Busy");
		AutomationProperties.SetHeadingLevel(e, AutomationHeadingLevel.Level3);
		Assert.Equal("Ctrl+Shift+C", AutomationProperties.GetAcceleratorKey(e));
		Assert.Equal("Busy", AutomationProperties.GetItemStatus(e));
		Assert.Equal(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(e));
	}

	[Fact]
	public void ClearValue_restores_defaults()
	{
		var e = new Stub();
		AutomationProperties.SetAcceleratorKey(e, "A");
		AutomationProperties.SetItemStatus(e, "B");
		AutomationProperties.SetHeadingLevel(e, AutomationHeadingLevel.Level1);
		e.ClearValue(AutomationProperties.AcceleratorKeyProperty);
		e.ClearValue(AutomationProperties.ItemStatusProperty);
		e.ClearValue(AutomationProperties.HeadingLevelProperty);
		Assert.Equal(string.Empty, AutomationProperties.GetAcceleratorKey(e));
		Assert.Equal(string.Empty, AutomationProperties.GetItemStatus(e));
		Assert.Equal(AutomationHeadingLevel.None, AutomationProperties.GetHeadingLevel(e));
	}

	[Fact]
	public void Null_element_throws_ArgumentNullException()
	{
		Assert.Equal("element", ParamOf(() => AutomationProperties.GetAcceleratorKey(null!)));
		Assert.Equal("element", ParamOf(() => AutomationProperties.SetAcceleratorKey(null!, "a")));
		Assert.Equal("element", ParamOf(() => AutomationProperties.GetItemStatus(null!)));
		Assert.Equal("element", ParamOf(() => AutomationProperties.SetItemStatus(null!, "a")));
		Assert.Equal("element", ParamOf(() => AutomationProperties.GetHeadingLevel(null!)));
		Assert.Equal("element", ParamOf(
			() => AutomationProperties.SetHeadingLevel(null!, AutomationHeadingLevel.Level1)));
	}

	[Fact]
	public void Null_value_on_string_setters_throws_ArgumentException()
	{
		var e = new Stub();
		Assert.Throws<ArgumentException>(() => AutomationProperties.SetAcceleratorKey(e, null!));
		Assert.Throws<ArgumentException>(() => AutomationProperties.SetItemStatus(e, null!));
	}

	[Theory]
	[InlineData("AcceleratorKey", typeof(string))]
	[InlineData("ItemStatus", typeof(string))]
	[InlineData("HeadingLevel", typeof(AutomationHeadingLevel))]
	public void Registration_matches_WPF(string name, Type type)
	{
		var dp = DependencyProperty.FindByName(name, typeof(AutomationProperties))!;
		Assert.NotNull(dp);
		Assert.Equal(type, dp.PropertyType);
		Assert.Equal(typeof(AutomationProperties), dp.OwnerType);
		Assert.IsType<UIPropertyMetadata>(dp.DefaultMetadata);
		object expected = type == typeof(string) ? string.Empty : AutomationHeadingLevel.None;
		Assert.Equal(expected, dp.DefaultMetadata.DefaultValue);
		Assert.Equal(type == typeof(string), dp.ValidateValueCallback != null);
	}

	[Fact]
	public void Validation_rejects_null_and_accepts_strings_for_both_string_properties()
	{
		foreach (var dp in new[] { AutomationProperties.AcceleratorKeyProperty, AutomationProperties.ItemStatusProperty })
		{
			Assert.Throws<ArgumentException>(() => new Stub().SetValue(dp, null));
			var e = new Stub();
			e.SetValue(dp, "text");
			Assert.Equal("text", e.GetValue(dp));
		}
	}

	[Fact]
	public void Peer_reads_each_property()
	{
		var e = new Stub();
		var peer = new UIElementAutomationPeer(e);
		Assert.Equal(string.Empty, peer.GetAcceleratorKey());
		Assert.Equal(AutomationHeadingLevel.None, peer.GetHeadingLevel());
		Assert.Equal(string.Empty, peer.GetItemStatus());
		AutomationProperties.SetAcceleratorKey(e, "Ctrl+K");
		AutomationProperties.SetItemStatus(e, "Done");
		AutomationProperties.SetHeadingLevel(e, AutomationHeadingLevel.Level2);
		Assert.Equal("Ctrl+K", peer.GetAcceleratorKey());
		Assert.Equal("Done", peer.GetItemStatus());
		Assert.Equal(AutomationHeadingLevel.Level2, peer.GetHeadingLevel());
	}

	[Fact]
	public void Provider_reports_values_by_property_id()
	{
		var e = new Stub();
		var peer = new UIElementAutomationPeer(e);
		AutomationProperties.SetAcceleratorKey(e, "F5");
		AutomationProperties.SetItemStatus(e, "Syncing");
		Assert.Equal("F5", Provided(peer, AutomationElementIdentifiers.AcceleratorKeyProperty));
		Assert.Equal("Syncing", Provided(peer, AutomationElementIdentifiers.ItemStatusProperty));
	}

	[Fact]
	public void MenuItem_falls_back_to_InputGestureText() => Run(() =>
	{
		var item = new MenuItem { InputGestureText = "Ctrl+O" };
		Assert.Equal("Ctrl+O", UIElementAutomationPeer.CreatePeerForElement(item)!.GetAcceleratorKey());
	});

	[Fact]
	public void MenuItem_explicit_AcceleratorKey_wins() => Run(() =>
	{
		var item = new MenuItem { InputGestureText = "Ctrl+O" };
		AutomationProperties.SetAcceleratorKey(item, "Alt+O");
		Assert.Equal("Alt+O", UIElementAutomationPeer.CreatePeerForElement(item)!.GetAcceleratorKey());
	});

	[Fact]
	public void MenuItem_without_either_is_empty() => Run(() =>
		Assert.Equal(string.Empty, UIElementAutomationPeer.CreatePeerForElement(new MenuItem())!.GetAcceleratorKey()));

	[Fact]
	public void RaisePropertyChangedEvent_after_SetItemStatus() => Run(() =>
	{
		var button = new Button();
		var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
		var seen = new List<string>();
		void OnProp(AutomationPeer p, AutomationProperty prop, object? o, object? n) =>
			seen.Add($"{prop.Id}:{o}->{n}");
		AutomationPeer.AddListener(AutomationEvents.PropertyChanged);
		AutomationPeer.PropertyChangedRaised += OnProp;
		try
		{
			AutomationProperties.SetItemStatus(button, "Bad");
			peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.ItemStatusProperty, "", "Bad");
		}
		finally
		{
			AutomationPeer.PropertyChangedRaised -= OnProp;
			AutomationPeer.RemoveListener(AutomationEvents.PropertyChanged);
		}
		Assert.Equal(new[] { "30026:->Bad" }, seen);
		Assert.Same(peer, UIElementAutomationPeer.FromElement(button));
	});

	[Fact]
	public void Xaml_attached_setters_load() => Run(() =>
	{
		var b = (Button)XamlReader.Parse("<Button " + XamlReaderTests.Ns
			+ " AutomationProperties.HeadingLevel=\"Level1\" AutomationProperties.AcceleratorKey=\"Ctrl+Shift+C\""
			+ " AutomationProperties.ItemStatus=\"ok\"/>");
		Assert.Equal(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(b));
		Assert.Equal("Ctrl+Shift+C", AutomationProperties.GetAcceleratorKey(b));
		Assert.Equal("ok", AutomationProperties.GetItemStatus(b));
	});

	[Fact]
	public void Style_setter_resolves_HeadingLevel() => Run(() =>
	{
		var b = (Button)XamlReader.Parse("<Button " + XamlReaderTests.Ns + "><Button.Style><Style TargetType=\"Button\">"
			+ "<Setter Property=\"AutomationProperties.HeadingLevel\" Value=\"Level2\"/></Style></Button.Style></Button>");
		Assert.Equal(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(b));
	});
}
