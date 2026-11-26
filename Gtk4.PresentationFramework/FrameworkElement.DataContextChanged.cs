namespace System.Windows;

public partial class FrameworkElement
{
	/// <summary>Occurs when the effective <see cref="DataContext"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? DataContextChanged;

	void RaiseDataContextChanged(DependencyPropertyChangedEventArgs e) => DataContextChanged?.Invoke(this, e);
}
