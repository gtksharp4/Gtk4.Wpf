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
	/// <c>true</c>, raises <see cref="ErrorEvent"/> for every added and removed error.
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
		if (old != null && Same(old, errors)) return;
		if (errors.Count == 0) state.BySource.Remove(owner);
		else state.BySource[owner] = new List<ValidationError>(errors);

		if (old != null)
			foreach (var e in old) state.Items.Remove(e);
		foreach (var e in errors) state.Items.Add(e);

		bool has = state.Items.Count > 0;
		if (has != state.HasError)
		{
			state.HasError = has;
			element.SetValue(_hasErrorKey, has);
		}

		if (!notify || !(element is UIElement uie)) return;
		if (old != null) foreach (var e in old) Raise(uie, e, ValidationErrorEventAction.Removed);
		foreach (var e in errors) Raise(uie, e, ValidationErrorEventAction.Added);
	}

	// An unchanged error set is not a change: no collection churn and no events.
	static bool Same(List<ValidationError> a, IReadOnlyList<ValidationError> b)
	{
		if (a.Count != b.Count) return false;
		for (int i = 0; i < a.Count; i++)
			if (!ReferenceEquals(a[i].RuleInError, b[i].RuleInError) || !Equals(a[i].ErrorContent, b[i].ErrorContent)
			    || !ReferenceEquals(a[i].Exception, b[i].Exception)) return false;
		return true;
	}

	static void Raise(UIElement element, ValidationError error, ValidationErrorEventAction action)
	{
		var args = new ValidationErrorEventArgs(error, action);
		args.Source = element;
		element.RaiseEvent(args);
	}
}
