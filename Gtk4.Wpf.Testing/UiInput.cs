using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Routed input: events raised through the layer's own input path (the code the GTK event controllers call), so
/// <c>KeyDown</c>, <c>MouseDown</c>, <c>MouseEnter</c>, input bindings and capture behave as for a real device.
/// GTK's native widget handling (for example a text entry consuming Enter) does not run; that needs native input.
/// </summary>
public sealed partial class UiInput
{
	readonly WpfAppHost _host;

	internal UiInput(WpfAppHost host)
	{
		_host = host;
		Keyboard = new KeyboardInput(this);
		Mouse = new MouseInput(this);
	}

	public KeyboardInput Keyboard { get; }

	public MouseInput Mouse { get; }

	/// <summary>The WPF element behind <paramref name="element"/>, or the main window when it is <c>null</c>.</summary>
	/// <summary>
	/// A disabled element gets no mouse or keyboard input from a real device, so the driver throws
	/// <see cref="ElementNotEnabledException"/> instead of raising events the layer would never deliver.
	/// </summary>
	internal static UIElement RequireEnabled(UIElement target) =>
		target.IsEnabled ? target : throw new ElementNotEnabledException(
			"The element is disabled and receives no input: " + target.GetType().Name);

	internal UIElement Resolve(AutomationElement? element) => GtkThread.Invoke(() =>
		element == null ? _host.Window
			: element.Peer is UIElementAutomationPeer peer ? peer.Owner
			: throw new ArgumentException("The element is not backed by a WPF UIElement.", nameof(element)));

	/// <summary>Keyboard events. Without a target they go to the focused element, else the main window.</summary>
	public sealed class KeyboardInput
	{
		readonly UiInput _input;

		internal KeyboardInput(UiInput input) => _input = input;

		/// <summary>Key down then key up, with <paramref name="modifiers"/> held.</summary>
		public void Press(Key key, ModifierKeys modifiers = ModifierKeys.None, AutomationElement? target = null) =>
			GtkThread.Invoke(() =>
			{
				var element = RequireEnabled(Target(target));
				try
				{
					element.RaiseKeyDownForTest(key, modifiers);
					element.RaiseKeyUpForTest(key, modifiers);
				}
				finally { System.Windows.Input.Keyboard.Modifiers = ModifierKeys.None; }
			});

		public void Down(Key key, ModifierKeys modifiers = ModifierKeys.None, AutomationElement? target = null) =>
			GtkThread.Invoke(() => RequireEnabled(Target(target)).RaiseKeyDownForTest(key, modifiers));

		public void Up(Key key, ModifierKeys modifiers = ModifierKeys.None, AutomationElement? target = null) =>
			GtkThread.Invoke(() =>
			{
				RequireEnabled(Target(target)).RaiseKeyUpForTest(key, modifiers);
				System.Windows.Input.Keyboard.Modifiers = ModifierKeys.None;
			});

		UIElement Target(AutomationElement? explicitTarget) =>
			explicitTarget != null ? _input.Resolve(explicitTarget)
				: System.Windows.Input.Keyboard.FocusedElement as UIElement ?? _input._host.Window;
	}
}
