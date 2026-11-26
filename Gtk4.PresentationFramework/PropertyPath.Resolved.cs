using System.Text.RegularExpressions;

namespace System.Windows;

public sealed partial class PropertyPath
{
	static readonly Regex _qualified = new Regex(@"^\(\s*(?:[A-Za-z_][\w.\-]*:)?([A-Za-z_]\w*)\.([A-Za-z_]\w*)\s*\)$");

	// A lone dependency property resolves by its name.
	internal string ResolvedString
	{
		get
		{
			if (Path == "(0)" && PathParameters.Count == 1 && PathParameters[0] is DependencyProperty dp) return dp.Name;
			return Path;
		}
	}

	/// <summary>
	/// Strips the owner from a single-segment "(Owner.Name)" path when Owner is the source's type or a base type, so
	/// the CLR member resolves; an xmlns prefix on the owner is ignored (matched by simple name). Any other owner (an
	/// attached property) is left unresolved rather than matched to an
	/// unrelated CLR member of the same name.
	/// </summary>
	internal static string? ResolveOwner(object? source, string? path)
	{
		if (source == null || string.IsNullOrEmpty(path)) return path;
		var m = _qualified.Match(path!);
		if (!m.Success) return path;
		for (var t = source.GetType(); t != null; t = t.BaseType)
			if (t.Name == m.Groups[1].Value) return m.Groups[2].Value;
		return path;
	}
}
