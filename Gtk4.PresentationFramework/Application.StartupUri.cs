using System;
using System.Reflection;
using System.Windows.Markup;

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
	/// The markup compiler compiles each XAML's <c>x:Class</c> into a partial class whose constructor
	/// calls <c>InitializeComponent</c>; this resolves the URI's leaf file name (e.g.
	/// <c>MainWindow.xaml</c> → <c>MainWindow</c>) to that type in the application assembly.
	/// Any other FrameworkElement root (a <see cref="Controls.Page"/>, a panel) is hosted in a
	/// <see cref="Navigation.NavigationWindow"/>, as WPF does.
	/// Returns <c>null</c> when no matching type is found.
	/// </summary>
	internal Window? CreateWindowFromStartupUri(Uri startupUri)
	{
		// Exact registry path, then the x:Class name, then the registry's leaf-name match.
		object? registered = GtkFormsXamlRegistry.TryCreate(startupUri, GetType().Assembly, true);
		if (registered != null) return Host(registered);
		string leaf = StartupUriToTypeName(startupUri);
		object? byName = CreateInstance(typeof(Window), leaf) ?? CreateInstance(typeof(Controls.Page), leaf)
			?? CreateInstance(typeof(FrameworkElement), leaf);
		return Host(byName ?? GtkFormsXamlRegistry.TryCreate(startupUri, GetType().Assembly));
	}

	static Window? Host(object? element)
	{
		if (element != null) GtkFormsXamlRegistry.Initialize(element);
		if (element is Window window) return window;
		if (element is not FrameworkElement root) return null;
		var host = new Navigation.NavigationWindow();
		host.Navigate(root);
		return host;
	}

	object? CreateInstance(Type baseType, string leaf)
	{
		// App is defined in the consumer's assembly, so look there for the type.
		object? fallback = null;
		foreach (Type t in GetType().Assembly.GetTypes())
		{
			if (t.IsAbstract || !baseType.IsAssignableFrom(t)) continue;
			if (!string.Equals(t.Name, leaf, StringComparison.Ordinal)) continue;

			// Prefer a match in App's own namespace; otherwise take the first candidate.
			if (string.Equals(t.Namespace, GetType().Namespace, StringComparison.Ordinal))
				return Activator.CreateInstance(t)!;
			fallback ??= Activator.CreateInstance(t)!;
		}
		return fallback;
	}
}
