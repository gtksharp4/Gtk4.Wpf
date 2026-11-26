using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	const string ClrPrefix = "clr-namespace:";

	// xmlns URI -> (CLR namespace, declaring assembly), rebuilt when assemblies load.
	static readonly object _mapLock = new object();
	static Dictionary<string, List<(string Clr, Assembly Asm)>> _xmlnsMap =
		new Dictionary<string, List<(string, Assembly)>>();
	static int _mapAssemblyCount = -1;

	static List<(string Clr, Assembly Asm)> EntriesFor(string xmlns)
	{
		lock (_mapLock)
		{
			Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
			if (all.Length != _mapAssemblyCount)
			{
				var map = new Dictionary<string, List<(string, Assembly)>>();
				foreach (Assembly a in all)
				{
					if (a.IsDynamic) continue;
					try
					{
						foreach (var d in a.GetCustomAttributes<XmlnsDefinitionAttribute>())
						{
							if (!map.TryGetValue(d.XmlNamespace, out var list)) map[d.XmlNamespace] = list = new List<(string, Assembly)>();
							list.Add((d.ClrNamespace, a));
						}
					}
					catch (Exception ex) when (ex is System.IO.IOException || ex is BadImageFormatException || ex is TypeLoadException)
					{
					}
				}
				_xmlnsMap = map;
				_mapAssemblyCount = all.Length;
			}
			return _xmlnsMap.TryGetValue(xmlns, out var found) ? found : new List<(string, Assembly)>();
		}
	}

	// True when the CLR namespace is mapped into the WPF presentation xmlns.
	internal static bool IsPresentationNamespace(string clrNamespace) =>
		EntriesFor(PresentationNs).Exists(e => e.Clr == clrNamespace);

	Type ResolveType(string xmlns, string local, XObject node) =>
		TryResolveType(xmlns, local) ?? throw Fail(node, "Cannot find type '" + local + "' in namespace '" + xmlns + "'.");

	static Type? TryResolveType(string xmlns, string local)
	{
		if (xmlns.StartsWith(ClrPrefix, StringComparison.Ordinal)) return ResolveClr(xmlns, local);
		var assemblies = new List<Assembly> { typeof(Application).Assembly, typeof(DependencyObject).Assembly };
		var namespaces = new List<string>();
		foreach (var entry in EntriesFor(xmlns))
		{
			namespaces.Add(entry.Clr);
			if (!assemblies.Contains(entry.Asm)) assemblies.Add(entry.Asm);
		}
		if (xmlns == PresentationNs) namespaces.Add("System.Windows.Markup");
		foreach (string ns in namespaces)
			foreach (Assembly a in assemblies)
				if (a.GetType(ns + "." + local) is { } t) return t;
		return null;
	}

	// clr-namespace:X;assembly=Y; without an assembly part every loaded assembly is searched.
	static Type? ResolveClr(string xmlns, string local)
	{
		string body = xmlns.Substring(ClrPrefix.Length);
		string clr = body;
		string? asmName = null;
		int semi = body.IndexOf(';');
		if (semi >= 0)
		{
			clr = body.Substring(0, semi);
			const string key = "assembly=";
			string rest = body.Substring(semi + 1).Trim();
			if (rest.StartsWith(key, StringComparison.OrdinalIgnoreCase)) asmName = rest.Substring(key.Length).Trim();
		}
		string full = clr.Length == 0 ? local : clr + "." + local;
		if (asmName != null)
		{
			Assembly? named = FindAssembly(asmName);
			return named?.GetType(full);
		}
		foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			if (!a.IsDynamic && a.GetType(full) is { } t) return t;
		return null;
	}

	static Assembly? FindAssembly(string name)
	{
		foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			if (string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase)) return a;
		try { return Assembly.Load(new AssemblyName(name)); }
		catch (Exception ex) when (ex is System.IO.IOException || ex is BadImageFormatException || ex is ArgumentException)
		{
			return null;
		}
	}

	// "prefix:Name" or "Name" in the scope of <paramref name="scope"/>.
	Type ResolveTypeName(string name, XElement scope)
	{
		string prefix = "";
		string local = name;
		int colon = name.IndexOf(':');
		if (colon >= 0) { prefix = name.Substring(0, colon); local = name.Substring(colon + 1); }
		XNamespace? ns = prefix.Length == 0 ? scope.GetDefaultNamespace() : scope.GetNamespaceOfPrefix(prefix);
		if (ns == null) throw Fail(scope, "The prefix '" + prefix + "' is not declared.");
		return ResolveType(ns.NamespaceName, local, scope);
	}
}
