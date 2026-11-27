namespace System.Windows.Controls;

/// <summary>
/// Base of the views a <see cref="ListView"/> can show its items through (<see cref="GridView"/>). A view supplies
/// the header above the rows and the presenter that lays out the content of each <see cref="ListViewItem"/>.
/// </summary>
public abstract class ViewBase : DependencyObject
{
	/// <summary>Raised when the view changes in a way that its rows or header must be rebuilt.</summary>
	internal event EventHandler? Changed;

	internal void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);

	/// <summary>The header element shown above the rows, or null for a view without one.</summary>
	internal virtual UIElement? CreateHeader() => null;

	/// <summary>The element that presents the content of <paramref name="item"/>, or null for the default one.</summary>
	internal virtual UIElement? CreateRowPresenter(ListViewItem item) => null;
}
