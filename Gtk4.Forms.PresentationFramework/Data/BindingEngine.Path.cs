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

	/// <summary>
	/// Writes <paramref name="value"/> back through the dot-separated path on
	/// <paramref name="source"/>. Returns <c>true</c> on success.
	/// </summary>
	internal static bool WriteBackPath(object? source, string? path, object? value)
	{
		if (source == null) return false;
		if (string.IsNullOrEmpty(path)) return false;

		var current = source;
		var segments = path!.Split('.');
		for (int i = 0; i < segments.Length - 1; i++)
		{
			if (current == null) return false;
			var prop = current.GetType().GetProperty(segments[i],
				BindingFlags.Public | BindingFlags.Instance);
			if (prop == null) return false;
			current = prop.GetValue(current);
		}

		if (current == null) return false;
		var lastProp = current.GetType().GetProperty(
			segments[segments.Length - 1], BindingFlags.Public | BindingFlags.Instance);
		if (lastProp == null || !lastProp.CanWrite) return false;
		lastProp.SetValue(current, value);
		return true;
	}

	static object? ApplyStringFormat(string? fmt, object? value)
	{
		if (fmt == null) return value;
		string s = fmt.Contains("{0}") ? fmt : fmt + "{0}";
		return string.Format(CultureInfo.InvariantCulture, s, value);
	}
}
