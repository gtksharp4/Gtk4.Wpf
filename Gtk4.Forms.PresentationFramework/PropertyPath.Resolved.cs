using System.Text.RegularExpressions;

namespace System.Windows;

public sealed partial class PropertyPath
{
	static readonly Regex _qualified = new Regex(@"^\(\s*[A-Za-z_]\w*\.([A-Za-z_]\w*)\s*\)$");

	// A lone dependency property resolves by its name; the owner qualifier is not checked against the source.
	internal string ResolvedString
	{
		get
		{
			if (Path == "(0)" && PathParameters.Count == 1 && PathParameters[0] is DependencyProperty dp) return dp.Name;
			var m = _qualified.Match(Path);
			return m.Success ? m.Groups[1].Value : Path;
		}
	}
}
