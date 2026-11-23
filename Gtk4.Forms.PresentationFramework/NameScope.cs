using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Maps names to objects. Mirrors <c>System.Windows.NameScope</c>; the root of a XAML tree
/// owns one through the <see cref="NameScopeProperty"/> attached property.
/// </summary>
public partial class NameScope : INameScope
{
	/// <summary>Identifies the NameScope attached property.</summary>
	public static readonly DependencyProperty NameScopeProperty = DependencyProperty.RegisterAttached(
		"NameScope", typeof(INameScope), typeof(NameScope), new PropertyMetadata(null, OnNameScopeChanged));

	readonly Dictionary<string, object> _names = new Dictionary<string, object>();

	/// <summary>Gets the name scope attached to <paramref name="dependencyObject"/>.</summary>
	public static INameScope? GetNameScope(DependencyObject dependencyObject)
	{
		if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));
		return (INameScope?)dependencyObject.GetValue(NameScopeProperty);
	}

	/// <summary>Attaches <paramref name="value"/> to <paramref name="dependencyObject"/>.</summary>
	public static void SetNameScope(DependencyObject dependencyObject, INameScope? value)
	{
		if (dependencyObject == null) throw new ArgumentNullException(nameof(dependencyObject));
		dependencyObject.SetValue(NameScopeProperty, value);
	}

	/// <inheritdoc/>
	public object? FindName(string name) =>
		name != null && _names.TryGetValue(name, out var found) ? found : null;

	/// <inheritdoc/>
	public void RegisterName(string name, object scopedElement)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (scopedElement == null) throw new ArgumentNullException(nameof(scopedElement));
		if (name.Length == 0) throw new ArgumentException("Name must not be empty.", nameof(name));
		if (_names.ContainsKey(name))
			throw new ArgumentException($"Cannot register duplicate name '{name}' in this name scope.", nameof(name));
		_names[name] = scopedElement;
		NameChanged(name);
	}

	/// <inheritdoc/>
	public void UnregisterName(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (name.Length == 0) throw new ArgumentException("Name must not be empty.", nameof(name));
		if (!_names.Remove(name))
			throw new ArgumentException($"'{name}' is not registered in this name scope.", nameof(name));
		NameChanged(name);
	}

	internal static void Register(FrameworkElement owner, string name, object element)
	{
		if (!(owner.GetValue(NameScopeProperty) is INameScope scope))
		{
			scope = new NameScope();
			owner.SetValue(NameScopeProperty, scope);
		}
		scope.RegisterName(name, element);
	}

	internal static void Unregister(FrameworkElement owner, string name) =>
		(owner.GetValue(NameScopeProperty) as INameScope)?.UnregisterName(name);

	/// <summary>Walks up from <paramref name="start"/> to the first scope that holds <paramref name="name"/>.</summary>
	internal static object? FindName(FrameworkElement start, string name)
	{
		DependencyObject? current = start;
		while (current != null)
		{
			if (current.GetValue(NameScopeProperty) is INameScope scope && scope.FindName(name) is { } found)
				return found;
			current = current.InheritanceParent;
		}
		return null;
	}
}
