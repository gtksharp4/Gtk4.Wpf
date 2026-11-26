using System;

namespace System.Windows;

/// <summary>Arguments for <see cref="Application.Startup"/>.</summary>
public class StartupEventArgs : EventArgs
{
	readonly string[] _args;

	internal StartupEventArgs(string[]? args)
	{
		_args = args ?? Array.Empty<string>();
	}

	/// <summary>The command-line arguments, without the executable. Never <see langword="null"/>.</summary>
	public string[] Args => _args;

	/// <summary>Command-line arguments of the current process without the executable.</summary>
	internal static string[] ProcessArgs()
	{
		var all = Environment.GetCommandLineArgs();
		if (all.Length <= 1)
			return Array.Empty<string>();
		var rest = new string[all.Length - 1];
		Array.Copy(all, 1, rest, 0, rest.Length);
		return rest;
	}
}

/// <summary>Handles <see cref="Application.Startup"/>.</summary>
public delegate void StartupEventHandler(object sender, StartupEventArgs e);
