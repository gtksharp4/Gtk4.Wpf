using System;
using System.Collections.Generic;
using System.IO;

namespace Gtk4.MarkupCompiler
{
	static class CodeBehindScan
	{
		// Files under the output directory are earlier generated output, never code-behind.
		internal static string[] ReadSources(IEnumerable<string> paths, string outputDirectory)
		{
			var texts = new List<string>();
			string outDir = Path.GetFullPath(outputDirectory)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			foreach (string path in paths)
			{
				if (Path.GetFullPath(path).StartsWith(outDir, StringComparison.OrdinalIgnoreCase)) continue;
				try { texts.Add(File.ReadAllText(path)); }
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
			}
			return texts.ToArray();
		}
	}
}
