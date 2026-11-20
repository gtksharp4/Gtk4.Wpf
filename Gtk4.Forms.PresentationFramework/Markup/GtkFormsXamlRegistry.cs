using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>
/// Emitted by the markup compiler for a Page/Window XAML without <c>x:Class</c>: maps the XAML
/// path to the generated class.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GtkFormsXamlRegistrationAttribute : Attribute
{
	/// <summary>Creates a registration of <paramref name="type"/> under <paramref name="path"/>.</summary>
	public GtkFormsXamlRegistrationAttribute(string path, Type type)
	{
		Path = path;
		Type = type;
	}

	/// <summary>The XAML file path, relative to the project.</summary>
	public string Path { get; }

	/// <summary>The generated class.</summary>
	public Type Type { get; }
}

/// <summary>
/// Maps a XAML URI to a factory for the element it loads (used by <c>StartupUri</c>; available to
/// navigation). Factories registered here win over the assembly attributes.
/// </summary>
public static class GtkFormsXamlRegistry
{
	static readonly Dictionary<string, Func<object>> _factories =
		new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase);

	/// <summary>Registers <paramref name="factory"/> for <paramref name="path"/>.</summary>
	public static void Register(string path, Func<object> factory)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (factory == null) throw new ArgumentNullException(nameof(factory));
		lock (_factories) _factories[Normalize(path)] = factory;
	}

	/// <summary>Creates the element registered for <paramref name="uri"/>, or <c>null</c>.</summary>
	public static object? TryCreate(Uri uri, Assembly? assembly = null) => TryCreate(uri, assembly, false);

	// exactOnly: match the full path only, never the leaf file name.
	internal static object? TryCreate(Uri uri, Assembly? assembly, bool exactOnly)
	{
		string raw = uri.IsAbsoluteUri ? Uri.UnescapeDataString(uri.AbsolutePath) : uri.OriginalString;
		string path = Normalize(raw);
		Func<object>? factory;
		lock (_factories) factory = Find(path, _factories, exactOnly);
		if (factory != null) return factory();
		if (assembly == null) return null;

		var types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		foreach (var a in assembly.GetCustomAttributes<GtkFormsXamlRegistrationAttribute>())
			types[Normalize(a.Path)] = a.Type;
		Type? type = Find(path, types, exactOnly);
		return type == null ? null : Activator.CreateInstance(type);
	}

	static string Normalize(string path)
	{
		string p = path.Replace('\\', '/');
		int comma = p.LastIndexOf(",,,/", StringComparison.Ordinal);
		if (comma >= 0) p = p.Substring(comma + 4);
		return p.TrimStart('/');
	}

	// Exact path first, then the leaf file name.
	static T? Find<T>(string path, Dictionary<string, T> map, bool exactOnly) where T : class
	{
		if (map.TryGetValue(path, out T? exact)) return exact;
		if (exactOnly) return null;
		string leaf = Leaf(path);
		foreach (KeyValuePair<string, T> kv in map)
			if (string.Equals(Leaf(kv.Key), leaf, StringComparison.OrdinalIgnoreCase)) return kv.Value;
		return null;
	}

	static string Leaf(string path) => path.Substring(path.LastIndexOf('/') + 1);
}
