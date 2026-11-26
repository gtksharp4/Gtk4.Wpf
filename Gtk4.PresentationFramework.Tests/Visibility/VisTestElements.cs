using System;
using System.Windows;
using System.Windows.Input;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>Headless element: never materialises a GTK widget.</summary>
sealed class VisElement : FrameworkElement
{
	public VisElement(UIElement? parent = null)
	{
		if (parent != null) LogicalParent = parent;
	}

	public void Attach(UIElement? parent) => LogicalParent = parent;

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in headless visibility tests.");
}

/// <summary>Headless element whose <see cref="UIElement.IsEnabledCore"/> is switchable.</summary>
sealed class CoreElement : FrameworkElement
{
	public bool CoreEnabled = true;

	protected override bool IsEnabledCore => base.IsEnabledCore && CoreEnabled;

	public void Recoerce() => CoerceValue(IsEnabledProperty);

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in headless visibility tests.");
}

/// <summary>Command with a switchable CanExecute.</summary>
sealed class SwitchCommand : ICommand
{
	public bool Can = true;

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) => Can;

	public void Execute(object? parameter) { }

	public void Raise() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>Resets the global capture/focus singletons around each test.</summary>
public abstract class VisStateBase : IDisposable
{
	protected VisStateBase() => Reset();

	public void Dispose() => Reset();

	static void Reset()
	{
		Mouse.Capture(null);
		(Keyboard.FocusedElement as UIElement)?.SetFocusedInternal(false);
	}
}
