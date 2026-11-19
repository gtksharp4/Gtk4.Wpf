using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Controls;

public static partial class Validation
{
	// WPF keeps errors per binding and merges them into the element's single Errors collection, so one
	// binding never clears another's errors. The collection instance is created once per element.
	sealed class ErrorState
	{
		public readonly Dictionary<object, List<ValidationError>> BySource =
			new Dictionary<object, List<ValidationError>>();
		public readonly ObservableCollection<ValidationError> Items = new ObservableCollection<ValidationError>();
		public readonly ReadOnlyObservableCollection<ValidationError> ReadOnly;
		public bool HasError;

		public ErrorState() => ReadOnly = new ReadOnlyObservableCollection<ValidationError>(Items);
	}

	static readonly ConditionalWeakTable<DependencyObject, ErrorState> _states =
		new ConditionalWeakTable<DependencyObject, ErrorState>();

	/// <summary>
	/// Replaces the errors <paramref name="owner"/> (a binding) contributes to <paramref name="element"/>;
	/// an empty list removes them. Other owners' errors are untouched. When <paramref name="notify"/> is
	/// <c>true</c>, raises <see cref="ErrorEvent"/> only for errors that changed.
	/// </summary>
	internal static void SetErrorsFor(
		DependencyObject element, object owner, IReadOnlyList<ValidationError> errors, bool notify)
	{
		if (!_states.TryGetValue(element, out var state))
		{
			if (errors.Count == 0) return;
			state = new ErrorState();
			_states.Add(element, state);
			element.SetValue(_errorsKey, state.ReadOnly);
		}

		state.BySource.TryGetValue(owner, out var old);
		if (old == null && errors.Count == 0) return;

		// Unchanged errors keep their instance and raise nothing; new errors are added before stale ones are
		// removed (WPF UpdateValidationError order).
		var stale = old == null ? new List<ValidationError>() : new List<ValidationError>(old);
		var current = new List<ValidationError>(errors.Count);
		var added = new List<ValidationError>();
		foreach (var e in errors)
		{
			int i = stale.FindIndex(o => Equivalent(o, e));
			if (i >= 0) { current.Add(stale[i]); stale.RemoveAt(i); }
			else { current.Add(e); added.Add(e); }
		}
		if (added.Count == 0 && stale.Count == 0) return;

		if (current.Count == 0) state.BySource.Remove(owner);
		else state.BySource[owner] = current;

		foreach (var e in added) state.Items.Add(e);
		foreach (var e in stale) state.Items.Remove(e);

		bool has = state.Items.Count > 0;
		if (has != state.HasError)
		{
			state.HasError = has;
			element.SetValue(_hasErrorKey, has);
		}

		if (!notify || !(element is UIElement uie)) return;
		foreach (var e in added) Raise(uie, e, ValidationErrorEventAction.Added);
		foreach (var e in stale) Raise(uie, e, ValidationErrorEventAction.Removed);
	}

	static bool Equivalent(ValidationError a, ValidationError b) =>
		ReferenceEquals(a.RuleInError, b.RuleInError) && Equals(a.ErrorContent, b.ErrorContent)
		&& ReferenceEquals(a.Exception, b.Exception);

	static void Raise(UIElement element, ValidationError error, ValidationErrorEventAction action)
	{
		var args = new ValidationErrorEventArgs(error, action);
		args.Source = element;
		element.RaiseEvent(args);
	}
}
