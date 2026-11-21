using System.Windows.Automation.Peers;

namespace System.Windows.Controls;

public partial class ComboBox
{
	protected override AutomationPeer? OnCreateAutomationPeer() => new ComboBoxAutomationPeer(this);

	/// <summary>The drop-down carries the accessible node; an editable host box is only a wrapper.</summary>
	internal Gtk.Widget AccessibleTarget => _dropDown ?? Widget;
}
