using System.Windows.Automation.Provider;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="Window"/>.</summary>
public class WindowAutomationPeer : FrameworkElementAutomationPeer
{
	public WindowAutomationPeer(Window owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Window ? new WindowProvider((Window)Owner, this)
			: base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "Window";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Window;

	protected override bool IsDialogCore() => ((Window)Owner).IsShowingAsDialog;

	internal override string ContentName() => ((Window)Owner).Title ?? string.Empty;

	internal override Gtk.AccessibleRole? AccessibleRole =>
		IsDialog() ? Gtk.AccessibleRole.Dialog : Gtk.AccessibleRole.Window;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == Window.TitleProperty || base.AffectsAccessible(property);
}

/// <summary>The <see cref="IWindowProvider"/> of a <see cref="Window"/>, backed by its chrome properties.</summary>
sealed class WindowProvider : IWindowProvider
{
	readonly Window _window;
	readonly AutomationPeer _peer;

	internal WindowProvider(Window window, AutomationPeer peer)
	{
		_window = window;
		_peer = peer;
	}

	public bool IsModal => _window.IsShowingAsDialog;

	public bool IsTopmost => _window.Topmost;

	public bool Maximizable => _window.ResizeMode >= ResizeMode.CanResize;

	public bool Minimizable => _window.ResizeMode != ResizeMode.NoResize;

	public WindowInteractionState InteractionState => WindowInteractionState.ReadyForUserInteraction;

	public WindowVisualState VisualState => _window.WindowState switch
	{
		WindowState.Maximized => WindowVisualState.Maximized,
		WindowState.Minimized => WindowVisualState.Minimized,
		_ => WindowVisualState.Normal,
	};

	public void Close()
	{
		if (!_peer.IsEnabled()) throw new ElementNotEnabledException();
		_window.Close();
	}

	public void SetVisualState(WindowVisualState state)
	{
		if (!_peer.IsEnabled()) throw new ElementNotEnabledException();
		if (state == WindowVisualState.Maximized && !Maximizable) throw new InvalidOperationException();
		if (state == WindowVisualState.Minimized && !Minimizable) throw new InvalidOperationException();
		_window.WindowState = state switch
		{
			WindowVisualState.Maximized => WindowState.Maximized,
			WindowVisualState.Minimized => WindowState.Minimized,
			_ => WindowState.Normal,
		};
	}

	public bool WaitForInputIdle(int milliseconds) => true;
}
