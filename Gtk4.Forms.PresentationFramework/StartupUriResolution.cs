using System;
using System.Reflection;

namespace System.Windows;

public partial class Application
{
	/// <summary>
	/// Reduces a StartupUri to the leaf XAML file name without extension — the expected
	/// <see cref="Window"/> type name. Handles relative (<c>MainWindow.xaml</c>,
	/// <c>Views/MainWindow.xaml</c>), backslash, and absolute pack/file URIs.
	/// </summary>
	internal static string StartupUriToTypeName(Uri startupUri)
	{
		string path = startupUri.IsAbsoluteUri ? startupUri.AbsolutePath : startupUri.OriginalString;
		string leaf = path.Replace('\\', '/');
		int slash = leaf.LastIndexOf('/');
		if (slash >= 0) leaf = leaf.Substring(slash + 1);
		if (leaf.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
			leaf = leaf.Substring(0, leaf.Length - ".xaml".Length);
		return leaf;
	}

	/// <summary>
	/// Instantiates the <see cref="Window"/> that <paramref name="startupUri"/> refers to.
	/// The markup compiler compiles each XAML's <c>x:Class</c> into a partial
	/// <see cref="Window"/> whose constructor calls <c>InitializeComponent</c>; this resolves
	/// the URI's leaf file name (e.g. <c>MainWindow.xaml</c> → <c>MainWindow</c>) to that type
	/// in the application assembly and constructs it. Returns <c>null</c> when no matching
	/// window type is found.
	/// </summary>
	Window? CreateWindowFromStartupUri(Uri startupUri)
	{
		string leaf = StartupUriToTypeName(startupUri);

		// App is defined in the consumer's assembly, so look there for the window type.
		Assembly appAssembly = GetType().Assembly;
		Window? fallback = null;
		foreach (Type t in appAssembly.GetTypes())
		{
			if (t.IsAbstract || !typeof(Window).IsAssignableFrom(t)) continue;
			if (!string.Equals(t.Name, leaf, StringComparison.Ordinal)) continue;

			// Prefer a match in App's own namespace; otherwise take the first candidate.
			if (string.Equals(t.Namespace, GetType().Namespace, StringComparison.Ordinal))
				return (Window)Activator.CreateInstance(t)!;
			fallback ??= (Window)Activator.CreateInstance(t)!;
		}
		return fallback;
	}
}
