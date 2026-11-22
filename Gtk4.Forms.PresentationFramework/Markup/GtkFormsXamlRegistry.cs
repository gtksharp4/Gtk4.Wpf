using System;
using System.Collections.Generic;
using System.IO;
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
public static partial class GtkFormsXamlRegistry
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

	/// <summary>
	/// Loads the dictionary registered for <paramref name="source"/> into <paramref name="target"/>.
	/// Returns <c>false</c> instead of throwing when nothing resolves (used by generated code).
	/// </summary>
	public static bool TryLoadDictionary(ResourceDictionary target, Uri source)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		return target.TrySetSource(source);
	}

	/// <summary>As <see cref="TryLoadDictionary(ResourceDictionary, Uri)"/>; also accepts <c>pack://application:,,,</c> text.</summary>
	public static bool TryLoadDictionary(ResourceDictionary target, string source)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		return !string.IsNullOrWhiteSpace(source) && target.TrySetSource(UnitConverters.ParseUri(source));
	}

	// exactOnly: match the full path only, never the leaf file name.
	internal static object? TryCreate(Uri uri, Assembly? assembly, bool exactOnly)
	{
		string raw = uri.IsAbsoluteUri ? Uri.UnescapeDataString(uri.AbsolutePath) : uri.OriginalString;
		string path = Normalize(raw);
		string? asmName = SplitAssembly(ref path);
		Func<object>? factory;
		lock (_factories) factory = Find(path, _factories, exactOnly);
		if (factory != null) return factory();

		Type? type = null;
		if (asmName != null)
		{
			Assembly? named = FindAssembly(asmName);
			if (named != null) type = Find(path, Registrations(named), exactOnly);
		}
		else
		{
			var own = assembly == null ? null : Registrations(assembly);
			type = own == null ? null : Find(path, own, true);
			if (type == null)
				foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (a == assembly || a.IsDynamic) continue;
					if ((type = Find(path, Registrations(a), true)) != null) break;
				}
			if (type == null && own != null && !exactOnly) type = Find(path, own, false);
		}
		return type == null ? null : Initialize(Activator.CreateInstance(type)!);
	}

	static Dictionary<string, Type> Registrations(Assembly assembly)
	{
		var types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		try
		{
			foreach (var a in assembly.GetCustomAttributes<GtkFormsXamlRegistrationAttribute>())
				types[Normalize(a.Path)] = a.Type;
		}
		catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is TypeLoadException)
		{
		}
		return types;
	}

	// "Asm;component/path" (any version/key parts after the name are ignored): returns Asm, leaves path.
	static string? SplitAssembly(ref string path)
	{
		int i = path.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
		if (i < 0) return null;
		string name = path.Substring(0, i);
		int semi = name.IndexOf(';');
		if (semi >= 0) name = name.Substring(0, semi);
		path = path.Substring(i + ";component/".Length).TrimStart('/');
		return name.Length == 0 ? null : name;
	}

	static Assembly? FindAssembly(string name)
	{
		foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			if (string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase)) return a;
		try { return Assembly.Load(new AssemblyName(name)); }
		catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is ArgumentException)
		{
			return null;
		}
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
