using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Round-8: VisualStateManager — GoToState, state-changed events, setter application.
/// Each test fails without the VSM implementation.
/// Pure (no GTK).
/// </summary>
public class VisualStateManagerTests
{
	// ── Helpers ───────────────────────────────────────────────────────────

	// A control that exposes a simple string DP for setter-apply testing.
	sealed class LabelControl : System.Windows.Controls.Control
	{
		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabelControl),
				new PropertyMetadata(string.Empty));

		public string Label
		{
			get => (string?)GetValue(LabelProperty) ?? string.Empty;
			set => SetValue(LabelProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// Creates a button with a single group called "CommonStates" containing
	/// the given state names.
	/// </summary>
	static WpfButton MakeButtonWithStates(params string[] stateNames)
	{
		var btn = new WpfButton();
		var group = new VisualStateGroup { Name = "CommonStates" };
		foreach (var name in stateNames)
			group.States.Add(new VisualState { Name = name });
		VisualStateManager.SetVisualStateGroups(btn, new List<VisualStateGroup> { group });
		return btn;
	}

	// ── GoToState returns true for known state ─────────────────────────────

	[Fact]
	public void GoToState_returns_true_for_known_state()
	{
		var btn = MakeButtonWithStates("Normal", "MouseOver");
		bool result = VisualStateManager.GoToState(btn, "Normal", false);
		Assert.True(result);
	}

	[Fact]
	public void GoToState_returns_false_for_unknown_state()
	{
		var btn = MakeButtonWithStates("Normal");
		bool result = VisualStateManager.GoToState(btn, "Pressed", false);
		Assert.False(result);
	}

	[Fact]
	public void GoToState_returns_false_when_no_groups_attached()
	{
		var btn = new WpfButton();
		bool result = VisualStateManager.GoToState(btn, "Normal", false);
		Assert.False(result);
	}

	// ── GoToState updates CurrentState ────────────────────────────────────

	[Fact]
	public void GoToState_sets_CurrentState_on_group()
	{
		var btn = new WpfButton();
		var state = new VisualState { Name = "Active" };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(btn, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(btn, "Active", false);
		Assert.Same(state, group.CurrentState);
	}

	// ── GoToState raises state-changed events ────────────────────────────

	[Fact]
	public void GoToState_raises_CurrentStateChanging()
	{
		var btn = MakeButtonWithStates("Normal", "Active");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		bool fired = false;
		groups[0].CurrentStateChanging += (s, e) => { fired = true; };

		VisualStateManager.GoToState(btn, "Active", false);
		Assert.True(fired);
	}

	[Fact]
	public void GoToState_raises_CurrentStateChanged()
	{
		var btn = MakeButtonWithStates("Normal", "Active");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		bool fired = false;
		groups[0].CurrentStateChanged += (s, e) => { fired = true; };

		VisualStateManager.GoToState(btn, "Active", false);
		Assert.True(fired);
	}

	[Fact]
	public void GoToState_CurrentStateChanging_new_state_is_target()
	{
		var btn = MakeButtonWithStates("Normal", "Hover");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		VisualStateChangedEventArgs? captured = null;
		groups[0].CurrentStateChanging += (s, e) => { captured = e; };

		VisualStateManager.GoToState(btn, "Hover", false);
		Assert.NotNull(captured);
		Assert.Equal("Hover", captured!.NewState.Name);
	}

	[Fact]
	public void GoToState_CurrentStateChanged_new_state_is_target()
	{
		var btn = MakeButtonWithStates("Normal", "Hover");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		VisualStateChangedEventArgs? captured = null;
		groups[0].CurrentStateChanged += (s, e) => { captured = e; };

		VisualStateManager.GoToState(btn, "Hover", false);
		Assert.NotNull(captured);
		Assert.Equal("Hover", captured!.NewState.Name);
	}

	[Fact]
	public void GoToState_CurrentStateChanging_old_state_is_null_on_first_transition()
	{
		var btn = MakeButtonWithStates("Normal");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		VisualStateChangedEventArgs? captured = null;
		groups[0].CurrentStateChanging += (s, e) => { captured = e; };

		VisualStateManager.GoToState(btn, "Normal", false);
		Assert.NotNull(captured);
		Assert.Null(captured!.OldState);
	}

	[Fact]
	public void GoToState_CurrentStateChanging_old_state_is_previous_on_second_transition()
	{
		var btn = MakeButtonWithStates("Normal", "Hover");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		VisualStateManager.GoToState(btn, "Normal", false);

		VisualStateChangedEventArgs? captured = null;
		groups[0].CurrentStateChanging += (s, e) => { captured = e; };
		VisualStateManager.GoToState(btn, "Hover", false);

		Assert.NotNull(captured);
		Assert.Equal("Normal", captured!.OldState?.Name);
	}

	// ── GoToState applies setter-based state ──────────────────────────────

	[Fact]
	public void GoToState_applies_setter_to_control()
	{
		var ctrl = new LabelControl();
		var state = new VisualState { Name = "Special" };
		// A setter that writes "active" to Label.
		state.Setters.Add(new Setter
		{
			Property = LabelControl.LabelProperty,
			Value = "active",
		});
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "Special", false);
		Assert.Equal("active", ctrl.Label);
	}

	[Fact]
	public void GoToState_no_setter_state_raises_events_still()
	{
		var btn = MakeButtonWithStates("Plain");
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		bool fired = false;
		groups[0].CurrentStateChanged += (s, e) => { fired = true; };

		bool result = VisualStateManager.GoToState(btn, "Plain", false);
		Assert.True(result);
		Assert.True(fired);
	}

	// ── GetVisualStateGroups returns empty list when not set ───────────────

	[Fact]
	public void GetVisualStateGroups_returns_empty_when_not_set()
	{
		var btn = new WpfButton();
		var groups = VisualStateManager.GetVisualStateGroups(btn);
		Assert.NotNull(groups);
		Assert.Empty(groups);
	}

	// ── T2: ConditionalWeakTable — API round-trips unchanged ──────────────

	[Fact]
	public void T2_GetSet_VisualStateGroups_roundtrip_works()
	{
		// Verifies the API still works after the Dictionary→ConditionalWeakTable change.
		var ctrl = new LabelControl();
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(new VisualState { Name = "A" });
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		var retrieved = VisualStateManager.GetVisualStateGroups(ctrl);
		Assert.Single(retrieved);
		Assert.Equal("G", retrieved[0].Name);
	}

	[Fact]
	public void T2_GoToState_still_works_after_weak_table_change()
	{
		var ctrl = new LabelControl();
		var state = new VisualState { Name = "X" };
		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(state);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		bool result = VisualStateManager.GoToState(ctrl, "X", false);
		Assert.True(result);
		Assert.Same(state, group.CurrentState);
	}

	// ── T4: Setter revert — leaving a state clears its DPs ───────────────

	[Fact]
	public void T4_Leaving_state_A_reverts_its_setter_before_applying_state_B()
	{
		// State A sets LabelProperty = "A-value".
		// State B sets a different DP — does not touch LabelProperty.
		// After GoToState(B): LabelProperty must revert to its default, not remain "A-value".
		var ctrl = new LabelControl();

		var stateA = new VisualState { Name = "A" };
		stateA.Setters.Add(new Setter { Property = LabelControl.LabelProperty, Value = "A-value" });

		// State B sets a different int DP (we reuse a declared one).
		var stateB = new VisualState { Name = "B" };
		// No setters on LabelProperty in state B.

		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(stateA);
		group.States.Add(stateB);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "A", false);
		Assert.Equal("A-value", ctrl.Label); // A applied

		VisualStateManager.GoToState(ctrl, "B", false);
		// LabelProperty was only in state A — must be reverted to the default.
		Assert.Equal(string.Empty, ctrl.Label);
	}

	[Fact]
	public void T4_Setter_still_applied_when_new_state_also_sets_same_dp()
	{
		// State A sets Label = "from-A"; state B sets Label = "from-B".
		// After GoToState(B): Label must be "from-B", not reverted to default.
		var ctrl = new LabelControl();

		var stateA = new VisualState { Name = "A" };
		stateA.Setters.Add(new Setter { Property = LabelControl.LabelProperty, Value = "from-A" });

		var stateB = new VisualState { Name = "B" };
		stateB.Setters.Add(new Setter { Property = LabelControl.LabelProperty, Value = "from-B" });

		var group = new VisualStateGroup { Name = "G" };
		group.States.Add(stateA);
		group.States.Add(stateB);
		VisualStateManager.SetVisualStateGroups(ctrl, new List<VisualStateGroup> { group });

		VisualStateManager.GoToState(ctrl, "A", false);
		VisualStateManager.GoToState(ctrl, "B", false);
		Assert.Equal("from-B", ctrl.Label);
	}

	// ── T5: CurrentStateChanged reports correct OldState ─────────────────

	[Fact]
	public void T5_CurrentStateChanged_OldState_is_the_previous_state()
	{
		var btn = MakeButtonWithStates("Normal", "Active");
		var groups = VisualStateManager.GetVisualStateGroups(btn);

		VisualStateManager.GoToState(btn, "Normal", false);

		VisualStateChangedEventArgs? args = null;
		groups[0].CurrentStateChanged += (s, e) => { args = e; };

		VisualStateManager.GoToState(btn, "Active", false);

		Assert.NotNull(args);
		Assert.Equal("Normal", args!.OldState?.Name); // was Normal before transition
		Assert.Equal("Active", args.NewState.Name);   // now Active
	}

	[Fact]
	public void T5_CurrentStateChanged_OldState_is_null_on_first_transition()
	{
		var btn = MakeButtonWithStates("Initial");
		var groups = VisualStateManager.GetVisualStateGroups(btn);

		VisualStateChangedEventArgs? args = null;
		groups[0].CurrentStateChanged += (s, e) => { args = e; };

		VisualStateManager.GoToState(btn, "Initial", false);

		Assert.NotNull(args);
		Assert.Null(args!.OldState); // no previous state
	}
}