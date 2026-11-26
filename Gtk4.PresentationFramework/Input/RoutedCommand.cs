using System;

namespace System.Windows.Input;

/// <summary>
/// A WPF <see cref="ICommand"/> whose execution and can-execute queries are routed
/// through the element tree via <see cref="CommandManager"/>.
/// </summary>
public class RoutedCommand : ICommand
{
	/// <summary>Name of this command.</summary>
	public string Name { get; }

	/// <summary>Declaring type of this static command; <c>null</c> for the parameterless constructor.</summary>
	public Type? OwnerType { get; }

	/// <summary>Keyboard/mouse gestures that trigger this command.</summary>
	public InputGestureCollection InputGestures { get; }

	/// <summary>Creates an unnamed command with no owner type.</summary>
	public RoutedCommand()
	{
		Name = string.Empty;
		InputGestures = new InputGestureCollection();
	}

	/// <param name="name">Command name.</param>
	/// <param name="ownerType">Declaring type.</param>
	public RoutedCommand(string name, Type ownerType)
		: this(name, ownerType, new InputGestureCollection()) { }

	/// <param name="name">Command name.</param>
	/// <param name="ownerType">Declaring type.</param>
	/// <param name="inputGestures">Associated gestures.</param>
	public RoutedCommand(string name, Type ownerType, InputGestureCollection inputGestures)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		if (name.Length == 0) throw new ArgumentException("The name must not be empty.", nameof(name));
		OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
		InputGestures = inputGestures ?? new InputGestureCollection();
	}

	// ── ICommand ─────────────────────────────────────────────────────────────

	/// <inheritdoc/>
	/// <remarks>
	/// Delegates to <see cref="CommandManager.CanExecute"/> walking the
	/// <paramref name="target"/> element tree for a matching <see cref="CommandBinding"/>.
	/// When <paramref name="target"/> is <c>null</c> returns <c>true</c> (no tree to walk).
	/// </remarks>
	public bool CanExecute(object? parameter) =>
		CanExecute(parameter, null);

	/// <summary>Routed overload: walks <paramref name="target"/>'s element tree.</summary>
	public bool CanExecute(object? parameter, System.Windows.IInputElement? target) =>
		CommandManager.CanExecute(this, parameter, target as System.Windows.UIElement);

	/// <inheritdoc/>
	/// <remarks>Delegates to <see cref="CommandManager.Execute"/>.</remarks>
	public void Execute(object? parameter) =>
		Execute(parameter, null);

	/// <summary>Routed overload: walks <paramref name="target"/>'s element tree.</summary>
	public void Execute(object? parameter, System.Windows.IInputElement? target) =>
		CommandManager.Execute(this, parameter, target as System.Windows.UIElement);

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}
}

/// <summary>
/// A <see cref="RoutedCommand"/> with a text label for use in menu items and tooltips.
/// </summary>
public class RoutedUICommand : RoutedCommand
{
	/// <summary>Display text for this command.</summary>
	public string Text { get; set; }

	/// <summary>Creates a command with empty text and no owner type.</summary>
	public RoutedUICommand() { Text = string.Empty; }

	/// <param name="text">Display text.</param>
	/// <param name="name">Command name.</param>
	/// <param name="ownerType">Declaring type.</param>
	public RoutedUICommand(string text, string name, Type ownerType)
		: base(name, ownerType)
	{
		Text = text ?? string.Empty;
	}

	/// <param name="text">Display text.</param>
	/// <param name="name">Command name.</param>
	/// <param name="ownerType">Declaring type.</param>
	/// <param name="inputGestures">Associated gestures.</param>
	public RoutedUICommand(
		string text, string name, Type ownerType, InputGestureCollection inputGestures)
		: base(name, ownerType, inputGestures)
	{
		Text = text ?? string.Empty;
	}
}