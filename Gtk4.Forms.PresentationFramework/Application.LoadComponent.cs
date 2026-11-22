using System;
using System.IO;
using System.Windows.Markup;

namespace System.Windows;

public partial class Application
{
	void ReplaceResources(ResourceDictionary value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		ResourceDictionary old = _resources;
		if (ReferenceEquals(old, value)) return;
		_resources = value;
		old.NotifyReplaced();
	}

	/// <summary>Loads the object registered for <paramref name="resourceLocator"/> (pack or relative URI).</summary>
	/// <exception cref="ArgumentNullException"><paramref name="resourceLocator"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The URI is absolute and not a <c>pack:</c> URI.</exception>
	/// <exception cref="IOException">Nothing is registered for the URI.</exception>
	public static object LoadComponent(Uri resourceLocator)
	{
		Uri uri = Locate(resourceLocator);
		return GtkFormsXamlRegistry.TryCreate(uri, Current?.GetType().Assembly)
			?? throw new IOException("Cannot locate resource '" + resourceLocator.OriginalString + "'.");
	}

	/// <summary>Runs the loader registered for <paramref name="resourceLocator"/> into <paramref name="component"/>.</summary>
	/// <exception cref="IOException">No loader is registered for the URI.</exception>
	public static void LoadComponent(object component, Uri resourceLocator)
	{
		if (component == null) throw new ArgumentNullException(nameof(component));
		Uri uri = Locate(resourceLocator);
		if (!GtkFormsXamlRegistry.TryLoad(uri, component))
			throw new IOException("Cannot locate resource '" + resourceLocator.OriginalString + "'.");
	}

	// A leading slash is relative on every OS (on Unix .NET would read "/a" as a file URI).
	static Uri Locate(Uri resourceLocator)
	{
		if (resourceLocator == null) throw new ArgumentNullException(nameof(resourceLocator));
		string text = resourceLocator.OriginalString;
		bool rooted = text.StartsWith("/", StringComparison.Ordinal) || text.StartsWith("\\", StringComparison.Ordinal);
		if (rooted || !resourceLocator.IsAbsoluteUri) return new Uri(text, UriKind.Relative);
		if (!string.Equals(resourceLocator.Scheme, "pack", StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException("Only relative and pack URIs are supported.", nameof(resourceLocator));
		return resourceLocator;
	}
}
