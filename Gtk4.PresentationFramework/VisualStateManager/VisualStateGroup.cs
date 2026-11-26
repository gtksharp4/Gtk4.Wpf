using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Groups mutually-exclusive <see cref="VisualState"/> objects.
/// A <see cref="Controls.Control"/> is in exactly one state per group at a time.
/// </summary>
public class VisualStateGroup
{
	/// <summary>The name of this group (e.g. <c>"CommonStates"</c>).</summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>The states that belong to this group.</summary>
	public IList<VisualState> States { get; } = new List<VisualState>();

	/// <summary>The currently active state, or <c>null</c> when none has been applied.</summary>
	public VisualState? CurrentState { get; internal set; }

	/// <summary>
	/// Raised just before the state transition completes.
	/// Passes the new <see cref="VisualState"/> as <c>e.NewState</c>.
	/// </summary>
	public event EventHandler<VisualStateChangedEventArgs>? CurrentStateChanging;

	/// <summary>
	/// Raised after the state transition completes.
	/// Passes the new <see cref="VisualState"/> as <c>e.NewState</c>.
	/// </summary>
	public event EventHandler<VisualStateChangedEventArgs>? CurrentStateChanged;

	/// <summary>
	/// Finds the state with the given <paramref name="stateName"/>, or <c>null</c>.
	/// </summary>
	internal VisualState? FindState(string stateName)
	{
		foreach (var s in States)
			if (s.Name == stateName) return s;
		return null;
	}

	internal void RaiseChanging(Controls.Control control, VisualState newState)
		=> CurrentStateChanging?.Invoke(control,
			new VisualStateChangedEventArgs(CurrentState, newState, control, this));

	/// <param name="oldState">The state that was active before the transition.</param>
	/// <param name="newState">The state that is now active.</param>
	internal void RaiseChanged(Controls.Control control, VisualState? oldState, VisualState newState)
		=> CurrentStateChanged?.Invoke(control,
			new VisualStateChangedEventArgs(oldState, newState, control, this));
}

/// <summary>Event args for <see cref="VisualStateGroup"/> state-change events.</summary>
public sealed class VisualStateChangedEventArgs : EventArgs
{
	/// <summary>The state that was active before the transition (may be <c>null</c>).</summary>
	public VisualState? OldState { get; }

	/// <summary>The state that is now active.</summary>
	public VisualState NewState { get; }

	/// <summary>The control whose state changed.</summary>
	public Controls.Control Control { get; }

	/// <summary>The group in which the transition occurred.</summary>
	public VisualStateGroup StateGroup { get; }

	internal VisualStateChangedEventArgs(
		VisualState? oldState,
		VisualState newState,
		Controls.Control control,
		VisualStateGroup stateGroup)
	{
		OldState = oldState;
		NewState = newState;
		Control = control;
		StateGroup = stateGroup;
	}
}