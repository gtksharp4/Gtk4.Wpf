namespace System.Windows;

public abstract partial class FrameworkElement
{
	// The key of a XAML-attribute {DynamicResource} on dp, or null; read by XamlWriter.
	internal object? LocalResourceKeyOf(DependencyProperty dp) =>
		_resRefs?.Find(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, LocalLayerContributor)).Key;
}
