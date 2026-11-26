using System.Windows.Input;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>Called when the element's access key is pressed; the default focuses the element.</summary>
	protected virtual void OnAccessKey(AccessKeyEventArgs e) => Focus();

	internal void InvokeAccessKey(AccessKeyEventArgs e) => OnAccessKey(e);
}
