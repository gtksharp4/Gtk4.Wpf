using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Win32;

public abstract partial class FileDialog
{
	/// <summary>Splits a WPF filter string into its entries. An empty string yields none.</summary>
	internal static IReadOnlyList<FileDialogFilter> ParseFilter(string filter)
	{
		var entries = new List<FileDialogFilter>();
		if (string.IsNullOrEmpty(filter))
			return entries;
		var parts = filter.Split('|');
		for (int i = 0; i + 1 < parts.Length; i += 2)
		{
			var patterns = parts[i + 1].Split(';').Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
			entries.Add(new FileDialogFilter(parts[i], patterns));
		}
		return entries;
	}
}
