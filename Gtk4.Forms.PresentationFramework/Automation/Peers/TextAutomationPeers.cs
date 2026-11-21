using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="TextBox"/>.</summary>
public class TextBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
{
	TextProvider? _text;

	public TextBoxAutomationPeer(TextBox owner) : base(owner)
	{
		owner.SelectionChanged += (s, e) => RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
		owner.TextChanged += (s, e) => RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
	}

	public override object? GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Value) return this;
		if (patternInterface == PatternInterface.Text)
			return _text ??= new TextProvider(new TextBoxTextSource((TextBox)Owner), this);
		return base.GetPattern(patternInterface);
	}

	protected override string GetClassNameCore() => "TextBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;

	bool IValueProvider.IsReadOnly => ((TextBox)Owner).IsReadOnly;

	string IValueProvider.Value => ((TextBox)Owner).Text ?? string.Empty;

	// WPF checks in this order: enabled, read-only, then the value.
	void IValueProvider.SetValue(string value)
	{
		var box = (TextBox)Owner;
		if (!IsEnabled()) throw new ElementNotEnabledException();
		if (box.IsReadOnly) throw new ElementNotEnabledException();
		if (value == null) throw new ArgumentNullException(nameof(value));
		box.Text = value;
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

	bool IValueProvider.IsReadOnly => false;

	string IValueProvider.Value => throw new InvalidOperationException("The value of a password box cannot be read.");

	void IValueProvider.SetValue(string value)
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

	internal override string ContentName() => ((TextBlock)Owner).Text ?? string.Empty;

	// The hyperlinks the block shows are its children.
	protected override System.Collections.Generic.List<AutomationPeer>? GetChildrenCore()
	{
		var result = new System.Collections.Generic.List<AutomationPeer>();
		foreach (var link in ((TextBlock)Owner).AutomationHyperlinks())
			result.Add(HyperlinkAutomationPeer.For(link, this));
		return result.Count == 0 ? null : result;
	}
}

/// <summary>The automation peer of a <see cref="Label"/>.</summary>
public class LabelAutomationPeer : FrameworkElementAutomationPeer
{
	public LabelAutomationPeer(Label owner) : base(owner) { }

	protected override string GetClassNameCore() => "Label";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;

	internal override string ContentName() => PeerText.FromContent(((Label)Owner).Content);

	// Label wraps its Gtk.Label in a box; the inner label is the accessible node.
	internal override Gtk.Widget Target(Gtk.Widget widget) => ((Label)Owner).AccessibleTarget;
}
