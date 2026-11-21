using System;
using System.Collections.Generic;

namespace System.Windows.Markup;

public static partial class GtkFormsXamlRegistry
{
	static readonly Dictionary<string, Action<object>> _loaders =
		new Dictionary<string, Action<object>>(StringComparer.OrdinalIgnoreCase);

	/// <summary>Registers the loader that fills an existing <c>x:Class</c> instance from <paramref name="path"/>.</summary>
	public static void RegisterLoader(string path, Action<object> loader)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (loader == null) throw new ArgumentNullException(nameof(loader));
		lock (_loaders) _loaders[Normalize(path)] = loader;
	}

	// Runs the loader registered for the URI into component; false when none is registered.
	internal static bool TryLoad(Uri uri, object component)
	{
		string raw = uri.IsAbsoluteUri ? Uri.UnescapeDataString(uri.AbsolutePath) : uri.OriginalString;
		string path = Normalize(raw);
		SplitAssembly(ref path);
		Action<object>? loader;
		lock (_loaders) loader = Find(path, _loaders, false);
		if (loader == null) return false;
		loader(component);
		return true;
	}
}
