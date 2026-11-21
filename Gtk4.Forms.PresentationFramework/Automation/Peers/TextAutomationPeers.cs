using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="TextBox"/>.</summary>
public class TextBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
{
	public TextBoxAutomationPeer(TextBox owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Value ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "TextBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;

	public bool IsReadOnly => ((TextBox)Owner).IsReadOnly;

	public string Value => ((TextBox)Owner).Text ?? string.Empty;

	public void SetValue(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (IsReadOnly) throw new InvalidOperationException("The text box is read-only.");
		((TextBox)Owner).Text = value;
	}

	// A multi-line box wraps its text view in a scrolled window; the editor is the accessible node.
	internal override Gtk.Widget Target(Gtk.Widget widget) => Owner.FocusWidget;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == TextBox.IsReadOnlyProperty || property == TextBox.AcceptsReturnProperty
		|| base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		var box = (TextBox)Owner;
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ReadOnly, box.IsReadOnly);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.MultiLine, box.AcceptsReturn);
	}

	internal override void OwnerPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == TextBox.TextProperty)
			RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
		else if (e.Property == TextBox.IsReadOnlyProperty)
			RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, e.OldValue, e.NewValue);
	}
}

/// <summary>The automation peer of a <see cref="PasswordBox"/>. The password is never readable.</summary>
public class PasswordBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
{
	public PasswordBoxAutomationPeer(PasswordBox owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Value ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "PasswordBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;

	protected override bool IsPasswordCore() => true;

	public bool IsReadOnly => false;

	public string Value => throw new InvalidOperationException("The value of a password box cannot be read.");

	public void SetValue(string value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (!IsEnabled()) throw new ElementNotEnabledException();
		((PasswordBox)Owner).Password = value;
	}
}

/// <summary>The automation peer of a <see cref="TextBlock"/>.</summary>
public class TextBlockAutomationPeer : FrameworkElementAutomationPeer
{
	public TextBlockAutomationPeer(TextBlock owner) : base(owner) { }

	protected override string GetClassNameCore() => "TextBlock";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

	protected override string GetNameCore()
	{
		var name = base.GetNameCore();
		return name.Length > 0 ? name : ((TextBlock)Owner).Text ?? string.Empty;
	}
}

/// <summary>The automation peer of a <see cref="Label"/>.</summary>
public class LabelAutomationPeer : FrameworkElementAutomationPeer
{
	public LabelAutomationPeer(Label owner) : base(owner) { }

	protected override string GetClassNameCore() => "Label";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

	protected override string GetNameCore()
	{
		var name = base.GetNameCore();
		return name.Length > 0 ? name : ((Label)Owner).Content as string ?? string.Empty;
	}
}
