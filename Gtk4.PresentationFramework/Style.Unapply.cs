namespace System.Windows;

public partial class Style
{
	/// <summary>
	/// Removes the live state (resource links, bindings, event handlers) this style's setters
	/// attached to <paramref name="target"/>. Called before the style layer is cleared.
	/// </summary>
	internal void Unapply(DependencyObject target)
	{
		foreach (var style in BuildChain())
			foreach (var setter in style.Setters)
				setter.Unapply(target, null);
	}
}
