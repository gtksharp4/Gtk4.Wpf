using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Round-15A: VisualStateManager XAML-API tests.
/// Verifies that <c>SetVisualStateGroups</c> and <c>GetVisualStateGroups</c>
/// accept a <see cref="FrameworkElement"/> (not only <see cref="System.Windows.Controls.Control"/>),
/// and that the programmatic group-build pattern the XAML compiler generates works end-to-end.
/// Pure (no GTK).
/// </summary>
public class VisualStateManagerXamlApiTests
{
	// ── Helpers ───────────────────────────────────────────────────────────

	// Minimal DP for setter-apply testing.
	sealed class StyledControl : System.Windows.Controls.Control
	{
		public static readonly DependencyProperty TagStringProperty =
			DependencyProperty.Register(nameof(TagString), typeof(string), typeof(StyledControl),
				new PropertyMetadata(string.Empty));

		public string TagString
		{
			get => (string?)GetValue(TagStringProperty) ?? string.Empty;
			set => SetValue(TagStringProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// Builds groups and states the same way the XAML compiler generates them, then
	/// calls <c>SetVisualStateGroups</c> on a control. Used to verify the generated pattern.
	/// </summary>
	static WpfButton MakeButtonViaXamlPattern(out VisualStateGroup group)
	{
		var btn = new WpfButton();

		var vsgList = new List<VisualStateGroup>();
		var vsg = new VisualStateGroup();
		vsg.Name = "CommonStates";
		var vsNormal = new VisualState();
		vsNormal.Name = "Normal";
		vsg.States.Add(vsNormal);
		var vsHover = new VisualState();
		vsHover.Name = "MouseOver";
		vsg.States.Add(vsHover);
		vsgList.Add(vsg);

		VisualStateManager.SetVisualStateGroups(btn, vsgList);
		group = vsg;
		return btn;
	}

	// ── SetVisualStateGroups / GetVisualStateGroups via FrameworkElement ──

	[Fact]
	public void SetVisualStateGroups_accepts_FrameworkElement()
	{
		// Button is a FrameworkElement; the method must compile and not throw.
		var btn = new WpfButton();
		VisualStateManager.SetVisualStateGroups((FrameworkElement)btn, new List<VisualStateGroup>());
		// No exception → pass.
	}

	[Fact]
	public void GetVisualStateGroups_accepts_FrameworkElement()
	{
		var btn = new WpfButton();
		IList<VisualStateGroup> result =
			VisualStateManager.GetVisualStateGroups((FrameworkElement)btn);
		Assert.NotNull(result);
	}

	[Fact]
	public void GetVisualStateGroups_returns_groups_set_via_FrameworkElement_overload()
	{
		var btn = new WpfButton();
		var group = new VisualStateGroup { Name = "G" };
		VisualStateManager.SetVisualStateGroups((FrameworkElement)btn,
			new List<VisualStateGroup> { group });

		var retrieved = VisualStateManager.GetVisualStateGroups((FrameworkElement)btn);
		Assert.Single(retrieved);
		Assert.Equal("G", retrieved[0].Name);
	}

	[Fact]
	public void GetVisualStateGroups_returns_empty_on_fresh_FrameworkElement()
	{
		var btn = new WpfButton();
		var groups = VisualStateManager.GetVisualStateGroups((FrameworkElement)btn);
		Assert.NotNull(groups);
		Assert.Empty(groups);
	}

	// ── XAML-generated pattern: List<VisualStateGroup> + SetVisualStateGroups ──

	[Fact]
	public void Xaml_pattern_GoToState_returns_true_for_known_state()
	{
		var btn = MakeButtonViaXamlPattern(out _);
		bool ok = VisualStateManager.GoToState(btn, "Normal", false);
		Assert.True(ok);
	}

	[Fact]
	public void Xaml_pattern_GoToState_returns_false_for_unknown_state()
	{
		var btn = MakeButtonViaXamlPattern(out _);
		bool ok = VisualStateManager.GoToState(btn, "Pressed", false);
		Assert.False(ok);
	}

	[Fact]
	public void Xaml_pattern_GoToState_sets_CurrentState()
	{
		var btn = MakeButtonViaXamlPattern(out var group);
		VisualStateManager.GoToState(btn, "MouseOver", false);
		Assert.Equal("MouseOver", group.CurrentState?.Name);
	}

	[Fact]
	public void Xaml_pattern_GoToState_raises_CurrentStateChanged()
	{
		var btn = MakeButtonViaXamlPattern(out var group);
		bool fired = false;
		group.CurrentStateChanged += (s, e) => { fired = true; };
		VisualStateManager.GoToState(btn, "Normal", false);
		Assert.True(fired);
	}

	[Fact]
	public void Xaml_pattern_state_Name_set_on_VisualState_object()
	{
		var btn = MakeButtonViaXamlPattern(out var group);
		Assert.Equal("Normal", group.States[0].Name);
		Assert.Equal("MouseOver", group.States[1].Name);
	}

	// ── Setter application via XAML pattern ──────────────────────────────

	[Fact]
	public void Xaml_pattern_GoToState_applies_setter()
	{
		var ctrl = new StyledControl();

		var vsgList = new List<VisualStateGroup>();
		var vsg = new VisualStateGroup();
		vsg.Name = "CommonStates";

		var vsHover = new VisualState();
		vsHover.Name = "MouseOver";
		vsHover.Setters.Add(new Setter { Property = StyledControl.TagStringProperty, Value = "hover" });
		vsg.States.Add(vsHover);

		var vsNormal = new VisualState();
		vsNormal.Name = "Normal";
		vsg.States.Add(vsNormal);

		vsgList.Add(vsg);
		VisualStateManager.SetVisualStateGroups(ctrl, vsgList);

		VisualStateManager.GoToState(ctrl, "MouseOver", false);
		Assert.Equal("hover", ctrl.TagString);
	}

	[Fact]
	public void Xaml_pattern_leaving_state_reverts_setter()
	{
		var ctrl = new StyledControl();

		var vsgList = new List<VisualStateGroup>();
		var vsg = new VisualStateGroup();
		vsg.Name = "CommonStates";

		var vsHover = new VisualState();
		vsHover.Name = "MouseOver";
		vsHover.Setters.Add(new Setter { Property = StyledControl.TagStringProperty, Value = "hover" });
		vsg.States.Add(vsHover);

		var vsNormal = new VisualState();
		vsNormal.Name = "Normal";
		vsg.States.Add(vsNormal);

		vsgList.Add(vsg);
		VisualStateManager.SetVisualStateGroups(ctrl, vsgList);

		VisualStateManager.GoToState(ctrl, "MouseOver", false);
		VisualStateManager.GoToState(ctrl, "Normal", false);

		// TagStringProperty must revert to default after leaving MouseOver.
		Assert.Equal(string.Empty, ctrl.TagString);
	}
}
