using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>
/// E1: Per-contributor trigger-layer stack — multiple triggers on the same DP
/// must not corrupt each other on un-apply.
/// </summary>
public class TriggerLayerStackTests
{
	sealed class Host : DependencyObject
	{
		public static readonly DependencyProperty ColorProp =
			DependencyProperty.Register("TLSColor", typeof(string), typeof(Host),
				new PropertyMetadata("default"));

		public string? Color
		{
			get => (string?)GetValue(ColorProp);
			set => SetValue(ColorProp, value);
		}
	}

	/// <summary>
	/// Two triggers both set ColorProp. The second one (last-declared) wins while both
	/// are active. Un-applying the second reveals the first; un-applying both
	/// falls back to the style/default layer.
	/// </summary>
	[Fact]
	public void Two_triggers_on_same_dp_last_declared_wins()
	{
		var host = new Host();
		object contrib1 = new object();
		object contrib2 = new object();

		// Apply first trigger.
		host.SetTriggerValue(Host.ColorProp, contrib1, "red");
		Assert.Equal("red", host.Color);

		// Apply second trigger — should win.
		host.SetTriggerValue(Host.ColorProp, contrib2, "blue");
		Assert.Equal("blue", host.Color);

		// Un-apply second — first trigger's value must resurface.
		host.ClearTriggerValue(Host.ColorProp, contrib2);
		Assert.Equal("red", host.Color);

		// Un-apply first — falls to default.
		host.ClearTriggerValue(Host.ColorProp, contrib1);
		Assert.Equal("default", host.Color);
	}

	[Fact]
	public void Clearing_first_contributor_still_shows_second()
	{
		var host = new Host();
		object contrib1 = new object();
		object contrib2 = new object();

		host.SetTriggerValue(Host.ColorProp, contrib1, "red");
		host.SetTriggerValue(Host.ColorProp, contrib2, "blue");

		// Un-apply the first; second is still active → blue.
		host.ClearTriggerValue(Host.ColorProp, contrib1);
		Assert.Equal("blue", host.Color);
	}

	[Fact]
	public void Clearing_nonexistent_contributor_is_safe()
	{
		var host = new Host();
		object contrib = new object();

		// No crash when clearing a contributor that was never added.
		host.ClearTriggerValue(Host.ColorProp, contrib);
		Assert.Equal("default", host.Color);
	}

	[Fact]
	public void Trigger_does_not_shadow_local_value()
	{
		var host = new Host();
		object contrib = new object();

		host.Color = "local";
		host.SetTriggerValue(Host.ColorProp, contrib, "triggered");

		// Local still wins.
		Assert.Equal("local", host.Color);
	}

	[Fact]
	public void Style_layer_visible_after_all_trigger_contributors_cleared()
	{
		var host = new Host();
		object contrib = new object();

		host.SetStyleValue(Host.ColorProp, "styled");
		host.SetTriggerValue(Host.ColorProp, contrib, "triggered");
		Assert.Equal("triggered", host.Color);

		host.ClearTriggerValue(Host.ColorProp, contrib);
		Assert.Equal("styled", host.Color);
	}
}