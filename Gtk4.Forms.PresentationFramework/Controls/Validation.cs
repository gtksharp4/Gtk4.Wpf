using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Controls;

/// <summary>
/// Provides methods and attached properties that support data validation.
/// </summary>
public static partial class Validation
{
	// ── Errors (read-only attached DP) ───────────────────────────────────────

	static readonly DependencyPropertyKey _errorsKey =
		DependencyProperty.RegisterReadOnly(
			"Errors",
			typeof(ReadOnlyObservableCollection<ValidationError>),
			typeof(Validation),
			new PropertyMetadata(null));

	/// <summary>Identifies the <c>Validation.Errors</c> read-only attached property.</summary>
	public static readonly DependencyProperty ErrorsProperty = _errorsKey.DependencyProperty;

	/// <summary>Gets the collection of <see cref="ValidationError"/>s on <paramref name="element"/>.</summary>
	public static ReadOnlyObservableCollection<ValidationError> GetErrors(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (ReadOnlyObservableCollection<ValidationError>?)element.GetValue(ErrorsProperty)
		       ?? _empty;
	}

	static readonly ReadOnlyObservableCollection<ValidationError> _empty =
		new ReadOnlyObservableCollection<ValidationError>(new ObservableCollection<ValidationError>());

	// ── HasError (read-only attached DP) ─────────────────────────────────────

	static readonly DependencyPropertyKey _hasErrorKey =
		DependencyProperty.RegisterReadOnly(
			"HasError",
			typeof(bool),
			typeof(Validation),
			new PropertyMetadata(false));

	/// <summary>Identifies the <c>Validation.HasError</c> read-only attached property.</summary>
	public static readonly DependencyProperty HasErrorProperty = _hasErrorKey.DependencyProperty;

	/// <summary>Returns <c>true</c> when <paramref name="element"/> has at least one validation error.</summary>
	public static bool GetHasError(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool?)element.GetValue(HasErrorProperty) ?? false;
	}

	// ── ErrorTemplate (stub attached DP) ─────────────────────────────────────

	/// <summary>Stub: identifies the <c>Validation.ErrorTemplate</c> attached property.</summary>
	public static readonly DependencyProperty ErrorTemplateProperty =
		DependencyProperty.RegisterAttached(
			"ErrorTemplate",
			typeof(object),
			typeof(Validation),
			new PropertyMetadata(null));

	/// <summary>Gets the error template on <paramref name="element"/> (stub).</summary>
	public static object? GetErrorTemplate(DependencyObject element)
		=> element?.GetValue(ErrorTemplateProperty);

	/// <summary>Sets the error template on <paramref name="element"/> (stub).</summary>
	public static void SetErrorTemplate(DependencyObject element, object? value)
		=> element?.SetValue(ErrorTemplateProperty, value);

	// ── Error attached routed event ───────────────────────────────────────────

	/// <summary>
	/// Attached routed event raised when a validation error is added or cleared
	/// and <c>Binding.NotifyOnValidationError</c> is <c>true</c>.
	/// </summary>
	public static readonly RoutedEvent ErrorEvent =
		EventManager.RegisterRoutedEvent(
			"Error",
			RoutingStrategy.Bubble,
			typeof(EventHandler<ValidationErrorEventArgs>),
			typeof(Validation));

	/// <summary>Adds a handler for the <see cref="ErrorEvent"/> on <paramref name="element"/>.</summary>
	public static void AddErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
	{
		if (element is UIElement uie) uie.AddHandler(ErrorEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="ErrorEvent"/> on <paramref name="element"/>.</summary>
	public static void RemoveErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
	{
		if (element is UIElement uie) uie.RemoveHandler(ErrorEvent, handler);
	}


}

/// <summary>Action that caused a <see cref="Validation.ErrorEvent"/>.</summary>
public enum ValidationErrorEventAction
{
	/// <summary>A validation error was added.</summary>
	Added,
	/// <summary>A validation error was removed.</summary>
	Removed,
}

/// <summary>Event args for <see cref="Validation.ErrorEvent"/>.</summary>
public class ValidationErrorEventArgs : RoutedEventArgs
{
	/// <summary>The validation error.</summary>
	public ValidationError Error { get; }

	/// <summary>Whether the error was added or removed.</summary>
	public ValidationErrorEventAction Action { get; }

	internal ValidationErrorEventArgs(ValidationError error, ValidationErrorEventAction action)
		: base(Validation.ErrorEvent)
	{
		Error = error;
		Action = action;
	}
}