namespace System.Windows;

public partial class ResourceDictionary
{
	// Tells live references the dictionary was swapped out (same signal as Clear()).
	internal void NotifyReplaced() => _resourceChangedHandlers.Raise(null, DependencyProperty.UnsetValue);
}
