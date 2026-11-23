using System.Collections.Generic;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>
/// Attached-property path steps, <c>(Owner.Name)</c> or <c>(prefix:Owner.Name)</c>. The owner is a type's simple
/// name: the xmlns prefix is not resolved (no parser context reaches the engine), so every loaded type with that
/// name is a candidate, the source's own type chain first, and the first one that registers the property wins.
/// </summary>
internal static partial class BindingEngine
{
	static readonly Dictionary<string, List<Type>> _ownerTypes = new Dictionary<string, List<Type>>();

	/// <summary>Splits "(prefix:Owner.Name)" into owner and name; false for any other step.</summary>
	internal static bool TrySplitAttached(string step, out string owner, out string name)
	{
		owner = name = "";
		if (step.Length < 5 || step[0] != '(' || step[step.Length - 1] != ')') return false;
		var inner = step.Substring(1, step.Length - 2).Trim();
		var colon = inner.LastIndexOf(':');
		if (colon >= 0) inner = inner.Substring(colon + 1);
		var dot = inner.LastIndexOf('.');
		if (dot <= 0 || dot == inner.Length - 1) return false;
		owner = inner.Substring(0, dot).Trim();
		name = inner.Substring(dot + 1).Trim();
		return true;
	}

	/// <summary>The dependency property an attached step names on <paramref name="source"/>, or null.</summary>
	internal static DependencyProperty? AttachedProperty(object source, string step)
	{
		if (source is not DependencyObject || !TrySplitAttached(step, out var owner, out var name)) return null;
		for (var t = source.GetType(); t != null; t = t.BaseType)
			if (t.Name == owner && DependencyProperty.FindByName(name, t) is { } own) return own;
		foreach (var type in TypesNamed(owner))
			if (DependencyProperty.FindByName(name, type) is { } dp) return dp;
		return null;
	}

	/// <summary>The CLR property a step names when its owner is the source's type or a base type, else null.</summary>
	internal static string? ClrNameOfOwnedStep(object source, string step)
	{
		if (!TrySplitAttached(step, out var owner, out var name)) return null;
		for (var t = source.GetType(); t != null; t = t.BaseType)
			if (t.Name == owner) return name;
		return null;
	}

	/// <summary>The dependency property named <paramref name="name"/> by a plain step on <paramref name="source"/>.</summary>
	internal static DependencyProperty? NamedProperty(object source, string name) =>
		source is DependencyObject ? DependencyProperty.FindByName(name, source.GetType()) : null;

	static List<Type> TypesNamed(string name)
	{
		lock (_ownerTypes)
		{
			if (_ownerTypes.TryGetValue(name, out var cached)) return cached;
			var found = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				foreach (var t in SafeTypes(assembly))
					if (t.Name == name) found.Add(t);
			if (found.Count > 0) _ownerTypes[name] = found;
			return found;
		}
	}

	static IEnumerable<Type> SafeTypes(Assembly assembly)
	{
		try { return assembly.GetTypes(); }
		catch (ReflectionTypeLoadException ex) { return Array.FindAll(ex.Types, t => t != null)!; }
		catch (Exception) { return Array.Empty<Type>(); }
	}
}
