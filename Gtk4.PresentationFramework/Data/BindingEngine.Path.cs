using System.Globalization;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>Binding path resolution, write-back and StringFormat helpers.</summary>
internal static partial class BindingEngine
{
	// ── Path resolution ──────────────────────────────────────────────────────

	/// <summary>Resolves a path ('.' separates properties, '/' steps to a current item) on the source.</summary>
	internal static object? ResolvePath(object? source, string? path)
	{
		if (source == null) return null;
		return TryWalk(source, path, null, out var value) ? value : null;
	}

	/// <summary>Resolves the object and property that a write-back through <paramref name="path"/> targets.</summary>
	internal static bool TryResolveWriteTarget(
		object? source, string? path, out object owner, out PropertyInfo prop)
	{
		owner = null!;
		prop = null!;
		path = PropertyPath.ResolveOwner(source, path);
		if (source == null || string.IsNullOrEmpty(path)) return false;

		SplitLeaf(path!, out var head, out var leaf);
		if (leaf == null) return false;
		if (!TryWalk(source, head, null, out var current) || current == null) return false;
		current = LeafOwner(current, leaf);
		var lastProp = current?.GetType().GetProperty(leaf, Members);
		if (current == null || lastProp == null || !lastProp.CanWrite) return false;
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
		return string.Format(culture, GetEffectiveStringFormat(fmt), value);
	}

	/// <summary>
	/// A format without braces (N2, C) is a composite-format item body, as in WPF. A leading <c>{}</c> is the XAML
	/// escape, which the generated code passes through; "{}" is never a valid composite format, so it is dropped.
	/// </summary>
	internal static string GetEffectiveStringFormat(string fmt)
	{
		// The generated code keeps the quotes of StringFormat='{}...'. "'{}" cannot start a valid format, so unwrap it.
		if (fmt.Length >= 4 && fmt[0] == '\'' && fmt[fmt.Length - 1] == '\'' && fmt[1] == '{' && fmt[2] == '}')
			fmt = fmt.Substring(1, fmt.Length - 2);
		if (fmt.StartsWith("{}", StringComparison.Ordinal)) fmt = fmt.Substring(2);
		return fmt.IndexOf('{') < 0 ? "{0:" + fmt + "}" : fmt;
	}
}
