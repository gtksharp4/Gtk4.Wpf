namespace System.Windows;

public partial class Window
{
	internal bool IsShowingAsDialog => _showingAsDialog;

	/// <summary>Returns the <see cref="Window"/> that hosts <paramref name="dependencyObject"/>, or null.</summary>
	public static Window? GetWindow(DependencyObject dependencyObject)
	{
		if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));
		DependencyObject? current = dependencyObject;
		while (current != null)
		{
			if (current is Window w) return w;
			current = System.Windows.Media.VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
		}
		return null;
	}
}
