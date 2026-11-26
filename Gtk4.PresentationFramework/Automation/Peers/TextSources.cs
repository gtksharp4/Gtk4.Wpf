using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

sealed class TextBoxTextSource : ITextSource
{
	readonly TextBox _box;

	internal TextBoxTextSource(TextBox box) => _box = box;

	public string Text => _box.Text ?? string.Empty;

	public int SelectionStart => _box.SelectionStart;

	public int SelectionLength => _box.SelectionLength;

	public void Select(int start, int length) => _box.Select(start, length);
}

sealed class RichTextBoxTextSource : ITextSource
{
	readonly RichTextBox _box;

	internal RichTextBoxTextSource(RichTextBox box) => _box = box;

	public string Text => _box.AutomationText;

	public int SelectionStart => _box.AutomationSelection.Start;

	public int SelectionLength => _box.AutomationSelection.Length;

	public void Select(int start, int length) => _box.AutomationSelect(start, length);
}

/// <summary>The automation peer of a <see cref="RichTextBox"/>: a document with the text pattern.</summary>
public class RichTextBoxAutomationPeer : FrameworkElementAutomationPeer
{
	TextProvider? _text;

	public RichTextBoxAutomationPeer(RichTextBox owner) : base(owner)
	{
		owner.SelectionChanged += (s, e) => RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
		owner.TextChanged += (s, e) => RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
	}

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Text
			? _text ??= new TextProvider(new RichTextBoxTextSource((RichTextBox)Owner), this)
			: base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "RichTextBox";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Document;

	internal override Gtk.Widget Target(Gtk.Widget widget) => Owner.FocusWidget;

	internal override Gtk.AccessibleRole? AccessibleRole => Gtk.AccessibleRole.TextBox;

	internal override bool AffectsAccessible(DependencyProperty property) =>
		property == RichTextBox.IsReadOnlyProperty || base.AffectsAccessible(property);

	internal override void PushState(Gtk.Widget widget)
	{
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.ReadOnly, ((RichTextBox)Owner).IsReadOnly);
		PeerAccessible.SetProperty(widget, Gtk.AccessibleProperty.MultiLine, true);
	}
}
