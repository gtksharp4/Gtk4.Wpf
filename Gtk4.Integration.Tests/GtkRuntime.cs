using System;
using System.IO;

namespace Gtk4.Integration.Tests;

/// <summary>Detects whether the native GTK 4 runtime is available to run the built app.</summary>
static class GtkRuntime
{
	/// <summary>
	/// True when the native GTK 4 library is discoverable — either on <c>PATH</c> or in
	/// GtkSharp's per-user install location (<c>%LOCALAPPDATA%\Gtk\&lt;version&gt;\bin</c> on
	/// Windows), which is where the app itself loads it from.
	/// </summary>
	public static bool IsAvailable()
	{
		string name =
			OperatingSystem.IsWindows() ? "gtk-4-1.dll" :
			OperatingSystem.IsMacOS()   ? "libgtk-4.4.dylib" :
			"libgtk-4.so.1";

		return OnPath(name) || InGtkSharpInstall(name);
	}

	static bool OnPath(string fileName)
	{
		string pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
		char separator = OperatingSystem.IsWindows() ? ';' : ':';

		foreach (string dir in pathVar.Split(separator, StringSplitOptions.RemoveEmptyEntries))
		{
			if (Exists(dir, fileName))
				return true;
		}
		return false;
	}

	// GtkSharp installs GTK under %LOCALAPPDATA%\Gtk\<version>\bin via its one-time installer.
	static bool InGtkSharpInstall(string fileName)
	{
		if (!OperatingSystem.IsWindows())
			return false;

		string gtkRoot = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gtk");
		if (!Directory.Exists(gtkRoot))
			return false;

		foreach (string versionDir in Directory.EnumerateDirectories(gtkRoot))
		{
			if (Exists(Path.Combine(versionDir, "bin"), fileName))
				return true;
		}
		return false;
	}

	static bool Exists(string dir, string fileName)
	{
		try { return File.Exists(Path.Combine(dir, fileName)); }
		catch { return false; } // ignore malformed path entries
	}
}