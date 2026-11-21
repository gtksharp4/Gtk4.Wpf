using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>The automation peer of a <see cref="RepeatButton"/>.</summary>
public class RepeatButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
	public RepeatButtonAutomationPeer(RepeatButton owner) : base(owner) { }

	public override object? GetPattern(PatternInterface patternInterface) =>
		patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);

	protected override string GetClassNameCore() => "RepeatButton";

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

	void IInvokeProvider.Invoke()
	{
		if (!IsEnabled()) throw new ElementNotEnabledException();
		var owner = (ButtonBase)Owner;
		Dispatcher.BeginInvoke(owner.AutomationClick);
	}
}
