using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Manages the visual states of a <see cref="Controls.Control"/> by transitioning
/// between <see cref="VisualState"/>s within <see cref="VisualStateGroup"/>s.
/// <para>
/// <c>GetVisualStateGroups</c> and <c>SetVisualStateGroups</c> operate on any
/// <see cref="FrameworkElement"/> (WPF semantics: VSM groups may be declared on
/// any element, not only <see cref="Controls.Control"/>).
/// <c>GoToState</c> still requires a <see cref="Controls.Control"/> because setter
/// application and storyboard Begin use the control's dependency-object identity.
/// </para>
/// <para>
/// Storyboard application is deferred to the animation track (a future round).
/// This implementation: records the current state, raises
/// <see cref="VisualStateGroup.CurrentStateChanging"/> and
/// <see cref="VisualStateGroup.CurrentStateChanged"/>, and applies any
/// <see cref="Setter"/>s defined on the new <see cref="VisualState"/>.
/// Previous-state setters are reverted before the new state's setters are applied.
/// </para>
/// </summary>
public class VisualStateManager
{
	// ── VisualStateGroups attached property ────────────────────────────────

	// ConditionalWeakTable: element is the key; no strong ref kept, so elements
	// are eligible for GC once they are unreachable from the app side.
	// Value type is ObservableCollection<VisualStateGroup> which has a public
	// parameterless constructor, satisfying ConditionalWeakTable's constraint.
	static readonly ConditionalWeakTable<FrameworkElement, ObservableCollection<VisualStateGroup>> _groups =
		new ConditionalWeakTable<FrameworkElement, ObservableCollection<VisualStateGroup>>();

	/// <summary>
	/// Returns the <see cref="VisualStateGroup"/>s attached to <paramref name="element"/>,
	/// or an empty list when none have been set.
	/// </summary>
	public static IList<VisualStateGroup> GetVisualStateGroups(FrameworkElement element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return _groups.GetOrCreateValue(element);
	}

	/// <summary>
	/// Replaces the <see cref="VisualStateGroup"/> list attached to
	/// <paramref name="element"/>.
	/// </summary>
	public static void SetVisualStateGroups(
		FrameworkElement element,
		IList<VisualStateGroup> groups)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));

		// Normalise to ObservableCollection<VisualStateGroup> so it fits the table value type.
		var collection = groups as ObservableCollection<VisualStateGroup>
		                 ?? new ObservableCollection<VisualStateGroup>(
			                 groups ?? (IEnumerable<VisualStateGroup>)Array.Empty<VisualStateGroup>());

		// ConditionalWeakTable: remove any existing entry then add the new one.
		_groups.Remove(element);
		_groups.Add(element, collection);
	}

	// ── GoToState ─────────────────────────────────────────────────────────

	/// <summary>
	/// Transitions <paramref name="control"/> to the state named
	/// <paramref name="stateName"/> in whichever group contains that state.
	/// </summary>
	/// <param name="control">The control to transition.</param>
	/// <param name="stateName">The target state name.</param>
	/// <param name="useTransitions">
	/// When <c>true</c> animated transitions are requested; storyboard application
	/// is a future-round seam — currently ignored.
	/// </param>
	/// <returns>
	/// <c>true</c> when the named state was found (even if already active);
	/// <c>false</c> when no group contains a state with that name.
	/// </returns>
	public static bool GoToState(
		Controls.Control control,
		string stateName,
		bool useTransitions)
	{
		if (control == null) throw new ArgumentNullException(nameof(control));
		if (stateName == null) throw new ArgumentNullException(nameof(stateName));

		if (!_groups.TryGetValue(control, out var groups)) return false;

		foreach (var group in groups)
		{
			var state = group.FindState(stateName);
			if (state == null) continue;

			// Raise CurrentStateChanging with the OLD CurrentState still set.
			group.RaiseChanging(control, state);

			// Stop any running storyboard from the previous state.
			group.CurrentState?.Storyboard?.Stop();

			// Revert the previous state's setters before applying the new ones.
			// This prevents properties set by a state from "leaking" after leaving it.
			RevertSetters(control, group.CurrentState, state);

			// Apply the new state's setters.
			ApplySetters(control, state);

			// Start the new state's storyboard (if any).
			if (state.Storyboard != null && useTransitions)
				state.Storyboard.Begin(control);

			// Capture the old state, update CurrentState, then raise Changed with correct args.
			var old = group.CurrentState;
			group.CurrentState = state;
			group.RaiseChanged(control, old, state);

			return true;
		}

		return false;
	}

	// ── Setter application / revert ───────────────────────────────────────

	static void ApplySetters(Controls.Control control, VisualState state)
	{
		foreach (var setter in state.Setters)
			setter.Apply(control);
	}

	/// <summary>
	/// Clears DPs that were set by <paramref name="previousState"/> but are NOT
	/// also set by <paramref name="nextState"/>.  Reverts them to the layer beneath
	/// (local value or default) so the state's effects do not persist after exit.
	/// When <paramref name="previousState"/> is null (first transition) nothing is done.
	/// </summary>
	static void RevertSetters(
		Controls.Control control,
		VisualState? previousState,
		VisualState nextState)
	{
		if (previousState == null) return;

		// Build a fast lookup of DPs the NEXT state touches.
		var nextDps = new System.Collections.Generic.HashSet<DependencyProperty>(
			System.Collections.Generic.ReferenceEqualityComparer.Instance);
		foreach (var s in nextState.Setters)
			if (s.Property != null) nextDps.Add(s.Property);

		// Clear any DP the previous state applied that the next state does not re-set.
		foreach (var setter in previousState.Setters)
		{
			if (setter.Property != null && !nextDps.Contains(setter.Property))
				control.ClearValue(setter.Property);
		}
	}
}
