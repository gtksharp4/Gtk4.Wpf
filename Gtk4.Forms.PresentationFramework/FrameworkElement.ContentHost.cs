namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>Resource lookup entry for a <see cref="FrameworkContentElement"/> hosted by this element.</summary>
	internal bool TryFindResourceFromContent(object key, out object? value) => TryFindResourceCore(key, out value);
}
