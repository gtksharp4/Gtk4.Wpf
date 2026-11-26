namespace System.Windows.Controls;

public partial class DataTemplate
{
	/// <summary>
	/// The triggers declared with the template. Store-only: they are read by XAML and kept on the template,
	/// but are not yet applied to the templated element.
	/// </summary>
	public TriggerCollection Triggers { get; } = new TriggerCollection();
}
