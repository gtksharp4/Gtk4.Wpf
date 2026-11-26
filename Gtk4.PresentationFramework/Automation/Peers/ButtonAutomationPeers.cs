using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="ButtonBase"/>.</summary>
public abstract class ButtonBaseAutomationPeer : FrameworkElementAutomationPeer
{
	protected ButtonBaseAutomationPeer(ButtonBase owner) : base(owner) { }

	internal override string ContentName() => PeerText.FromContent(((ContentControl)Owner).Content);
}

/// <summary>The automation peer of a <see cref="Button"/>.</summary>
public class ButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
	public ButtonAutomationPeer(Button owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "Button";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

	// Like WPF, the click runs through the dispatcher.
	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		var owner = (ButtonBase)Owner;
		Dispatcher.BeginInvoke(owner.AutomationClick);
	}
}

/// <summary>The automation peer of a <see cref="ToggleButton"/>.</summary>
public class ToggleButtonAutomationPeer : ButtonBaseAutomationPeer, IToggleProvider
{
	public ToggleButtonAutomationPeer(ToggleButton owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Toggle ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "ToggleButton";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

	ToggleState IToggleProvider.ToggleState => ToState(((ToggleButton)Owner).IsChecked);

	void IToggleProvider.Toggle()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		((ToggleButton)Owner).AutomationToggle();
	}

	internal override Gtk.AccessibleRole? AccessibleRole => Gtk.AccessibleRole.ToggleButton;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == ToggleButton.IsCheckedProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget) =>
		PeerAccessible.SetTristate(widget, Gtk.AccessibleState.Pressed, ((ToggleButton)Owner).IsChecked);

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == ToggleButton.IsCheckedProperty)
			RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty,
				ToState(e.OldValue as bool?), ToState(e.NewValue as bool?));
	}

	static ToggleState ToState(bool? value) =>
		value == true ? ToggleState.On : value == false ? ToggleState.Off : ToggleState.Indeterminate;
}

/// <summary>The automation peer of a <see cref="CheckBox"/>.</summary>
public class CheckBoxAutomationPeer : ToggleButtonAutomationPeer
{
	public CheckBoxAutomationPeer(CheckBox owner) : base(owner) { }

	protected override string GetClassNameCore() => "CheckBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.CheckBox;

	internal override Gtk.AccessibleRole? AccessibleRole => Gtk.AccessibleRole.Checkbox;

	internal override void PushState(Gtk.Widget widget) =>
		PeerAccessible.SetTristate(widget, Gtk.AccessibleState.Checked, ((ToggleButton)Owner).IsChecked);
}

/// <summary>The automation peer of a <see cref="RadioButton"/>.</summary>
public class RadioButtonAutomationPeer : ToggleButtonAutomationPeer, ISelectionItemProvider
{
	public RadioButtonAutomationPeer(RadioButton owner) : base(owner) { }

	// A radio button is selected, not toggled: WPF exposes SelectionItem only.
	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.SelectionItem ? this
			: patternInterface == PatternInterface.SynchronizedInput ? base.GetPattern(patternInterface)
			: null;

	protected override string GetClassNameCore() => "RadioButton";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.RadioButton;

	internal override Gtk.AccessibleRole? AccessibleRole => Gtk.AccessibleRole.Radio;

	internal override void PushState(Gtk.Widget widget) =>
		PeerAccessible.SetTristate(widget, Gtk.AccessibleState.Checked, ((ToggleButton)Owner).IsChecked);

	bool ISelectionItemProvider.IsSelected => ((ToggleButton)Owner).IsChecked == true;

	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => null!;

	void ISelectionItemProvider.Select()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (((ToggleButton)Owner).IsChecked != true) ((ToggleButton)Owner).AutomationToggle();
	}

	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (((ToggleButton)Owner).IsChecked != true)
			throw new InvalidOperationException("A radio button cannot be added to a selection.");
	}

	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (((ToggleButton)Owner).IsChecked == true)
			throw new InvalidOperationException("A radio button cannot be removed from a selection.");
	}
}
