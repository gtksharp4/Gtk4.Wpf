namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>
	/// Called when a template has been applied. Only <see cref="Controls.Control"/> applies templates
	/// here; a FrameworkElement with its own CreateWidget never calls it.
	/// </summary>
	public virtual void OnApplyTemplate() { }
}
