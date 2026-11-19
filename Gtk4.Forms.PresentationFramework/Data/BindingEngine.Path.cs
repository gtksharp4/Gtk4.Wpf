using System.Globalization;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>Binding path resolution, write-back and StringFormat helpers.</summary>
internal static partial class BindingEngine
{
	// ── Path resolution ──────────────────────────────────────────────────────

	/// <summary>Resolves a dot-separated path on <paramref name="source"/>.</summary>
	internal static object? ResolvePath(object? source, string? path)
	{
		if (source == null) return null;
		if (string.IsNullOrEmpty(path)) return source;

		var current = source;
		foreach (var segment in path!.Split('.'))
		{
			if (current == null) return null;
			var prop = current.GetType().GetProperty(
				segment, BindingFlags.Public | BindingFlags.Instance);
			if (prop == null) return null;
			current = prop.GetValue(current);
		}
		return current;
	}

	/// <summary>Resolves the object and property that a write-back through <paramref name="path"/> targets.</summary>
	internal static bool TryResolveWriteTarget(
		object? source, string? path, out object owner, out PropertyInfo prop)
	{
		owner = null!;
		prop = null!;
		if (source == null || string.IsNullOrEmpty(path)) return false;

		var current = source;
		var segments = path!.Split('.');
		for (int i = 0; i < segments.Length - 1; i++)
		{
			var p = current.GetType().GetProperty(segments[i], BindingFlags.Public | BindingFlags.Instance);
			if (p == null) return false;
			var next = p.GetValue(current);
			if (next == null) return false;
			current = next;
		}

		var lastProp = current.GetType().GetProperty(
			segments[segments.Length - 1], BindingFlags.Public | BindingFlags.Instance);
		if (lastProp == null || !lastProp.CanWrite) return false;
		owner = current;
		prop = lastProp;
		return true;
	}

	/// <summary>
	/// Writes <paramref name="value"/> back through the dot-separated path on
	/// <paramref name="source"/>. Returns <c>true</c> on success.
	/// </summary>
	internal static bool WriteBackPath(object? source, string? path, object? value)
	{
		if (!TryResolveWriteTarget(source, path, out var owner, out var prop)) return false;
		prop.SetValue(owner, value);
		return true;
	}

	internal static object? ApplyStringFormat(string? fmt, object? value, CultureInfo culture)
	{
		if (fmt == null) return value;
		string s = fmt.Contains("{0") ? fmt : fmt + "{0}";
		return string.Format(culture, s, value);
	}
}
