using System.Windows.Media;

namespace System.Windows.Input;

public static partial class FocusManager
{
	/// <summary>
	/// Identifies the IsFocusScope attached property (default false): marks a container that keeps its own
	/// logical <see cref="FocusedElementProperty"/>. Store-only here: command routing through the parent
	/// scope's focused element is not implemented, so the flag only changes <see cref="GetFocusScope"/>.
	/// </summary>
	public static readonly DependencyProperty IsFocusScopeProperty =
		DependencyProperty.RegisterAttached("IsFocusScope", typeof(bool), typeof(FocusManager),
			new UIPropertyMetadata(false));

	/// <summary>Writes IsFocusScope on <paramref name="element"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static void SetIsFocusScope(DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsFocusScopeProperty, value);
	}

	/// <summary>Reads IsFocusScope from <paramref name="element"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static bool GetIsFocusScope(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)element.GetValue(IsFocusScopeProperty)!;
	}

	/// <summary>
	/// The closest self-or-ancestor (logical parent first, then visual parent) with IsFocusScope true;
	/// the root element when there is none.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static DependencyObject GetFocusScope(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		var current = element;
		while (!(bool)current.GetValue(IsFocusScopeProperty)!)
		{
			DependencyObject? parent = current is UIElement ui
				? (DependencyObject?)ui.LogicalParent ?? VisualTreeHelper.GetParent(current)
				: (current as FrameworkContentElement)?.Parent;
			if (parent == null) break;
			current = parent;
		}
		return current;
	}
}
