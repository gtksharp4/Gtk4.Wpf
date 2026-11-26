namespace System.Windows.Controls;

public partial class ControlTemplate
{
	/// <summary>
	/// The triggers declared with the template. Store-only: they are read by XAML and kept on the template,
	/// but are not yet applied to the templated control.
	/// </summary>
	public TriggerCollection Triggers { get; } = new TriggerCollection();
}
