using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows
{
	/// <summary>
	/// Manages the visual states of a <see cref="Controls.Control"/> by transitioning
	/// between <see cref="VisualState"/>s within <see cref="VisualStateGroup"/>s.
	/// <para>
	/// Storyboard application is deferred to the animation track (a future round).
	/// This implementation: records the current state, raises
	/// <see cref="VisualStateGroup.CurrentStateChanging"/> and
	/// <see cref="VisualStateGroup.CurrentStateChanged"/>, and applies any
	/// <see cref="Setter"/>s defined on the new <see cref="VisualState"/>.
	/// </para>
	/// </summary>
	public class VisualStateManager
	{
		// ── VisualStateGroups attached property ────────────────────────────────

		static readonly Dictionary<Controls.Control, IList<VisualStateGroup>> _groups =
			new Dictionary<Controls.Control, IList<VisualStateGroup>>();

		/// <summary>
		/// Returns the <see cref="VisualStateGroup"/>s attached to <paramref name="control"/>,
		/// or an empty list when none have been set.
		/// </summary>
		public static IList<VisualStateGroup> GetVisualStateGroups(Controls.Control control)
		{
			if (control == null) throw new ArgumentNullException(nameof(control));
			if (!_groups.TryGetValue(control, out var list))
			{
				list = new ObservableCollection<VisualStateGroup>();
				_groups[control] = list;
			}
			return list;
		}

		/// <summary>
		/// Replaces the <see cref="VisualStateGroup"/> list attached to
		/// <paramref name="control"/>.
		/// </summary>
		public static void SetVisualStateGroups(
			Controls.Control control,
			IList<VisualStateGroup> groups)
		{
			if (control == null) throw new ArgumentNullException(nameof(control));
			_groups[control] = groups ?? new ObservableCollection<VisualStateGroup>();
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

				// Apply setters.
				ApplySetters(control, state);

				// Future-round seam: apply state.Storyboard here when animation is wired.

				// Update the recorded state and raise CurrentStateChanged.
				group.CurrentState = state;
				group.RaiseChanged(control, state);

				return true;
			}

			return false;
		}

		// ── Setter application ────────────────────────────────────────────────

		static void ApplySetters(Controls.Control control, VisualState state)
		{
			foreach (var setter in state.Setters)
				setter.Apply(control);
		}
	}
}
