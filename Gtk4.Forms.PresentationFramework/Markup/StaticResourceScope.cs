using System;

namespace System.Windows.Markup;

/// <summary>
/// Load-time <c>{StaticResource}</c> resolution for generated code. Probes the enclosing parse scopes
/// innermost first, then the root and the application, as WPF's parser scope stack does.
/// </summary>
public static class StaticResourceScope
{
	/// <summary>Resolves a key against the scopes (innermost first), then the root.</summary>
	/// <exception cref="InvalidOperationException">The key is not found anywhere.</exception>
	public static object? Find(object key, object root, params object?[] scopes)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (root == null) throw new ArgumentNullException(nameof(root));
		if (TryFindInScopes(key, scopes, out var value)) return value;
		return root switch
		{
			FrameworkElement fe => fe.FindResource(key),
			Application app => app.FindResource(key),
			_ => throw new InvalidOperationException($"Resource with key '{key}' not found."),
		};
	}

	/// <summary>As <see cref="Find"/>, but returns <c>null</c> instead of throwing when the key is not found.</summary>
	public static object? TryFind(object key, object root, params object?[] scopes)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (root == null) throw new ArgumentNullException(nameof(root));
		if (TryFindInScopes(key, scopes, out var value)) return value;
		return root switch
		{
			FrameworkElement fe => fe.TryFindResource(key),
			Application app => app.TryFindResource(key),
			_ => null,
		};
	}

	static bool TryFindInScopes(object key, object?[] scopes, out object? value)
	{
		foreach (var scope in scopes)
		{
			var dict = scope switch
			{
				FrameworkElement fe => fe.Resources,
				Application app => app.Resources,
				ResourceDictionary rd => rd,
				_ => null,
			};
			if (dict != null && dict.TryGetValue(key, out value)) return true;
		}
		value = null;
		return false;
	}
}
