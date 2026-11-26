namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>Removes <paramref name="name"/> from this element's name scope.</summary>
	public void UnregisterName(string name) => NameScope.Unregister(this, name);
}
