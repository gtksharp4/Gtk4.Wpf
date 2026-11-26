using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Gtk4.MarkupCompiler
{
	/// <summary>Detects whether a directory sits on a case-sensitive file system (APFS and NTFS usually are not).</summary>
	static class FileSystemCase
	{
		/// <summary>
		/// Probes <paramref name="directory"/> with a throw-away file; falls back to the OS convention (Windows and
		/// macOS insensitive, others sensitive) when the directory cannot be written.
		/// </summary>
		internal static bool IsCaseSensitive(string directory)
		{
			string name = "gfcase-" + Guid.NewGuid().ToString("N");
			string path = Path.Combine(directory, name);
			try
			{
				File.WriteAllText(path, "");
				try { return !File.Exists(Path.Combine(directory, name.ToUpperInvariant())); }
				finally { File.Delete(path); }
			}
			catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
			{
				return Path.DirectorySeparatorChar == '/' && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
			}
		}
	}
}
