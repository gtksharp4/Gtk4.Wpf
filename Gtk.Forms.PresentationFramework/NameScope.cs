using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows
{
	/// <summary>
	/// Per-element name registry. Stores a dictionary on the root element of a XAML tree
	/// (the one that owns the generated <c>InitializeComponent</c>) so that
	/// <see cref="FrameworkElement.FindName"/> can resolve <c>x:Name</c> references.
	/// </summary>
	internal static class NameScope
	{
		// Use ConditionalWeakTable so the dictionary is released when the owner is GC'd.
		static readonly ConditionalWeakTable<FrameworkElement, Dictionary<string, object>> _scopes =
			new ConditionalWeakTable<FrameworkElement, Dictionary<string, object>>();

		/// <summary>
		/// Registers <paramref name="name"/> → <paramref name="element"/> on the scope
		/// rooted at <paramref name="owner"/>.
		/// </summary>
		internal static void Register(FrameworkElement owner, string name, object element)
		{
			var dict = _scopes.GetOrCreateValue(owner);
			dict[name] = element;
		}

		/// <summary>
		/// Walks up from <paramref name="start"/> to find the first scope that contains
		/// <paramref name="name"/>. Returns the element, or null if not found.
		/// </summary>
		internal static object? FindName(FrameworkElement start, string name)
		{
			DependencyObject? current = start;
			while (current != null)
			{
				if (current is FrameworkElement fe &&
					_scopes.TryGetValue(fe, out var dict) &&
					dict.TryGetValue(name, out var found))
				{
					return found;
				}
				current = current.InheritanceParent;
			}
			return null;
		}
	}
}
