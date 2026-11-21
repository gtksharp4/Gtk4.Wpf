using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Automation.Provider;
using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>
/// The automation peer of a <see cref="Hyperlink"/>. A hyperlink is not an element in this layer (it has no
/// widget of its own), so the peer derives from <see cref="AutomationPeer"/> and is reached through the peer
/// of the <see cref="System.Windows.Controls.TextBlock"/> that shows it.
/// </summary>
public class HyperlinkAutomationPeer : AutomationPeer, IInvokeProvider
{
	static readonly ConditionalWeakTable<Hyperlink, HyperlinkAutomationPeer> _peers =
		new ConditionalWeakTable<Hyperlink, HyperlinkAutomationPeer>();

	readonly Hyperlink _owner;
	AutomationPeer? _parentPeer;

	public HyperlinkAutomationPeer(Hyperlink owner)
	{
		_owner = owner ?? throw new ArgumentNullException(nameof(owner));
	}

	internal static HyperlinkAutomationPeer For(Hyperlink link, AutomationPeer parent)
	{
		var peer = _peers.GetValue(link, l => new HyperlinkAutomationPeer(l));
		peer._parentPeer = parent;
		return peer;
	}

	internal override AutomationPeer? ResolveParent() => _parentPeer ?? base.ResolveParent();

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Invoke ? this : null;

	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		Dispatcher.BeginInvoke(_owner.OnClick);
	}

	protected override string GetClassNameCore() => "Hyperlink";

	protected override string GetNameCore()
	{
		var sb = new StringBuilder();
		AppendText(_owner, sb);
		return sb.ToString();
	}

	static void AppendText(Inline inline, StringBuilder sb)
	{
		switch (inline)
		{
			case Run run: sb.Append(run.Text); break;
			case LineBreak _: sb.Append('\n'); break;
			case Span span:
				foreach (var child in span.Inlines) AppendText(child, sb);
				break;
		}
	}

	protected override string GetAutomationIdCore() => string.Empty;

	protected override string GetHelpTextCore() => string.Empty;

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Hyperlink;

	protected override Rect GetBoundingRectangleCore() => _parentPeer?.GetBoundingRectangle() ?? Rect.Empty;

	protected override Point GetClickablePointCore()
	{
		var r = GetBoundingRectangle();
		return r.IsEmpty ? new Point(double.NaN, double.NaN) : new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
	}

	protected override List<AutomationPeer>? GetChildrenCore() => null;

	protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;

	protected override bool HasKeyboardFocusCore() => false;

	protected override bool IsContentElementCore() => true;

	protected override bool IsControlElementCore() => true;

	protected override bool IsEnabledCore() => _parentPeer?.IsEnabled() ?? true;

	protected override bool IsKeyboardFocusableCore() => false;

	protected override bool IsOffscreenCore() => _parentPeer?.IsOffscreen() ?? true;

	protected override bool IsPasswordCore() => false;

	protected override bool IsRequiredForFormCore() => false;

	protected override void SetFocusCore() => _parentPeer?.SetFocus();
}
